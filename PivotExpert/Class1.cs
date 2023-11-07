#define WRITE_OA

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using NotVisualBasic;
using NotVisualBasic.FileIO;
using static PivotExpert.Pivoter;


namespace PivotExpert
{
	//	Region,              Country,Item Type,   Sales Channel,Order Priority, Order Date,Order ID, Ship Date,Units Sold, Unit Price,Unit Cost, Total Revenue,Total Cost, Total Profit
	//Australia and Oceania, Palau, Office Supplies,Online,     H,              3/6/2016,517073523,  3/26/2016,2401,651.21,524.96,1563555.21,1260428.96,303126.25
	public class CsvRow
	{
		[Index(0)]
		public string Region { get; set; }
		[Index(1)]
		public string Country { get; set; }
		[Index(2)]
		public string ItemType { get; set; }
		[Index(3)]
		public string SalesChannel { get; set; }
		[Index(4)]
		public string OrderPriority { get; set; }
		[Index(5)]
		public DateTime OrderDate { get; set; }
		[Index(6)]
		public string OrderID { get; set; }
		[Index(7)]
		public DateTime ShipDate { get; set; }
		[Index(8)]
		public long UnitsSold { get; set; }
		[Index(9)]
		public double UnitPrice { get; set; }
		[Index(10)]
		public double UnitCost { get; set; }
		[Index(11)]
		public double TotalRevenue { get; set; }
		[Index(12)]
		public double TotalCost { get; set; }
		[Index(13)]
		public double TotalProfit { get; set; }
		
	}



	public class TableColumn
	{
		public string Name { get; set; }

		[JsonIgnore]
		public Type DataType { get; set; }

		public string TypeName => DataType.Name;

		public FieldType FieldType { get; set; }
		public int GroupIndex { get; set; }

		public Sorting Sorting { get; set; }
		public int SortIndex { get; set; }

		public object?[]? GroupValues { get; set; }
	}



	public class Pivoter
	{
		public static void Main()
		{
			Version v = null;

			string fff = "" + v;

			var t = new Pivoter();
			t.Test();
		}

		public void Test()
		{

			//using (var f = File.Open(@"d:\testwrite.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, listtt, new JsonSerializerOptions { WriteIndented = true });
			//}

			//var datas = new CsvTextFieldParser(@"d:\5m Sales Records.csv");

			//while (!datas.EndOfData)
			//{
			//	var fields = datas.ReadFields();
			//}

			List<CsvRow> allRTows = null;

			using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var records = csv.GetRecords<CsvRow>();

				allRTows = records.ToList();
			}

			//var props = TypeDescriptor.GetProperties(typeof(CsvRow));

			var fieldsss = CreateFieldsFromType<CsvRow>();// (props);



			// TODO: Should maybe had a way to set index after all? That was independent of order by ienumerable?
	//		MoveToTop(fieldsss, "OrderDate");
//			MoveToTop(fieldsss, "ItemType");
			//MoveToTop(fieldsss, "OrderID");
			//MoveToTop(fieldsss, "ItemType");

			GetField(fieldsss, "Region").FieldType = FieldType.RowGroup;
			GetField(fieldsss, "Region").GroupIndex = 1;
			GetField(fieldsss, "Region").Sorting = Sorting.Asc;
			GetField(fieldsss, "Region").SortIndex = 1;

			GetField(fieldsss, "Country").FieldType = FieldType.RowGroup;
			GetField(fieldsss, "Country").GroupIndex = 2;
			GetField(fieldsss, "Country").Sorting = Sorting.Asc;
			GetField(fieldsss, "Country").SortIndex = 2;

			GetField(fieldsss, "SalesChannel").FieldType = FieldType.ColGroup;
			GetField(fieldsss, "SalesChannel").Sorting = Sorting.Asc;
			GetField(fieldsss, "SalesChannel").SortIndex = 1;
			GetField(fieldsss, "SalesChannel").GroupIndex = 3;

			GetField(fieldsss, "ItemType").FieldType = FieldType.ColGroup;
			GetField(fieldsss, "ItemType").Sorting = Sorting.Asc;
			GetField(fieldsss, "ItemType").SortIndex = 0;
			GetField(fieldsss, "ItemType").GroupIndex = 0;

			//GetField(fieldsss, "Country").Area = Area.Group;
			//GetField(fieldsss, "Country").Sort = Sort.Asc;

			//GetField(fieldsss, "ShipDate").Sort = Sort.Desc;

			fieldsss.Add(new Field { FieldType = FieldType.Data, FieldName = "RowCount", Sorting = Sorting.None, DataType = typeof(int), SortIndex = 0 });

			var props = new List<PropertyDescriptor>();

			props.Add(new Column<CsvRow, string>(nameof(CsvRow.Region), rows => Aggregators.CommaList(rows, row => row.Region)));
			props.Add(new Column<CsvRow, string>(nameof(CsvRow.Country), rows => Aggregators.CommaList(rows, row => row.Country)));
			props.Add(new Column<CsvRow, string>(nameof(CsvRow.ItemType), rows => Aggregators.CommaList(rows, row => row.ItemType)));
			props.Add(new Column<CsvRow, string>(nameof(CsvRow.SalesChannel), rows => Aggregators.CommaList(rows, row => row.SalesChannel)));
			props.Add(new Column<CsvRow, string>(nameof(CsvRow.OrderPriority), rows => Aggregators.CommaList(rows, row => row.OrderPriority)));
			props.Add(new Column<CsvRow, DateTime>(nameof(CsvRow.OrderDate), rows => rows.Max(r => r.OrderDate)));
			props.Add(new Column<CsvRow, string>(nameof(CsvRow.OrderID), rows => Aggregators.SingleOrCount(rows, row => row.OrderID)));
			props.Add(new Column<CsvRow, int>("RowCount", rows => rows.Count()));
			props.Add(new Column<CsvRow, DateTime>(nameof(CsvRow.ShipDate), rows => rows.Max(r => r.ShipDate)));
			props.Add(new Column<CsvRow, long>(nameof(CsvRow.UnitsSold), rows => rows.Sum(r => r.UnitsSold)));
			props.Add(new Column<CsvRow, double>(nameof(CsvRow.UnitPrice), rows => rows.Sum(r => r.UnitPrice)));
			props.Add(new Column<CsvRow, double>(nameof(CsvRow.UnitCost), rows => rows.Sum(r => r.UnitCost)));
			props.Add(new Column<CsvRow, double>(nameof(CsvRow.TotalRevenue), rows => rows.Sum(r => r.TotalRevenue)));
			props.Add(new Column<CsvRow, double>(nameof(CsvRow.TotalCost), rows => rows.Sum(r => r.TotalCost)));
			props.Add(new Column<CsvRow, double>(nameof(CsvRow.TotalProfit), rows => rows.Sum(r => r.TotalProfit)));

			var sw = Stopwatch.StartNew();

//			TypeValue: object, name, fullname

			var pp = new Pivoter<CsvRow>(fieldsss, allRTows, new PropertyDescriptorCollection(props.ToArray()));

			var tblll = pp.GetTableFastIntersect_DictArrNEsted();

			using (var f = File.Open(@"d:\testdt5mill2_fast_nested_min.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tblll, new JsonSerializerOptions { WriteIndented = true });
			}

			var fast = pp.GetTableFastIntersect();
			var slow = pp.GetTableSlowIntersect();

			var tbl = pp.GetTableFastIntersect_DictArr();
			
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tbl, new JsonSerializerOptions { WriteIndented = true });
			}

			var datat = pp.GetDataTable();

			datat.WriteXml(@"d:\testdt5mill2_fast.xml");

			var dt = pp.GetTableSlowIntersect_objectArr();
//			dt.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_slow.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dt, new JsonSerializerOptions { WriteIndented=true});
			}

			dt = null;

			var dtF = pp.GetTableFastIntersect_objectArr();
	//		dtF.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			//sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dtF, new JsonSerializerOptions { WriteIndented = true });
			}

			// 37 sec without DT or object arrays
			// 35 sec with DT??
			// 186 rows

			// 58sec,  3.6GB, Count = 2097153
			// DT: 2min,  5.4GB, Count = 2097153
			// SLOW: 4.37, 4GB

			//var pdc = new PropertyDescriptorCollection(new PropertyDescriptor[]
			//{
			//	new SiteNameCol(),
			//	new UnitNameCol(),
			//	new SpeciesNameCol().Col,
			//	new IndCountCol().Col
			//});
			
			//var list = new Rows<Row>(pdc);

			// TODO: optin: RowNumber auto column? 1 to n?
			// TODO: optin: top row fieldnames? (or caption?)

			//list.Add(new Row() { IndCount = 11, SiteName = "S1", UnitName = "U1", SpecName = "Frog"});
			//list.Add(new Row() { IndCount = 13, SiteName = "S1", UnitName = "U1", SpecName = "Hog" });
			//list.Add(new Row() { IndCount = 22, SiteName = "S1", UnitName = "U2", SpecName = "Bird" });
			//list.Add(new Row() { IndCount = 33, SiteName = "S2", UnitName = "U1", SpecName = "Dog" });
			//list.Add(new Row() { IndCount = 44, SiteName = "S3", UnitName = "U1", SpecName = "Human" });
			//list.Add(new Row() { IndCount = 111, SiteName = "S1", UnitName = "U11", SpecName = "Frog" });
			//list.Add(new Row() { IndCount = 222, SiteName = "S1", UnitName = "U22", SpecName = "Bird" });
			//list.Add(new Row() { IndCount = 333, SiteName = "S2", UnitName = "U11", SpecName = "Dog" });
			//list.Add(new Row() { IndCount = 444, SiteName = "S3", UnitName = "U11", SpecName = "Human" });


			var siteF = new Field<string>() { FieldType = FieldType.Data, FieldName = "SiteName", Sorting = Sorting.Asc, SortIndex = 0 };

			

//			var unitF = new FieldGen<string>() { Area = Area.Group, FieldName = "UnitName"  };
			var specF = new Field<string>() { FieldType = FieldType.Data, FieldName = "SpeciesName", Sorting = Sorting.Desc, SortIndex = 1 };
			var indF = new Field<int>() { FieldType = FieldType.Data, FieldName = "IndCount" };
			var ff = new Field[] { specF,   indF, siteF };

			//var p = new Pivoter<Row>(ff, list, new PropertyDescriptorCollection(props.ToArray()));
			//p.GetTable();
			// TODO: dt can be slow? add option to use different construct? and then need different sorting?
		}



		static class Aggregators
		{
			public static string CommaList<TRow>(IEnumerable<TRow> rows, Func<TRow, string> value)
			{
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
		}

		private List<Field> CreateFieldsFromType<T>()
		{
			return typeof(T).GetProperties().Select(pd => new Field { FieldType = FieldType.Data, FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}

		private Field GetField(List<Field> fieldsss, string v)
		{
			return fieldsss.Where(f => f.FieldName == v).Single();
		}

		private void MoveToTop(List<Field> fieldsss, string field)
		{
			var sing = fieldsss.Where(f => f.FieldName == field).Single();
			fieldsss.Remove(sing);
			fieldsss.Insert(0, sing);
		}

		private List<Field> CreateFieldsFromProps(PropertyDescriptorCollection props)
		{
			return props.Cast<PropertyDescriptor>().Select(pd => new Field { FieldType = FieldType.Data, FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}




		public class Column<TRow, TProp> : PropertyDescriptor
		{
			//Type _propType;
			readonly Func<IEnumerable<TRow>, TProp> _getValue;

			public Column(string propName, Func<IEnumerable<TRow>, TProp> getValue)
				: base(propName, null)
			{
				//_propType = propType;
				_getValue = getValue;
			}

			public Column(string propName)
				: base(propName, null)
			{
				//_propType = propType;
				_getValue = GetValue;
			}

			public override object? GetValue(object? component)
			{
				//			if (component is RT r)
				//					return GetRowValue(r);
				if (component is IEnumerable<TRow> rows)
				{
					return _getValue(rows);//: GetRowValue(rows);
				}
				else
					throw new Exception("wrong component type");
			}

			protected virtual TProp GetValue(IEnumerable<TRow> rows)
				=> throw new InvalidOperationException("GetValue(IEnumerable<TRow> rows) must be overridden");

			public override Type PropertyType => typeof(TProp);

			public override void ResetValue(object component)
			{
				// Not relevant.
			}

			public override void SetValue(object? component, object? value) => throw new NotImplementedException();

			public override bool ShouldSerializeValue(object component) => true;
			public override bool CanResetValue(object component) => false;

			public override Type ComponentType => typeof(IEnumerable<TRow>);
			public override bool IsReadOnly => true;
		}


		class Rows<T> : List<T>, ITypedList
		{
			PropertyDescriptorCollection _pdc;

			public Rows(PropertyDescriptorCollection pdc)
            {
				_pdc = pdc;
            }

            public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
			{
				return _pdc;
			}

			public string GetListName(PropertyDescriptor[] listAccessors)
			{
				return "";
			}
		}


	}









	public class TableGroup
	{
		public string Name { get; set; }
		//public object DataType { get; set; }

		public object? Value { get; set; }

		//public TableGroup Parent { get; set; }
	}




}
