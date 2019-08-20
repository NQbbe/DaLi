/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 06-11-2009
 * Time: 18:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Ckode.DaLi.Entities;

namespace DaLi.Tests.DAL
{
	/// <summary>
	/// Description of simpletable.
	/// </summary>
	public class SimpleTable : Table
	{
		public SimpleTable() : base()
		{
		}
		
		public SimpleTable(uint id) : base(id)
		{
			
		}
		
		protected override string EntityName
		{
			get
			{
				return "simpletable";
			}
		}
		protected override Ckode.DaLi.Factories.Factory Factory
		{
			get
			{
				return TestFactory.Factory;
			}
		}
		
		public Column<string> Property { get; set; }
		public Column<string> Field;
	}
}
