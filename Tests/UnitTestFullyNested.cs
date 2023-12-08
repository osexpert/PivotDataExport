using PivotExpert;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tests
{
	public class UnitTestFullyNested
	{
		[Fact]
		public void Test()
		{
			var pivoter = GetPivoterTestData();

			var fields = pivoter.Fields.ToDictionary(k => k.FieldName);
			fields[nameof(Test1Row.Site)].FieldType = FieldType.RowGroup;
			fields[nameof(Test1Row.Site)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Site)].SortIndex = 0;

			fields[nameof(Test1Row.Country)].FieldType = FieldType.ColGroup;
			fields[nameof(Test1Row.Country)].GroupIndex = 0;
			fields[nameof(Test1Row.Country)].Sorting = Sorting.Asc;

			fields[nameof(Test1Row.Company)].FieldType = FieldType.ColGroup;
			fields[nameof(Test1Row.Company)].GroupIndex = 1;
			fields[nameof(Test1Row.Company)].Sorting = Sorting.Desc;

			// TODO: gir det noen mening med SortIndex på kolonnegrupper???? Må ikke sortIndex i dette tilfellet alltid følge groupindeksen???
			// TEST!!

			var gdata = pivoter.GetGroupedData_FastIntersect();

			var pres = new Presentation<Test1Row>(gdata);
			var nested = pres.GetTable_NestedDict_NG_TODO();
			nested.Columns = null;
			nested.ColumnGroups = null;
			nested.RowGroups = null;

			var js = ToJson(nested);

			//var slow = p.GetGroupedData_SlowIntersect();
			//var fast = p.GetGroupedData_FastIntersect();

			//var slowData = new Presentation<Test1Row>(slow);
			//var fastData = new Presentation<Test1Row>(fast);
		}

		private static string ToJson<T>(T table)
		{
			return JsonSerializer.Serialize<T>(table, new JsonSerializerOptions() { WriteIndented = true });
		}

		class Test1Row
		{
			public string Site { get; set; }
			public string Unit { get; set; }
			public string Group { get; set; }
			public string Company { get; set; }
			public string Country { get; set; }
			public string Name { get; set; }
			public int Number { get; set; }
			public double Weight { get; set; }
		}

		private static Pivoter<Test1Row> GetPivoterTestData()
		{
			var r1 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 1, Weight = 1.1, Country = "Oman", Company = "VG" };
			var r2 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group2", Name = "Name1", Number = 2, Weight = 1.2, Country = "Oman", Company = "Soft" };
			var r3 = new Test1Row { Site = "Site3", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 5, Weight = 2.1, Country = "Italy", Company = "Hard" };
			var r4 = new Test1Row { Site = "Site1", Unit = "Unit2", Group = "Group1", Name = "Name3", Number = 4, Weight = 1.4, Country = "USA", Company = "Evil corp" };
			var r5 = new Test1Row { Site = "Site5", Unit = "Unit6", Group = "Group1", Name = "NameLol", Number = 42, Weight = 5.1, Country = "Lux", Company = "Corp"  };
			var r6 = new Test1Row { Site = "Site6", Unit = "Unit0", Group = "Group10", Name = "NameDole", Number = 64, Weight = 55.1, Country = "Clyx", Company = "aCorp" };
			var r7 = new Test1Row { Site = "Site7", Unit = "Unit0", Group = "Group11", Name = "NameBill", Number = 62, Weight = 5.51, Country = "Bman", Company = "none" };
			var r8 = new Test1Row { Site = "Site8", Unit = "Unit42", Group = "Group100", Name = "NameHello", Number = 0, Weight = 95.1, Country = "Heman", Company = "VG" };
			var r9 = new Test1Row { Site = "Site5", Unit = "Unit123", Group = "Group1000", Name = "NameLol", Number = 666, Weight = 5.91, Country = "Nan", Company = "none" };
			var r10 = new Test1Row { Site = "Site5", Unit = "Unit4", Group = "Group0", Name = "NameBob", Number = 69, Weight = 5.5, Country = "Nan", Company = "none" };
			var rows = new[] { r1, r2, r3, r4, r5, r6, r7, r8, r9, r10 };

			var p1 = new Property<Test1Row, string>(nameof(Test1Row.Site), rows => Aggregators.CommaList(rows, r => r.Site));
			var p2 = new Property<Test1Row, string>(nameof(Test1Row.Unit), rows => Aggregators.CommaList(rows, r => r.Unit));
			var p3 = new Property<Test1Row, string>(nameof(Test1Row.Group), rows => Aggregators.CommaList(rows, r => r.Group));
			var p4 = new Property<Test1Row, string>(nameof(Test1Row.Name), rows => Aggregators.CommaList(rows, r => r.Name));
			var p5 = new Property<Test1Row, string>(nameof(Test1Row.Country), rows => Aggregators.CommaList(rows, r => r.Country));
			var p6 = new Property<Test1Row, string>(nameof(Test1Row.Company), rows => Aggregators.CommaList(rows, r => r.Company));
			var p7 = new Property<Test1Row, int>(nameof(Test1Row.Number), rows => rows.Sum(r => r.Number));
			var p8 = new Property<Test1Row, double>(nameof(Test1Row.Weight), rows => rows.Average(r => r.Weight));
			var p9 = new Property<Test1Row, int>("RowCount", rows => rows.Count());
			var props = new PropertyDescriptor[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 };

			var p = new Pivoter<Test1Row>(rows, props);
			return p;
		}

		private static string DTToXml(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXml(writer);
				writer.Flush();

				return writer.GetStringBuilder().ToString();
			}
		}
	}


}
