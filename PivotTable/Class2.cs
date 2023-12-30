//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace osexpert.PivotTable
//{
//	public static class Class2
//	{
//		public static dynamic[] ToPivotArray(
//this IEnumerable source,
//		Func columnSelector,
//Expression<Func> rowSelector,
//Func<IEnumerable, TData> dataSelector)
//		{

//			var arr = new List();
//			var cols = new List();
//			var array = new List();
//			// String rowName = ((MemberExpression)rowSelector.Body).Member.Name;
//			var rowsName = ((NewExpression)rowSelector.Body).Members.Select(s => s).ToList();
//			foreach (var row in rowsName)
//			{
//				var name = row.Name;
//				array.Add(name);
//				// table.Columns.Add(new DataColumn(name));
//			}
//			array.ToArray();
//			var columns = source.Select(columnSelector).Distinct();

//			// cols =(new []{ rowName}).Concat(columns.Select(x=>x.ToString())).ToList();

//			cols = (array).Concat(columns.Select(x => x.ToString())).ToList();

//			var rows = source.GroupBy(rowSelector.Compile())
//			.Select(rowGroup => new
//			{
//				Key = rowGroup.Key,
//				Values = columns.GroupJoin(
//			rowGroup,
//			c => c,
//			r => columnSelector(r),
//			(c, columnGroup) => dataSelector(columnGroup))
//			}).ToArray();

//			foreach (var row in rows)
//			{
//				var items = row.Values.Cast().ToList();

//				string[] keyRow = row.Key.ToString().Split(‘,’);
//				int index = 0;
//				foreach (var key in keyRow)
//				{
//					string keyValue = key.Replace(“}”, “”).Split(‘=’)[1].Trim();
//				items.Insert(index, keyValue);
//				index++;
//			}

//			// items.Insert(0, row.Key);
//			var obj = GetAnonymousObject(cols, items);
//			arr.Add(obj);
//		}
//return arr.ToArray();
//}

//	private static dynamic GetAnonymousObject(IEnumerable columns, IEnumerable values)
//	{
//		IDictionary eo = new ExpandoObject() as IDictionary;
//		int i;
//		for (i = 0; i < columns.Count(); i++)
//		{
//			eo.Add(columns.ElementAt(i), values.ElementAt(i));
//		}
//		return eo;
//	}
//	var pivotArray = attendances.ToPivotArray(
//	item => item.CurrentDate.Day,
//	item => new { item.ProjectId, item.ResourceId },
//	items => items.Any() ? items.Sum(x => x.DailyEffort) : 0);
//}
//}
