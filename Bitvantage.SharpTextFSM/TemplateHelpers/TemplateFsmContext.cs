/*
   Bitvantage.SharpTextFSM
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

namespace Bitvantage.SharpTextFSM.TemplateHelpers;

internal class TemplateFsmContext
{
    internal TemplateFsmContext(TextFsmState state, TemplateState currentState)
    {
        State = state;
        CurrentState = currentState;
    }

    internal TemplateFsmContext(long line, string? text, TextFsmState state, TemplateState currentState, Rule rule, int ruleIndex)
    {
        Line = line;
        Text = text;
        State = state;
        CurrentState = currentState;
        Rule = rule;
        RuleIndex = ruleIndex;
    }

    public TemplateState CurrentState { get; internal set; }
    public long Line { get; internal set; }
    public Rule Rule { get; internal set; }
    public int RuleIndex { get; internal set; }
    public TextFsmState State { get; internal set; }
    public string? Text { get; internal set; }

    internal TemplateFsmContext Clone()
    {
        return new TemplateFsmContext(Line, Text, State, CurrentState, Rule, RuleIndex);
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
                return CurrentState.Name;

            case Metadata.RuleIndex:
                return RuleIndex.ToString();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}