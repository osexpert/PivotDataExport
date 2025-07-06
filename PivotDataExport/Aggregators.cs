
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("Tests")]
[assembly: InternalsVisibleToAttribute("Examples")]

namespace PivotDataExport
{
	public static class Aggregators
	{
		//public static string CommaList<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value)
		//{
		//	if (GetCountZeroOrOneAndSingle(rows, out var count, out var single))
		//		return count == 0 ? "" : value(single!);

		//	return string.Join(", ", rows.Select(value).Distinct().OrderBy(v => v));
		//}

		//public static string CommaList(IEnumerable<string> values)//, string noValue = "", string separator = ", ")
		//{
		//	if (GetCountZeroOrOneAndSingle(values, out var count, out var single))
		//		return count == 0 ? "" : single!;

		//	return string.Join(", ", values.Distinct().OrderBy(v => v));
		//}

		//public static string CommaList(IEnumerable<int> values)//, string noValue = "", string separator = ", ")
		//{
		//	if (GetCountZeroOrOneAndSingle(values, out var count, out var single))
		//		return count == 0 ? "" : single!.ToString();

		//	return string.Join(", ", values.Distinct().OrderBy(v => v));
		//}

		public static string CommaList<T>(IEnumerable<T> values)//, string noValue = "", string separator = ", ")
		{
			if (GetCountZeroOrOneAndSingle(values, out var count, out var single))
				return count == 0 ? "" : single!.ToString();

			return string.Join(", ", values.Distinct().OrderBy(v => v));
		}

		//public static string SingleOrCount(IEnumerable<string> rows)
		//	=> SingleOr(rows, rows => $"Count: {rows.Count()}");

		//public static string SingleOr(IEnumerable<string> rows, string orValue)
		//	=> SingleOr(rows, _ => orValue);

		//public static string SingleOr(IEnumerable<string> rows, Func<IEnumerable<string>, string> orValue)
		//{
		//	if (GetCountZeroOrOneAndSingle(rows, out var count, out var single))
		//		return count == 0 ? "" : single!;

		//	return orValue(rows);
		//}

		//public static double AverageOr<TRow>(IEnumerable<TRow> rows, Func<TRow, double> value, Func<IEnumerable<TRow>, double> orValue)
		//{
		//	if (!rows.Any())
		//		return orValue(rows);
		//	else
		//		return rows.Average(value);
		//}

		//public static double AverageOr<TRow>(IEnumerable<TRow> vals, double orValue)
		//{
		//	if (!vals.Any())
		//		return orValue;
		//	else
		//		return vals.Average();
		//}

		/// <summary>
		/// return count 0, 1 or null
		/// if Count is 1, a single is also returned
		/// </summary>
		public static bool GetCountZeroOrOneAndSingle<TRow>(IEnumerable<TRow> rows, out int count, out TRow? single)
		{
			if (TryGetCountWithoutEnumerating(rows, out count, out single) && count <= 1)
			{
				return true;
			}

			// https://stackoverflow.com/a/6059711/2671330
			int constrainedCount = rows.Take(2).Count();
			if (constrainedCount <= 1)
			{
				count = constrainedCount;

				if (constrainedCount == 1)
					single = rows.Single();

				return true;
			}

			return false;
		}

		public static bool TryGetCountWithoutEnumerating<TSource>(this IEnumerable<TSource> source, out int count, out TSource? single)
		{
#if NET6_0_OR_GREATER
			// System.Linq will throw ArgumentNullException if necessary
			return source.TryGetNonEnumeratedCount(out count);
#else
			single = default;

			switch (source)
			{
				case null:
					throw new ArgumentNullException(nameof(source));
				case ICollection<TSource> genericCollection:
					count = genericCollection.Count;
					if (count == 1)
					{
						var arr = new TSource[1];
						genericCollection.CopyTo(arr, 0);
						single = arr[0];
					}
					return true;
				case ICollection collection:
					count = collection.Count;
					if (count == 1)
					{
						var arr = new TSource[1];
						collection.CopyTo(arr, 0);
						single = arr[0];
					}
					return true;
				default:
					count = 0;
					return false;
			}
#endif
		}
	}
}
