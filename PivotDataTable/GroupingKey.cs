#if false

namespace PivotDataTable
{
	public class GroupingKey<T> : IEquatable<GroupingKey<T>>
	{
		public T[] Groups { get; init; }
		static EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

		public GroupingKey(T[] groups)
		{
			Groups = groups;
		}

		public override int GetHashCode()
		{
			var hc = new HashCode();
			foreach (var g in Groups)
				hc.Add(g);
			return hc.ToHashCode();
		}

		public override bool Equals(object? other)
		{
			return Equals(other as GroupingKey<T>);
		}

		public bool Equals(GroupingKey<T>? other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (Groups.Length != other.Groups.Length)
				return false;

			for (int i = 0; i < Groups.Length; i++)
			{
				if (!equalityComparer.Equals(Groups[i], other.Groups[i]))
					return false;
			}

			return true;
		}

		public override string ToString()
		{
			string[] array = new string[Groups.Length];
			for (int i = 0; i < Groups.Length; i++)
				array[i] = $"Group{i} = {Groups[i]}";

			return $"{{ {string.Join(", ", array)} }}";
		}
	}
}

#endif