/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 05-11-2009
 * Time: 20:12
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using DaLi.Tests.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaLi.Tests.Tests
{
	[TestClass]
	public class TypeTest
	{
		[TestMethod]
		public void TestAllDataTypesArePersistedCorrectly()
		{
			DateTime date = DateTime.Now;
			date = date.AddMilliseconds(1000 - date.Millisecond); // removes milliseconds
			string text = "Foobar";
			AllType row = new AllType();
			row.Bool = true;
			row.Date = date;
			row.Decimal = 4.2m;
			row.Double = 4.2;
			row.Float = 4.2f;
			row.Int = 42;
			row.Text = text;
			row.Uint = 42;
			row.Varchar = text;
			row.NullableInt = 42;
			row.NullableTime = TimeSpan.FromHours(10);
			Assert.IsTrue(row.Save());
			Assert.IsTrue(row.ID > 0);
			
			row = new AllType(row.ID);
			Assert.IsTrue(row.Load());
			Assert.AreEqual(row.Bool, true);
			Assert.AreEqual(row.Decimal, 4.2m);
			Assert.AreEqual(row.Double, 4.2);
			Assert.AreEqual(row.Float, 4.2f);
			Assert.AreEqual(row.Int, 42);
			Assert.AreEqual(row.Text, text);
			Assert.AreEqual(row.Uint, (uint)42);
			Assert.AreEqual(row.Varchar, text);
			Assert.IsTrue(row.NullableInt.Value.HasValue);
			Assert.IsTrue(row.NullableTime.Value.HasValue);
			Assert.AreEqual(10, row.NullableTime.Value.Value.Hours);
			
			row.NullableInt = null;
			row.Save();
			row = new AllType(row.ID);
			row.Load();
			Assert.IsTrue(!row.NullableInt.Value.HasValue);
			
			
			row.NullableInt = 42;
			row.Save();
			row.NullableInt = (int?)null;
			row.Save();
			row.Load();
			Assert.IsTrue(!row.NullableInt.Value.HasValue);
			
			// Date validation, only compare date and hour, min, second, as it's all the DB will store
			Assert.AreEqual(row.Date.Value.Date, date.Date);
			Assert.AreEqual(row.Date.Value.Hour, date.Hour);
			Assert.AreEqual(row.Date.Value.Minute, date.Minute);
			Assert.AreEqual(row.Date.Value.Second, date.Second);
			row.Delete();
		}
	}
}
