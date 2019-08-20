
using DaLi.Tests.DAL;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaLi.Tests.Tests
{
	[TestClass]
	public class QueryViewTest
	{
		[TestMethod]
		public void TestMethod()
		{
			var list = QueryView.GetList();
			Assert.IsTrue(list.Count > 0);
			
			var view = list[0];
			Assert.IsTrue(!string.IsNullOrEmpty(view.Text));
			Assert.IsTrue(view.Id > 0);
		}
	}
}
