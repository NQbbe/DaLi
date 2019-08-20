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
namespace Ckode.DaLi.Entities
{
	public abstract class View : DbEntity
	{
		#region Constructors
		protected View()
			: base()
		{
		}

		protected View(uint id)
			: base(id)
		{
		}
		#endregion

		#region Fill
		public override void Fill(IDataReader dr)
		{
			base.Fill(dr);
			DataTable dt = dr.GetSchemaTable();
			if (dt.Columns.Contains("ID"))
				ID = Convert.ToUInt32(dr["ID"]);
			else
				ID = 0;
		}

		public override void Fill(DataRow dr)
		{
			base.Fill(dr);
			DataTable dt = dr.Table;
			if (dt.Columns.Contains("ID"))
				ID = Convert.ToUInt32(dr["ID"]);
			else
				ID = 0;
		}
		#endregion
	}
}