using CsvHelper;
using Kazinix.PivotTable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test;

namespace Examples
{
	internal class Kazinixx
	{
		public static void test()
		{


			List<CsvRow> salesRecords = null!;

			using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var records = csv.GetRecords<CsvRow>();
				salesRecords = records.ToList();
			}

			var s = Stopwatch.StartNew();

			var pivotTable = salesRecords
				.GetPivotTableBuilder(l => l.Sum(e => e.UnitsSold))
				.SetRow(e => e.Region)
				.SetRow(e => e.Country)
				.SetColumn(e => e.ItemType)
				.SetColumn(e => e.SalesChannel)
				.Build();

			s.Stop(); // 14 sec

		}
	}
}
