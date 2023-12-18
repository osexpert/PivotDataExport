using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osexpert.PivotTable
{
	public enum enRootType
	{
		NotRoot = 0,
		Row = 1,
		Col = 2,
	}

	public class Group<T> where T : class
	{
		/// <summary>
		/// If true, not a group but all the rows
		/// </summary>
		public enRootType RootType;

		public bool IsRoot => RootType != enRootType.NotRoot;

		// I assume this is not set if the group doesn't have IntersectData?
		public object? Key; // data or display? this is raw value. the field funcs decide the groupings via funcs.

		//public IEnumerable<Group<T>> Groups;

		public IEnumerable<T> Rows;

		public Field Field;

		/// <summary>
		/// 
		/// </summary>
		public FieldType FieldType
		{
			get
			{
				if (Field != null)
					return Field.FieldType;
				if (RootType == enRootType.Col)
					return FieldType.ColGroup;
				if (RootType == enRootType.Row)
					return FieldType.RowGroup;
				throw new Exception("Invalid: neither Field not IsRoot is set correctly");
			}
		}

		public Group<T>? ParentGroup;

		public Dictionary<Group<T>, object?[]> IntersectData { get; internal set; }

		internal object? GetKeyByField(Field colField)
		{
			var current = this;
			do
			{
				if (current.Field == colField)
					return current.Key;
				current = current.ParentGroup;

			} while (current != null);

			throw new Exception("Bug");
		}

		//internal IEnumerable<Group<T>> GetParents()
		//{
		//	throw new NotImplementedException();
		//}

		/// <summary>
		/// Get top parent first and me last
		/// </summary>
		internal IEnumerable<Group<T>> GetParentsAndMe()
		{
			if (this.IsRoot)
			{
				return Enumerable.Empty<Group<T>>();
			}

			var st = new Stack<Group<T>>();

			var current = this;
			do
			{
				st.Push(current);
				current = current.ParentGroup;
			} while (!current.IsRoot);

			return st;
		}
	}
}
