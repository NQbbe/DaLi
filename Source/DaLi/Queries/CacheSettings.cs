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

namespace Ckode.DaLi.Queries
{
	public enum ExpirationType : byte
	{
		AbsoluteExpiration,
		SlidingExpiration
	}

	public class CacheSettings
	{
		public bool Enabled { get; set; }

		public ExpirationType ExpirationType { get; set; }

		public TimeSpan ExpirationTime { get; set; }

		public bool Flush { get; set; }

		public CacheSettings(bool enabled, ExpirationType expirationType, TimeSpan expirationTime)
		{
			this.Enabled = enabled;
			this.ExpirationType = expirationType;
			this.ExpirationTime = expirationTime;
			this.Flush = false;
		}
	}
}
