
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("Tests")]
[assembly: InternalsVisibleToAttribute("Examples")]

namespace PivotDataExport;

public static class Aggregators
{
	public static string CommaList<T>(IEnumerable<T> values)//, string noValue = "", string separator = ", ")
	{
		if (GetCountZeroOrOneFaster(values, out var count))
			return count == 0 ? "" : values.First()?.ToString() ?? "";

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
	public static bool GetCountZeroOrOneFaster<TRow>(IEnumerable<TRow> rows, out int count)
	{
#if NET6_0_OR_GREATER
		// System.Linq will throw ArgumentNullException if necessary
		if (source.TryGetNonEnumeratedCount(out count) && count <= 1)
		{
			return true;
		}
#endif

		// https://stackoverflow.com/a/6059711/2671330
		int constrainedCount = rows.Take(2).Count();
		if (constrainedCount <= 1)
		{
			count = constrainedCount;
			return true;
		}

		count = 0;
		return false;
	}
}
