/*
   Bitvantage.SharpTextFsm
   Copyright (C) 2024 Michael Crino

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU Affero General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU Affero General Public License for more details.

   You should have received a copy of the GNU Affero General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Bitvantage.SharpTextFsm.Cache;
using Bitvantage.SharpTextFsm.Definitions;
using Bitvantage.SharpTextFsm.Exceptions;
using Bitvantage.SharpTextFsm.Explain;
using Bitvantage.SharpTextFsm.TemplateHelpers;

namespace Bitvantage.SharpTextFsm;

public enum TextFsmState
{
    Initialize,
    ReadNextLine,
    Match,
    EndProcessing,
}

public class Template
{
    private static readonly Regex ValidStateNameRegex = new("""
        ^
        	(
        		\w{1,32}|	# any word characters
        		~Global # the special global state
        	)
        $
        """, RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex ValidValueNameRegex = new("""
        ^
        	(
        		\S{1,48}	# any non-white space character s
        	)
        $
        """, RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex ValidRegexValueNameRegex = new("""
        ^
        	(
        		[a-zA-Z0-9\-_:@#%&]{1,48}
        	)
        $
        """, RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

    private readonly Template? _template;

    private readonly TemplateOptions _templateOptions;
    private readonly ConcurrentDictionary<Type, object> _typeSerializerCache;

    private readonly ValueDescriptorCollection _valueDescriptorCollection;

    internal readonly ImmutableHashSet<TemplateState> UserStates;
    public StateCollection States => _template != null ? _template.States : _states;

    private readonly StateCollection _states;

    public ValueDescriptorCollection Values => _template != null ? _template._valueDescriptorCollection : _valueDescriptorCollection;

    public Template(string template) : this(template, TemplateOptions.Default)
    {
    }

    public Template(string template, TemplateOptions templateOptions)
    {
        var cache = ConcurrentLruCache.TemplateCache;
        var cachedTemplate = cache.GetOrAdd(new TemplateCacheKey(template, templateOptions), key => new Template(templateOptions, TemplateDefinition.Parse(template)));
        _template = cachedTemplate;
    }

    public Template(TemplateDefinition templateDefinition) : this(templateDefinition, TemplateOptions.Default)
    {
    }

    public Template(TemplateDefinition templateDefinition, TemplateOptions templateOptions)
    {
        var textFsmTemplate = templateDefinition.ToString();
        var cache = ConcurrentLruCache.TemplateCache;
        var cachedTemplate = cache.GetOrAdd(new TemplateCacheKey(textFsmTemplate, templateOptions), key => new Template(templateOptions, templateDefinition));
        _template = cachedTemplate;
    }

    private Template(TemplateOptions templateOptions, TemplateDefinition templateDefinition)
    {
        _states = new StateCollection(this);
        _templateOptions = templateOptions;
        _typeSerializerCache = new ConcurrentDictionary<Type, object>();

        // verify that there is at least one value
        if (!templateDefinition.Values.Any())
            throw new TemplateParseException("No 'Value' definitions found", ParseError.NoValueDefinitions);

        // verify that value names are valid
        foreach (var value in templateDefinition.Values.Where(item => !ValidValueNameRegex.IsMatch(item.Name)))
            throw new TemplateParseException($"Invalid value name: {value.Name}", ParseError.InvalidValueName);

        // validate the name of values with the regex option set
        // the regex option has a more restrictive naming convention due to it being used in other value statements. 
        foreach (var valueDefinition in templateDefinition.Values.Where(item => item.Options == Option.Regex && !ValidRegexValueNameRegex.IsMatch(item.Name)))
            throw new TemplateParseException($"Invalid value name: {valueDefinition.Name}", ParseError.InvalidValueName);

        // verify that the value names are not reserved
        foreach (var value in templateDefinition.Values.Where(item => item.Name is "Filldown" or "Key" or "Required" or "List" or "Fillup"))
            throw new TemplateParseException($"The value '{value.Name}' is a reserved keyword", ParseError.ReservedValueKeyword);

        // verify that there is at least one state
        if (!templateDefinition.States.Any())
            throw new TemplateParseException("No state definitions found", ParseError.NoStateDefinitions);

        // verify that there is a 'Start' state
        if (templateDefinition.States.All(state => state.Name != "Start"))
            throw new TemplateParseException("No 'Start' state definitions found", ParseError.NoStartState);

        // verify that the state name is valid
        foreach (var state in templateDefinition.States.Where(item => !ValidStateNameRegex.IsMatch(item.Name)))
            throw new TemplateParseException($"Invalid state name: {state.Name}", ParseError.InvalidStateName);

        // verify that state names are unique
        foreach (var stateGroup in templateDefinition.States.GroupBy(item => item.Name).Where(item => item.Count() > 1))
            throw new TemplateParseException($"State is defined more then once: {stateGroup.Key}", ParseError.DuplicateState);

        // verify that the 'End' & 'EOF' states either do not exist or are empty
        foreach (var stateName in new[] { "End", "EOF" })
        {
            var state = templateDefinition.States.SingleOrDefault(item => item.Name == stateName);
            if (state is { Rules.Count: > 0 })
                throw new TemplateParseException($"Non-Empty '{stateName}' state", ParseError.StateMustBeEmpty);
        }

        // verify that there are no state filters on rules in regular states
        foreach (var ruleDefinition in templateDefinition.States.Where(state => state.Name != "~Global").SelectMany(state => state.Rules.Select(definition => new { state, rule = definition })).Where(rule => rule.rule.StateFilter != null))
            throw new TemplateParseException($"State filter on rule in non-~Global state: '{ruleDefinition.state.ToString()!.Replace("\r\n", "")}' in state '{ruleDefinition.state.Name}'", ParseError.StateFilterInRegularRule);

        // create initial states
        var templateStates = templateDefinition.States.ToDictionary(item => item.Name, item => new TemplateState(this, item.Name));

        // Add an End dummy state
        if (!templateStates.ContainsKey("End"))
            templateStates.Add("End", new TemplateState(this, "End") { Rules = ImmutableArray.Create<Rule>() });

        // add value descriptors
        _valueDescriptorCollection = new ValueDescriptorCollection(this, templateDefinition.Values);

        // build rules for each state
        UserStates = templateStates
            .Where(state => state.Key != "EOF" && state.Key != "End")
            .Select(state => state.Value)
            .ToImmutableHashSet();

        // the EOF state is a special case
        // if no EOF state is not specified, then there is an implicit '^.* -> Record' rule
        // if an EOF state is specified, it can have no rules, which in effect is implicitly a '^.* -> NoRecord' rule
        // additionally both rules have an implicit state change to End
        // to simplify the rule processing engine, explicitly add all the implicit stuff
        var endOfFileState = templateStates.Values.SingleOrDefault(item => item.Name == "EOF");
        if (endOfFileState != null)
        {
            endOfFileState.Rules = new Rule[]
            {
                new(this, endOfFileState, "^.*", new StateFilter(null, false, UserStates), LineAction.Next, RecordAction.NoRecord, new ChangeStateAction(templateStates["End"]), _valueDescriptorCollection)
            }.ToImmutableArray();

            States.Add(endOfFileState);
        }
        else
        {
            endOfFileState = new TemplateState(this, "EOF");

            endOfFileState.Rules = new Rule[]
            {
                new(this, endOfFileState, "^.*", new StateFilter(null, false, UserStates), LineAction.Next, RecordAction.Record, new ChangeStateAction(templateStates["End"]), _valueDescriptorCollection)
            }.ToImmutableArray();

            templateStates.Add("EOF", endOfFileState);
            States.Add(endOfFileState);
        }

        // verify each rule of each state
        foreach (var state in templateDefinition.States)
        {
            var ruleId = 0;
            foreach (var rule in state.Rules)
            {
                ruleId++;

                // verify that the state uses a supported action
                if (rule.MatchAction != null && rule.MatchAction is not ChangeStateActionDefinition and not ErrorActionDefinition)
                    throw new TemplateParseException($"Rule #{ruleId} in state '{state.Name}' specifies and unsupported action type of {rule.MatchAction.GetType()}", ParseError.InvalidAction);

                // if the rule has a state transition, verify the state name is defined
                // there is always an implicit EOF state if it is not explicitly defined later on 
                if (rule.MatchAction is ChangeStateActionDefinition changeStateActionNew && changeStateActionNew.StateName != "EOF" && !templateStates.ContainsKey(changeStateActionNew.StateName))
                    throw new TemplateParseException($"Rule #{ruleId} in state '{state.Name}' references an undefined state of '{changeStateActionNew.StateName}' in the state transition", ParseError.UndefinedState);

                // The global state is not permitted as a state transition to ensure that the state machines is loop free
                if (rule.MatchAction is ChangeStateActionDefinition { StateName: "~Global" })
                    throw new TemplateParseException($"Rule #{ruleId} in state '{state.Name}' references a restrict state of '~Global' in the state transition", ParseError.StateRestricted);

                // validate state filter
                if (rule.StateFilter != null)
                {
                    // if the rule has a state filter, verify that each state name is valid
                    foreach (var filterState in rule.StateFilter)
                        if (!templateStates.ContainsKey(filterState))
                            throw new TemplateParseException($"Rule #{ruleId} in state '{state.Name}' references an undefined state of '{filterState}' in the state filter", ParseError.UndefinedStateInStateFilter);

                    // verify that there are no duplicate state names in the state filter
                    foreach (var filterState in rule.StateFilter.GroupBy(item => item).Where(item => item.Count() > 1))
                        throw new TemplateParseException($"Rule #{ruleId} in state '{state.Name}' references an the filter state '{filterState}' more then once", ParseError.DuplicateStateFilterState);
                }
            }
        }

        // add each state
        // if there is a ~Global state add it first
        foreach (var state in templateDefinition.States.Where(item=>item.Name != "EOF").OrderBy(item => item.Name != "~Global"))
        {
            var templateState = templateStates[state.Name];
            var rules = new List<Rule>();

            foreach (var newRule in state.Rules)
            {
                // construct state filter
                var stateFilter = new StateFilter(newRule.StateFilter?.Select(item => templateStates[item]), newRule.NegatesStateFilter, UserStates);
                RuleAction? ruleAction = null;

                switch (newRule.MatchAction)
                {
                    // construct a rule action
                    case ErrorActionDefinition action:
                        ruleAction = new ErrorAction(action.Message);
                        break;

                    case ChangeStateActionDefinition changeStateActionNew:
                        ruleAction = new ChangeStateAction(templateStates[changeStateActionNew.StateName]);
                        break;

                    case null:
                        break;

                    default:
                        // TODO: this needs to be verified prior to here
                        throw new TemplateParseException($"Unsupported type {newRule.MatchAction.GetType()}", ParseError.UnsupportedAction);
                }

                // construct the rule
                var rule = new Rule(this, templateState, newRule.Pattern, stateFilter, newRule.LineAction, newRule.RecordAction, ruleAction, _valueDescriptorCollection);
                rules.Add(rule);
            }

            templateState.Rules = rules.ToImmutableArray();
            States.Add(templateState);
        }

        // in the reference implementation the 'Continue' action does not accept a state transition to ensure that the state machines is loop free
        // relax this rule such that if no loop can possibly exist the 'Continue' action is allowed with state transitions
        // if a loop could occur; throw an exception
        ThrowOnCycle();
    }

    public ExplainResult<T> Explain<T>(string text)
    {
        return Explain<T>(text, null);
    }

    public ExplainResult<T> Explain<T>(string text, object? state)
    {
        if (_template != null)
            return _template.Explain<T>(text, state);

        var explainResult = new ExplainResult<T>();
        ParseInternal(text, explainResult);

        return explainResult;
    }

    public ExplainResult Explain(string text)
    {
        if (_template != null)
            return _template.Explain(text);

        var explainResult = new ExplainResult();
        Run(text, explainResult);

        return explainResult;
    }

    /// <summary>
    /// Creates a new <c>Template</c>, extracting the TextFSM template from the <c>ITemplate.TextFSM property</c> of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Template FromType<T>() where T : ITemplate, new()
    {
        return FromType<T>(TemplateOptions.Default);
    }

    /// <summary>
    /// Creates a new <c>Template</c>, extracting the TextFSM template from the <c>ITemplate.TextFSM property</c> of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="templateOptions"></param>
    /// <returns></returns>
    public static Template FromType<T>(TemplateOptions templateOptions) where T : ITemplate, new()
    {
        var iTemplate = new T();

        return new Template(iTemplate.TextFsmTemplate, templateOptions);
    }

    internal RowCollection Run(string text, ExplainResult? explainResult = null)
    {
        return ParseInternal(new StringReader(text), explainResult);
    }

    public IEnumerable<T> Run<T>(string text)
    {
        return Run<T>(text, null);
    }

    public IEnumerable<T> Run<T>(string text, object? state)
    {
        if (_template != null)
            return _template.Run<T>(text, state);

        var valueCollection = Run(text, null);

        var typeSerializer = (TypeSerializer<T>)_typeSerializerCache.GetOrAdd(typeof(T), _ => new TypeSerializer<T>(_valueDescriptorCollection));
        var values = typeSerializer.Serialize(valueCollection, state);

        return values;
    }

    public RowCollection Run(string text)
    {
        if (_template != null)
            return _template.Run(text);

        var valueCollection = Run(text, null);

        return valueCollection;
    }

    private RowCollection ParseInternal(TextReader text, ExplainResult? explainResult = null)
    {
        var values = new RowCollection(_valueDescriptorCollection, _templateOptions);
        if (explainResult != null)
            explainResult.RowCollection = values;

        var context = new TemplateFsmContext(TextFsmState.Initialize, States["Start"]);
        do
        {
            switch (context.FsmState)
            {
                case TextFsmState.Initialize:
                    explainResult?.Add(new StateChange(context.TemplateState, context.Text, context.Line));

                    context.FsmState = TextFsmState.ReadNextLine;

                    break;

                case TextFsmState.ReadNextLine:
                    context.Text = text.ReadLine();

                    context.CurrentIndex = 0;
                    context.Line++;
                    context.FsmState = TextFsmState.Match;
                    
                    if (context.Text == null)
                    {
                        context.Text = string.Empty;
                        context.TemplateState = States["EOF"];

                        explainResult?.Add(new LineRead(context.TemplateState, null, context.Line));
                    }
                    else
                        explainResult?.Add(new LineRead(context.TemplateState, context.Text, context.Line));

                    break;

                case TextFsmState.Match:

                    bool matchSuccess;
                    TemplateState.RuleMatch? matchResults;

                    if(explainResult == null)
                        matchSuccess = context.TemplateState.TryGetMatch(context, out matchResults);
                    else
                        matchSuccess = context.TemplateState.TryGetMatch(context, values.CurrentRow, explainResult, out matchResults);

                    if (!matchSuccess)
                    {
                        context.FsmState = TextFsmState.ReadNextLine;
                        continue;
                    }

                    context.Rule = matchResults!.Rule;
                    context.CurrentIndex = matchResults.RuleIndex;

                    // if the rule has an error action, then throw
                    if (context.Rule.Action is ErrorAction errorAction)
                    {
                        if (explainResult == null)
                            throw new TemplateErrorException(errorAction, context.TemplateState.Name, context.Rule.ToString(), context.Text!, context.Line);

                        explainResult.Error = errorAction.Message;

                        return values;
                    }

                    // set metadata values
                    values.SetMetadata(context);

                    // extract matching value descriptor and store values
                    foreach (var captureGroup in context.Rule.CaptureGroupMapping)
                    {
                        var group = matchResults.Match.Groups[captureGroup.Key];
                        values.SetValue(captureGroup.Value, group);
                    }

                    // execute rule action
                    switch (context.Rule.RecordAction)
                    {
                        case RecordAction.Record: // store the current row and clear non-filldown values
                            values.Record();
                            break;

                        case RecordAction.Clear: // clear non-filldown values
                            values.Clear();
                            break;

                        case RecordAction.ClearAll: // clear all values
                            values.ClearAll();
                            break;
                    }

                    // advance the rule index such that the next this state executes it starts at the next rule
                    context.CurrentIndex++;

                    // if the rule changes the template state, set it
                    // unless we are processing global rules and the global rule changes the state name to the current state
                    if (context.Rule.Action is ChangeStateAction changeState)
                    {
                        // reset the rule index since the state was changed
                        // BUG: if the changed state is the same as the current state, this can create a loop
                        context.CurrentIndex = 0;

                        // end is a reserved state that ends processing without executing the EOF state
                        if (changeState.NewState.Name == "End")
                        {
                            context.FsmState = TextFsmState.EndProcessing;
                        }
                        else
                        {
                            context.TemplateState = changeState.NewState;

                            // BUG: no need to set this, it is the current state
                            context.FsmState = TextFsmState.Match;

                            // set the next rule index to the first rule of the state
                            // skipping the rules in global
                            context.CurrentIndex = changeState.NewState.EffectiveRules.Length - changeState.NewState.Rules.Length;
                        }

                        explainResult?.Add(new StateChange(context.TemplateState, context.Text, context.Line));
                    }

                    if (context.FsmState == TextFsmState.EndProcessing)
                        break;

                    switch (matchResults.Rule.LineAction)
                    {
                        // if a rule matched, and the matched rule has the line action set to next, change the fsm state to ReadNextLine
                        case LineAction.Next:
                            context.FsmState = TextFsmState.ReadNextLine;
                            break;

                        // if the matched rule has the line action set to continue without changing states, stay in the same FSM state to get the next match
                        case LineAction.Continue:

                            // if there are no more rules to continue to, read the next line
                            if(context.CurrentIndex >= context.TemplateState.EffectiveRules.Length)
                                context.FsmState = TextFsmState.ReadNextLine;

                            break;
                    }

                    break;
            }
        } while (context.FsmState != TextFsmState.EndProcessing);
        
        return values;
    }

    private IEnumerable<T> ParseInternal<T>(string text, ExplainResult<T>? explainResult = null, object? state = null)
    {
        if (_template != null)
            return _template.ParseInternal(text, explainResult);

        var typeSerializer = (TypeSerializer<T>)_typeSerializerCache.GetOrAdd(typeof(T), _ => new TypeSerializer<T>(_valueDescriptorCollection));

        var valueCollection = ParseInternal(new StringReader(text), explainResult);

        var values = typeSerializer.Serialize(valueCollection, state);

        if (explainResult != null)
        {
            var results = values.ToList();
            explainResult.Results = results;
            return results;
        }

        return values;
    }

    public override string ToString()
    {
        if (_template != null)
            return _template.ToString();

        var sb = new StringBuilder();

        sb.Append(_valueDescriptorCollection.ToString());

        if (_valueDescriptorCollection.Any())
            sb.AppendLine();

        foreach (var state in States.Where(item=>item.Name != "End" && item.Name != "EOF"))
        {
            sb.AppendLine($"{state.Name}");

            foreach (var rule in state.Rules)
                sb.AppendLine($"{rule.ToString()}");

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd('\n', '\r');
    }

    private void ThrowOnCycle()
    {
        // build a tree of all connected states
        var nodes = BuildStateCycleTree();

        // generate a graph of the state tree
        // var dotGraph = StateCycleTreeToDotGraph(nodes);
        // var url = $"https://dreampuf.github.io/GraphvizOnline/#{Uri.EscapeDataString(dotGraph)}";
        // Process.Start(new ProcessStartInfo(url) { UseShellExecute = true })
        // Process.Start(new ProcessStartInfo($"https://dreampuf.github.io/GraphvizOnline/#{Uri.EscapeDataString(StateCycleTreeToDotGraph(nodes))}") { UseShellExecute = true });


        // if there are no connections, then there is nothing to do
        if (nodes.Count == 0)
            return;

        // search the tree for any nodes that have state transitions with the 'Continue' line operation and create a loop
        // the root node is the node closets to the 'Start' node, inclusive, that is not a stub node
        var root = nodes.FirstOrDefault(node => !node.Stub);

        // if there are only stub nodes then there can be no cycle
        if (root == null)
            return;

        var cycle = FindCycle(root);

        // if there are no cycles, then there is nothing to do
        if (cycle == null)
            return;

        // include regular rules that change state
        var localRule = States
            .SelectMany(item => item.Rules)
            .Where(item => item.Action is ChangeStateAction)
            .Select(rule => new { Rule = rule, State = rule.State.Name, Neighbor = ((ChangeStateAction)rule.Action!).NewState.Name })
            .Where(rule => rule.State == cycle.StateCycleLink.Node.Name && rule.Neighbor == cycle.StateCycleLink.Neighbor.Name)
            .Select(rule => rule.Rule)
            .First();

        var loopStateNames = cycle.History.Select(item => item.Name).ToArray();
        var dotGraph = StateCycleTreeToDotGraph(nodes);
        var dotGraphUrl = $"https://dreampuf.github.io/GraphvizOnline/#{Uri.EscapeDataString(dotGraph)}";

        var exception = new TemplateParseException($"State loop detected between states '{string.Join(" > ", loopStateNames)}' from state '{localRule.State.Name}' on rule '{localRule.ToString().Replace("\r\n", " ").TrimStart()}'", ParseError.StateLoop);
        exception.Data.Add("loop", cycle.History);
        exception.Data.Add("loopNames", string.Join(",", cycle.History.Select(item => item.Name)));
        exception.Data.Add("rule", localRule);
        exception.Data.Add("digraph", dotGraph);
        exception.Data.Add("digraphLink", dotGraphUrl);

        throw exception;
    }

    private StateCycle? FindCycle(StateCycleNode rootStateCycleNode)
    {
        var queue = new Queue<StateCycleFrame>();
        queue.Enqueue(new StateCycleFrame(rootStateCycleNode));

        while (queue.TryDequeue(out var stackFrame))
        {
            if (stackFrame.Node.Stub)
                continue;

            // work through each neighbor link
            foreach (var nodeLink in stackFrame.Node.OutgoingLinks)
            {
                // skip stub nodes and any edge that is not a transition
                if (nodeLink.Neighbor.Stub || nodeLink.StateCycleEdgeType != StateCycleEdgeType.Transition)
                    continue;

                // if the link has already been visited; then there is a cycle
                if (stackFrame.NodeHistory.Contains(nodeLink.Neighbor))
                {
                    var stateHistory = stackFrame.NodeHistory.Select(item => States[item.Name])
                        .Concat(new[] { States[nodeLink.Neighbor.Name] })
                        .ToArray();

                    return new StateCycle(nodeLink, stateHistory);
                }

                // add the neighbor to the stack to be visited
                queue.Enqueue(new StateCycleFrame(nodeLink.Neighbor, nodeLink, stackFrame));
            }
        }

        return null;
    }

    private record StateCycle(StateCycleLink StateCycleLink, TemplateState[] History);

    private record StateCycleFrame
    {
        public StateCycleNode Node { get; }
        public List<StateCycleNode> NodeHistory { get; } = new();
        public List<StateCycleLink> LinkHistory { get; } = new();

        public StateCycleFrame(StateCycleNode node)
        {
            Node = node;
            NodeHistory.Add(node);
        }

        public StateCycleFrame(StateCycleNode node, StateCycleLink link, StateCycleFrame lastStateCycleFrame)
        {
            Node = node;

            NodeHistory = new List<StateCycleNode>(NodeHistory.Count + 1);
            NodeHistory.AddRange(lastStateCycleFrame.NodeHistory);
            NodeHistory.Add(node);

            LinkHistory = new List<StateCycleLink>(LinkHistory.Count + 1);
            LinkHistory.AddRange(lastStateCycleFrame.LinkHistory);
            LinkHistory.Add(link);
        }
    }

    private List<StateCycleNode> BuildStateCycleTree()
    {
        // build a list of all rules
        var nodes = new Dictionary<string, StateCycleNode>();

        foreach (var state in States.Where(state => state.Name is not "End" and not "EOF"))
        {
            // include regular rules that change state
            var stateRules = state
                .Rules
                .Where(item => item.Action is ChangeStateAction);

            // get the node, add it if it does not already exist
            if (!nodes.TryGetValue(state.Name, out var node))
            {
                node = new StateCycleNode(state.Name);
                nodes.Add(node.Name, node);
            }

            foreach (var rule in stateRules)
            {
                var action = (ChangeStateAction)rule.Action!;

                // get the target node, add it if it does not already exist
                if (!nodes.TryGetValue(action.NewState.Name, out var neighbor))
                {
                    neighbor = new StateCycleNode(action.NewState.Name);
                    nodes.Add(action.NewState.Name, neighbor);
                }

                var edgeType = rule.LineAction switch
                {
                    LineAction.Continue => StateCycleEdgeType.Transition,
                    LineAction.Next => StateCycleEdgeType.Link,
                    _ => throw new ArgumentOutOfRangeException()
                };

                // Transition links supersede regular Links

                // if this is a regular Link and there is already a Transition link, do nothing
                if (edgeType == StateCycleEdgeType.Link && node.OutgoingLinks.Contains(new StateCycleLink(node, neighbor, StateCycleEdgeType.Transition)))
                    continue;

                // if this is a Transition Link and there is already a Regular link, get rid of the regular link
                if (edgeType == StateCycleEdgeType.Transition)
                    node.OutgoingLinks.Remove(new StateCycleLink(node, neighbor, StateCycleEdgeType.Link));

                neighbor.IncomingLinks.Add(new StateCycleLink(neighbor, node, edgeType));
                node.OutgoingLinks.Add(new StateCycleLink(node, neighbor, edgeType));
            }
        }

        // if there is no Start state then there is no graph
        if (!nodes.TryGetValue("Start", out var startNode))
            return new List<StateCycleNode>();

        // mark nodes without both an incoming and outgoing Transition links as stubs
        // a stub node is a node that has no way to form a loop by virtue of it having either no incoming transitions or no outgoing transitions.
        // an indirect stub node is a node that connects to only other stub nodes
        // in a sense a stub node will 'infect' other nodes until it reaches a node with both inbound and outbound transitions
        var recheckNodes = new HashSet<StateCycleNode>(nodes.Values);
        while (recheckNodes.Count > 0)
        {
            var checkNodes = new Queue<StateCycleNode>(recheckNodes);
            recheckNodes.Clear();
            while (checkNodes.TryDequeue(out var node))
            {
                if (node.Stub)
                    continue;

                if (node.OutgoingLinks.All(item => item.StateCycleEdgeType != StateCycleEdgeType.Transition || item.Neighbor.Stub) || node.IncomingLinks.All(item => item.StateCycleEdgeType != StateCycleEdgeType.Transition || item.Neighbor.Stub))
                {
                    node.Stub = true;

                    // recheck nodes attached to incoming links
                    var links = Enumerable
                        .Empty<StateCycleLink>()
                        .Concat(node.IncomingLinks)
                        .Concat(node.OutgoingLinks)
                        .Where(item => !item.Neighbor.Stub);

                    foreach (var stateCycleLink in links)
                        recheckNodes.Add(stateCycleLink.Neighbor);
                }
            }
        }

        // set the distance from the root node
        recheckNodes = new HashSet<StateCycleNode> { startNode };
        var distance = -1;
        while (recheckNodes.Count > 0)
        {
            distance++;
            var checkNodes = new Queue<StateCycleNode>(recheckNodes);
            recheckNodes.Clear();
            while (checkNodes.TryDequeue(out var node))
            {
                if (node.Distance > -1 && node.Distance < distance)
                    continue;

                node.Distance = distance;

                foreach (var stateCycleLink in node.OutgoingLinks)
                    recheckNodes.Add(stateCycleLink.Neighbor);
            }
        }

        var dotNodes = nodes
            .Values
            .OrderBy(node => node.Name != "Start")
            .ThenBy(node => node.Distance)
            .ThenBy(node => node.Name)
            .ToList();

        return dotNodes;
    }

    private static string StateCycleTreeToDotGraph(List<StateCycleNode> nodes)
    {
        var sb = new StringBuilder();

        sb.AppendLine("digraph G {");
        sb.AppendLine("\tnode [shape=rectangle];");
        sb.AppendLine("\tStart [shape=oval];");
        sb.AppendLine("");

        foreach (var node in nodes)
        {
            var nodeAttributes = new List<KeyValuePair<string, string>>();
            nodeAttributes.Add(new KeyValuePair<string, string>("tooltip", $"\"Distance from root={node.Distance}\""));

            if (node.Stub)
                nodeAttributes.Add(new KeyValuePair<string, string>("color", "grey"));

            sb.Append($"\t\"{node.Name}\"");

            if (nodeAttributes.Count > 0)
                sb.AppendLine($" [{string.Join(" ", nodeAttributes.Select(item => $"{item.Key}={item.Value}"))}]");
            else
                sb.AppendLine();

            foreach (var link in node.OutgoingLinks)
            {
                nodeAttributes.Clear();

                if (link.StateCycleEdgeType == StateCycleEdgeType.Link)
                {
                    nodeAttributes.Add(new KeyValuePair<string, string>("color", "grey"));
                    nodeAttributes.Add(new KeyValuePair<string, string>("style", "dashed"));
                }

                sb.Append($"\t\"{node.Name}\" -> \"{link.Neighbor!.Name}\"");

                if (nodeAttributes.Count > 0)
                    sb.AppendLine($" [{string.Join(" ", nodeAttributes.Select(item => $"{item.Key}={item.Value}"))}]");
                else
                    sb.AppendLine();
            }
        }

        sb.Append("}");

        return sb.ToString();
    }

    private enum StateCycleEdgeType
    {
        Transition = 1,
        Link = 2,
    }

    private class StateCycleNode(string name)
    {
        public string Name { get; } = name;
        public bool Stub { get; set; } = false;
        public HashSet<StateCycleLink> OutgoingLinks { get; } = new();
        public HashSet<StateCycleLink> IncomingLinks { get; } = new();
        public int Distance { get; set; } = -1;
    }

    private record StateCycleLink(StateCycleNode Node, StateCycleNode Neighbor, StateCycleEdgeType StateCycleEdgeType);
}