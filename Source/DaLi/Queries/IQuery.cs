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

namespace Ckode.DaLi.Queries
{
	public interface IQuery
	{
		void AddParameter(string name, object value);
		void AddParameter(string name, DbType dbType, object value);
		void AddParameter(IDbDataParameter parameter);

		CacheSettings CacheSettings { get; }

		int ExecuteNonQuery();
		DataTable GetDataTable();

		TValue GetEntityItem<TValue>() where TValue : Entity;
		TValue GetPrimitiveItem<TValue>();

		IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector) where TValue : Entity;
		IDictionary<TKey, TValue> GetEntityDictionary<TKey, TValue>(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) where TValue : Entity;
					
		IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn);
		IDictionary<TKey, TValue> GetPrimitiveDictionary<TKey, TValue>(string keyColumn, string valueColumn, IEqualityComparer<TKey> comparer);

		IList<TValue> GetEntityList<TValue>() where TValue : Entity;
		IList<TValue> GetPrimitiveList<TValue>();

		TValue[] GetEntityArray<TValue>() where TValue : Entity;
		TValue[] GetPrimitiveArray<TValue>();

		HashSet<TValue> GetEntityHashSet<TValue>() where TValue : Entity;
		HashSet<TValue> GetPrimitiveHashSet<TValue>();

		void Flush();
	}
}
