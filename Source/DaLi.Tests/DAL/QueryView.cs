
using System;
using System.Collections.Generic;
using Ckode.DaLi.Entities;

namespace DaLi.Tests.DAL
{
	/// <summary>
	/// Description of QueryView.
	/// </summary>
	public class QueryView : Entity
	{
		public static IList<QueryView> GetList()
		{
			var query = TestFactory.Factory.CreateQuery("SELECT ID, Text FROM alltype");
			return query.GetEntityList<QueryView>();
		}
		public QueryView() : base()
		{
		}
		
		public Column<string> Text { get; private set; }
		public Column<uint> Id;
	}
}
