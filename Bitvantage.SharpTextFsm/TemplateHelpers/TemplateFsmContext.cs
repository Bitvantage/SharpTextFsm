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

namespace Bitvantage.SharpTextFsm.TemplateHelpers;

internal class TemplateFsmContext
{
    public int CurrentIndex { get; internal set; }
    public TextFsmState FsmState { get; internal set; }
    public long Line { get; internal set; }
    public Rule Rule { get; internal set; }

    public int RuleIndex
    {
        get
        {
            // if the state that contains the rule is ~Global or there is no ~Global state then the rule index does not need to be adjusted
            if (RuleState.Name == "~Global" || TemplateState.Template.States.GlobalState == null)
                return CurrentIndex;

            // if there is a ~Global state and the state of the rule is not contained within it
            // then the index needs to be offset by the number of rules in ~Global
            return CurrentIndex - TemplateState.Template.States.GlobalState.Rules.Length;
        }
    }

    public TemplateState RuleState => Rule.State;

    public TemplateState TemplateState { get; internal set; }
    public string? Text { get; internal set; }

    internal TemplateFsmContext(TextFsmState fsmState, TemplateState templateState)
    {
        FsmState = fsmState;
        TemplateState = templateState;
    }

    internal TemplateFsmContext(long line, string? text, TextFsmState fsmState, TemplateState templateState, Rule rule, int currentIndex)
    {
        Line = line;
        Text = text;
        FsmState = fsmState;
        TemplateState = templateState;
        Rule = rule;
        CurrentIndex = currentIndex;
    }

    internal TemplateFsmContext Clone()
    {
        return new TemplateFsmContext(Line, Text, FsmState, TemplateState, Rule, CurrentIndex);
    }

    internal string? Get(Metadata type)
    {
        switch (type)
        {
            case Metadata.Line:
                return Line.ToString();

            case Metadata.Text:
                return Text;

            case Metadata.State:
                return TemplateState.Name;

            case Metadata.RuleIndex:
                return RuleIndex.ToString();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}