
namespace osexpert.PivotTable
{
	public enum RootType
	{
		NotRoot = 0,
		Row = 1,
		Col = 2,
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

		public Field Field = null!;

		/// <summary>
		/// 
		/// </summary>
		public Area FieldType
		{
			get
			{
				if (Field != null)
					return Field.Area;
				if (RootType == RootType.Col)
					return Area.Column;
				if (RootType == RootType.Row)
					return Area.Row;
				throw new Exception("Invalid: neither Field not IsRoot is set correctly");
			}
		}

		public Group<TRow>? ParentGroup;

		public Dictionary<Group<TRow>, object?[]> IntersectData { get; internal set; } = null!;

		internal object? GetKeyByField(Field colField)
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

		//internal IEnumerable<Group<T>> GetParents()
		//{
		//	throw new NotImplementedException();
		//}

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

