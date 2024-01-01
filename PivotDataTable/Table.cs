using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PivotDataTable
{
	[XmlRoot("Table")]
	public class Table<TTableRow> : IXmlSerializable
		where TTableRow : class, IEnumerable
	{
	//	[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
//		public List<TableColumn> RowGroups { get; set; } = null!;

		//[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
		//public List<TableColumn> ColumnGroups { get; set; } = null!;

		[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> Columns { get; set; } = null!;

		/// <summary>
		/// If PartialRows, the columns in the rows do not necesarely have the same numbers of columns, the same number of columns as in the Columns-list.
		/// This means, we can't use rows for making fixed number of columns formats such as csv.
		/// </summary>
		[JsonIgnore]
		public bool PartialRows;


		/// <summary>
		/// At least one intersect without data. Means that createEmptyIntersects = false
		/// </summary>
		[JsonIgnore]
		public bool PartialIntersects;

		public List<TTableRow> Rows { get; set; } = null!;

		public XmlSchema? GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Rows");
			foreach (var row in Rows)
			{
				writer.WriteStartElement("Row");
				WriterRow(writer, row);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private void WriterRow(XmlWriter writer, TTableRow row)
		{
			if (row is IEnumerable<KeyValuePair<string, object?>> enu)
			{
				Write(writer, enu);
			}
			else
			{
				foreach (var col in Columns.ZipForceEqual(row.Cast<object>(), (f, s) => new { First = f, Second = s }))
				{
					writer.WriteStartElement(col.First.Name);
					writer.WriteValue(col.Second);
					writer.WriteEndElement();
				}
			}
		}

		private static void Write(XmlWriter writer, IEnumerable<KeyValuePair<string, object?>> enu)
		{
			foreach (var ele in enu)
			{
				if (ele is KeyValuePair<string, object?> kvp)
				{
					writer.WriteStartElement(kvp.Key);

					if (kvp.Value is IEnumerable<KeyValuePair<string, object?>> se)
						Write(writer, se);
					else if (kvp.Value is IEnumerable<IEnumerable<KeyValuePair<string, object?>>> lse)
					{
						foreach (var lsee in lse)
						{
							writer.WriteStartElement("Entry");
							Write(writer, lsee);
							writer.WriteEndElement();
						}
					}
					else if (kvp.Value != null)
						writer.WriteValue(kvp.Value);

					writer.WriteEndElement();
				}
				else
					writer.WriteValue(ele);
			}
		}


		//		public TRow? GrandTotalRow { get; set; }

		//public void ChangeTypeToName(bool fullName = false)
		//{
		//	foreach (var c in this.Columns)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//	foreach (var c in this.ColumnGroups)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//}


		public string ToXml()
		{
			XmlSerializer xsSubmit = new XmlSerializer(this.GetType());
			using (var sww = new ExtentedStringWriter(Encoding.UTF8))
			{
				using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
				{
					xsSubmit.Serialize(writer, this);
					return sww.ToString();
				}
			}
		}

		public sealed class ExtentedStringWriter : StringWriter
		{
			private readonly Encoding stringWriterEncoding;

			public ExtentedStringWriter(Encoding desiredEncoding)
				: base()
			{
				this.stringWriterEncoding = desiredEncoding;
			}

			public ExtentedStringWriter(StringBuilder builder, Encoding desiredEncoding)
				: base(builder)
			{
				this.stringWriterEncoding = desiredEncoding;
			}

			public override Encoding Encoding
			{
				get
				{
					return this.stringWriterEncoding;
				}
			}
		}

		public string ToCsv(char separator = ';', bool addHeaderRow = true)
		{
			if (PartialRows)
				throw new Exception("Can't create cvs with (potentionally) partial rows");

			StringBuilder sb = new();

			if (addHeaderRow)
			{
				sb.AppendLine(FormatCsvRow(separator, Columns.Select(c => XLinq_GetStringValue(c.Name))));
			}

			foreach (var row in Rows)
			{
				if (row is KeyValueList kvl)
				{
					sb.AppendLine(FormatCsvRow(separator, kvl.Select(o => XLinq_GetStringValue(o.Value!))));
				}
				else
					sb.AppendLine(FormatCsvRow(separator, row.Cast<object>().Select(o => XLinq_GetStringValue(o))));
			}
			return sb.ToString();
		}

		/// <summary>
		/// https://github.com/microsoft/referencesource/blob/master/System.Xml.Linq/System/Xml/Linq/XLinq.cs
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		internal static string XLinq_GetStringValue(object value)
		{
			string s;
			if (value is string)
			{
				s = (string)value;
			}
			else if (value is double)
			{
				s = XmlConvert.ToString((double)value);
			}
			else if (value is float)
			{
				s = XmlConvert.ToString((float)value);
			}
			else if (value is decimal)
			{
				s = XmlConvert.ToString((decimal)value);
			}
			else if (value is bool)
			{
				s = XmlConvert.ToString((bool)value);
			}
			else if (value is DateTime)
			{
				s = GetDateTimeString((DateTime)value);
			}
			else if (value is DateTimeOffset)
			{
				s = XmlConvert.ToString((DateTimeOffset)value);
			}
			else if (value is TimeSpan)
			{
				s = XmlConvert.ToString((TimeSpan)value);
			}
			else if (value is XObject)
			{
				throw new ArgumentException("XObjectValue");// Res.GetString(Res.Argument_XObjectValue));
			}
			else
			{
				s = value.ToString();
			}
			if (s == null) throw new ArgumentException("ConvertToString");// Res.GetString(Res.Argument_ConvertToString));
			return s;
		}

		internal static string GetDateTimeString(DateTime value)
		{
			return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
		}

		// https://stackoverflow.com/questions/12963117/is-there-a-write-counterpart-to-microsoft-visualbasic-fileio-textfieldparser
		// https://stackoverflow.com/questions/6377454/escaping-tricky-string-to-csv-format
		public static string FormatCsvCell(char separator, string cell, bool alwaysQuote = false)
		{
			bool mustQuote(string str) => str.IndexOfAny(new char[] { separator, '"', '\r', '\n' }) > -1;

			if (alwaysQuote || mustQuote(cell))
			{
				StringBuilder sb = new();
				sb.Append('\"');
				foreach (char nextChar in cell)
				{
					sb.Append(nextChar);
					if (nextChar == '"')
						sb.Append('\"');
				}
				sb.Append('\"');
				return sb.ToString();
			}

			return cell;
		}

		public static string FormatCsvRow(char separator, IEnumerable<string> cells, bool alwaysQuote = false)
		{
			return string.Join(separator.ToString(), cells.Select(cell => FormatCsvCell(separator, cell, alwaysQuote)));
		}




	}




	public class TableColumn
	{
		public string Name { get; set; } = null!;

		[JsonIgnore]
		public Type DataType { get; set; } = null!;

		public string TypeName => DataType.Name;

		public Area FieldArea { get; set; }
		public int GroupIndex { get; set; }

		public SortOrder SortOrder { get; set; }

		public object?[]? GroupValues { get; set; }
	}

}
