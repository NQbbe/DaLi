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
using Ckode.DaLi.Entities;
using Ckode.DaLi.Factories;

namespace Ckode.DaLi.Logging
{
	internal class LogEntry : Table
	{
		private Factory factory;
		protected override string EntityName
		{
			get { return "dali_log"; }
		}

		protected override Factory Factory
		{
			get { return factory; }
		}

		protected override LoggingLevel LogLevel
		{
			get
			{
				return LoggingLevel.None;
			}
		}

		#region Constructors
		public LogEntry(Factory factory)
			: base()
		{
			this.factory = factory;
		}
		#endregion

		public Column<string> SQL;
		public Column<string> Exception;
		public Column<uint> ExecutionTime;
		public Column<DateTime> DateTime;
		public Column<bool> Success;
	}
}
