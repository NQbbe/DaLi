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
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Ckode.DaLi.Factories
{
	public class MySqlFactory : Factory
	{
		public MySqlFactory(string connectionString)
			: base(connectionString)
		{
		}

		public override DbConnection CreateConnection()
		{
			return new MySqlConnection(ConnectionString);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override DbCommand CreateCommand(CommandType commandType)
		{
			var cmd = new MySqlCommand();
			cmd.CommandType = CommandType.Text; // HACK: MySql requires root access to execute stored procedures - therefore call it like this instead.
			return cmd;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override DbDataAdapter CreateAdapter(DbCommand command)
		{
			DbDataAdapter adapter = new MySqlDataAdapter();
			adapter.SelectCommand = command;
			return adapter;
		}

		public override IDbDataParameter CreateParameter(string parameterName, DbType type, object value)
		{
			MySqlParameter param;
			if (parameterName[0] == '@')
            	param = new MySqlParameter(parameterName, value);
			else
				param = new MySqlParameter("@" + parameterName, value);
			
			param.DbType = type;
			param.SourceColumn = parameterName;

			return param;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		internal override DbCommand CreateCommandWithParameters(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters)
		{
			var cmd = CreateCommand(commandType);
			if (commandType == CommandType.StoredProcedure) // HACK: MySql requires root access to execute stored procedures - therefore call it like this instead.
			{
				StringBuilder sb = new StringBuilder(200);
				sb.Append("call " + commandText + "(");
				foreach (IDbDataParameter parameter in parameters)
				{
					sb.Append(parameter.ParameterName + ",");
				}
				if (sb[sb.Length - 1] != '(') // at least one parameter was added
					sb.Length--;
				sb.Append(")");
				cmd.CommandText = sb.ToString();
			}
			else
				cmd.CommandText = commandText;

			foreach (IDbDataParameter parameter in parameters)
			{
				cmd.Parameters.Add(parameter);
			}
			return cmd;
		}

		internal override string CreateSQLFromCommand(IDbCommand command)
		{
			// All MySql commands are made as Text, since the MySqlClient doesn't properly
			// support StoredProcedure.
			// Therefore all parameter names are always already included in the commandtext.
			StringBuilder sb = new StringBuilder(command.CommandText);
			foreach (IDbDataParameter parameter in command.Parameters)
			{
				if (parameter.Value is string)
					sb.Replace(parameter.ParameterName, "'" + (string)parameter.Value + "'");
				else
				{
					string value = (parameter.Value ?? "NULL").ToString();
					sb.Replace(parameter.ParameterName, value);
				}
			}
			return sb.ToString();
		}

		internal override string SelectLastInsertedIdentitySQL
		{
			get { return "SELECT LAST_INSERT_ID();"; }
		}

		public override string FormatSQLName(string name)
		{
			return "`" + name + "`";
		}
	}
}