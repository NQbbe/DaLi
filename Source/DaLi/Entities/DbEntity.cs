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
using Ckode.DaLi.Factories;
using Ckode.DaLi.Logging;

namespace Ckode.DaLi.Entities
{
	public abstract class DbEntity : Entity
	{
		public uint ID { get; internal set; }
		private string _className;
		protected virtual string EntityName
		{
			get
			{
				if (string.IsNullOrEmpty(_className))
					_className = GetType().Name.ToLower();
				return _className;
			}
		}
		protected abstract Factory Factory { get; }

		protected virtual LoggingLevel LogLevel { get { return Configuration.LogLevel; } }

		public DbEntity(uint id)
			: base()
		{
			this.ID = id;
		}
		public DbEntity()
			: base()
		{
		}

		#region Load
		/// <summary>
		/// Attempts to load with the given ID. Note: If the load is not succesful, the ID of this instance will not be altered!
		/// </summary>
		public virtual bool Load(uint id)
		{
			var oldID = this.ID;
			this.ID = id;
			if (Load())
				return true;
			else
			{
				this.ID = oldID;
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public virtual bool Load()
		{
			Factory factory = Factory;
			bool result = false;
			IDataReader dr = null;
			using (var cmd = factory.CreateCommand(CommandType.Text))
			{
				using (cmd.Connection = factory.CreateConnection())
				{
					cmd.CommandText = "SELECT * FROM " + factory.FormatSQLName(EntityName) + " WHERE ID = @ID";
					cmd.Parameters.Add(factory.CreateParameter("ID", DbType.UInt32, ID));

					cmd.Connection.Open();
					Log logHelper = new Log(factory, cmd, LogLevel);
					logHelper.ExecuteWithLog(() =>
					{
						using (dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
						{

							if (result = dr.Read() == true)
								Fill(dr);
						}
					});
					return result;
				}
			}
		}
		#endregion
	}
}
