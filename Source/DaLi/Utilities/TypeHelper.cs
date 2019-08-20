/*
 *  DaLi is a small libray to ease the use of MsSQL / MySQL databases from .Net.
 *  Copyright (C) 2009 Steffen Skov

 *  This file is part of DaLi.

 *  DaLi is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  DaLi is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.

 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DaLi.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Data;
using Ckode.DaLi.Entities;
using System.Reflection;
using System.Reflection.Emit;

namespace Ckode.DaLi.Utilities
{
	internal static class TypeHelper
	{
		private static Dictionary<Type, Func<Entity>> entityConstructors = new Dictionary<Type, Func<Entity>>();
		private static object entityConstructorLock = new object();
		private static readonly Type entityCtorDelegateType = typeof(Func<Entity>);
		
		private static Dictionary<Type, Func<Entity, BaseColumn>> columnConstructors = new Dictionary<Type, Func<Entity, BaseColumn>>();
		private static object columnConstructorLock = new object();
		private static readonly Type[] columnConstructorTypeArgs = new Type[] {typeof(Entity) };
		private static readonly Type columnCtorDelegateType = typeof(Func<Entity, BaseColumn>);
		
		private static readonly Type stringType = typeof(string);
		private static readonly Type timeType = typeof(TimeSpan);
		private static readonly Type nullAbleType = typeof(int?).GetGenericTypeDefinition();

		internal static DbType GetDbType(Type type)
		{
			if (type == stringType)
			{
				if (Configuration.UseUnicodeStrings)
					return DbType.String;
				else
					return DbType.AnsiString;
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == nullAbleType)
				type = type.GetGenericArguments()[0];
			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			if (type == timeType)
				return DbType.Time;
			
			string name = type.Name;
			if (Enum.IsDefined(typeof(DbType), name))
				return (DbType)Enum.Parse(typeof(DbType), name);
			else
				throw new ArgumentException("Could not infer DbType from the given argument's type.", "type");
		}

		internal static bool IsType<T>(object value)
		{
			Type expectedType = typeof(T);
			return IsType(value, expectedType);
		}

		internal static bool IsType(object value, Type expectedType)
		{
			if (value == null)
				return true;
			Type valueType = value.GetType();
			return (valueType == expectedType || (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == nullAbleType && expectedType.GetGenericArguments()[0] == valueType));
		}

		internal static bool IsNullable(Type valueType)
		{
			return (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == nullAbleType);
		}

		internal static TValue ConvertToType<TValue>(object value)
		{
			TValue result;
			if (typeof(TValue).IsEnum)
				result = (TValue)Enum.ToObject(typeof(TValue), value);
			else
			{
				if (IsType<TValue>(value))
					result = (TValue)value;
				else
					result = (TValue)Convert.ChangeType(value, typeof(TValue));
			}
			return result;
		}

		internal static T CreateEntity<T>() where T : Entity
		{
			Func<Entity> ctorDelegate = null;
			Type type = typeof(T);
			if (!entityConstructors.TryGetValue(type, out ctorDelegate))
			{
				lock (entityConstructorLock)
				{
					if (!entityConstructors.TryGetValue(type, out ctorDelegate))
					{
						ConstructorInfo ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);
						ctorDelegate = (Func<Entity>)CreateDelegate(ctorInfo, entityCtorDelegateType);
						entityConstructors.Add(type, ctorDelegate);
					}
				}
			}
			return (T)ctorDelegate();
		}
		
		internal static BaseColumn CreateColumn(Type type, Entity initialParent)
		{
			Func<Entity, BaseColumn> ctorDelegate = null;
			if (!columnConstructors.TryGetValue(type, out ctorDelegate))
			{
				lock (columnConstructorLock)
				{
					if (!columnConstructors.TryGetValue(type, out ctorDelegate))
					{
						ConstructorInfo ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, columnConstructorTypeArgs, null);
						ctorDelegate = (Func<Entity, BaseColumn>)CreateDelegate(ctorInfo, columnCtorDelegateType);
						columnConstructors.Add(type, ctorDelegate);
					}
				}
			}
			return ctorDelegate(initialParent);
		}

		private static Delegate CreateDelegate(ConstructorInfo constructor, Type delegateType)
		{
			if (constructor == null)
			{
				throw new ArgumentNullException("constructor");
			}

			// Validate the delegate return type
			MethodInfo delMethod = delegateType.GetMethod("Invoke");
			if (delMethod.ReturnType != constructor.DeclaringType && !constructor.DeclaringType.IsSubclassOf(delMethod.ReturnType))
			{
				throw new InvalidOperationException("The return type of the delegate must match the constructors declaring type");
			}
			ParameterInfo[] constructorParam = constructor.GetParameters();

			// Validate the signatures
			ParameterInfo[] delParams = delMethod.GetParameters();
			if (delParams.Length != constructorParam.Length)
			{
				throw new InvalidOperationException("The delegate signature does not match that of the constructor");
			}
			for (int i = 0; i < delParams.Length; i++)
			{
				if (delParams[i].ParameterType != constructorParam[i].ParameterType ||  // Probably other things we should check ??
					delParams[i].IsOut)
				{
					throw new InvalidOperationException("The delegate signature does not match that of the constructor");
				}
			}
			// Create the dynamic method
			DynamicMethod method =
				new DynamicMethod(
					string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
					constructor.DeclaringType,
					Array.ConvertAll<ParameterInfo, Type>(constructorParam, p => p.ParameterType),
					true
					);

			// Create the il
			ILGenerator gen = method.GetILGenerator();
			for (int i = 0; i < constructorParam.Length; i++)
			{
				if (i < 4)
				{
					switch (i)
					{
						case 0:
						gen.Emit(OpCodes.Ldarg_0);
						break;
						case 1:
						gen.Emit(OpCodes.Ldarg_1);
						break;
						case 2:
						gen.Emit(OpCodes.Ldarg_2);
						break;
						case 3:
						gen.Emit(OpCodes.Ldarg_3);
						break;
					}
				}
				else
				{
					gen.Emit(OpCodes.Ldarg_S, i);
				}
			}
			gen.Emit(OpCodes.Newobj, constructor);
			gen.Emit(OpCodes.Ret);

			// Return the delegate :)
			return method.CreateDelegate(delegateType);

		}
	}
}
