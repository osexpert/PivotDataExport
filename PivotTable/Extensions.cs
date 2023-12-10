using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
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
