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
using System.Runtime.Caching;
using System;

namespace Ckode.DaLi.Queries
{
	public static class QueryCache
	{
		public static void Set(string key, DataTable value, ExpirationType expirationType, TimeSpan expirationTime)
		{
			MemoryCache.Default.Set(CreateKey(key), value, CreateCacheItemPolicy(expirationType, expirationTime));
		}

		private static CacheItemPolicy CreateCacheItemPolicy(ExpirationType expirationType, TimeSpan expirationTime)
		{
			if (expirationType == ExpirationType.AbsoluteExpiration)
				return new CacheItemPolicy
				{
					AbsoluteExpiration = DateTime.Now.Add(expirationTime)
				};
			else
				return new CacheItemPolicy
				{
					SlidingExpiration = expirationTime
				};
		}

		public static DataTable Get(string key)
		{
			return MemoryCache.Default.Get(CreateKey(key)) as DataTable;
		}

		public static void Remove(string key)
		{
			MemoryCache.Default.Remove(CreateKey(key));
		}

		private static string CreateKey(string key)
		{
			return "DaLi.QueryCache." + key;
		}
	}
}
