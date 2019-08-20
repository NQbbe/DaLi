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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ckode.DaLi.Entities
{
	public abstract class Entity
	{
		#region Static

		private static object typeLock = new object();
		private static Dictionary<Type, Dictionary<string, ColumnMember>> typeDefinitions = new Dictionary<Type, Dictionary<string, ColumnMember>>();

		private static void InitializeType(Entity entity)
		{
			Type type = entity.GetType();
			Dictionary<string, ColumnMember> columns = new Dictionary<string, ColumnMember>();
			foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.NonPublic))
			{
				if (!fi.IsDefined(typeof(CompilerGeneratedAttribute), false) && IsColumnType(fi.FieldType))
				{
					columns.Add(fi.Name, new ColumnMember(fi));
				}
			}
			foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.NonPublic))
			{
				if (IsColumnType(pi.PropertyType))
				{
					columns.Add(pi.Name, new ColumnMember(pi));
				}
			}

			typeDefinitions[type] = columns;
		}

		private static bool IsColumnType(Type type)
		{
			if (!type.IsGenericType)
				return false;

			Type genericTypeDefination = type.GetGenericTypeDefinition();
			return genericTypeDefination == Column<object>.ColumnGenericType
			|| genericTypeDefination == CompressedColumn<object>.CompressedColumnGenericType;
		}

		#endregion

		internal Dictionary<string, ColumnMember> columns = null;

		public bool IsLoaded { get; private set; }

		#region Constructors/Initializer

		protected Entity()
		{
			Type type = this.GetType();
			if (!typeDefinitions.TryGetValue(type, out columns))
			{
				lock (typeLock)
				{
					if (!typeDefinitions.TryGetValue(type, out columns))
					{
						InitializeType(this);
						columns = typeDefinitions[type];
					}
				}
			}
			InitializeFields();
		}

		private void InitializeFields()
		{
			foreach (ColumnMember fp in columns.Values)
			{
				fp.InstantiateField(this);
			}
		}

		#endregion

		#region Fill

		public virtual void Fill(IDataReader dr)
		{
			InitializeFields(); // To ensure it doesn't load to a column instance some other table references
			foreach (KeyValuePair<string, ColumnMember> pair in columns)
			{
				pair.Value.SetValueWithoutChangingParent(this, dr[pair.Key]);
			}
			IsLoaded = true;
			PostFill();
		}

		public virtual void Fill(DataRow dr)
		{
			InitializeFields(); // To ensure it doesn't load to a column instance some other table references
			foreach (KeyValuePair<string, ColumnMember> pair in columns)
			{
				pair.Value.SetValueWithoutChangingParent(this, dr[pair.Key]);
			}
			IsLoaded = true;
			PostFill();
		}

		#endregion

		protected virtual void PostFill()
		{
			// Empty by design
		}
	}
}
