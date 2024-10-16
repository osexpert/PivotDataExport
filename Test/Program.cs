using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Examples;
using Kazinix.PivotTable;
using PivotDataExport;
using static System.Net.Mime.MediaTypeNames;

namespace Examples
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
			List<CsvRow> salesRecords = null!;

			using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var records = csv.GetRecords<CsvRow>();
				salesRecords = records.ToList();
			}

			Kazinixx.test(salesRecords);

			//var props = TypeDescriptor.GetProperties(typeof(CsvRow));

			var fields = new List<Field<CsvRow>>();

			fields.Add(new Field<CsvRow, string>(nameof(CsvRow.Region), r => r.Region, Aggregators.CommaList)
			{
				GroupIndex = 0,
				Area = Area.Row,
				SortOrder = SortOrder.Asc,
			});
			fields.Add(new Field<CsvRow, string>(nameof(CsvRow.Country), r => r.Country, Aggregators.CommaList)
			{
				GroupIndex = 1,
				Area = Area.Row,
				SortOrder = SortOrder.Desc
			});

			fields.Add(new Field<CsvRow, string>(nameof(CsvRow.ItemType), r => r.ItemType, Aggregators.CommaList)
			{
				GroupIndex = 0,
				Area = Area.Column,
				SortOrder = SortOrder.Desc
			});
			fields.Add(new Field<CsvRow, string>(nameof(CsvRow.SalesChannel), r => r.SalesChannel, Aggregators.CommaList)
			{
				GroupIndex = 1,
				Area = Area.Column,
				SortOrder = SortOrder.Asc
			});

			fields.Add(new Field<CsvRow, long>(nameof(CsvRow.UnitsSold), r => r.UnitsSold, Enumerable.Sum));

			// TODO: Should maybe had a way to set index after all? That was independent of order by ienumerable?
			//		MoveToTop(fieldsss, "OrderDate");
			//			MoveToTop(fieldsss, "ItemType");
			//MoveToTop(fieldsss, "OrderID");
			//MoveToTop(fieldsss, "ItemType");


			//var sw3 = Stopwatch.StartNew();

			////NRecoTest(allRTows, props, fieldsss);
			//var res = allRTows.ToPivotArray(cs => new { cs.ItemType, cs.SalesChannel }
			//	, rs => new { rs.Region, rs.Country }, ds => ds.Any() ? ds.Sum(x => x.UnitsSold) : 0);

			//sw3.Stop(); // 6.9 sec

			////			TypeValue: object, name, fullname

			var pivot = new Pivoter<CsvRow>(salesRecords, fields);//, new PropertyDescriptorCollection(props.ToArray()));
			var pivot2 = new PivoterPtb<CsvRow>(salesRecords, fields);//, new PropertyDescriptorCollection(props.ToArray()));

			var s3 = Stopwatch.StartNew();
			var gdata_ptb = pivot2.GetGroupedData();
			s3.Stop(); // 11.8 sec ?? mem?? now that we get single row value directly, its much faster. But it did show that PTB aggregate a lot more than FIS.

			var s = Stopwatch.StartNew();
			var gdata_fis = pivot.GetGroupedData();
			s.Stop(); // 6.14 sec


			//			var pivotTable = salesRecords
			//	.GetPivotTableBuilder(l => l.Sum(e => e.UnitsSold))
			//	.SetRow(e => e.Region)
			//	.SetRow(e => e.Country)
			//	.SetColumn(e => e.ItemType)
			//	.SetColumn(e => e.SalesChannel)
			//	.Build();

			//			var pivotTable2 = salesRecords
			//	.GetPivotTableBuilder(l => l.Sum(e => e.UnitsSold))
			//	.SetRow(e => e.Region)
			//	.SetRow(e => e.Country)
			//	.Build();

			//			var pivotTable3 = salesRecords
			//.GetPivotTableBuilder(l => new
			//{
			//	Sum = l.Sum(e => e.UnitsSold),
			//	Avg = l.Average(e => e.UnitsSold)
			//})
			//.SetRow(e => e.Region)
			//.SetRow(e => e.Country)
			//.Build();

			var s2 = Stopwatch.StartNew();

			var pres_fis = new Presentation<CsvRow>(gdata_fis);
			var nested_kv_tbl = pres_fis.GetTable_NestedKeyValueList_VariableColumns();

			s2.Stop();

			var s4 = Stopwatch.StartNew();

			var pres_ptb = new PresentationPtb<CsvRow>(gdata_ptb);
			var nested_kv_tbl2 = pres_fis.GetTable_NestedKeyValueList_VariableColumns();

			s4.Stop();


			using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, nested_kv_tbl, new JsonSerializerOptions { WriteIndented = true });
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv.xml", FileMode.Create))
			{
				nested_kv_tbl.WriteXml(f);
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv2.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, nested_kv_tbl2, new JsonSerializerOptions { WriteIndented = true });
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv2.xml", FileMode.Create))
			{
				nested_kv_tbl2.WriteXml(f);
			}



			// fails
			//using (var f = File.Open(@"d:\test5mill_nested_kv.csv", FileMode.Create))
			//{
			//	nested_kv_tbl.WriteCsv(f);
			//}

			var flat_kv_tbl = pres_fis.GetTable_FlatKeyValueList_CompleteColumns();

			using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, flat_kv_tbl, new JsonSerializerOptions { WriteIndented = true });
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.xml", FileMode.Create))
			{
				flat_kv_tbl.WriteXml(f);
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.csv", FileMode.Create))
			{
				flat_kv_tbl.WriteCsv(f);
			}

			var array_tbl = pres_fis.GetTable_Array();

			using (var f = File.Open(@"d:\pivottest\test5mill_array.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, array_tbl.AddHeaderRowClone(), new JsonSerializerOptions { WriteIndented = true });
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_array.xml", FileMode.Create))
			{
				array_tbl.WriteXml(f);
			}

			using (var f = File.Open(@"d:\pivottest\test5mill_array.csv", FileMode.Create))
			{
				array_tbl.WriteCsv(f);
			}



			//var tblll = new Presentation<CsvRow>(fast).GetTable_NestedKeyValueList_VariableColumns();

			//using (var f = File.Open(@"d:\testdt5mill2_fast_nested_min.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, tblll, new JsonSerializerOptions { WriteIndented = true });
			//}


			//var tbl = new Presentation<CsvRow>(fast).GetTable_FlatKeyValueList_CompleteColumns();

			//using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, tbl, new JsonSerializerOptions { WriteIndented = true });
			//}

			//var datat = new Presentation<CsvRow>(fast).GetDataTable();

			//datat.WriteXml(@"d:\testdt5mill2_fast.xml");

			//var dt = new Presentation<CsvRow>(fast).GetTable_Array();
			////			dt.ChangeTypeToName();

			////var dt = pp.GetTableSlowIntersect();

			//sw.Stop();

			////dt.WriteXml(@"d:\testdt5mill.xml");
			//using (var f = File.Open(@"d:\testdt5mill2_slow.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, dt, new JsonSerializerOptions { WriteIndented = true });
			//}

			//dt = null;

			//var dtF = new Presentation<CsvRow>(fast).GetTable_Array();
			//		dtF.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			//sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			//using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, dtF, new JsonSerializerOptions { WriteIndented = true });
			//}

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



		//private Field<CsvRow> GetField(IEnumerable<Field<CsvRow>> fieldsss, string v)
		//{
		//	return fieldsss.Where(f => f.Name == v).Single();
		//}

		//private void MoveToTop(List<Field> fieldsss, string field)
		//{
		//	var sing = fieldsss.Where(f => f.FieldName == field).Single();
		//	fieldsss.Remove(sing);
		//	fieldsss.Insert(0, sing);
		//}


	}






}
