﻿
namespace PivotDataExport
{
	public enum RootType
	{
		NotRoot = 0,
		Row = 1,
		Column = 2,
	}

	public class Group<TRow> where TRow : class
	{
		/// <summary>
		/// If true, not a group but all the rows
		/// </summary>
		public RootType RootType;

		public bool IsRoot => RootType != RootType.NotRoot;

		// I assume this is not set if the group doesn't have IntersectData?
		public object? Key; // data or display? this is raw value. the field funcs decide the groupings via funcs.

		//public IEnumerable<Group<T>> Groups;

		public IEnumerable<TRow> Rows = null!;

		public Field<TRow> Field = null!;

		/// <summary>
		/// 
		/// </summary>
		public Area FieldType
		{
			get
			{
				if (Field != null)
					return Field.Area;
				if (RootType == RootType.Column)
					return Area.Column;
				if (RootType == RootType.Row)
					return Area.Row;
				throw new Exception("Invalid: neither Field nor RootType is set correctly");
			}
		}

		public Group<TRow>? ParentGroup;

		public Dictionary<Group<TRow>, object?[]> IntersectData { get; internal set; } = null!;

		internal object? GetKeyByField(Field<TRow> colField)
		{
			var current = this;
			do
			{
				if (current.Field == colField)
					return current.Key;
				current = current.ParentGroup;

			} while (current != null);

			throw new Exception($"Bug: field '{colField.Name}' not found");
		}

		/// <summary>
		/// Get top parent first and me last
		/// </summary>
		internal IEnumerable<Group<TRow>> GetParentsAndMe()//bool includeMeIfRoot)
		{
			if (this.IsRoot)
			{
				//if (includeMeIfRoot)
				//	return this.Yield();
				//else
				return Enumerable.Empty<Group<TRow>>();
			}

			var st = new Stack<Group<TRow>>();

			var current = this;
			do
			{
				st.Push(current);
				current = current.ParentGroup!;
			} while (!current.IsRoot);

			return st;
		}
	}
}

