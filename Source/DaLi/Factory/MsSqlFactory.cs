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
using System.Data.SqlClient;
using System.Text;
using System.Data.Common;

namespace Ckode.DaLi.Factories
{
	public class MsSqlFactory : Factory
	{
		public MsSqlFactory(string connectionString)
			: base(connectionString)
		{
		}

		public override DbConnection CreateConnection()
		{
			return new SqlConnection(ConnectionString);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override DbCommand CreateCommand(CommandType commandType)
		{
			var cmd = new SqlCommand();
			cmd.CommandType = commandType;
			return cmd;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public override DbDataAdapter CreateAdapter(DbCommand command)
		{
			DbDataAdapter adapter = new SqlDataAdapter();
			adapter.SelectCommand = command;
			return adapter;
		}

		public override IDbDataParameter CreateParameter(string parameterName, DbType type, object value)
		{
			SqlParameter param;
			if (parameterName[0] == '@')
            	param = new SqlParameter(parameterName, value);
			else
				param = new SqlParameter("@" + parameterName, value);
            string typeName = type.ToString();
            if (typeName[0] == 'U') // Replaces UInt16, 32 and 64 with Int16, 32 and 64
            {
                typeName = typeName.Substring(1);
                type = (DbType)Enum.Parse(typeof(DbType), typeName);
            }
			param.DbType = type;
            param.SourceColumn = parameterName;

			return param;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		internal override DbCommand CreateCommandWithParameters(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            var cmd = CreateCommand(commandType);
            cmd.CommandText = commandText;

            foreach (IDbDataParameter parameter in parameters)
            {
                cmd.Parameters.Add(parameter);
            }
            return cmd;
        }

        internal override string CreateSQLFromCommand(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();
            if (command.CommandType == CommandType.StoredProcedure)
            {
                sb.Append("exec " + command.CommandText + " ");
                foreach (IDbDataParameter parameter in command.Parameters)
                {
                    sb.Append(parameter.ParameterName + "=");
                    if (parameter.Value is string)
                        sb.Append("'" + (string)parameter.Value + "'");
                    else
                        sb.Append(parameter.Value.ToString());
                    sb.Append(", ");
                }
                if (command.Parameters.Count > 0)
                    sb.Length -= 2;
            }
            else
            {
                sb.Append(command.CommandText);
                foreach (IDbDataParameter parameter in command.Parameters)
                {
                    if (parameter.Value is string)
                        sb.Replace(parameter.ParameterName, "'" + (string)parameter.Value + "'");
                    else
                        sb.Replace(parameter.ParameterName, parameter.Value.ToString());
                }
            }
            return sb.ToString();
        }

        internal override string SelectLastInsertedIdentitySQL
        {
            get { return "SELECT SCOPE_IDENTITY();"; }
        }

		public override string FormatSQLName(string name)
		{
			return "[" + name + "]";
		}
	}
}
