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
using Ckode.DaLi.Entities;
using Ckode.DaLi.Factories;

namespace Ckode.DaLi.Queries
{
	internal class Query : IQuery
	{
		private BaseQuery baseQuery;
		public Query(Factory factory, string query)
		{
			this.baseQuery = new BaseQuery(factory, CommandType.Text, query);
		}

		public CacheSettings CacheSettings
		{
			get
			{
				return baseQuery.CacheSettings;
			}
		}

		public void AddParameter(string name, object value)
		{
			baseQuery.AddParameter(name, value);
		}

		public void AddParameter(string name, DbType dbType, object value)
		{
			baseQuery.AddParameter(name, dbType, value);
		}

		public void AddParameter(IDbDataParameter parameter)
		{
			baseQuery.AddParameter(parameter);
		}

		public int ExecuteNonQuery()
		{
			return baseQuery.ExecuteNonQuery();
		}

		public DataTable GetDataTable()
		{
			return baseQuery.GetDataTable();
		}

		public TValue GetPrimitiveItem<TValue>()
		{
			return baseQuery.GetPrimitiveItem<TValue>();
		}

		public TValue GetEntityItem<TValue>() where TValue : Entity
		{
			return baseQuery.GetEntityItem<TValue>();
		}

		public IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector) where TValue : Entity
		{
			return baseQuery.GetEntityDictionary<TKey, TValue>(keySelector);
		}

		public IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) where TValue : Entity
		{
			return baseQuery.GetEntityDictionary<TKey, TValue>(keySelector, comparer);
		}

		public IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn)
		{
			return baseQuery.GetPrimitiveDictionary<TKey, TValue>(keyColumn, valueColumn);
		}
		
		public IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn, IEqualityComparer<TKey> comparer)
		{
			return baseQuery.GetPrimitiveDictionary<TKey, TValue>(keyColumn, valueColumn, comparer);
		}

		public IList<TValue> GetEntityList<TValue>() where TValue : Entity
		{
			return baseQuery.GetEntityList<TValue>();
		}

		public IList<TValue> GetPrimitiveList<TValue>()
		{
			return baseQuery.GetPrimitiveList<TValue>();
		}

		public TValue[] GetEntityArray<TValue>() where TValue : Entity
		{
			return baseQuery.GetEntityArray<TValue>();
		}

		public TValue[] GetPrimitiveArray<TValue>()
		{
			return baseQuery.GetPrimitiveArray<TValue>();
		}

		public HashSet<TValue> GetEntityHashSet<TValue>() where TValue : Entity
		{
			return baseQuery.GetEntityHashSet<TValue>();
		}

		public HashSet<TValue> GetPrimitiveHashSet<TValue>()
		{
			return baseQuery.GetPrimitiveHashSet<TValue>();
		}

		public void Flush()
		{
			baseQuery.Flush();
		}
	}
}