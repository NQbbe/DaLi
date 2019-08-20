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
using System.Data;

namespace Ckode.DaLi.Entities
{
	public abstract class BaseColumn
	{
		protected BaseColumn(Entity initialParent)
		{
			SetInitialParent(initialParent);
		}
		internal Entity InitialParent { get; private set; }

		internal bool HasNoInitialParent()
		{
			return InitialParent == null;
		}

		internal void SetInitialParent(Entity initialParent)
		{
			this.InitialParent = initialParent;
		}

		internal bool IsChanged(Entity initialParent)
		{
			return this.InitialParent != initialParent;
		}

		internal abstract void SetValueWithoutChangingParent(object value);
		public abstract DbType DbType { get; }
		internal abstract object GetValue();
	}
}