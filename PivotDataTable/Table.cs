using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PivotDataTable
{
	[XmlRoot("Table")]
	public class Table<TRow> : IXmlSerializable
		where TRow : class
	{
		[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> RowGroups { get; set; } = null!;

		[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> ColumnGroups { get; set; } = null!;

		[JsonIgnore]//(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> Columns { get; set; } = null!;

		public List<TRow> Rows { get; set; } = null!;

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

		private static void WriterRow(XmlWriter writer, TRow row)
		{
			if (row is IEnumerable<KeyValuePair<string, object?>> enu)
				Write(writer, enu);
			else
				writer.WriteValue(row);
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
