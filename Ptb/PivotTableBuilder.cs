#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PivotDataExport;

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
internal class PivotTableBuilder<TRow, TAgg>
	   where TRow : class
{
	private readonly IList<(Func<TRow, object?>, Field<TRow>)> _rowFunctions;
	private readonly IList<(Func<TRow, object?>, Field<TRow>)> _columnFunctions;
	private readonly Func<IEnumerable<TRow>, TAgg> _aggregateFunction;
	private readonly IEnumerable<TRow> _list;

	internal PivotTableBuilder(IEnumerable<TRow> list, Func<IEnumerable<TRow>, TAgg> aggregateFunction)
	{
		_list = list;
		_aggregateFunction = aggregateFunction;
		_rowFunctions = new List<(Func<TRow, object?>, Field<TRow>)>();
		_columnFunctions = new List<(Func<TRow, object?>, Field<TRow>)>();
	}
	public PivotTableBuilder<TRow, TAgg> AddRow((Func<TRow, object?>, Field<TRow>) rowFunction)
	{
		_rowFunctions.Add(rowFunction);
		return this;
	}

	public PivotTableBuilder<TRow, TAgg> AddColumn((Func<TRow, object?>, Field<TRow>) columnFunction)
	{
		_columnFunctions.Add(columnFunction);
		return this;
	}

	public PivotTable<TRow, TAgg> Build()
	{
		var pivotTable = new PivotTable<TRow, TAgg>();

		//compute aggregates for the whole table
		pivotTable.Aggregates = _aggregateFunction(_list);
		pivotTable.ColumnAggregates = ComputeColumns(null, _list, _columnFunctions);
		pivotTable.Rows = ComputeRows(null, _list, _rowFunctions, _columnFunctions);

		return pivotTable;
	}

	private List<Row<TRow, TAgg>> ComputeRows(Row<TRow, TAgg>? parent, IEnumerable<TRow> list,
		IEnumerable<(Func<TRow, object?>, Field<TRow>)> rowFunctions,
		IEnumerable<(Func<TRow, object?>, Field<TRow>)> columnFunctions)
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
			newRow.Aggregates = _aggregateFunction(groupRows);

			newRow.Children = ComputeRows(newRow, groupRows, rowFunctionsCopy, columnFunctions);

			newRow.ColumnAggregates = ComputeColumns(null /* hmm...maybe the newRow is the parent here?? in case, parent must be IGroup? */, groupRows, _columnFunctions);

			rows.Add(newRow);
		}
		
		if (field.SortOrder == SortOrder.Ascending)
		{
			rows = rows.OrderBy(c => field.GetSortValue(c.Value), field.SortComparer).ToList();
		}
		else if (field.SortOrder == SortOrder.Descending)
		{
			rows = rows.OrderByDescending(c => field.GetSortValue(c.Value), field.SortComparer).ToList();
		}

		return rows;
	}

	private List<Column<TRow, TAgg>> ComputeColumns(Column<TRow, TAgg>? parent, IEnumerable<TRow> list,
		IEnumerable<(Func<TRow, object?>, Field<TRow>)> columnFunctions)
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
			newColumn.Aggregates = _aggregateFunction(groupRows);//, newColumn);

			newColumn.Children = ComputeColumns(newColumn, groupRows, columnFunctionsCopy);

			columns.Add(newColumn);
		}

		if (field.SortOrder == SortOrder.Ascending)
		{
			columns = columns.OrderBy(c => field.GetSortValue(c.Value), field.SortComparer).ToList();
		}
		else if (field.SortOrder == SortOrder.Descending)
		{
			columns = columns.OrderByDescending(c => field.GetSortValue(c.Value), field.SortComparer).ToList();
		}

		return columns;
	}
}

public interface IGroup<TRow, TAgg> where TRow : class
{
	object? Value { get; }
	TAgg Aggregates { get; }
	IEnumerable<IGroup<TRow, TAgg>> Children { get; }

	IGroup<TRow, TAgg>? Parent { get; }

	Field<TRow> Field { get; }
}

internal class Row<TRow, TAgg> : IGroup<TRow, TAgg> where TRow : class
{
	internal Row() { }
	public object? Value { get; set; }
	public TAgg Aggregates { get; set; }
	public IEnumerable<Column<TRow, TAgg>> ColumnAggregates { get; set; }
	public IEnumerable<Row<TRow, TAgg>> Children { get; set; }

	public Row<TRow, TAgg>? Parent { get; set; }

	public Field<TRow> Field { get; set; }

	IEnumerable<IGroup<TRow, TAgg>> IGroup<TRow, TAgg>.Children => Children;
	IGroup<TRow, TAgg>? IGroup<TRow, TAgg>.Parent => Parent;
}

internal class Column<TRow, TAgg> : IGroup<TRow, TAgg> where TRow : class
{
	internal Column() { }
	public object? Value { get; set; }
	public TAgg Aggregates { get; set; }
	public IEnumerable<Column<TRow, TAgg>> Children { get; set; }

	public Column<TRow, TAgg>? Parent { get; set; }

	public Field<TRow> Field { get; set; }

	IEnumerable<IGroup<TRow, TAgg>> IGroup<TRow, TAgg>.Children => Children;
	IGroup<TRow, TAgg>? IGroup<TRow, TAgg>.Parent => Parent;
}

// This one is kind of similar to a row and share all 3 things with a row: Aggregates, ColumnAggregates and Rows (Children)
// Could this be extracted into an iface? Or could a row inherit PivotTable?
// So its kind of weid that the table is kind of a row?
internal class PivotTable<TRow, TAgg> where TRow : class
{
	internal PivotTable() { }
	public TAgg Aggregates { get; set; }
	public IEnumerable<Column<TRow, TAgg>> ColumnAggregates { get; set; }
	public IEnumerable<Row<TRow, TAgg>> Rows { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.