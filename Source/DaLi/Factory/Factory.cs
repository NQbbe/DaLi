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
using Ckode.DaLi.Queries;
using System.Data.Common;

namespace Ckode.DaLi.Factories
{
	public abstract class Factory
	{
		protected string ConnectionString { get; private set; }

		protected Factory(string connectionString)
		{
			ConnectionString = connectionString;
		}

		internal abstract string SelectLastInsertedIdentitySQL { get; }

		public abstract DbConnection CreateConnection();

		public abstract DbCommand CreateCommand(CommandType commandType);

		public abstract DbDataAdapter CreateAdapter(DbCommand command);

		public abstract IDbDataParameter CreateParameter(string parameterName, DbType type, object value);

		internal abstract DbCommand CreateCommandWithParameters(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters);

		internal abstract string CreateSQLFromCommand(IDbCommand command);

		public IQuery CreateQuery(string query)
		{
			return new Query(this, query);
		}

		public IQuery CreateStoredProcedure(string query)
		{
			return new StoredProcedure(this, query);
		}

		public abstract string FormatSQLName(string name);
	}
}
