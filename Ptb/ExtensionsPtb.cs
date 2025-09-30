
using System.Buffers.Text;
using System.Data;
using System.Runtime.CompilerServices;

namespace PivotDataExport;

public static class ExtensionsPtb
{
	internal static IEnumerable<T> TopogicalSequenceDFS<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> deps)
	{
		var yielded = new HashSet<T>();
		var visited = new HashSet<T>();
		var stack = new Stack<(T, IEnumerator<T>)>();

		foreach (T t in source)
		{
			if (visited.Add(t))
				stack.Push((t, deps(t).GetEnumerator()));

			while (stack.Any())
			{
				var p = stack.Peek();
				bool depPushed = false;
				while (p.Item2.MoveNext())
				{
					var curr = p.Item2.Current;
					if (visited.Add(curr))
					{
						stack.Push((curr, deps(curr).GetEnumerator()));
						depPushed = true;
						break;
					}
					else if (!yielded.Contains(curr))
						throw new Exception("cycle");
				}

				if (!depPushed)
				{
					p = stack.Pop();
					if (!yielded.Add(p.Item1))
						throw new Exception("bug");
					yield return p.Item1;
				}
			}
		}
	}

}
