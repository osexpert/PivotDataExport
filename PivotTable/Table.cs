using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace osexpert.PivotTable
{
	public class Table<TRow>
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> RowGroups { get; set; } = null!;

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> ColumnGroups { get; set; } = null!;

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<TableColumn> Columns { get; set; } = null!;

		public List<TRow> Rows { get; set; } = null!;
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

		public FieldType FieldType { get; set; }
		public int GroupIndex { get; set; }

		public SortOrder SortOrder { get; set; }

		public object?[]? GroupValues { get; set; }
	}

}
