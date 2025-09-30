
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
		if (GetCountZeroOrOne(values, out var count))
		{
			return count switch
			{
				0 => "",
				1 => values.First()?.ToString() ?? "",
				_ => throw new InvalidOperationException($"Unexpected count: {count}")
			};
		}

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
	/// Returns true if number of values are 0 or 1.
	/// </summary>
	public static bool GetCountZeroOrOne<TRow>(IEnumerable<TRow> rows, out int count)
	{
#if NET6_0_OR_GREATER
		if (source.TryGetNonEnumeratedCount(out count))
		{
			return count <= 1;
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
