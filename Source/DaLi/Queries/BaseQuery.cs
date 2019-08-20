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
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Ckode.DaLi.Entities;
using Ckode.DaLi.Factories;
using Ckode.DaLi.Logging;
using Ckode.DaLi.Utilities;
using System.Data.Common;

namespace Ckode.DaLi.Queries
{
	internal class BaseQuery : IQuery
	{
		private Factory factory;
		private List<IDbDataParameter> parameters;
		private CommandType commandType;
		private string query;

		private static bool useParallel;
		private const int minimumRowCountForParallel = 20;

		static BaseQuery()
		{
			useParallel = Environment.ProcessorCount >= 4; // It yields too little on dual cores.
		}

		public BaseQuery(Factory factory, CommandType commandType, string query)
		{
			this.factory = factory;
			this.parameters = new List<IDbDataParameter>();
			this.commandType = commandType;
			this.query = query.Trim();
			CacheSettings = new CacheSettings(false, ExpirationType.AbsoluteExpiration, new TimeSpan(0, 30, 0));
		}

		public CacheSettings CacheSettings { get; private set; }

		#region AddParameter

		public void AddParameter(string name, object value)
		{
			parameters.Add(factory.CreateParameter(name, TypeHelper.GetDbType(value.GetType()), value));
		}

		public void AddParameter(string name, DbType dbType, object value)
		{
			if (value == DBNull.Value)
				value = null;
			parameters.Add(factory.CreateParameter(name, dbType, value));
		}

		public void AddParameter(IDbDataParameter param)
		{
			parameters.Add(param);
		}

		#endregion

		#region Get / Execute functions

		public int ExecuteNonQuery()
		{
			using (var cmd = CreateCommandFromQuery())
			{
				using (cmd.Connection)
				{
					try
					{
						cmd.Connection.Open();
						int result = 0;
						Log logHelper = new Log(factory, cmd, Configuration.LogLevel);
						logHelper.ExecuteWithLog(() => result = cmd.ExecuteNonQuery());
						return result;
					}
					finally
					{
						cmd.Parameters.Clear();
					}
				}
			}
		}

		public TValue GetPrimitiveItem<TValue>()
		{
			DataTable dataTable = GetDataTable();
			TValue result = default(TValue);
			if (dataTable != null && dataTable.Rows.Count > 0 && dataTable.Rows[0].ItemArray.Length > 0)
			{
				object value = dataTable.Rows[0].ItemArray[0];
				result = TypeHelper.ConvertToType<TValue>(value);
			}

			return result;
		}

		public TValue GetEntityItem<TValue>() where TValue : Entity
		{
			DataTable dataTable = GetDataTable();
			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				TValue result = TypeHelper.CreateEntity<TValue>();
				result.Fill(dataTable.Rows[0]);
				return result;
			}
			else
				return null;
		}

		public IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector) where TValue : Entity
		{
			return GetEntityDictionary<TKey, TValue>(keySelector, null);
		}

		public IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) where TValue : Entity
		{
			DataTable dataTable = GetDataTable();
			if (dataTable != null)
			{
				var values = GetEntitiesFromDatatable<TValue>(dataTable);

				return comparer != null
					? values.ToDictionary(keySelector, comparer)
					: values.ToDictionary(keySelector);
			}
			else
			{
				return comparer != null
					? new Dictionary<TKey, TValue>(comparer)
					: new Dictionary<TKey, TValue>();
			}
		}

		public IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn)
		{
			return GetPrimitiveDictionary<TKey, TValue>(keyColumn, valueColumn, null);
		}

		public IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn, IEqualityComparer<TKey> comparer)
		{
			DataTable dataTable = GetDataTable();
			IDictionary<TKey, TValue> result;
			if (comparer != null)
				result = new Dictionary<TKey, TValue>(comparer);
			else
				result = new Dictionary<TKey, TValue>();
			if (dataTable != null)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					TKey key = TypeHelper.ConvertToType<TKey>(row[keyColumn]);
					TValue value = TypeHelper.ConvertToType<TValue>(row[valueColumn]);
					result.Add(key, value);
				}
			}
			return result;
		}

		public IList<TValue> GetEntityList<TValue>() where TValue : Entity
		{
			DataTable dataTable = GetDataTable();
			if (dataTable != null)
			{
				return GetEntitiesFromDatatable<TValue>(dataTable)
						.ToList();
			}
			else
				return new List<TValue>();
		}

		public IList<TValue> GetPrimitiveList<TValue>()
		{
			DataTable dataTable = GetDataTable();
			IList<TValue> result = new List<TValue>();

			if (dataTable != null)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					TValue value = TypeHelper.ConvertToType<TValue>(row[0]);
					result.Add(value);
				}
			}
			return result;
		}

		public TValue[] GetEntityArray<TValue>() where TValue : Entity
		{
			DataTable dataTable = GetDataTable();

			if (dataTable != null)
			{
				return GetEntitiesFromDatatable<TValue>(dataTable)
						.ToArray();
			}
			else
				return new TValue[0];
		}

		public TValue[] GetPrimitiveArray<TValue>()
		{
			DataTable dataTable = GetDataTable();
			TValue[] result;

			if (dataTable != null)
			{
				result = new TValue[dataTable.Rows.Count];
				DataRowCollection rows = dataTable.Rows;
				for (int i = 0; i < result.Length; i++)
				{
					TValue value = TypeHelper.ConvertToType<TValue>(rows[i][0]);
					result[i] = value;
				}
			}
			else
				result = new TValue[0];

			return result;
		}

		public HashSet<TValue> GetEntityHashSet<TValue>() where TValue : Entity
		{
			DataTable dataTable = GetDataTable();
			HashSet<TValue> result = new HashSet<TValue>();

			if (dataTable != null)
			{
				foreach (var entity in GetEntitiesFromDatatable<TValue>(dataTable))
				{
					result.Add(entity);
				}
			}

			return result;
		}

		public HashSet<TValue> GetPrimitiveHashSet<TValue>()
		{
			DataTable dataTable = GetDataTable();
			HashSet<TValue> result = new HashSet<TValue>();

			if (dataTable != null)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					TValue value = TypeHelper.ConvertToType<TValue>(row[0]);
					result.Add(value);
				}
			}
			return result;
		}

		private static IEnumerable<TValue> GetEntitiesFromDatatable<TValue>(DataTable dataTable) where TValue : Entity
		{
			if (useParallel && dataTable.Rows.Count >= minimumRowCountForParallel)
			{
				return dataTable.Rows
						.Cast<DataRow>()
						.AsParallel()
						.AsOrdered()
						.Select(row =>
					{
						TValue entity = TypeHelper.CreateEntity<TValue>();
						entity.Fill(row);
						return entity;
					});
			}
			else
			{
				return dataTable.Rows
						.Cast<DataRow>()
						.Select(row =>
					{
						TValue entity = TypeHelper.CreateEntity<TValue>();
						entity.Fill(row);
						return entity;
					});
			}
		}

		public DataTable GetDataTable()
		{
			DataTable result;
			if (CacheSettings.Enabled)
			{
				string queryKey = GetQueryCacheKey();

				if (CacheSettings.Flush)
					QueryCache.Remove(queryKey);

				result = QueryCache.Get(queryKey);
				if (result == null)
				{
					result = GetDataTableDirectlyFromDatabase();
					QueryCache.Set(queryKey, result, CacheSettings.ExpirationType, CacheSettings.ExpirationTime);
				}
			}
			else
				result = GetDataTableDirectlyFromDatabase();

			return result;

		}

		private DataTable GetDataTableDirectlyFromDatabase()
		{
			using (var cmd = CreateCommandFromQuery())
			{
				using (cmd.Connection)
				{
					try
					{
						cmd.Connection.Open();

						using (var adapter = factory.CreateAdapter(cmd))
						{
							using (var result = new DataSet())
							{
								result.Locale = System.Globalization.CultureInfo.CurrentCulture;

								Log logHelper = new Log(factory, cmd, Configuration.LogLevel);
								logHelper.ExecuteWithLog(() => adapter.Fill(result));
								if (result.Tables.Count > 0)
									return result.Tables[0];
							}
						}
						return null;

					}
					finally
					{
						cmd.Parameters.Clear();
					}
				}
			}
		}

		#endregion

		public void Flush()
		{
			string queryKey = GetQueryCacheKey();
			QueryCache.Remove(queryKey);
		}

		#region Helper

		private string GetQueryCacheKey()
		{
			StringBuilder queryKey = new StringBuilder(query);
			foreach (IDbDataParameter parameter in parameters)
			{
				queryKey.Append("&" + parameter.ParameterName + "=" + parameter.Value.ToString());
			}
			return queryKey.ToString();
		}

		private DbCommand CreateCommandFromQuery()
		{
			DbCommand cmd = factory.CreateCommandWithParameters(commandType, query, parameters);
			cmd.Connection = factory.CreateConnection();
			parameters.Clear();
			return cmd;
		}

		#endregion
	}
}
