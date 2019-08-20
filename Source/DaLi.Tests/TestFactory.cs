/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 05-11-2009
 * Time: 20:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Ckode.DaLi.Factories;

namespace DaLi.Tests
{
	/// <summary>
	/// Description of TestFactory.
	/// </summary>
	public static class TestFactory
	{
		public static Factory Factory { get; private set; }
		static TestFactory()
		{
			Factory = new MySqlFactory("server=mysql2.unoeuro.com;database=ckode_dk_db;uid=ckode_dk;pwd=efjbEEbxKCRVF8bH0yiJ");
		}
	}
}
