using System;
using System.Reflection;
using DaLi.Tests.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaLi.Tests.Tests
{
	[TestClass]
	public class CompressionTest
	{
		[TestMethod]
		public void TestCompressionHappens()
		{
			const string expectedValue = "Hello world";
			CompressionTable row = new CompressionTable();
			row.CompressedBody = expectedValue;

			var baseType = row.CompressedBody.GetType().BaseType;
			var fieldInfo = baseType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
			var rawValue = fieldInfo.GetValue(row.CompressedBody);

			Assert.AreEqual(expectedValue, row.CompressedBody.Value);
			Assert.IsNotNull(rawValue);
			Assert.AreNotEqual(expectedValue, rawValue);
		}

		[TestMethod]
		public void TestDecompressionHappens()
		{
			const string expectedValue = "Hello world";
			CompressionTable row = new CompressionTable();
			row.CompressedBody = expectedValue;
			row.Save();

			CompressionTable row2 = new CompressionTable(row.ID);
			row2.Load();

			try
			{
				Assert.AreEqual(expectedValue, row2.CompressedBody.Value);
			}
			finally
			{
				row.Delete();
			}
		}

		[TestMethod]
		public void TestColumnToCompressedColumnAssignments()
		{
			const string expectedValue = "Hello world";

			CompressionTable row = new CompressionTable();
			row.CompressedBody = expectedValue;
			row.UncompressedBody = row.CompressedBody;
			row.Save();

			CompressionTable row2 = new CompressionTable();
			row2.UncompressedBody = row.CompressedBody;
			row2.CompressedBody = row.UncompressedBody;
			row2.Save();

			CompressionTable row3 = new CompressionTable(row.ID);
			row3.Load();

			CompressionTable row4 = new CompressionTable(row2.ID);
			row4.Load();

			try
			{
				Assert.AreEqual(expectedValue, row3.CompressedBody.Value);
				Assert.AreEqual(expectedValue, row3.UncompressedBody.Value);
				Assert.AreEqual(expectedValue, row4.CompressedBody.Value);
				Assert.AreEqual(expectedValue, row4.UncompressedBody.Value);
			}
			finally
			{
				row.Delete();
				row2.Delete();
			}
		}
	}
}
