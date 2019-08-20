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
using System.Configuration;
using Ckode.DaLi.Logging;
namespace Ckode.DaLi
{
	public static class Configuration
	{
		private static LoggingLevel? logLevel;
		private static bool? useUnicodeStrings;
		public static LoggingLevel LogLevel
		{
			get
			{
				if (!logLevel.HasValue)
				{
					string appsetting = ConfigurationManager.AppSettings["DaLi_LogLevel"];
					if (!string.IsNullOrEmpty(appsetting) && Enum.IsDefined(typeof(LoggingLevel), appsetting))
						logLevel = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), appsetting);
					else
						logLevel = LoggingLevel.None;
				}
				return logLevel.Value;
			}
			set
			{
				logLevel = value;
			}
		}
		
		public static bool UseUnicodeStrings
		{
			get
			{
				if (!useUnicodeStrings.HasValue)
				{
					string appsetting = ConfigurationManager.AppSettings["DaLi_UseUnicodeStrings"];
					bool tmp;
					if (bool.TryParse(appsetting, out tmp))
						useUnicodeStrings = tmp;
					else
						useUnicodeStrings = false;
				}
				return useUnicodeStrings.Value;
			}
			set
			{
				useUnicodeStrings = value;
			}
		}
	}
}
