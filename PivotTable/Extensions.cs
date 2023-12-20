
namespace osexpert.PivotTable
{
	public static class Extensions
	{
		public static IEnumerable<T> Yield<T>(this T t)
		{
			// Alternative: return new[] { t };
			yield return t;
		}
	}
}
