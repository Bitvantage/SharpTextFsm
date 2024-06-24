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

using System.Text.RegularExpressions;

namespace Bitvantage.SharpTextFSM.TemplateHelpers;

[Flags]
public enum Option
{
    None = 0,
    Filldown = 1 << 1,
    Key = 1 << 2,
    Required = 1 << 3,
    List = 1 << 4,
    Fillup = 1 << 5,
    Metadata = 1 << 6,
    Regex = 1 << 7,
}

internal enum Metadata
{
    Line = 1,
    Text = 2,
    State = 3,
    RuleIndex = 5
}

public record ValueDescriptor
{
    public ValueDescriptor(Template? template, string name, Option options, Regex regex)
    {
        Template = template;
        Name = name;
        Options = options;

        Regex = regex;

        // if the metadata option is specified, parse the pattern as the metadata type and cache it
        if (options.HasFlag(Option.Metadata))
        {
            if (!Enum.TryParse(regex.ToString(), out Metadata metadata))
                throw new ArgumentException($"The value '{regex}' could not be parsed into type {typeof(Metadata)}");

            Metadata = metadata;
        }
    }

    internal Metadata? Metadata { get; }
    public string Name { get; init; }
    public Option Options { get; init; }
    public Regex Regex { get; init; }
    public Template? Template { get; }

    public override string ToString()
    {
        if (Options == Option.None)
            return $"Value {Name} ({Regex})";

        return $"Value {Options.ToString().Replace(" ", "")} {Name} ({Regex})";
    }
}