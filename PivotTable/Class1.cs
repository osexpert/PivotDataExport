using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace osexpert.PivotTable
{
	public static class Ext
	{
		public static dynamic[] ToPivotArray<T, TColumn, TRow, TData>(
	this IEnumerable<T> source,
	Func<T, TColumn> columnSelector,
	Expression<Func<T, TRow>> rowSelector,
	Func<IEnumerable<T>, TData> dataSelector)
		{

			var arr = new List<object>();
			var cols = new List<string>();
			//String rowName = ((MemberExpression)rowSelector.Body).Member.Name;
			var rowsName = ((NewExpression)rowSelector.Body).Members.Select(s => s.Name);//.ToArray();
			var columns = source.Select(columnSelector).Distinct();

			cols = rowsName.Concat(columns.Select(x => x.ToString())).ToList();


			var rows = source.GroupBy(rowSelector.Compile())
							 .Select(rowGroup => new
							 {
								 Key = rowGroup.Key,
								 Values = columns.GroupJoin(
									 rowGroup,
									 c => c,
									 r => columnSelector(r),
									 (c, columnGroup) => dataSelector(columnGroup))
							 }).ToArray();


			foreach (var row in rows)
			{
				var items = row.Values.Cast<object>().ToList();

				//items.Insert(0, row.Key);

				string[] keyRow = row.Key.ToString().Split(",");
				int index = 0;
				foreach (var key in keyRow)
				{
					string keyValue = key.Replace("}", "").Split("=")[1].Trim();
					items.Insert(index, keyValue);
					index++;
				}


				var obj = GetAnonymousObject(cols, items);
				arr.Add(obj);
			}
			return arr.ToArray();
		}
		private static dynamic GetAnonymousObject(IEnumerable<string> columns, IEnumerable<object> values)
		{
			IDictionary<string, object> eo = new ExpandoObject() as IDictionary<string, object>;
			int i;
			for (i = 0; i < columns.Count(); i++)
			{
				eo.Add(columns.ElementAt<string>(i), values.ElementAt<object>(i));
			}
			return eo;
		}
	}
}