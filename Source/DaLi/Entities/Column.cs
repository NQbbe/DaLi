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
using Ckode.DaLi.Utilities;

namespace Ckode.DaLi.Entities
{
	public sealed class Column<T> : BaseColumn
	{
		internal static readonly Type ColumnGenericType = typeof(Column<>);
		private T value;
		private Type valueType;

		internal Column(Entity initialParent)
			: base(initialParent)
		{
			this.value = default(T);
			this.valueType = typeof(T);
		}

		internal Column(T value)
			: base(null)
		{
			this.value = value;
			this.valueType = typeof(T);
		}

		internal Column()
			: base(null)
		{
			this.value = default(T);
			this.valueType = typeof(T);
		}

		public T Value
		{
			get
			{
				return this.value;
			}
		}

		#region Implicit converters

		public static implicit operator Column<T>(T value)
		{
			if (value == null)
				return new Column<T>();
			else
				return new Column<T>(value);
		}

		public static implicit operator T(Column<T> value)
		{
			if (value == null)
				return default(T);
			return value.Value;
		}

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			BaseColumn objCol = obj as BaseColumn;
			if (objCol != null)
				return this.value.Equals(objCol.GetValue());
			return this.value.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		public override string ToString()
		{
			return this.value.ToString();
		}

		internal override void SetValueWithoutChangingParent(object value)
		{
			if (this.valueType.IsEnum)
				this.value = (T)Enum.ToObject(this.valueType, value);
			else
			{
				if (TypeHelper.IsType<T>(value))
					this.value = (T)value;
				else
					this.value = (T)Convert.ChangeType(value, this.valueType);
			}
		}

		public override DbType DbType
		{
			get
			{
				return TypeHelper.GetDbType(this.valueType);
			}
		}

		internal override object GetValue()
		{
			return this.value;
		}

		#endregion
	}
}
