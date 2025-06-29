using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PivotDataExport
{
	/// <summary>
	/// Group and aggregate rows (uses PivotTableBuilder/Ptb)
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	internal class PivotBuilderPtb<TRow> where TRow : class // class notnull
	{
		List<Field<TRow>> _fields;
		IEnumerable<TRow> _rows;

		public List<Field<TRow>> Fields => _fields;

		public PivotBuilderPtb(IEnumerable<TRow> rows, IEnumerable<Field<TRow>> fields)
		{
			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;
			_fields = fields.ToList();
		}

		private void Validate()
		{
			if (_fields.Any(f => f.Area == Area.Column) && _fields.Any(f => f.Area == Area.Data && f.SortOrder != SortOrder.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.GroupBy(f => f.Name).Any(g => g.Count() > 1))
				throw new ArgumentException("More than one field with same fieldName");
		}

		private IEnumerable<Field<TRow>> GetDataFields()
		{
			return _fields.Where(f => f.Area == Area.Data);//.OrderBy(f => f.Index);
		}

		//private IEnumerable<Field> GetGroupFields()
		//{
		//	return _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex)
		//		.Concat(_fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex));
		//}


		/// <summary>
		/// GetGroupedData (uses PivotTableBuilder/Ptb)
		/// I see this one uses Lazy, but not the other one (fast intersect).
		/// PS: this seems to be a lot slower than Kazinix.PivotTable.Test.cs? It was because of using Yield/IEnumerable to get row value. Now has own method for this.
		/// </summary>
		/// <returns></returns>
		public GroupedDataPtb<TRow, Lazy<KeyValueList>> GetGroupedData()//bool padEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Row).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Column).OrderBy(f => f.GroupIndex).ToArray();

			var ptb = new PivotTableBuilder<TRow, Lazy<KeyValueList>>(_rows, rows =>
			{
				// Lazy: avoid calling GetRowsValue "too much" (performance). We won't read all of them.
				return new Lazy<KeyValueList>(() =>
				{
					KeyValueList res = new();
					foreach (var dataField in dataFields)
					{
						var theValue = dataField.GetRowsValue(rows);
						res.Add(dataField.Name, theValue);
					}
					return res;
				});
			});

			foreach (var rowF in rowFieldsInGroupOrder)
			{
				ptb.AddRow((row => rowF.GetRowValue(row), rowF));
			}
			foreach (var colF in colFieldsInGroupOrder)
			{
				ptb.AddColumn((col => colF.GetRowValue(col), colF));
			}
			var rbl = ptb.Build();

			// flip so we get the last groups (they have no children)
			var lastRows = GetLast(rbl.Rows).ToList();
			var lastCols = GetLast(rbl.ColumnAggregates).ToList();

			return new GroupedDataPtb<TRow, Lazy<KeyValueList>>()
			{
				ColFieldsInGroupOrder = colFieldsInGroupOrder,
				RowFieldsInGroupOrder = rowFieldsInGroupOrder,
				DataFields = dataFields,
				Table = rbl,
				Fields = _fields,
				LastCols = lastCols,
				LastRows = lastRows
			};
		}

		/// <summary>
		/// return groups without children (Last groups)
		/// </summary>
		static IEnumerable<IGroup<TRow, TAgg>> GetLast<TAgg>(IEnumerable<IGroup<TRow, TAgg>> source)
		{
			return source.TopogicalSequenceDFS<IGroup<TRow, TAgg>>(d => d.Children).Where(r => !r.Children.Any());
		}
	}

	internal class GroupedDataPtb<TRow, TAggregates> where TRow : class
	{
		public Field<TRow>[] RowFieldsInGroupOrder = null!;
		public Field<TRow>[] ColFieldsInGroupOrder = null!;

		public Field<TRow>[] DataFields = null!;

		public List<Field<TRow>> Fields = null!;

		public PivotTable<TRow, TAggregates> Table = null!;
		public IEnumerable<IGroup<TRow, TAggregates>> LastCols = null!;
		public IEnumerable<IGroup<TRow, TAggregates>> LastRows = null!;
	}
}
