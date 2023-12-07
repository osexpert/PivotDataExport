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
using PivotExpert.CsvTest;
//using static PivotExpert.Pivoter;


namespace PivotExpert
{






	public class Program
	{
		public static void Main()
		{


			var t = new Program();
			t.Test();
		}


		//public static void Main()
		//{
		//	Version v = null;

		//	string fff = "" + v;

		//	var t = new Pivoter();
		//	t.Test();
		//}

		public void Test()
		{


			//			select CAST((CAST(cast('27AAF6A9-6531-4B6E-8E9F-B17C74CFE419' as uniqueidentifier) as varbinary(12)) +CAST(42 AS varbinary(4))) AS uniqueidentifier ) AS[ActionID]
			//27AAF6A9 - 6531 - 4B6E - 8E9F - B17C0000002A

			var g = new Guid("27AAF6A9-6531-4B6E-8E9F-B17C74CFE419");
			var b = g.ToByteArray().Take(12).Concat(BitConverter.GetBytes(42)).ToArray();
			var gg = new Guid(b);


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

			var fieldsss = Field.CreateFieldsFromType<CsvRow>();// (props);



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

			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.Region), rows => Aggregators.CommaList(rows, row => row.Region)));
			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.Country), rows => Aggregators.CommaList(rows, row => row.Country)));
			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.ItemType), rows => Aggregators.CommaList(rows, row => row.ItemType)));
			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.SalesChannel), rows => Aggregators.CommaList(rows, row => row.SalesChannel)));
			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.OrderPriority), rows => Aggregators.CommaList(rows, row => row.OrderPriority)));
			props.Add(new PropertyColumn<CsvRow, DateTime>(nameof(CsvRow.OrderDate), rows => rows.Max(r => r.OrderDate)));
			props.Add(new PropertyColumn<CsvRow, string>(nameof(CsvRow.OrderID), rows => Aggregators.SingleOrCount(rows, row => row.OrderID)));
			props.Add(new PropertyColumn<CsvRow, int>("RowCount", rows => rows.Count()));
			props.Add(new PropertyColumn<CsvRow, DateTime>(nameof(CsvRow.ShipDate), rows => rows.Max(r => r.ShipDate)));
			props.Add(new PropertyColumn<CsvRow, long>(nameof(CsvRow.UnitsSold), rows => rows.Sum(r => r.UnitsSold)));
			props.Add(new PropertyColumn<CsvRow, double>(nameof(CsvRow.UnitPrice), rows => rows.Sum(r => r.UnitPrice)));
			props.Add(new PropertyColumn<CsvRow, double>(nameof(CsvRow.UnitCost), rows => rows.Sum(r => r.UnitCost)));
			props.Add(new PropertyColumn<CsvRow, double>(nameof(CsvRow.TotalRevenue), rows => rows.Sum(r => r.TotalRevenue)));
			props.Add(new PropertyColumn<CsvRow, double>(nameof(CsvRow.TotalCost), rows => rows.Sum(r => r.TotalCost)));
			props.Add(new PropertyColumn<CsvRow, double>(nameof(CsvRow.TotalProfit), rows => rows.Sum(r => r.TotalProfit)));

			

			//			TypeValue: object, name, fullname

			var pp = new Pivoter<CsvRow>(allRTows, props, fieldsss);//, new PropertyDescriptorCollection(props.ToArray()));

			var sw = Stopwatch.StartNew();

			var fast = pp.GetGroupedData_FastIntersect();

			sw.Stop();

			sw.Restart();

			var slow = pp.GetGroupedData_SlowIntersect();

			sw.Stop();



			var tblll = new DataPresentor<CsvRow>(fast).GetTable_DictArrNested();

			using (var f = File.Open(@"d:\testdt5mill2_fast_nested_min.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tblll, new JsonSerializerOptions { WriteIndented = true });
			}

			
			var tbl = new DataPresentor<CsvRow>(fast).GetTable_DictArr_WithZip(); // WithZIP???
			
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tbl, new JsonSerializerOptions { WriteIndented = true });
			}

			var datat = new DataPresentor<CsvRow>(fast).GetDataTable();

			datat.WriteXml(@"d:\testdt5mill2_fast.xml");

			var dt = new DataPresentor<CsvRow>(fast).GetTable_objectArr();
//			dt.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_slow.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dt, new JsonSerializerOptions { WriteIndented=true});
			}

			dt = null;

			var dtF = new DataPresentor<CsvRow>(fast).GetTable_objectArr();
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





		private Field GetField(IEnumerable<Field> fieldsss, string v)
		{
			return fieldsss.Where(f => f.FieldName == v).Single();
		}

		//private void MoveToTop(List<Field> fieldsss, string field)
		//{
		//	var sing = fieldsss.Where(f => f.FieldName == field).Single();
		//	fieldsss.Remove(sing);
		//	fieldsss.Insert(0, sing);
		//}


	}






}
