
using System.Data;

namespace PivotDataTable
{
	public static class Extensions
	{
		public static IEnumerable<T> Yield<T>(this T t)
		{
			// Alternative: return new[] { t };
			yield return t;
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
	}
}
