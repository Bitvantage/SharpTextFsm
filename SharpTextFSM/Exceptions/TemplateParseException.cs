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

namespace Bitvantage.SharpTextFSM.Exceptions;

[Serializable]
public class TemplateParseException : Exception
{
    public ParseError ErrorCode { get; }

    public TemplateParseException(string message, ParseError errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public TemplateParseException(string message, ParseError errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    protected TemplateParseException(SerializationInfo info, StreamingContext context)
    {
        ErrorCode = (ParseError)info.GetValue(nameof(ErrorCode), typeof(ParseError))!;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(ErrorCode), ErrorCode, typeof(ParseError));
    }
}

public enum ParseError
{
    DuplicateState,
    DuplicateStateFilterState,
    InvalidAction,
    InvalidRegularExpression,
    InvalidStateName,
    InvalidValueName,
    InvalidValueRegularExpression,
    NoStartState,
    UndefinedState,
    NoStateDefinitions,
    NoValueDefinitions,
    ReservedValueKeyword,
    StateLoop,
    StateMustBeEmpty,
    StateRestricted,
    SyntaxError,
    UndeclaredValue,
    UndefinedStateInStateFilter,
    UnsupportedAction,
    StateFilterInRegularRule,
    DuplicateValueName
}