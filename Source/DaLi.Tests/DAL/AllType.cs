/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 05-11-2009
 * Time: 20:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data;
using Ckode.DaLi.Entities;
using Ckode.DaLi.Queries;

namespace DaLi.Tests.DAL
{
	/// <summary>
	/// Description of AllType.
	/// </summary>
	public class AllType : Table
	{
		public static DataTable GetTable()
		{
			IQuery query = TestFactory.Factory.CreateQuery("SELECT * FROM alltype");
			return query.GetDataTable();
		}
		
		public static DataTable GetTableCached()
		{
			IQuery query = TestFactory.Factory.CreateQuery("SELECT * FROM alltype ORDER BY Id");
			query.CacheSettings.Enabled = true;
			return query.GetDataTable();
		}
		
		public static IList<AllType> GetListCached()
		{
			IQuery query = TestFactory.Factory.CreateQuery("SELECT * FROM alltype ORDER BY Id");
			query.CacheSettings.Enabled = true;
			return query.GetEntityList<AllType>();
		}
		
		public static IList<AllType> GetList()
		{
			IQuery query = TestFactory.Factory.CreateQuery("SELECT * FROM alltype");
			return query.GetEntityList<AllType>();
		}

		public static void EmptyTable()
		{
			IQuery query = TestFactory.Factory.CreateQuery("DELETE FROM alltype");
			query.ExecuteNonQuery();
		}
		
		public AllType() : base()
		{
		}
		public AllType(uint id) : base(id)
		{
		}
		
		protected override string EntityName
		{
			get
			{
				return "alltype";
			}
		}
		protected override Ckode.DaLi.Factories.Factory Factory
		{
			get
			{
				return TestFactory.Factory;
			}
		}
		
		public Column<DateTime> Date;
		public Column<string> Text;
		public Column<string> Varchar;
		public Column<bool> Bool;
		public Column<double> Double;
		public Column<float> Float;
		public Column<decimal> Decimal;
		public Column<int> Int;
		public Column<uint> Uint;
		public Column<int?> NullableInt;
		public Column<TimeSpan?> NullableTime;
	}
	
	public class PerformanceAllType
	{
		public PerformanceAllType()
		{
			
		}
		
		public void Fill(DataRow dr)
		{
			Date = (DateTime)dr["Date"];
			Text = (string)dr["Text"];
			Varchar = (string)dr["Varchar"];
			Bool = (bool)dr["Bool"];
			Double = (double)dr["Double"];
			Float = (float)dr["Float"];
			Decimal = (decimal)dr["Decimal"];
			Int = (int)dr["Int"];
			Uint = (uint)dr["Uint"];
			object obj = dr["NullableInt"];
			if (obj is DBNull)
				NullableInt = null;
			else
				NullableInt = (int)obj;
		}
		
		public DateTime Date;
		public string Text;
		public string Varchar;
		public bool Bool;
		public double Double;
		public float Float;
		public decimal Decimal;
		public int Int;
		public uint Uint;
		public int? NullableInt;
	}
}
