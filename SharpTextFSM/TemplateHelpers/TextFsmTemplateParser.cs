/*
   SharpTextFSM
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

using Bitvantage.SharpTextFSM.Attributes;
using Bitvantage.SharpTextFSM.Definitions;
using Bitvantage.SharpTextFSM.TypeConverters;
using Bitvantage.SharpTextFSM.TypeConverters.EnumHelpers;

namespace Bitvantage.SharpTextFSM.TemplateHelpers
{
    internal class TextFsmTemplateParser
    {
        public enum StateState
        {
            [EnumAlias("~Global")]
            Global,

            Start,

            [EnumAlias("Value_Section")]
            ValueSection,

            [EnumAlias("State_Section")]
            StateSection,

            [EnumAlias("Rule_State_Filter_Section")]
            StateFilter,

            [EnumAlias("Rule_Section")]
            RuleSection,

            [EnumAlias("EOF")]
            EndOfFile,
        }

        [TemplateVariable(Name = "STATE")]
        public StateState State { get; set; }

        [TemplateVariable(Name = "TEXT")]
        public string Text { get; set; }

        [TemplateVariable(Name = "LINE")]
        public long Line { get; set; }

        [TemplateVariable(Name = "COMMENTS")]
        public string[] Comment { get; set; }

        [TemplateVariable(Name = "RULE_LINE_ACTION", ThrowOnConversionFailure = false)]
        public LineAction? LineAction { get; set; }

        [TemplateVariable(Name = "RULE_PATTERN")]
        public string Pattern { get; set; }

        [TemplateVariable(Name = "RULE_RECORD_ACTION", ThrowOnConversionFailure = false)]
        public RecordAction? RecordAction { get; set; }

        [TemplateVariable(Name = "RULE_STATE_ERROR", Converter = typeof(AnyValueAsTrue))]
        public bool ErrorState { get; set; }

        [TemplateVariable(Name = "RULE_STATE_ERROR_MESSAGE")]
        public string ErrorMessage { get; set; }

        [TemplateVariable(Name = "RULE_STATE_ERROR_STRING")]
        public string ErrorString { get; set; }

        [TemplateVariable(Name = "RULE_STATE_FILTER")]
        public List<string> StateFilters { get; set; }

        [TemplateVariable(Name = "RULE_STATE_FILTER_INVERSION", Converter = typeof(AnyValueAsTrue))]
        public bool StateFilterInversion { get; set; }

        [TemplateVariable(Name = "RULE_STATE_NAME")]
        public string StateName { get; set; }

        [TemplateVariable(Name = "VALUE_FLAGS", ThrowOnConversionFailure = false)]
        public List<Option> ValueFlags { get; set; }

        [TemplateVariable(Name = "VALUE_NAME")]
        public string ValueName { get; set; }

        [TemplateVariable(Name = "VALUE_PATTERN")]
        public string ValuePattern { get; set; }

        [TemplateVariable(Name = "STATE_NAME")]
        public string StartName2 { get; set; }

        public static Template Instance { get; } = CreateTemplate();
        private static Template CreateTemplate()
        {
            var template = new Template(new TemplateDefinition(
                new List<ValueDefinition>
                {
                    new("STATE", Option.Metadata, "State"),
                    new("TEXT", Option.Metadata, "Text"),
                    new("LINE", Option.Metadata, "Line"),
                    new("COMMENTS", Option.List, ".*"),
                    new("RULE_LINE_ACTION", Option.None, "(Next|Continue)"),
                    new("RULE_PATTERN", Option.None, "\\^.*?"),
                    new("RULE_RECORD_ACTION", Option.None, "(NoRecord|Record|Clear|Clearall)"),
                    new("RULE_STATE_ERROR", Option.None, "Error"),
                    new("RULE_STATE_ERROR_MESSAGE", Option.None, "\\w+"),
                    new("RULE_STATE_ERROR_STRING", Option.None, ".*"),
                    new("RULE_STATE_FILTER", Option.List, "\\w+"),
                    new("RULE_STATE_FILTER_INVERSION", Option.None, "\\^"),
                    new("RULE_STATE_NAME", Option.None, "\\w+"),
                    new("STATE_NAME", Option.Filldown, "([a-zA-Z0-9]+|~Global)"),
                    new("VALUE_FLAGS", Option.List, "(Filldown|Fillup|Key|List|Metadata|Required|Regex)"),
                    new("VALUE_NAME", Option.None, "\\S+"),
                    new("VALUE_PATTERN", Option.None, ".*"),
                },
                new List<StateDefinition>
                {
                    new("~Global", new List<RuleDefinition>
                    {
                        new("^\\s*#${COMMENTS}"), // skip comment lines
                        new("^Value ") { LineAction = TemplateHelpers.LineAction.Continue, StateFilter = new List<string> { "Start" }, MatchAction = new ChangeStateActionDefinition("Value_Section") },
                        new("^([a-zA-Z0-9]+|~Global)$$") { LineAction = TemplateHelpers.LineAction.Continue, StateFilter = new List<string> { "Value_Section", "Rule_Section" }, MatchAction = new ChangeStateActionDefinition("State_Section") },
                        // TODO: replace this with something more elegant
                        new("^( {1,2}|\\t)\\[${RULE_STATE_FILTER_INVERSION}?((${RULE_STATE_FILTER}(,(?!\\])|)))*\\]$") { StateFilter = new List<string> { "Rule_Section" }, MatchAction = new ChangeStateActionDefinition("Rule_State_Filter_Section") },
                        new("^( {1,2}|\\t)\\^") { LineAction = TemplateHelpers.LineAction.Continue, StateFilter = new List<string> { "Rule_State_Filter_Section" }, MatchAction = new ChangeStateActionDefinition("Rule_Section") },
                    }),
                    new("Start", new List<RuleDefinition>
                    {
                        new("^.*") { MatchAction = new ErrorActionDefinition() },
                    }),
                    new("Value_Section", new List<RuleDefinition>
                    {
                        new("^Value (${VALUE_FLAGS}((?= )|(?:,(?! ))))* ?${VALUE_NAME} \\(${VALUE_PATTERN}\\)\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },
                        new("^\\s*$$") { MatchAction = new ChangeStateActionDefinition("Value_Section_Empty_Lines") },
                        new("^.*") { MatchAction = new ErrorActionDefinition() },
                    }),
                    new("Value_Section_Empty_Lines", new List<RuleDefinition>
                    {
                        new("^\\s*$$"),
                        new("^.*") { MatchAction = new ChangeStateActionDefinition("State_Section"), LineAction = TemplateHelpers.LineAction.Continue},
                    }),
                    new("State_Section", new List<RuleDefinition>
                    {
                        new("^${STATE_NAME}$$") { MatchAction = new ChangeStateActionDefinition("Rule_Section"), RecordAction = TemplateHelpers.RecordAction.Record},
                        new("^.*") { MatchAction = new ErrorActionDefinition() },
                    }),
                    new("Rule_State_Filter_Section", new List<RuleDefinition>
                    {
                        //new("( {1,2}|\\t)\\[${RULE_STATE_FILTER_INVERSION}?((${RULE_STATE_FILTER}(,(?!\\])|)))*\\]$") { MatchAction = new ChangeStateActionNew("Rule_Section"), StateFilter = new List<string> {"State_Section","Rule_Section"}},
                        new("^.*") { MatchAction = new ErrorActionDefinition() },
                    }),
                    new("Rule_Section", new List<RuleDefinition>
                    {
                        // ^rule -> Error ErrorMessage
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_STATE_ERROR}\\s+${RULE_STATE_ERROR_MESSAGE}\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> Error ErrorMessage
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_STATE_ERROR}\\s+\"${RULE_STATE_ERROR_STRING}\"\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> Error
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_STATE_ERROR}\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> Next.Record NewState or Record.Next
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_LINE_ACTION}\\.${RULE_RECORD_ACTION}(\\s+${RULE_STATE_NAME})?\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> Record NewState or Record
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_RECORD_ACTION}(\\s+${RULE_STATE_NAME})?\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> Next NewState or Next
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_LINE_ACTION}(\\s+${RULE_STATE_NAME})?\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> NewState
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s+${RULE_STATE_NAME}\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule ->
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        // ^rule -> invalid text
                        new("^( {1,2}|\\t)${RULE_PATTERN} ->\\s*.+$$") { MatchAction = new ErrorActionDefinition() },

                        // ^rule
                        new("^( {1,2}|\\t)${RULE_PATTERN}\\s*$$") { RecordAction = TemplateHelpers.RecordAction.Record },

                        new("^\\s*$$") { MatchAction = new ChangeStateActionDefinition("Rule_Section_Empty_Lines")},
                        new("^.*") { MatchAction = new ErrorActionDefinition() },
                    }),
                    new("Rule_Section_Empty_Lines", new List<RuleDefinition>
                    {
                        new("^\\s*$$"),
                        new("^.*") { MatchAction = new ChangeStateActionDefinition("State_Section"), LineAction = TemplateHelpers.LineAction.Continue},
                    }),
                }), new TemplateOptions { UnmatchedListHandling = UnmatchedHandling.Null, UnmatchedValueHandling = UnmatchedHandling.Null });

            return template;
        }
    }
}