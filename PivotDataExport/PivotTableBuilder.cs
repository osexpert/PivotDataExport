using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PivotDataExport
{
	//public interface IPivotTableBuilder<TRow, TAggregates>
	//where TRow : class
	//{
	//	IPivotTableBuilder<TRow, TAggregates> SetRow(Func<TRow, object> rowFunction);
	//	IPivotTableBuilder<TRow, TAggregates> SetColumn(Func<TRow, object> columnFunction);
	//	PivotTable<TAggregates> Build();
	//}

	//public enum AggregateContext
	//{
	//	//Table,
	//	//Row,
	//	//Column,
	//	Row_Aggregates,
	//	Row_ColumnAggregates,
	//	Table_Aggregates,
	//	Table_ColumnAggregates,
	//	//Column_Aggregates
	//}

	/// <summary>
	/// Based on https://github.com/Kazinix/PivotTable
	/// Added sorting
	/// Added common iface for row and col (IGroup)
	/// Introduced Field
	/// This impl. seems to be just as fast as GetGroupedData_FastIntersect and more readable,
	/// so idea is to switch to use this completely.
	/// It has 1 con: aggregates a lot, waistefull if we dont need them. Could maybe use lazy. Or some other way (eg. callback context in the _aggregateFunction, WIP)
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	/// <typeparam name="TAgg"></typeparam>
	public class PivotTableBuilder<TRow, TAgg> //: IPivotTableBuilder<TRow, TAggregates>
		   where TRow : class
	{
		private readonly IList<(Func<TRow, object?>, IField<TRow>)> _rowFunctions;
		private readonly IList<(Func<TRow, object?>, IField<TRow>)> _columnFunctions;
		private readonly Func<IEnumerable<TRow>, TAgg> _aggregateFunction;
		private readonly IEnumerable<TRow> _list;

		//public bool _calcRootColumnAggregates = true;

		internal PivotTableBuilder(IEnumerable<TRow> list, Func<IEnumerable<TRow>, TAgg> aggregateFunction)
		{
			_list = list;
			_aggregateFunction = aggregateFunction;
			_rowFunctions = new List<(Func<TRow, object?>, IField<TRow>)>();
			_columnFunctions = new List<(Func<TRow, object?>, IField<TRow>)>();
		}
		public PivotTableBuilder<TRow, TAgg> AddRow((Func<TRow, object?>, IField<TRow>) rowFunction)
		{
			_rowFunctions.Add(rowFunction);
			return this;
		}

		public PivotTableBuilder<TRow, TAgg> AddColumn((Func<TRow, object?>, IField<TRow>) columnFunction)
		{
			_columnFunctions.Add(columnFunction);
			return this;
		}

		public PivotTable<TRow, TAgg> Build()
		{
			var pivotTable = new PivotTable<TRow, TAgg>();

			//compute aggregates for the whole table
			pivotTable.Aggregates = _aggregateFunction(_list);//, null, AggregateContext.Table_Aggregates);
			pivotTable.ColumnAggregates = ComputeColumns(null, _list, _columnFunctions);//, AggregateContext.Table_ColumnAggregates);
			pivotTable.Rows = ComputeRows(null, _list, _rowFunctions, _columnFunctions);

			return pivotTable;
		}

		private List<Row<TRow, TAgg>> ComputeRows(Row<TRow, TAgg>? parent, IEnumerable<TRow> list,
			IEnumerable<(Func<TRow, object?>, IField<TRow>)> rowFunctions,
			IEnumerable<(Func<TRow, object?>, IField<TRow>)> columnFunctions)
		{
			var rows = new List<Row<TRow, TAgg>>();
			if (!rowFunctions.Any())
				return rows;

			//create a list that will be modified by the scope
			var rowFunctionsCopy = rowFunctions.ToList();

			//pop the row function
			var rowFunction = rowFunctionsCopy.First();
			rowFunctionsCopy.Remove(rowFunction);

			var field = rowFunction.Item2;
			
			//group items by row
			var groups = list.GroupBy(rowFunction.Item1);

			foreach (var group in groups)
			{
				var newRow = new Row<TRow, TAgg>();
				newRow.Field = field;
				newRow.Parent = parent;
				newRow.Value = group.Key;

				var groupRows = group.ToList();

				// ToList seems useless? At least 3 times...fixed now?
				// Do Aggregate after Compute, so the delegate can use info from newRow to decide if to calc the agg.
				newRow.Aggregates = _aggregateFunction(groupRows);//, newRow, AggregateContext.Row_Aggregates);

				newRow.Children = ComputeRows(newRow, groupRows, rowFunctionsCopy, columnFunctions);

				newRow.ColumnAggregates = ComputeColumns(null /* hmm...maybe the newRow is the parent here?? in case, parent must be IGroup? */, groupRows, _columnFunctions);//, AggregateContext.Row_ColumnAggregates);

				rows.Add(newRow);
			}
			
			if (field.SortOrder == SortOrder.Asc)
			{
				rows = rows.OrderBy(c => c.Value, field.SortComparer).ToList();
			}
			else if (field.SortOrder == SortOrder.Desc)
			{
				rows = rows.OrderByDescending(c => c.Value, field.SortComparer).ToList();
			}

			return rows;
		}

		private List<Column<TRow, TAgg>> ComputeColumns(Column<TRow, TAgg>? parent, IEnumerable<TRow> list,
			IEnumerable<(Func<TRow, object?>, IField<TRow>)> columnFunctions)//, AggregateContext agg_ctx)
		{
			var columns = new List<Column<TRow, TAgg>>();
			if (!columnFunctions.Any())
				return columns;

			//create a list that will be modified by the scope
			var columnFunctionsCopy = columnFunctions.ToList();

			//pop the column function 
			var columnFunction = columnFunctionsCopy.First();
			columnFunctionsCopy.Remove(columnFunction);

			var field = columnFunction.Item2;

			//group items by column
			var groups = list.GroupBy(columnFunction.Item1);

			foreach (var group in groups)
			{
				var newColumn = new Column<TRow, TAgg>();
				newColumn.Field = field;
				newColumn.Parent = parent;
				newColumn.Value = group.Key;

				var groupRows = group.ToList();

				// ToList seems useless? At least 2 times...fixed now?
				// Do Aggregate after Compute, so the delegate can use info from newColumn to decide if to calc the agg.
				newColumn.Aggregates = _aggregateFunction(groupRows);//, newColumn);//, agg_ctx);

				newColumn.Children = ComputeColumns(newColumn, groupRows, columnFunctionsCopy);//, agg_ctx);

				columns.Add(newColumn);
			}

			if (field.SortOrder == SortOrder.Asc)
			{
				columns = columns.OrderBy(c => c.Value, field.SortComparer).ToList();
			}
			else if (field.SortOrder == SortOrder.Desc)
			{
				columns = columns.OrderByDescending(c => c.Value, field.SortComparer).ToList();
			}

			return columns;
		}
	}

	public interface IGroup<TRow, TAgg>
	{
		object? Value { get; }
		TAgg Aggregates { get; }
		IEnumerable<IGroup<TRow, TAgg>> Children { get; }

		IGroup<TRow, TAgg>? Parent { get; }

		IField<TRow> Field { get; }
	}

	public class Row<TRow, TAgg> : IGroup<TRow, TAgg>
	{
		internal Row() { }
		public object? Value { get; set; }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TRow, TAgg>> ColumnAggregates { get; set; }
		public IEnumerable<Row<TRow, TAgg>> Children { get; set; }

		public Row<TRow, TAgg>? Parent { get; set; }

		public IField<TRow> Field { get; set; }

		IEnumerable<IGroup<TRow, TAgg>> IGroup<TRow, TAgg>.Children => Children;
		IGroup<TRow, TAgg>? IGroup<TRow, TAgg>.Parent => Parent;
	}

	public class Column<TRow, TAgg> : IGroup<TRow, TAgg>
	{
		internal Column() { }
		public object? Value { get; set; }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TRow, TAgg>> Children { get; set; }

		public Column<TRow, TAgg>? Parent { get; set; }

		public IField<TRow> Field { get; set; }

		IEnumerable<IGroup<TRow, TAgg>> IGroup<TRow, TAgg>.Children => Children;
		IGroup<TRow, TAgg>? IGroup<TRow, TAgg>.Parent => Parent;
	}

	// This one is kind of similar to a row and share all 3 things with a row: Aggregates, ColumnAggregates and Rows (Children)
	// Could this be extracted into an iface? Or could a row inherit PivotTable?
	// So its kind of weid that the table is kind of a row?
	public class PivotTable<TRow, TAgg>
	{
		internal PivotTable() { }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TRow, TAgg>> ColumnAggregates { get; set; }
		public IEnumerable<Row<TRow, TAgg>> Rows { get; set; }
	}


}
