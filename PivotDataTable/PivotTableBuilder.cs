using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PivotDataTable
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
	//	Table,
	//	Row,
	//	Column
	//}

	public class PivotTableBuilder<TRow, TAgg> //: IPivotTableBuilder<TRow, TAggregates>
		   where TRow : class
	{
		private readonly IList<(Func<TRow, object?>, Field)> _rowFunctions;
		private readonly IList<(Func<TRow, object?>, Field)> _columnFunctions;
		private readonly Func<IEnumerable<TRow>, IGroup<TAgg>?, TAgg> _aggregateFunction;
		private readonly IEnumerable<TRow> _list;

		//public bool _calcRootColumnAggregates = true;

		internal PivotTableBuilder(IEnumerable<TRow> list, Func<IEnumerable<TRow>, IGroup<TAgg>?, TAgg> aggregateFunction)
		{
			_list = list;
			_aggregateFunction = aggregateFunction;
			_rowFunctions = new List<(Func<TRow, object?>, Field)>();
			_columnFunctions = new List<(Func<TRow, object?>, Field)>();
		}
		public PivotTableBuilder<TRow, TAgg> AddRow((Func<TRow, object?>, Field) rowFunction)
		{
			_rowFunctions.Add(rowFunction);
			return this;
		}

		public PivotTableBuilder<TRow, TAgg> AddColumn((Func<TRow, object?>, Field) columnFunction)
		{
			_columnFunctions.Add(columnFunction);
			return this;
		}

		public PivotTable<TAgg> Build()
		{
			var pivotTable = new PivotTable<TAgg>();

			//compute aggregates for the whole table
			pivotTable.Aggregates = _aggregateFunction(_list, null);
			pivotTable.ColumnAggregates = ComputeColumns(null, _list, _columnFunctions);
			pivotTable.Rows = ComputeRows(null, _list, _rowFunctions, _columnFunctions);

			return pivotTable;
		}

		private IEnumerable<Row<TAgg>> ComputeRows(Row<TAgg>? parent, IEnumerable<TRow> list,
			IEnumerable<(Func<TRow, object?>, Field)> rowFunctions,
			IEnumerable<(Func<TRow, object?>, Field)> columnFunctions)
		{
			var rows = new List<Row<TAgg>>();
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
				var newRow = new Row<TAgg>();
				newRow.Field = field;
				newRow.Parent = parent;
				newRow.Value = group.Key;
				
				newRow.Children = ComputeRows(newRow, group.ToList(), rowFunctionsCopy, columnFunctions);

				newRow.ColumnAggregates = ComputeColumns(null /* hmm...maybe the newRow is the parent here?? in case, parent must be IGroup? */, group.ToList(), _columnFunctions);

				// ToList seems useless? At least 3 times...
				newRow.Aggregates = _aggregateFunction(group.ToList(), newRow);

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

		private IEnumerable<Column<TAgg>> ComputeColumns(Column<TAgg>? parent, IEnumerable<TRow> list,
			IEnumerable<(Func<TRow, object?>, Field)> columnFunctions)
		{
			var columns = new List<Column<TAgg>>();
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
				var newColumn = new Column<TAgg>();
				newColumn.Field = field;
				newColumn.Parent = parent;
				newColumn.Value = group.Key;
				newColumn.Children = ComputeColumns(newColumn, group.ToList(), columnFunctionsCopy);

				// ToList seems useless? At least 2 times...
				newColumn.Aggregates = _aggregateFunction(group.ToList(), newColumn);

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

	public interface IGroup<TAgg>
	{
		object? Value { get; }
		TAgg Aggregates { get; }
		IEnumerable<IGroup<TAgg>> Children { get; }

		IGroup<TAgg>? Parent { get; }

		Field Field { get; }
	}

	public class Row<TAgg> : IGroup<TAgg>
	{
		internal Row() { }
		public object? Value { get; set; }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TAgg>> ColumnAggregates { get; set; }
		public IEnumerable<Row<TAgg>> Children { get; set; }

		public Row<TAgg>? Parent { get; set; }

		public Field Field { get; set; }

		IEnumerable<IGroup<TAgg>> IGroup<TAgg>.Children => Children;
		IGroup<TAgg>? IGroup<TAgg>.Parent => Parent;
	}

	public class Column<TAgg> : IGroup<TAgg>
	{
		internal Column() { }
		public object? Value { get; set; }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TAgg>> Children { get; set; }

		public Column<TAgg>? Parent { get; set; }

		public Field Field { get; set; }

		IEnumerable<IGroup<TAgg>> IGroup<TAgg>.Children => Children;
		IGroup<TAgg>? IGroup<TAgg>.Parent => Parent;
	}

	public class PivotTable<TAgg>
	{
		internal PivotTable() { }
		public TAgg Aggregates { get; set; }
		public IEnumerable<Column<TAgg>> ColumnAggregates { get; set; }
		public IEnumerable<Row<TAgg>> Rows { get; set; }
	}


}
