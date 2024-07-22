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
    /// Converts values to the target type by using the static 'Parse' method in the target type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericParseConverter<T> : ValueConverter<T>
    {
        private static readonly Func<string, ParseResult>? Converter = null;

        static GenericParseConverter()
        {
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);
            underlyingType ??= type;

            // Get the MethodInfo for the Parse method
            var parseMethod = underlyingType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, new[] { typeof(string) });
            // check if there is a Parse method
            // if there is no TryParse method, then the type cannot be converted
            if (parseMethod == null)
                return;

            // verify the return type is of the correct type
            if (parseMethod.ReturnType != underlyingType)
                return;

            // Create parameters for the method
            var inputParam = Expression.Parameter(typeof(string), "input");
            var resultParam = Expression.Variable(underlyingType, "result");

            var parseResultConstructor = typeof(ParseResult).GetConstructor(new[] { type, typeof(bool) });

            // generate an expression tree that takes a string and converts it to the target type by calling TryParse
            // the generated expression tree is equivalent to the following code 
            /*
               input =>
               {
                   try
                   {
                       int result;
                       result = int.Parse(input);

                       return new GenericParseConverter<int>.ParseResult((int)result, true);
                   }
                   catch
                   {
                       return new GenericParseConverter<int>.ParseResult(default(int), false);
                   }
               }
            */

            var parseBlock = Expression.TryCatch(
                Expression.Block(new[] { resultParam },
                    Expression.Assign(
                        resultParam,
                        Expression.Call(
                            null,
                            parseMethod,
                            inputParam
                        )
                    ),
                    Expression.New(parseResultConstructor, Expression.Convert(resultParam, type), Expression.Constant(true))
                ),
                Expression.Catch(typeof(Exception),
                    Expression.New(parseResultConstructor, Expression.Default(type), Expression.Constant(false))
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