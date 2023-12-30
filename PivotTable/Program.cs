using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using osexpert.PivotTable.CsvTest;

namespace osexpert.PivotTable
{
	public class Program
	{
		public static void Main()
		{


			var t = new Program();
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


			

			List<CsvRow> allRTows = null!;

			using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var records = csv.GetRecords<CsvRow>();

				allRTows = records.ToList();
			}

			//var props = TypeDescriptor.GetProperties(typeof(CsvRow));



			var fieldsss = new List<Field>();

			fieldsss.Add(new Field<CsvRow, string>(nameof(CsvRow.Region), rows => Aggregators.CommaList(rows, row => row.Region)));
			fieldsss.Add(new Field<CsvRow, string>(nameof(CsvRow.Country), rows => Aggregators.CommaList(rows, row => row.Country)));
			fieldsss.Add(new Field<CsvRow, string>(nameof(CsvRow.ItemType), rows => Aggregators.CommaList(rows, row => row.ItemType)));
			fieldsss.Add(new Field<CsvRow, string>(nameof(CsvRow.SalesChannel), rows => Aggregators.CommaList(rows, row => row.SalesChannel)));
			//props.Add(new Property<CsvRow, string>(nameof(CsvRow.OrderPriority), rows => Aggregators.CommaList(rows, row => row.OrderPriority)));
			//props.Add(new Property<CsvRow, DateTime>(nameof(CsvRow.OrderDate), rows => rows.Max(r => r.OrderDate)));
			//props.Add(new Property<CsvRow, string>(nameof(CsvRow.OrderID), rows => Aggregators.SingleOrCount(rows, row => row.OrderID)));
			//props.Add(new Property<CsvRow, int>("RowCount", rows => rows.Count()));
			//props.Add(new Property<CsvRow, DateTime>(nameof(CsvRow.ShipDate), rows => rows.Max(r => r.ShipDate)));
			fieldsss.Add(new Field<CsvRow, long>(nameof(CsvRow.UnitsSold), rows => rows.Sum(r => r.UnitsSold)));
			//props.Add(new Property<CsvRow, double>(nameof(CsvRow.UnitPrice), rows => rows.Sum(r => r.UnitPrice)));
			//props.Add(new Property<CsvRow, double>(nameof(CsvRow.UnitCost), rows => rows.Sum(r => r.UnitCost)));
			//props.Add(new Property<CsvRow, double>(nameof(CsvRow.TotalRevenue), rows => rows.Sum(r => r.TotalRevenue)));
			//props.Add(new Property<CsvRow, double>(nameof(CsvRow.TotalCost), rows => rows.Sum(r => r.TotalCost)));
			//props.Add(new Property<CsvRow, double>(nameof(CsvRow.TotalProfit), rows => rows.Sum(r => r.TotalProfit)));


			//var fieldsss = Field.CreateFieldsFromType<CsvRow>();// (props);
			//var fieldsss = Field.CreateFieldsFromProperties(props);



			// TODO: Should maybe had a way to set index after all? That was independent of order by ienumerable?
			//		MoveToTop(fieldsss, "OrderDate");
			//			MoveToTop(fieldsss, "ItemType");
			//MoveToTop(fieldsss, "OrderID");
			//MoveToTop(fieldsss, "ItemType");

			GetField(fieldsss, "Region").Area = Area.Row;
			GetField(fieldsss, "Region").GroupIndex = 1;
			//			GetField(fieldsss, "Region").SortOrder = SortOrder.Asc;

			GetField(fieldsss, "Country").Area = Area.Row;
			GetField(fieldsss, "Country").GroupIndex = 2;
			//GetField(fieldsss, "Country").SortOrder = SortOrder.Asc;

			GetField(fieldsss, "ItemType").Area = Area.Column;
			//GetField(fieldsss, "ItemType").SortOrder = SortOrder.Asc;
			GetField(fieldsss, "ItemType").GroupIndex = 0;

			GetField(fieldsss, "SalesChannel").Area = Area.Column;
			//			GetField(fieldsss, "SalesChannel").SortOrder = SortOrder.Asc;
			GetField(fieldsss, "SalesChannel").GroupIndex = 3;


			//GetField(fieldsss, "Country").Area = Area.Group;
			//GetField(fieldsss, "Country").Sort = Sort.Asc;

			//GetField(fieldsss, "ShipDate").Sort = Sort.Desc;

			//			fieldsss.Add(new Field { FieldType = FieldType.Data, FieldName = "RowCount", SortOrder = SortOrder.None, DataType = typeof(int) });



			var sw3 = Stopwatch.StartNew();

			//NRecoTest(allRTows, props, fieldsss);
			var res = allRTows.ToPivotArray(cs => new { cs.ItemType, cs.SalesChannel }
				, rs => new { rs.Region, rs.Country}, ds => ds.Any() ? ds.Sum(x => x.UnitsSold) : 0);


			sw3.Stop(); // 6.9sek



			//			TypeValue: object, name, fullname

			var pp = new Pivoter<CsvRow>(allRTows, fieldsss);//, new PropertyDescriptorCollection(props.ToArray()));

			var sw = Stopwatch.StartNew();

			var fast = pp.GetGroupedData_FastIntersect();

			sw.Stop();

			var sw2 = Stopwatch.StartNew();

			var slow = pp.GetGroupedData_SlowIntersect();

			sw2.Stop();






			var tblll = new Presentation<CsvRow>(fast).GetTable_NestedDict();

			using (var f = File.Open(@"d:\testdt5mill2_fast_nested_min.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tblll, new JsonSerializerOptions { WriteIndented = true });
			}

			
			var tbl = new Presentation<CsvRow>(fast).GetTable_FlatDict();
			
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tbl, new JsonSerializerOptions { WriteIndented = true });
			}

			var datat = new Presentation<CsvRow>(fast).GetDataTable();

			datat.WriteXml(@"d:\testdt5mill2_fast.xml");

			var dt = new Presentation<CsvRow>(fast).GetTable_Array();
//			dt.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_slow.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dt, new JsonSerializerOptions { WriteIndented=true});
			}

			dt = null;

			var dtF = new Presentation<CsvRow>(fast).GetTable_Array();
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


//			var siteF = new Field<string>() { FieldArea = Area.Value, FieldName = "SiteName", SortOrder = SortOrder.Asc };

			

////			var unitF = new FieldGen<string>() { Area = Area.Group, FieldName = "UnitName"  };
//			var specF = new Field<string>() { FieldArea = Area.Value, FieldName = "SpeciesName", SortOrder = SortOrder.Desc };
//			var indF = new Field<int>() { FieldArea = Area.Value, FieldName = "IndCount" };
//			var ff = new Field[] { specF,   indF, siteF };

			//var p = new Pivoter<Row>(ff, list, new PropertyDescriptorCollection(props.ToArray()));
			//p.GetTable();
			// TODO: dt can be slow? add option to use different construct? and then need different SortOrder?
		}

	

		private Field GetField(IEnumerable<Field> fieldsss, string v)
		{
			return fieldsss.Where(f => f.Name == v).Single();
		}

		//private void MoveToTop(List<Field> fieldsss, string field)
		//{
		//	var sing = fieldsss.Where(f => f.FieldName == field).Single();
		//	fieldsss.Remove(sing);
		//	fieldsss.Insert(0, sing);
		//}


	}






}
