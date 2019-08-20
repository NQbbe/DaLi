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
using System.Data;
using System.Diagnostics;
using Ckode.DaLi.Factories;

namespace Ckode.DaLi.Logging
{
	internal class Log
	{
		Factory factory;
		IDbCommand command;
		LoggingLevel logLevel;

		internal Log(Factory factory, IDbCommand command, LoggingLevel logLevel)
		{
			this.factory = factory;
			this.command = command;
			this.logLevel = logLevel;
		}

		internal void ExecuteWithLog(Action function)
		{
			if (logLevel > LoggingLevel.None)
			{
				var sw = Stopwatch.StartNew();
				try
				{
					function();
					sw.Stop();
					if (logLevel == LoggingLevel.All)
						SaveLog(null, sw.ElapsedMilliseconds);
				}
				catch (Exception ex)
				{
					sw.Stop();
					SaveLog(ex, sw.ElapsedMilliseconds);
					throw;
				}
			}
			else
				function();
		}
		private void SaveLog(Exception ex, long executionTime)
		{
			LogEntry log = new LogEntry(factory);
			if (ex != null)
				log.Exception = ex.ToString();
			else
				log.Exception = string.Empty;
			log.ExecutionTime = (uint)executionTime;
			log.Success = (ex == null);
			log.SQL = factory.CreateSQLFromCommand(command);
			log.DateTime = DateTime.Now;
			log.Save();
		}
	}
}
