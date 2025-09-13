using System.Diagnostics;
using Kazinix.PivotTable;

namespace Examples;

internal class Kazinixx
{
	public static void test(List<CsvRow> salesRecords)
	{


		//List<CsvRow> salesRecords = null!;

		//using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
		//using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
		//{
		//	var records = csv.GetRecords<CsvRow>();
		//	salesRecords = records.ToList();
		//}

		var s = Stopwatch.StartNew();

		var pivotTable = salesRecords
			.GetPivotTableBuilder(l => l.Sum(e => e.UnitsSold))
			.SetRow(e => e.Region)
			.SetRow(e => e.Country)
			.SetColumn(e => e.ItemType)
			.SetColumn(e => e.SalesChannel)
			.Build();

		s.Stop(); // 13.4 sec

	}
}
