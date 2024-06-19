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

using System.Runtime.Serialization;
using Bitvantage.SharpTextFSM.TemplateHelpers;

namespace Bitvantage.SharpTextFSM.Exceptions
{
    [Serializable]
    public class TemplateErrorException : Exception
    {
        public ErrorAction Error { get; }
        public string State { get; }
        public string Rule { get; }
        public string Text { get; }
        public long Line { get; }

        public TemplateErrorException(ErrorAction error, string state, string rule, string text, long line) : base(GenerateMessage(error,state,rule,text,line))
        {
            Error = error;

            State = state;
            Rule = rule;
            Text = text;
            Line = line;
        }

        protected TemplateErrorException(SerializationInfo info, StreamingContext context)
        {
            Error = (ErrorAction)info.GetValue(nameof(Error), typeof(ErrorAction));
            State = (string)info.GetValue(nameof(State), typeof(string));
            Rule = (string)info.GetValue(nameof(Rule), typeof(string));
            Text = (string)info.GetValue(nameof(Text), typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Error), Error, typeof(string));
            info.AddValue(nameof(State), State, typeof(string));
            info.AddValue(nameof(Rule), Rule, typeof(string));
            info.AddValue(nameof(Text), Text, typeof(string));
        }

        private static string GenerateMessage(ErrorAction error, string state, string rule, string text, long line)
        {
            if (string.IsNullOrEmpty(error.Message))
                return $"State = '{state}', Rule = '{rule}', Text = '{text}', Line = {line:N0}";
            
            return $"{error.Message}: State = '{state}', Rule = '{rule}', Text = '{text}', Line = {line:N0}";
        }
    }
}
