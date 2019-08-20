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
using System.Linq;
using System.Text;

using Ckode.DaLi.Factories;
using Ckode.DaLi.Logging;
using Ckode.DaLi.Utilities;
using Ckode.DaLi.Validation;
using System.Collections.Concurrent;
using Ckode.DaLi.Queries;

namespace Ckode.DaLi.Entities
{
	public abstract class Table : DbEntity
	{
		#region Constructors

		protected Table()
			: base()
		{
		}

		protected Table(uint id)
			: base(id)
		{
		}

		#endregion

		#region Fill

		public override void Fill(IDataReader dr)
		{
			base.Fill(dr);
			ID = Convert.ToUInt32(dr["ID"]);
		}

		public override void Fill(DataRow dr)
		{
			base.Fill(dr);
			ID = Convert.ToUInt32(dr["ID"]);
		}

		#endregion

		#region Reset

		protected void ResetFields()
		{
			ID = 0;
			foreach (ColumnMember field in columns.Values)
			{
				field.InstantiateField(this);
			}
		}

		protected void ResetFieldsChanged()
		{
			foreach (ColumnMember field in columns.Values)
			{
				field.ResetChanged(this);
			}
		}

		#endregion

		#region Save/Delete

		public virtual bool Delete()
		{
			bool result = InternalDelete();
			if (result)
			{
				OnDeletedItem();
				OnWrite();
				ResetFields();
			}
			return result;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private bool InternalDelete()
		{
			Factory factory = Factory;
			bool result = false;
			using (var cmd = factory.CreateCommand(CommandType.Text))
			{
				using (cmd.Connection = factory.CreateConnection())
				{
					cmd.CommandText = "DELETE FROM " + factory.FormatSQLName(EntityName) + " WHERE ID = @ID";
					cmd.Parameters.Add(factory.CreateParameter("ID", DbType.UInt32, ID));

					cmd.Connection.Open();
					Log logHelper = new Log(factory, cmd, LogLevel);
					logHelper.ExecuteWithLog(() =>
						{
							result = (cmd.ExecuteNonQuery() > 0);
						});
				}
			}

			return result;
		}

		public virtual bool Save()
		{
			if (!IsValid)
				throw new ValidationException("One or more validation errors occurred. Check the result of the Validate method for the exact errors.");

			bool insert = ID < 1;
			if (insert)
				OnPreInsert();
			bool saved = ID < 1 ? Insert() : Update();
			if (saved)
			{
				if (insert)
					OnNewItem();
				else
					OnChangedItem();
				OnWrite();
			}
			return saved;
		}

		#endregion

		#region Insert/Update

		private bool Insert()
		{
			bool result = InternalInsert(GetParameters());
			if (result)
				ResetFieldsChanged();
			return result;
		}

		private bool Update()
		{
			bool result = InternalUpdate(GetParameters());
			if (result)
				ResetFieldsChanged();
			return result;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private bool InternalInsert(ICollection<IDbDataParameter> parameters)
		{
			if (parameters.Count == 0)
				return false;

			Factory factory = Factory;
			string entityName = factory.FormatSQLName(EntityName);
			StringBuilder sql = new StringBuilder("INSERT INTO " + entityName + " (", 512);
			StringBuilder values = new StringBuilder(") VALUES (", 512);
			using (var cmd = factory.CreateCommand(CommandType.Text))
			{
				using (cmd.Connection = factory.CreateConnection())
				{
					foreach (IDbDataParameter parameter in parameters)
					{
						cmd.Parameters.Add(parameter);
						sql.Append("" + entityName + "." + factory.FormatSQLName(parameter.SourceColumn) + ",");
						values.Append(parameter.ParameterName + ",");
					}
					sql.Length--;
					values.Length--;

					sql.Append(values.ToString() + ");" + factory.SelectLastInsertedIdentitySQL);
					cmd.CommandText = sql.ToString();

					try
					{
						cmd.Connection.Open();
						Log logHelper = new Log(factory, cmd, LogLevel);
						logHelper.ExecuteWithLog(() =>
							{
								ID = Convert.ToUInt32(cmd.ExecuteScalar());
							});
						return true;
					}
					finally
					{
						cmd.Parameters.Clear();
					}
				}
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private bool InternalUpdate(ICollection<IDbDataParameter> parameters)
		{
			if (parameters.Count == 0)
				return true;

			Factory factory = Factory;
			bool result = false;
			string entityName = factory.FormatSQLName(EntityName);
			StringBuilder sql = new StringBuilder("UPDATE " + entityName + " SET ", 512);
			using (var cmd = factory.CreateCommand(CommandType.Text))
			{
				using (cmd.Connection = factory.CreateConnection())
				{
					foreach (IDbDataParameter parameter in parameters)
					{
						cmd.Parameters.Add(parameter);
						sql.Append(entityName + "." + factory.FormatSQLName(parameter.SourceColumn) + "=" + parameter.ParameterName + ",");
					}

					sql.Length--;
					sql.Append(" WHERE ID = @ID");
					cmd.CommandText = sql.ToString();
					cmd.Parameters.Add(factory.CreateParameter("ID", DbType.UInt32, ID));

					try
					{
						cmd.Connection.Open();
						Log logHelper = new Log(factory, cmd, LogLevel);
						logHelper.ExecuteWithLog(() =>
							{
								result = (cmd.ExecuteNonQuery() > 0);
							});
						return result;
					}
					finally
					{
						cmd.Parameters.Clear();
					}
				}
			}
		}

		#endregion

		#region Validation

		public virtual IEnumerable<ValidationError> Validate()
		{
			yield break;
		}

		public bool IsValid
		{
			get
			{
				return !Validate().Any();
			}
		}

		#endregion

		#region Virtual methods

		/// <summary>
		/// Used for setting default values before inserting
		/// </summary>
		protected virtual void OnPreInsert()
		{
		}

		protected virtual void OnNewItem()
		{
		}

		protected virtual void OnDeletedItem()
		{
		}

		protected virtual void OnChangedItem()
		{
		}

		/// <summary>
		/// Common "event" for any of the "events": New, Deleted, Changed
		/// Can be used to e.g. flush caches
		/// </summary>
		protected virtual void OnWrite()
		{
		}

		#endregion

		internal ICollection<IDbDataParameter> GetParameters()
		{
			List<IDbDataParameter> result = new List<IDbDataParameter>();

			foreach (ColumnMember fi in columns.Values)
			{
				BaseColumn col = fi.GetValue(this);
				if (col == null)
					result.Add(Factory.CreateParameter(fi.Name, fi.ColumnDbType, null));
				else if ((col.IsChanged(this)))
					result.Add(Factory.CreateParameter(fi.Name, col.DbType, col.GetValue()));
			}
			return result;
		}
	}
}