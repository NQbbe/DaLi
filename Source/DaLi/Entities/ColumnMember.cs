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
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using Ckode.DaLi.Utilities;

namespace Ckode.DaLi.Entities
{
	internal class ColumnMember
	{
		private Type columnFieldType;
		private MemberAccessor accessor;
		internal ColumnMember(MemberInfo memberInfo)
		{
			FieldInfo fi = memberInfo as FieldInfo;
			if (fi != null)
			{
				accessor = new MemberAccessor(fi);
				columnFieldType = fi.FieldType;
			}
			else
			{
				PropertyInfo pi = memberInfo as PropertyInfo;
				accessor = new MemberAccessor(pi);
				columnFieldType = pi.PropertyType;
			}
		}

		internal BaseColumn GetValue(Entity entity)
		{
			return accessor.getter(entity) as BaseColumn;
		}

		internal void SetValueWithoutChangingParent(Entity entity, object value)
		{
			BaseColumn col = GetValue(entity);
			if (value == DBNull.Value)
				value = null;
			col.SetValueWithoutChangingParent(value);
		}

		internal void InstantiateField(Entity initialParent)
		{
			SetValue(initialParent, TypeHelper.CreateColumn(columnFieldType, initialParent));
		}

		internal void ResetChanged(Entity initialParent)
		{
			BaseColumn column = GetValue(initialParent);
			if (column != null && column.IsChanged(initialParent))
			{
				if (column.HasNoInitialParent())
					column.SetInitialParent(initialParent);
				else
				{
					BaseColumn newColumn = TypeHelper.CreateColumn(column.GetType(), initialParent);
					newColumn.SetValueWithoutChangingParent(column.GetValue());
					SetValue(initialParent, newColumn);
				}
			}
		}

		private void SetValue(Entity entity, BaseColumn column)
		{
			accessor.setter(entity, column);
		}

		internal string Name
		{
			get
			{
				return accessor.Name;
			}
		}
		
		internal DbType ColumnDbType
		{
			get
			{
				Type type = columnFieldType.GetGenericArguments()[0];
				return TypeHelper.GetDbType(type);
			}
		}
	}
}
