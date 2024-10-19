
using System.Buffers.Text;
using System.Data;
using System.Runtime.CompilerServices;

namespace PivotDataExport
{
	public static class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Yield<T>(this T t)
		{
			// Alternative: return new[] { t };
			//yield return t;
			return new[] { t };
		}

		public static string ToXml(this DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXml(writer);
				writer.Flush();

				return writer.GetStringBuilder().ToString();
			}
		}

		public static IEnumerable<TResult> ZipForceEqual<TFirst, TSecond, TResult>(
			this IEnumerable<TFirst> first,
			IEnumerable<TSecond> second,
			Func<TFirst, TSecond, TResult> resultSelector)
		{
			if (first == null) throw new ArgumentNullException("first");
			if (second == null) throw new ArgumentNullException("second");
			if (resultSelector == null) throw new ArgumentNullException("resultSelector");

			return ZipForceEqualImpl(first, second, resultSelector);
		}

		static IEnumerable<TResult> ZipForceEqualImpl<TFirst, TSecond, TResult>(
			IEnumerable<TFirst> first,
			IEnumerable<TSecond> second,
			Func<TFirst, TSecond, TResult> resultSelector)
		{
			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
			{
				while (e1.MoveNext())
				{
					if (e2.MoveNext())
					{
						yield return resultSelector(e1.Current, e2.Current);
					}
					else
					{
						throw new InvalidOperationException("Sequences differed in length");
					}
				}
				if (e2.MoveNext())
				{
					throw new InvalidOperationException("Sequences differed in length");
				}
			}
		}

		public static IEnumerable<T> TopogicalSequenceDFS<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> deps)
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
}
