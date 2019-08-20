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
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ckode.DaLi.Utilities;

namespace Ckode.DaLi.Entities
{
	public sealed class CompressedColumn<T> : BaseColumn
	{
		internal static readonly Type CompressedColumnGenericType = typeof(CompressedColumn<>);

		private enum ColumnCompressionType : byte
		{
			ByteArray,
			String
		}

		private ColumnCompressionType? compressionType;
		private T uncompressedValue;
		private byte[] compressedValue;

		internal CompressedColumn(Entity initialParent)
			: base(initialParent)
		{
			compressedValue = null;
			uncompressedValue = default(T);
		}

		internal CompressedColumn(T value)
			: base(null)
		{
			uncompressedValue = value;
			compressedValue = CompressValue(value);
		}

		internal CompressedColumn()
			: base(null)
		{
		}

		#region Implicit converters

		public static implicit operator CompressedColumn<T>(T value)
		{
			if (value == null)
				return new CompressedColumn<T>();
			else
				return new CompressedColumn<T>(value);
		}

		public static implicit operator T(CompressedColumn<T> value)
		{
			if (value == null)
				return default(T);
			return value.Value;
		}

		public static implicit operator Column<T>(CompressedColumn<T> value)
		{
			if (value == null)
				return new Column<T>();
			else
				return new Column<T>(value.Value);
		}

		public static implicit operator CompressedColumn<T>(Column<T> value)
		{
			if (value == null)
				return new CompressedColumn<T>();
			else
				return new CompressedColumn<T>(value.Value);
		}

		#endregion

		public T Value
		{
			get
			{
				return uncompressedValue;
			}
		}

		#region Compression

		private ColumnCompressionType CompressionType
		{
			get
			{
				if (!compressionType.HasValue)
					compressionType = GetCompressionType(typeof(T));

				return compressionType.Value;
			}
		}

		private ColumnCompressionType GetCompressionType(Type type)
		{
			if (type == typeof(string))
				return ColumnCompressionType.String;
			else if (type == typeof(byte[]))
				return ColumnCompressionType.ByteArray;

			throw new NotSupportedException(string.Format(@"Type ""{0}"" is not supported for compression", type.Name));
		}

		private byte[] CompressValue(T value)
		{
			switch (CompressionType)
			{
				case ColumnCompressionType.ByteArray:
					{
						byte[] rawBytes = (byte[])(object)value;
						byte[] compressedBytes = Compress(rawBytes);
						return compressedBytes;
					}
				case ColumnCompressionType.String:
					{
						string rawValue = (string)(object)value;
						byte[] rawBytes = Encoding.UTF8.GetBytes(rawValue);
						byte[] compressedBytes = Compress(rawBytes);
						return compressedBytes;
					}
				default:
					{
						throw new NotImplementedException(string.Format(@"CompressValue not implemented for CompressionType ""{0}""", compressionType.ToString()));
					}
			}
		}

		private T DecompressValue(byte[] compressedBytes)
		{
			switch (CompressionType)
			{
				case ColumnCompressionType.ByteArray:
					{
						byte[] decompressedBytes = Decompress(compressedBytes);
						return (T)(object)decompressedBytes;
					}
				case ColumnCompressionType.String:
					{
						byte[] decompressedBytes = Decompress(compressedBytes);
						string decompressedValue = Encoding.UTF8.GetString(decompressedBytes);
						return (T)(object)decompressedValue;
					}
				default:
					{
						throw new NotImplementedException(string.Format(@"DecompressValue not implemented for CompressionType ""{0}""", compressionType.ToString()));
					}
			}
		}

		private byte[] Compress(byte[] rawBytes)
		{
			using (MemoryStream source = new MemoryStream(rawBytes))
			using (MemoryStream destination = new MemoryStream())
			using (GZipStream zipStream = new GZipStream(destination, CompressionLevel.Optimal))
			{
				source.CopyTo(zipStream);
				zipStream.Close();
				return destination.ToArray();
			}
		}

		private byte[] Decompress(byte[] compressedBytes)
		{
			using (MemoryStream source = new MemoryStream(compressedBytes))
			using (MemoryStream destination = new MemoryStream())
			using (GZipStream zipStream = new GZipStream(source, CompressionMode.Decompress))
			{
				zipStream.CopyTo(destination);
				zipStream.Close();
				return destination.ToArray();
			}
		}

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			BaseColumn objCol = obj as BaseColumn;
			if (objCol != null)
				return Value.Equals(objCol.GetValue());
			return Value.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		internal override void SetValueWithoutChangingParent(object value)
		{
			byte[] compressedBytes = (byte[])value;

			compressedValue = compressedBytes;
			uncompressedValue = DecompressValue(compressedBytes);
		}

		public override DbType DbType
		{
			get
			{
				return DbType.Binary;
			}
		}

		internal override object GetValue()
		{
			return this.compressedValue;
		}

		#endregion
	}
}
