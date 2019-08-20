/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 06-11-2009
 * Time: 18:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using DaLi.Tests.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaLi.Tests.Tests
{
	[TestClass]
	public class TableTest
	{
		[TestMethod]
		public void TestLoad()
		{
			var type = new AllType();
			Assert.IsTrue(type.Load(16));
			Assert.IsTrue(type.ID == 16);
			Assert.IsTrue(!string.IsNullOrEmpty(type.Text));
		}

		[TestMethod]
		public void TestUpdate()
		{
			var type = new AllType();
			Assert.IsTrue(type.Load(16));
			type.Text = "Hello world";
			Assert.IsTrue(type.Save());
			type.Load();
			Assert.AreEqual("Hello world", type.Text.Value);
		}

		[TestMethod]
		public void TestInsertAndDelete()
		{
			var simple = new SimpleTable();
			simple.Property = "Hello";
			simple.Field = "world";
			Assert.IsTrue(simple.Save());
			Assert.IsTrue(simple.ID > 0);
			var simple2 = new SimpleTable(simple.ID);
			Assert.IsTrue(simple2.Load());
			Assert.AreEqual("Hello", simple2.Property.Value);
			Assert.AreEqual("world", simple2.Field.Value);

			Assert.IsTrue(simple.Delete());
			Assert.IsFalse(simple2.Load());
		}


		[TestMethod]
		public void TestFieldAndProperty()
		{
			string text = "Foobar";
			SimpleTable row = new SimpleTable();
			row.Field = text;
			row.Property = text;
			
			Assert.IsTrue(row.Save());
			Assert.IsTrue(row.ID > 0);
			
			row = new SimpleTable(row.ID);
			Assert.IsTrue(row.Load());
			
			Assert.AreEqual(row.Field, text);
			Assert.AreEqual(row.Property, text);
		}
		
		[TestMethod]
		public void TestAssigmentFromDifferentInstance()
		{
			string text ="Foobar";
			string text2 = "Hello world";
			SimpleTable row = new SimpleTable();
			row.Field = text;
			row.Property = text;
			Assert.IsTrue(row.Save());
			
			SimpleTable row2 = new SimpleTable();
			row2.Field = row.Property;
			row2.Property = row.Field;
			Assert.IsTrue(row2.Save());
			
			Assert.AreEqual(row.Field, row2.Field);
			Assert.AreEqual(row.Property, row2.Property);
			
			row2.Field = text2;
			row2.Property = text2;
			
			Assert.IsTrue(row2.Save());
			
			Assert.AreNotEqual(row.Field, row2.Field);
			Assert.AreNotEqual(row.Property, row2.Property);
		}
	}
}
