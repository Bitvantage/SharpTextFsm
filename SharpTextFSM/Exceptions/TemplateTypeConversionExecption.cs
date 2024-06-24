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

using System.Reflection;
using System.Runtime.Serialization;

namespace Bitvantage.SharpTextFSM.Exceptions
{
    [Serializable]
    public class TemplateTypeConversionException : Exception
    {
        public Type Converter { get; }
        public MemberInfo MemberInfo { get; }
        public string Value { get; }

        public TemplateTypeConversionException(Type converter, MemberInfo memberInfo, string value) : base(GenerateMessage(converter, memberInfo, value))
        {
            Converter = converter;
            MemberInfo = memberInfo;
            Value = value;
        }

        private static string GenerateMessage(Type converter, MemberInfo memberInfo, string value)
        {
            return $"The converter {converter.FullName} was unable to convert the value '{value}' for {memberInfo.DeclaringType.FullName}.{memberInfo.Name}";
        }
        protected TemplateTypeConversionException(SerializationInfo info, StreamingContext context)
        {
            Converter = (Type)info.GetValue(nameof(Converter), typeof(Type));
            Value = (string)info.GetValue(nameof(Value), typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Converter), Converter, typeof(Type));
            info.AddValue(nameof(Value), Value, typeof(string));
        }
    }
}