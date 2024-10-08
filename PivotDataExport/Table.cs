﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PivotDataExport
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
		public IEnumerable<TableColumn> Columns { get; internal set; } = null!;

		/// <summary>
		/// If PartialRows, the rows do not (necesarely) have the same numbers of columns or the same number of columns as in Columns-list.
		/// This means, we can't use rows for making fixed number of columns formats such as csv.
		/// </summary>
		[JsonIgnore]
		public bool PartialRows { get; internal set; }


		/// <summary>
		/// At least one intersect without data. Means that createEmptyIntersects = false.
		/// Used for debug\informational purpose.
		/// </summary>
		[JsonIgnore]
		public bool PartialIntersects { get; internal set; }

		public IEnumerable<TTableRow> Rows { get; internal set; } = null!;

		internal TTableRow HeaderRow = null!;

		XmlSchema? IXmlSerializable.GetSchema() => null;

		void IXmlSerializable.ReadXml(XmlReader reader) => throw new NotImplementedException();

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Rows");
			foreach (var row in Rows)
			{
				writer.WriteStartElement("Row");
				WriterXmlRow(writer, row);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private void WriterXmlRow(XmlWriter writer, TTableRow row)
		{
			if (row is IEnumerable<KeyValuePair<string, object?>> keyValuePairs)
			{
				WriteXmlKeyValuePairs(writer, keyValuePairs);
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

		private static void WriteXmlKeyValuePairs(XmlWriter writer, IEnumerable<KeyValuePair<string, object?>> kvps)
		{
			foreach (var kvp in kvps)
			{
				//if (keyValuePair is KeyValuePair<string, object?> kvp)
				//				{
				writer.WriteStartElement(kvp.Key);

				if (kvp.Value is IEnumerable<KeyValuePair<string, object?>> se)
					WriteXmlKeyValuePairs(writer, se);
				else if (kvp.Value is IEnumerable<IEnumerable<KeyValuePair<string, object?>>> lse)
				{
					foreach (var lsee in lse)
					{
						writer.WriteStartElement("Entry");
						WriteXmlKeyValuePairs(writer, lsee);
						writer.WriteEndElement();
					}
				}
				else if (kvp.Value != null)
					writer.WriteValue(kvp.Value);

				writer.WriteEndElement();
				//				}
				//				else
				//				writer.WriteValue(keyValuePair);
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

		/// <summary>
		/// First row in Rows is a header row
		/// </summary>
		[JsonIgnore]
		public bool HasHeaderRow { get; private set; }

		public Table<TTableRow> AddHeaderRowClone()
		{
			if (HasHeaderRow)
				return this;

			return new Table<TTableRow>()
			{
				Columns = this.Columns,
				Rows = GetHeaderRow().Concat(this.Rows),
				HasHeaderRow = true
			};
		}

		private IEnumerable<TTableRow> GetHeaderRow()
		{
			yield return HeaderRow;
		}

		public void WriteXml(Stream s)
		{
			XmlSerializer xsSubmit = new XmlSerializer(this.GetType());

			using (XmlTextWriter writer = new XmlTextWriter(s, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
			{
				xsSubmit.Serialize(writer, this);
			}
		}

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

		sealed class ExtentedStringWriter : StringWriter
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

		public void WriteCsv(Stream s, char separator = ';', bool addHeaderRow = true)
		{
			using var w = new StreamWriter(s);

			if (PartialRows)
				throw new Exception("Can't create cvs with (potentionally) partial rows");

			if (addHeaderRow && !HasHeaderRow)
			{
				w.WriteLine(FormatCsvRow(separator, Columns.Select(c => XLinq_GetStringValue(c.Name))));
			}

			foreach (var row in Rows)
			{
				if (row is KeyValueList kvl)
				{
					w.WriteLine(FormatCsvRow(separator, kvl.Select(o =>
					{
						if (o.Value is KeyValueList)
							throw new InvalidOperationException("Nested keyValueList. Should never get here, PartialRows should be true in this case");

						return XLinq_GetStringValue(o.Value!);
					})));
				}
				else
					w.WriteLine(FormatCsvRow(separator, row.Cast<object>().Select(o => XLinq_GetStringValue(o))));
			}
		}


		public string ToCsv(char separator = ';', bool addHeaderRow = true)
		{
			if (PartialRows)
				throw new Exception("Can't create cvs with (potentionally) partial rows");

			StringBuilder sb = new();

			if (addHeaderRow && !HasHeaderRow)
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
		private static string XLinq_GetStringValue(object value)
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

		private static string GetDateTimeString(DateTime value)
		{
			return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
		}

		// https://stackoverflow.com/questions/12963117/is-there-a-write-counterpart-to-microsoft-visualbasic-fileio-textfieldparser
		// https://stackoverflow.com/questions/6377454/escaping-tricky-string-to-csv-format
		private static string FormatCsvCell(char separator, string cell, bool alwaysQuote = false)
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

		private static string FormatCsvRow(char separator, IEnumerable<string> cells, bool alwaysQuote = false)
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
