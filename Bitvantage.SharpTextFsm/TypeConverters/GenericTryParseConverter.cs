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

using System.Linq.Expressions;
using System.Reflection;

namespace Bitvantage.SharpTextFsm.TypeConverters
{
    /// <summary>
    /// Converts values to the target type by using the static 'TryParse' method in the target type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericTryParseConverter<T> : ValueConverter<T>
    {
        private static readonly Func<string, ParseResult>? Converter = null;

        static GenericTryParseConverter()
        {
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);
            underlyingType ??= type;

            // Get the MethodInfo for the TryParse method
            var parseMethod = underlyingType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, new[] { typeof(string), underlyingType.MakeByRefType() });

            // check if there is a TryParse method
            // if there is no TryParse method, then the type cannot be converted
            if (parseMethod == null)
                return;

            // verify the return type is a bool
            if (parseMethod.ReturnType != typeof(bool))
                return;

            // Create parameters for the method
            var inputParam = Expression.Parameter(typeof(string), "input");
            var successParam = Expression.Parameter(typeof(bool), "success");
            var resultParam = Expression.Variable(underlyingType, "result");

            var parseResultConstructor = typeof(ParseResult).GetConstructor(new[] { type, typeof(bool) });

            // generate an expression tree that takes a string and converts it to the target type by calling TryParse
            // the generated expression tree is equivalent to the following code 
            /*
            input =>
            {
                long result;
                var success = long.TryParse(input, out result);

                return new GenericParseOrDefaultConverter<long>.ParseResult(success ? (long)result : default(long), success);
            }
            */

            var parseBlock = Expression.Block(
                new[] { successParam, resultParam },

                Expression.Assign(
                    successParam,
                    Expression.Call(
                        null,
                        parseMethod,
                        inputParam,
                        resultParam
                    )
                ),
                Expression.New(
                    parseResultConstructor,
                    Expression.Condition(
                        Expression.IsTrue(successParam),
                        Expression.Convert(resultParam, type),
                        Expression.Default(type)
                    ),
                    successParam
                )
            );

            // Create a lambda expression
            var lambda = Expression.Lambda<Func<string, ParseResult>>(
                parseBlock,
                inputParam
            );

            // compile the function
            var func = lambda.Compile();

            // cache the conversion function
            Converter = func;
        }

        public static bool CanConvert()
        {
            return Converter != null;
        }

        public override bool TryConvert(string value, out T convertedValue)
        {
            var conversionResults = Converter(value);

            convertedValue = conversionResults.Result;
            return conversionResults.Success;

        }

        record ParseResult(T Result, bool Success);
    }
}