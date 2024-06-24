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

using System.Linq.Expressions;
using System.Reflection;

namespace Bitvantage.SharpTextFSM.TemplateHelpers;

internal static class FastInvoker
{
    public static Action<T, object> CreateUntypedSetterAction<T>(FieldInfo fieldInfo)
    {
        return CreateUntypedSetterAction<T>(fieldInfo, fieldInfo.FieldType);
    }

    public static Action<T, object> CreateUntypedSetterAction<T>(PropertyInfo propertyInfo)
    {
        return CreateUntypedSetterAction<T>(propertyInfo, propertyInfo.PropertyType);
    }

    public static Action<T, object> CreateUntypedSetterAction<T>(MemberInfo memberInfo, Type type)
    {
        var targetType = memberInfo.DeclaringType;

        var instanceParameter = Expression.Parameter(targetType, "instance");
        var valueParameter = Expression.Parameter(typeof(object), "value");

        var expression = Expression.Assign(
            Expression.MakeMemberAccess(instanceParameter, memberInfo),
            Expression.Convert(valueParameter, type)
        );

        var lambda = Expression.Lambda<Action<T, object>>(expression, instanceParameter, valueParameter);
        var action = lambda.Compile();

        return action;
    }
}