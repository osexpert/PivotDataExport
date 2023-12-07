using PivotExpert;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace Tests
{
	public class UnitTest1
	{

		class Test1Row
		{
			public string Site { get; set; }
			public string Unit { get; set; }
			public string Group { get; set; }
			public string Name { get; set; }
			public int Number { get; set; }
			public double Weight { get; set; }
		}

		const string str_TestCompareFastAndSlow_RowGroupOnSite = @"<DocumentElement>
  <row>
    <Site>Site1</Site>
    <Unit>Unit1, Unit2</Unit>
    <Group>Group1, Group2</Group>
    <Name>Name1, Name3</Name>
    <Number>7</Number>
    <Weight>3.6999999999999997</Weight>
    <RowCount>3</RowCount>
  </row>
  <row>
    <Site>Site3</Site>
    <Unit>Unit1</Unit>
    <Group>Group1</Group>
    <Name>Name1</Name>
    <Number>5</Number>
    <Weight>2.1</Weight>
    <RowCount>1</RowCount>
  </row>
  <row>
    <Site>Site5</Site>
    <Unit>Unit6</Unit>
    <Group>Group1</Group>
    <Name>Name1</Name>
    <Number>6</Number>
    <Weight>5.1</Weight>
    <RowCount>1</RowCount>
  </row>
</DocumentElement>";

		[Fact]
		public void TestCompareFastAndSlow_RowGroupOnSite()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.FieldName);
			fields[nameof(Test1Row.Site)].FieldType = FieldType.RowGroup;
			fields[nameof(Test1Row.Site)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Site)].SortIndex = 0;

			fields[nameof(Test1Row.Number)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Number)].SortIndex = 1;

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new DataPresentor<Test1Row>(slow);
			var fastData = new DataPresentor<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite, sFast);
			Assert.Equal(sFast, sSlow);
		}


		const string str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName = @"<DocumentElement>
  <row>
    <Site>Site1</Site>
    <_x002F_Name_x003A_Name1_x002F_Unit>Unit1</_x002F_Name_x003A_Name1_x002F_Unit>
    <_x002F_Name_x003A_Name1_x002F_Group>Group1, Group2</_x002F_Name_x003A_Name1_x002F_Group>
    <_x002F_Name_x003A_Name1_x002F_Number>3</_x002F_Name_x003A_Name1_x002F_Number>
    <_x002F_Name_x003A_Name1_x002F_Weight>2.3</_x002F_Name_x003A_Name1_x002F_Weight>
    <_x002F_Name_x003A_Name1_x002F_RowCount>2</_x002F_Name_x003A_Name1_x002F_RowCount>
    <_x002F_Name_x003A_Name3_x002F_Unit>Unit2</_x002F_Name_x003A_Name3_x002F_Unit>
    <_x002F_Name_x003A_Name3_x002F_Group>Group1</_x002F_Name_x003A_Name3_x002F_Group>
    <_x002F_Name_x003A_Name3_x002F_Number>4</_x002F_Name_x003A_Name3_x002F_Number>
    <_x002F_Name_x003A_Name3_x002F_Weight>1.4</_x002F_Name_x003A_Name3_x002F_Weight>
    <_x002F_Name_x003A_Name3_x002F_RowCount>1</_x002F_Name_x003A_Name3_x002F_RowCount>
  </row>
  <row>
    <Site>Site3</Site>
    <_x002F_Name_x003A_Name1_x002F_Unit>Unit1</_x002F_Name_x003A_Name1_x002F_Unit>
    <_x002F_Name_x003A_Name1_x002F_Group>Group1</_x002F_Name_x003A_Name1_x002F_Group>
    <_x002F_Name_x003A_Name1_x002F_Number>5</_x002F_Name_x003A_Name1_x002F_Number>
    <_x002F_Name_x003A_Name1_x002F_Weight>2.1</_x002F_Name_x003A_Name1_x002F_Weight>
    <_x002F_Name_x003A_Name1_x002F_RowCount>1</_x002F_Name_x003A_Name1_x002F_RowCount>
    <_x002F_Name_x003A_Name3_x002F_Unit />
    <_x002F_Name_x003A_Name3_x002F_Group />
    <_x002F_Name_x003A_Name3_x002F_Number>0</_x002F_Name_x003A_Name3_x002F_Number>
    <_x002F_Name_x003A_Name3_x002F_Weight>0</_x002F_Name_x003A_Name3_x002F_Weight>
    <_x002F_Name_x003A_Name3_x002F_RowCount>0</_x002F_Name_x003A_Name3_x002F_RowCount>
  </row>
  <row>
    <Site>Site5</Site>
    <_x002F_Name_x003A_Name1_x002F_Unit>Unit6</_x002F_Name_x003A_Name1_x002F_Unit>
    <_x002F_Name_x003A_Name1_x002F_Group>Group1</_x002F_Name_x003A_Name1_x002F_Group>
    <_x002F_Name_x003A_Name1_x002F_Number>6</_x002F_Name_x003A_Name1_x002F_Number>
    <_x002F_Name_x003A_Name1_x002F_Weight>5.1</_x002F_Name_x003A_Name1_x002F_Weight>
    <_x002F_Name_x003A_Name1_x002F_RowCount>1</_x002F_Name_x003A_Name1_x002F_RowCount>
    <_x002F_Name_x003A_Name3_x002F_Unit />
    <_x002F_Name_x003A_Name3_x002F_Group />
    <_x002F_Name_x003A_Name3_x002F_Number>0</_x002F_Name_x003A_Name3_x002F_Number>
    <_x002F_Name_x003A_Name3_x002F_Weight>0</_x002F_Name_x003A_Name3_x002F_Weight>
    <_x002F_Name_x003A_Name3_x002F_RowCount>0</_x002F_Name_x003A_Name3_x002F_RowCount>
  </row>
</DocumentElement>";

		[Fact]
		public void TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.FieldName);
			fields[nameof(Test1Row.Site)].FieldType = FieldType.RowGroup;
			fields[nameof(Test1Row.Site)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Site)].SortIndex = 0;

			fields[nameof(Test1Row.Name)].FieldType = FieldType.ColGroup;
			fields[nameof(Test1Row.Name)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Name)].SortIndex = 0;


			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new DataPresentor<Test1Row>(slow);
			var fastData = new DataPresentor<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName, sFast);
			Assert.Equal(sFast, sSlow);
		}

		const string str_TestCompareFastAndSlow_ColGroupOnName = @"<DocumentElement>
  <row>
    <_x002F_Name_x003A_Name1_x002F_Site>Site1, Site3, Site5</_x002F_Name_x003A_Name1_x002F_Site>
    <_x002F_Name_x003A_Name1_x002F_Unit>Unit1, Unit6</_x002F_Name_x003A_Name1_x002F_Unit>
    <_x002F_Name_x003A_Name1_x002F_Group>Group1, Group2</_x002F_Name_x003A_Name1_x002F_Group>
    <_x002F_Name_x003A_Name1_x002F_Number>14</_x002F_Name_x003A_Name1_x002F_Number>
    <_x002F_Name_x003A_Name1_x002F_Weight>9.5</_x002F_Name_x003A_Name1_x002F_Weight>
    <_x002F_Name_x003A_Name1_x002F_RowCount>4</_x002F_Name_x003A_Name1_x002F_RowCount>
    <_x002F_Name_x003A_Name3_x002F_Site>Site1</_x002F_Name_x003A_Name3_x002F_Site>
    <_x002F_Name_x003A_Name3_x002F_Unit>Unit2</_x002F_Name_x003A_Name3_x002F_Unit>
    <_x002F_Name_x003A_Name3_x002F_Group>Group1</_x002F_Name_x003A_Name3_x002F_Group>
    <_x002F_Name_x003A_Name3_x002F_Number>4</_x002F_Name_x003A_Name3_x002F_Number>
    <_x002F_Name_x003A_Name3_x002F_Weight>1.4</_x002F_Name_x003A_Name3_x002F_Weight>
    <_x002F_Name_x003A_Name3_x002F_RowCount>1</_x002F_Name_x003A_Name3_x002F_RowCount>
  </row>
</DocumentElement>";

		[Fact]
	// Expected: when only group in col, 1 row in the result with only totalt
		public void TestCompareFastAndSlow_ColGroupOnName()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.FieldName);

			fields[nameof(Test1Row.Name)].FieldType = FieldType.ColGroup;
			fields[nameof(Test1Row.Name)].Sorting = Sorting.Asc;
			fields[nameof(Test1Row.Name)].SortIndex = 0;

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new DataPresentor<Test1Row>(slow);
			var fastData = new DataPresentor<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_ColGroupOnName, sFast);
			Assert.Equal(sFast, sSlow);
		}

		const string str_TestCompareFastAndSlow_NoGroup = @"<DocumentElement>
  <row>
    <Site>Site1, Site3, Site5</Site>
    <Unit>Unit1, Unit2, Unit6</Unit>
    <Group>Group1, Group2</Group>
    <Name>Name1, Name3</Name>
    <Number>18</Number>
    <Weight>10.9</Weight>
    <RowCount>5</RowCount>
  </row>
</DocumentElement>";

		[Fact]
		// Expected: 1 row with totals
		public void TestCompareFastAndSlow_NoGroup()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.FieldName);

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new DataPresentor<Test1Row>(slow);
			var fastData = new DataPresentor<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_NoGroup, sFast);
			Assert.Equal(sFast, sSlow);
		}


		private static Pivoter<Test1Row> GetPivoterTestData()
		{
			var r1 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 1, Weight = 1.1 };
			var r2 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group2", Name = "Name1", Number = 2, Weight = 1.2 };
			var r3 = new Test1Row { Site = "Site3", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 5, Weight = 2.1 };
			var r4 = new Test1Row { Site = "Site1", Unit = "Unit2", Group = "Group1", Name = "Name3", Number = 4, Weight = 1.4 };
			var r5 = new Test1Row { Site = "Site5", Unit = "Unit6", Group = "Group1", Name = "Name1", Number = 6, Weight = 5.1 };
			var rows = new[] { r1, r2, r3, r4, r5 };

			var p1 = new PropertyColumn<Test1Row, string>(nameof(Test1Row.Site), rows => Aggregators.CommaList(rows, r => r.Site));
			var p2 = new PropertyColumn<Test1Row, string>(nameof(Test1Row.Unit), rows => Aggregators.CommaList(rows, r => r.Unit));
			var p3 = new PropertyColumn<Test1Row, string>(nameof(Test1Row.Group), rows => Aggregators.CommaList(rows, r => r.Group));
			var p4 = new PropertyColumn<Test1Row, string>(nameof(Test1Row.Name), rows => Aggregators.CommaList(rows, r => r.Name));
			var p5 = new PropertyColumn<Test1Row, int>(nameof(Test1Row.Number), rows => rows.Sum(r => r.Number));
			var p6 = new PropertyColumn<Test1Row, double>(nameof(Test1Row.Weight), rows => rows.Sum(r => r.Weight));
			var p7 = new PropertyColumn<Test1Row, int>("RowCount", rows => rows.Count());
			var props = new PropertyDescriptor[] { p1, p2, p3, p4, p5, p6, p7 };

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