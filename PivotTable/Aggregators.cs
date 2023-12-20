
namespace osexpert.PivotTable
{
	public static class Aggregators
	{
		public static string CommaList<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value)
		{
			// https://stackoverflow.com/a/6059711/2671330
			int constrainedCount = rows.Take(2).Count();
			if (constrainedCount == 0)
				return "";
			else if (constrainedCount == 1)
				return value(rows.Single());
			else
				return string.Join(", ", rows.Select(value).Distinct().OrderBy(v => v));
		}

		public static string SingleOrCount<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value)
			=> SingleOr(rows, value, rows => $"Count: {rows.Count()}");

		public static string SingleOr<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value, string orValue)
			=> SingleOr(rows, value, _ => orValue);

		public static string SingleOr<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value, Func<IEnumerable<TRow>, string> orValue)
		{
			int constrainedCount = rows.Take(2).Count();
			if (constrainedCount == 0)
				return "";
			else if (constrainedCount == 1)
				return value(rows.Single());
			else
				return orValue(rows);
		}
		public static double AverageOr<TRow>(IEnumerable<TRow> rows, Func<TRow, double> value, Func<IEnumerable<TRow>, double> orValue)
		{
			if (!rows.Any())
				return orValue(rows);
			else
				return rows.Average(value);
		}
	}
}
