using Ckode.DaLi.Entities;
using Ckode.DaLi.Factories;

namespace DaLi.Tests.DAL
{
	public class CompressionTable : Table
	{
		#region Necessary
		protected override Factory Factory
		{
			get { return TestFactory.Factory; }
		}

		public CompressionTable()
			: base()
		{
		}

		public CompressionTable(uint id)
			: base(id)
		{

		}

		protected override string EntityName
		{
			get
			{
				return "dali.compressiontable";
			}
		}
		#endregion

		public CompressedColumn<string> CompressedBody;
		public Column<string> UncompressedBody;
	}
}
