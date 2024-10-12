using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PivotDataExport
{
	/// <summary>
	/// Group and aggregate rows
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	public class Pivoter2<TRow> where TRow : class // class notnull
	{
		List<IField<TRow>> _fields;
		IEnumerable<TRow> _rows;

		public List<IField<TRow>> Fields => _fields;

		public Pivoter2(IEnumerable<TRow> rows, IEnumerable<IField<TRow>> fields)
		{
			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;
			_fields = fields.ToList();
			//_props = props.ToDictionary(pd => pd.Name);
		}

		private void Validate()
		{
			if (_fields.Any(f => f.Area == Area.Column) && _fields.Any(f => f.Area == Area.Data && f.SortOrder != SortOrder.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.GroupBy(f => f.Name).Any(g => g.Count() > 1))
				throw new ArgumentException("More than one field with same fieldName");
		}

		//private List<List<Group<TRow>>> GroupRows(IEnumerable<Field> fields, RootType rootType)//, bool sort = false)
		//{
		//	List<Group<TRow>> lastGroups = new List<Group<TRow>>();
		//	lastGroups.Add(new Group<TRow> { Rows = _rows, RootType = rootType });

		//	var res = GroupRows(lastGroups, fields);//, sort: sort);
		//											//			if (!res.Any())
		//											//			return new List<List<Group<TRow>>>() { lastGroups };
		//	return res;
		//}

		private List<List<Group<TRow>>> GroupRows(List<Group<TRow>> lastGroups, IEnumerable<IField<TRow>> fields, bool freeOriginalLastGroupsMem = true)//, bool sort = false)
		{
			List<List<Group<TRow>>> listRes = new();

			//if (!fields.Any())
			//{
			//	// make sure we include root
			//	listRes.Add(lastGroups);
			//	return listRes;
			//}

			List<Group<TRow>> originalLastGroups = lastGroups;

			//			List<Group<T>> lastGroups = new List<Group<T>>();
			//		lastGroups.Add(new Group<T> { Rows = _list, IsRoot = true });

			foreach (IField<TRow> gf in fields)
			{
				//var getter = _props[gf.FieldName];

				var allSubGroups = new List<Group<TRow>>();

				foreach (var go in lastGroups)
				{
					var subGroups = go.Rows.GroupBy(r => gf.GetRowValue(r), gf.GroupComparer).Select(g => new Group<TRow>()
					{
						Key = g.Key,
						Rows = g,
						Field = gf,
						ParentGroup = go
					});

					if (lastGroups == originalLastGroups)
					{
						if (freeOriginalLastGroupsMem)
							go.Rows = null!; // free mem, no longer needed now we have divided rows futher down in sub groups
					}
					else
					{
						go.Rows = null!; // free mem, no longer needed now we have divided rows futher down in sub groups
					}

					//if (sort)
					//{
					//	throw new Exception("never called");
					//	allSubGroups.AddRange(subGroups.OrderBy(sg => sg.Key)); // displayText or value?
					//}
					//else
					allSubGroups.AddRange(subGroups);
				}

				listRes.Add(allSubGroups);

				lastGroups = allSubGroups;
			}

			//return lastGroups;
			if (!listRes.Any())
				listRes.Add(originalLastGroups);
			return listRes;
		}


		private IEnumerable<IField<TRow>> GetDataFields()
		{
			return _fields.Where(f => f.Area == Area.Data);//.OrderBy(f => f.Index);
		}

		//private IEnumerable<Field> GetGroupFields()
		//{
		//	return _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex)
		//		.Concat(_fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex));
		//}


		/// <summary>
		/// Don't remeber why I made both GetGroupedData_FastIntersec and GetGroupedData_PivotTableBuilder
		/// But It seems GetGroupedData_PivotTableBuilder is 2 times slower than GetGroupedData_FastIntersect?
		/// This seemss weird thou.
		/// And I see this one uses Lazy, but not the other one.
		/// 
		/// PS: this seems to be a lot slower than Kazinix.PivotTable.Test.cs?
		/// </summary>
		/// <returns></returns>
		public GroupedData2<TRow, Lazy<KeyValueList>> GetGroupedData_PivotTableBuilder()//bool createEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Row).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Column).OrderBy(f => f.GroupIndex).ToArray();

			var ptb = new PivotTableBuilder<TRow, Lazy<KeyValueList>>(_rows, rows =>
			{
				// only leafs
				//if ((agg_ctx == AggregateContext.Row_ColumnAggregates || agg_ctx == AggregateContext.Row_Aggregates) && group is IGroup<KeyValueList?> g && !g.Children.Any())
				{
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
				}

				//return null;
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
			var lastRows = GetLast(rbl.Rows).ToList();// Flatten(rbl.Rows).Where(r => !r.Children.Any()).ToList();
			var lastCols = GetLast(rbl.ColumnAggregates).ToList();// Flatten(rbl.ColumnAggregates).Where(r => !r.Children.Any()).ToList();

			return new GroupedData2<TRow, Lazy<KeyValueList>>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				table = rbl,
				fields = _fields,
				lastCols = lastCols,
				lastRows = lastRows
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

	public class GroupedData2<TRow, TAggregates> where TRow : class
	{
		public IField<TRow>[] rowFieldsInGroupOrder = null!;
		public IField<TRow>[] colFieldsInGroupOrder = null!;

		public IField<TRow>[] dataFields = null!;

		public List<IField<TRow>> fields = null!;

		public PivotTable<TRow, TAggregates> table = null!;
		public IEnumerable<IGroup<TRow, TAggregates>> lastCols = null!;
		public IEnumerable<IGroup<TRow, TAggregates>> lastRows = null!;
	}
}
