using osexpert.PivotTable;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Tests.UnitTest1;

namespace Tests
{
	public class UnitTest1
	{

		public class Test1Row
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

		const string str_TestCompareFastAndSlow_RowGroupOnSite_json = @"{
  ""rows"": [
    [
      ""Site1"",
      ""Unit1, Unit2"",
      ""Group1, Group2"",
      ""Name1, Name3"",
      7,
      3.6999999999999997,
      3
    ],
    [
      ""Site3"",
      ""Unit1"",
      ""Group1"",
      ""Name1"",
      5,
      2.1,
      1
    ],
    [
      ""Site5"",
      ""Unit6"",
      ""Group1"",
      ""Name1"",
      6,
      5.1,
      1
    ]
  ]
}";

		const string str_TestCompareFastAndSlow_GetTable_DictArr = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit1, Unit2"",
      ""Group"": ""Group1, Group2"",
      ""Name"": ""Name1, Name3"",
      ""Number"": 7,
      ""Weight"": 3.6999999999999997,
      ""RowCount"": 3
    },
    {
      ""Site"": ""Site3"",
      ""Unit"": ""Unit1"",
      ""Group"": ""Group1"",
      ""Name"": ""Name1"",
      ""Number"": 5,
      ""Weight"": 2.1,
      ""RowCount"": 1
    },
    {
      ""Site"": ""Site5"",
      ""Unit"": ""Unit6"",
      ""Group"": ""Group1"",
      ""Name"": ""Name1"",
      ""Number"": 6,
      ""Weight"": 5.1,
      ""RowCount"": 1
    }
  ]
}";

		const string nested_TestCompareFastAndSlow_RowGroupOnSite = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit1, Unit2"",
      ""Group"": ""Group1, Group2"",
      ""Name"": ""Name1, Name3"",
      ""Number"": 7,
      ""Weight"": 3.6999999999999997,
      ""RowCount"": 3
    },
    {
      ""Site"": ""Site3"",
      ""Unit"": ""Unit1"",
      ""Group"": ""Group1"",
      ""Name"": ""Name1"",
      ""Number"": 5,
      ""Weight"": 2.1,
      ""RowCount"": 1
    },
    {
      ""Site"": ""Site5"",
      ""Unit"": ""Unit6"",
      ""Group"": ""Group1"",
      ""Name"": ""Name1"",
      ""Number"": 6,
      ""Weight"": 5.1,
      ""RowCount"": 1
    }
  ]
}";

		[Fact]
		public void TestCompareFastAndSlow_RowGroupOnSite()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.Name);
			fields[nameof(Test1Row.Site)].Area = Area.Row;
			fields[nameof(Test1Row.Site)].SortOrder = SortOrder.Asc;

			fields[nameof(Test1Row.Number)].SortOrder = SortOrder.Asc;

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new Presentation<Test1Row>(slow);
			var fastData = new Presentation<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite, sFast);
			Assert.Equal(sFast, sSlow);

			string slowJson = ToJson(slowData.GetTable_Array());
			string fastJson = ToJson(fastData.GetTable_Array());
			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite_json, slowJson);
			Assert.Equal(slowJson, fastJson);

			var slowTblDictArr = slowData.GetTable_FlatDict();
			var slowTblDictArrStr = ToJson(slowTblDictArr);
			Assert.Equal(str_TestCompareFastAndSlow_GetTable_DictArr, slowTblDictArrStr);

			// same as GetTable_FlatDict in this case
			var nest = ToJson(slowData.GetTable_NestedDict());
			Assert.Equal(nested_TestCompareFastAndSlow_RowGroupOnSite, nest);
		}

		private static string ToJson<T>(T table)
		{
			return JsonSerializer.Serialize<T>(table, new JsonSerializerOptions() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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

		const string str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName_json = @"{
  ""rows"": [
    [
      ""Site1"",
      ""Unit1"",
      ""Group1, Group2"",
      3,
      2.3,
      2,
      ""Unit2"",
      ""Group1"",
      4,
      1.4,
      1
    ],
    [
      ""Site3"",
      ""Unit1"",
      ""Group1"",
      5,
      2.1,
      1,
      """",
      """",
      0,
      0,
      0
    ],
    [
      ""Site5"",
      ""Unit6"",
      ""Group1"",
      6,
      5.1,
      1,
      """",
      """",
      0,
      0,
      0
    ]
  ]
}";
		const string str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName_DictArr = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""/Name:Name1/Unit"": ""Unit1"",
      ""/Name:Name1/Group"": ""Group1, Group2"",
      ""/Name:Name1/Number"": 3,
      ""/Name:Name1/Weight"": 2.3,
      ""/Name:Name1/RowCount"": 2,
      ""/Name:Name3/Unit"": ""Unit2"",
      ""/Name:Name3/Group"": ""Group1"",
      ""/Name:Name3/Number"": 4,
      ""/Name:Name3/Weight"": 1.4,
      ""/Name:Name3/RowCount"": 1
    },
    {
      ""Site"": ""Site3"",
      ""/Name:Name1/Unit"": ""Unit1"",
      ""/Name:Name1/Group"": ""Group1"",
      ""/Name:Name1/Number"": 5,
      ""/Name:Name1/Weight"": 2.1,
      ""/Name:Name1/RowCount"": 1,
      ""/Name:Name3/Unit"": """",
      ""/Name:Name3/Group"": """",
      ""/Name:Name3/Number"": 0,
      ""/Name:Name3/Weight"": 0,
      ""/Name:Name3/RowCount"": 0
    },
    {
      ""Site"": ""Site5"",
      ""/Name:Name1/Unit"": ""Unit6"",
      ""/Name:Name1/Group"": ""Group1"",
      ""/Name:Name1/Number"": 6,
      ""/Name:Name1/Weight"": 5.1,
      ""/Name:Name1/RowCount"": 1,
      ""/Name:Name3/Unit"": """",
      ""/Name:Name3/Group"": """",
      ""/Name:Name3/Number"": 0,
      ""/Name:Name3/Weight"": 0,
      ""/Name:Name3/RowCount"": 0
    }
  ]
}";

		const string nest_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""NameList"": [
        {
          ""Name"": ""Name1"",
          ""Unit"": ""Unit1"",
          ""Group"": ""Group1, Group2"",
          ""Number"": 3,
          ""Weight"": 2.3,
          ""RowCount"": 2
        },
        {
          ""Name"": ""Name3"",
          ""Unit"": ""Unit2"",
          ""Group"": ""Group1"",
          ""Number"": 4,
          ""Weight"": 1.4,
          ""RowCount"": 1
        }
      ]
    },
    {
      ""Site"": ""Site3"",
      ""NameList"": [
        {
          ""Name"": ""Name1"",
          ""Unit"": ""Unit1"",
          ""Group"": ""Group1"",
          ""Number"": 5,
          ""Weight"": 2.1,
          ""RowCount"": 1
        },
        {
          ""Name"": ""Name3"",
          ""Unit"": """",
          ""Group"": """",
          ""Number"": 0,
          ""Weight"": 0,
          ""RowCount"": 0
        }
      ]
    },
    {
      ""Site"": ""Site5"",
      ""NameList"": [
        {
          ""Name"": ""Name1"",
          ""Unit"": ""Unit6"",
          ""Group"": ""Group1"",
          ""Number"": 6,
          ""Weight"": 5.1,
          ""RowCount"": 1
        },
        {
          ""Name"": ""Name3"",
          ""Unit"": """",
          ""Group"": """",
          ""Number"": 0,
          ""Weight"": 0,
          ""RowCount"": 0
        }
      ]
    }
  ]
}";

		[Fact]
		public void TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.Name);
			fields[nameof(Test1Row.Site)].Area = Area.Row;
			fields[nameof(Test1Row.Site)].SortOrder = SortOrder.Asc;

			fields[nameof(Test1Row.Name)].Area = Area.Column;
			fields[nameof(Test1Row.Name)].SortOrder = SortOrder.Asc;

			var slow = p.GetGroupedData_SlowIntersect(createEmptyIntersects: true);
			var fast = p.GetGroupedData_FastIntersect(createEmptyIntersects: true);

			var slowData = new Presentation<Test1Row>(slow);
			var fastData = new Presentation<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName, sFast);
			Assert.Equal(sFast, sSlow);

			string slowJson = ToJson(slowData.GetTable_Array());
			string fastJson = ToJson(fastData.GetTable_Array());
			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName_json, slowJson);
			Assert.Equal(slowJson, fastJson);

			var slowTblDictArr = slowData.GetTable_FlatDict();
			var slowTblDictArrStr = ToJson(slowTblDictArr);
			Assert.Equal(str_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName_DictArr, slowTblDictArrStr);

			var nest = ToJson(slowData.GetTable_NestedDict());
			Assert.Equal(nest_TestCompareFastAndSlow_RowGroupOnSite_ColGroupOnName, nest);
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

		const string str_TestCompareFastAndSlow_ColGroupOnName_json = @"{
  ""rows"": [
    [
      ""Site1, Site3, Site5"",
      ""Unit1, Unit6"",
      ""Group1, Group2"",
      14,
      9.5,
      4,
      ""Site1"",
      ""Unit2"",
      ""Group1"",
      4,
      1.4,
      1
    ]
  ]
}";
		const string str_TestCompareFastAndSlow_ColGroupOnName_DictArr = @"{
  ""rows"": [
    {
      ""/Name:Name1/Site"": ""Site1, Site3, Site5"",
      ""/Name:Name1/Unit"": ""Unit1, Unit6"",
      ""/Name:Name1/Group"": ""Group1, Group2"",
      ""/Name:Name1/Number"": 14,
      ""/Name:Name1/Weight"": 9.5,
      ""/Name:Name1/RowCount"": 4,
      ""/Name:Name3/Site"": ""Site1"",
      ""/Name:Name3/Unit"": ""Unit2"",
      ""/Name:Name3/Group"": ""Group1"",
      ""/Name:Name3/Number"": 4,
      ""/Name:Name3/Weight"": 1.4,
      ""/Name:Name3/RowCount"": 1
    }
  ]
}";

		const string nested_TestCompareFastAndSlow_ColGroupOnName = @"{
  ""rows"": [
    {
      ""NameList"": [
        {
          ""Name"": ""Name1"",
          ""Site"": ""Site1, Site3, Site5"",
          ""Unit"": ""Unit1, Unit6"",
          ""Group"": ""Group1, Group2"",
          ""Number"": 14,
          ""Weight"": 9.5,
          ""RowCount"": 4
        },
        {
          ""Name"": ""Name3"",
          ""Site"": ""Site1"",
          ""Unit"": ""Unit2"",
          ""Group"": ""Group1"",
          ""Number"": 4,
          ""Weight"": 1.4,
          ""RowCount"": 1
        }
      ]
    }
  ]
}";

		[Fact]
	// Expected: when only group in col, 1 row in the result with only totalt
		public void TestCompareFastAndSlow_ColGroupOnName()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.Name);

			fields[nameof(Test1Row.Name)].Area = Area.Column;
			fields[nameof(Test1Row.Name)].SortOrder = SortOrder.Asc;

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new Presentation<Test1Row>(slow);
			var fastData = new Presentation<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_ColGroupOnName, sFast);
			Assert.Equal(sFast, sSlow);

			string slowJson = ToJson(slowData.GetTable_Array());
			string fastJson = ToJson(fastData.GetTable_Array());
			Assert.Equal(str_TestCompareFastAndSlow_ColGroupOnName_json, slowJson);
			Assert.Equal(slowJson, fastJson);

			var slowTblDictArr = slowData.GetTable_FlatDict();
			var slowTblDictArrStr = ToJson(slowTblDictArr);
			Assert.Equal(str_TestCompareFastAndSlow_ColGroupOnName_DictArr, slowTblDictArrStr);

			var nest = ToJson(slowData.GetTable_NestedDict());
			Assert.Equal(nested_TestCompareFastAndSlow_ColGroupOnName, nest);
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

		const string str_TestCompareFastAndSlow_NoGroup_json = @"{
  ""rows"": [
    [
      ""Site1, Site3, Site5"",
      ""Unit1, Unit2, Unit6"",
      ""Group1, Group2"",
      ""Name1, Name3"",
      18,
      10.9,
      5
    ]
  ]
}";
		const string str_TestCompareFastAndSlow_NoGroup_DictArr = @"{
  ""rows"": [
    {
      ""Site"": ""Site1, Site3, Site5"",
      ""Unit"": ""Unit1, Unit2, Unit6"",
      ""Group"": ""Group1, Group2"",
      ""Name"": ""Name1, Name3"",
      ""Number"": 18,
      ""Weight"": 10.9,
      ""RowCount"": 5
    }
  ]
}";

		const string nested_TestCompareFastAndSlow_NoGroup = @"{
  ""rows"": [
    {
      ""Site"": ""Site1, Site3, Site5"",
      ""Unit"": ""Unit1, Unit2, Unit6"",
      ""Group"": ""Group1, Group2"",
      ""Name"": ""Name1, Name3"",
      ""Number"": 18,
      ""Weight"": 10.9,
      ""RowCount"": 5
    }
  ]
}";

		[Fact]
		// Expected: 1 row with totals
		public void TestCompareFastAndSlow_NoGroup()
		{
			Pivoter<Test1Row> p = GetPivoterTestData();

			var fields = p.Fields.ToDictionary(k => k.Name);

			var slow = p.GetGroupedData_SlowIntersect();
			var fast = p.GetGroupedData_FastIntersect();

			var slowData = new Presentation<Test1Row>(slow);
			var fastData = new Presentation<Test1Row>(fast);

			var slowDT = slowData.GetDataTable();
			var fastDT = fastData.GetDataTable();

			string sFast = DTToXml(fastDT);
			string sSlow = DTToXml(slowDT);

			Assert.Equal(str_TestCompareFastAndSlow_NoGroup, sFast);
			Assert.Equal(sFast, sSlow);

			string slowJson = ToJson(slowData.GetTable_Array());
			string fastJson = ToJson(fastData.GetTable_Array());
			Assert.Equal(str_TestCompareFastAndSlow_NoGroup_json, slowJson);
			Assert.Equal(slowJson, fastJson);

			var slowTblDictArr = slowData.GetTable_FlatDict();
			var slowTblDictArrStr = ToJson(slowTblDictArr);
			Assert.Equal(str_TestCompareFastAndSlow_NoGroup_DictArr, slowTblDictArrStr);

			// this produce same result as GetTable_FlatDict in this case (no col groups)
			var nest = ToJson(slowData.GetTable_NestedDict());
			Assert.Equal(nested_TestCompareFastAndSlow_NoGroup, nest);
		}

		const string TestGroupSiteThenUnitSortBoth_nested = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit2"",
      ""GroupList"": [
        {
          ""Group"": ""Group1"",
          ""Name"": ""Name3"",
          ""Number"": 4,
          ""Weight"": 1.4,
          ""RowCount"": 1
        }
      ]
    },
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit1"",
      ""GroupList"": [
        {
          ""Group"": ""Group2"",
          ""Name"": ""Name1"",
          ""Number"": 2,
          ""Weight"": 1.2,
          ""RowCount"": 1
        },
        {
          ""Group"": ""Group1"",
          ""Name"": ""Name1"",
          ""Number"": 1,
          ""Weight"": 1.1,
          ""RowCount"": 1
        }
      ]
    },
    {
      ""Site"": ""Site3"",
      ""Unit"": ""Unit1"",
      ""GroupList"": [
        {
          ""Group"": ""Group1"",
          ""Name"": ""Name1"",
          ""Number"": 5,
          ""Weight"": 2.1,
          ""RowCount"": 1
        }
      ]
    },
    {
      ""Site"": ""Site5"",
      ""Unit"": ""Unit6"",
      ""GroupList"": [
        {
          ""Group"": ""Group1"",
          ""Name"": ""Name1"",
          ""Number"": 6,
          ""Weight"": 5.1,
          ""RowCount"": 1
        }
      ]
    }
  ]
}";

		const string TestGroupSiteThenUnitSortBoth_flat = @"{
  ""rows"": [
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit2"",
      ""/Group:Group2/Name"": null,
      ""/Group:Group2/Number"": null,
      ""/Group:Group2/Weight"": null,
      ""/Group:Group2/RowCount"": null,
      ""/Group:Group1/Name"": ""Name3"",
      ""/Group:Group1/Number"": 4,
      ""/Group:Group1/Weight"": 1.4,
      ""/Group:Group1/RowCount"": 1
    },
    {
      ""Site"": ""Site1"",
      ""Unit"": ""Unit1"",
      ""/Group:Group2/Name"": ""Name1"",
      ""/Group:Group2/Number"": 2,
      ""/Group:Group2/Weight"": 1.2,
      ""/Group:Group2/RowCount"": 1,
      ""/Group:Group1/Name"": ""Name1"",
      ""/Group:Group1/Number"": 1,
      ""/Group:Group1/Weight"": 1.1,
      ""/Group:Group1/RowCount"": 1
    },
    {
      ""Site"": ""Site3"",
      ""Unit"": ""Unit1"",
      ""/Group:Group2/Name"": null,
      ""/Group:Group2/Number"": null,
      ""/Group:Group2/Weight"": null,
      ""/Group:Group2/RowCount"": null,
      ""/Group:Group1/Name"": ""Name1"",
      ""/Group:Group1/Number"": 5,
      ""/Group:Group1/Weight"": 2.1,
      ""/Group:Group1/RowCount"": 1
    },
    {
      ""Site"": ""Site5"",
      ""Unit"": ""Unit6"",
      ""/Group:Group2/Name"": null,
      ""/Group:Group2/Number"": null,
      ""/Group:Group2/Weight"": null,
      ""/Group:Group2/RowCount"": null,
      ""/Group:Group1/Name"": ""Name1"",
      ""/Group:Group1/Number"": 6,
      ""/Group:Group1/Weight"": 5.1,
      ""/Group:Group1/RowCount"": 1
    }
  ]
}";

		const string TestGroupSiteThenUnitSortBoth_xml_nest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Table>
  <Rows>
    <Row>
      <Site>Site1</Site>
      <Unit>Unit2</Unit>
      <GroupList>
        <Entry>
          <Group>Group1</Group>
          <Name>Name3</Name>
          <Number>4</Number>
          <Weight>1.4</Weight>
          <RowCount>1</RowCount>
        </Entry>
      </GroupList>
    </Row>
    <Row>
      <Site>Site1</Site>
      <Unit>Unit1</Unit>
      <GroupList>
        <Entry>
          <Group>Group2</Group>
          <Name>Name1</Name>
          <Number>2</Number>
          <Weight>1.2</Weight>
          <RowCount>1</RowCount>
        </Entry>
        <Entry>
          <Group>Group1</Group>
          <Name>Name1</Name>
          <Number>1</Number>
          <Weight>1.1</Weight>
          <RowCount>1</RowCount>
        </Entry>
      </GroupList>
    </Row>
    <Row>
      <Site>Site3</Site>
      <Unit>Unit1</Unit>
      <GroupList>
        <Entry>
          <Group>Group1</Group>
          <Name>Name1</Name>
          <Number>5</Number>
          <Weight>2.1</Weight>
          <RowCount>1</RowCount>
        </Entry>
      </GroupList>
    </Row>
    <Row>
      <Site>Site5</Site>
      <Unit>Unit6</Unit>
      <GroupList>
        <Entry>
          <Group>Group1</Group>
          <Name>Name1</Name>
          <Number>6</Number>
          <Weight>5.1</Weight>
          <RowCount>1</RowCount>
        </Entry>
      </GroupList>
    </Row>
  </Rows>
</Table>";

		const string TestGroupSiteThenUnitSortBoth_xml_flat = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Table>
  <Rows>
    <Row>
      <Site>Site1</Site>
      <Unit>Unit2</Unit>
      </Group:Group2/Name />
      </Group:Group2/Number />
      </Group:Group2/Weight />
      </Group:Group2/RowCount />
      </Group:Group1/Name>Name3<//Group:Group1/Name>
      </Group:Group1/Number>4<//Group:Group1/Number>
      </Group:Group1/Weight>1.4<//Group:Group1/Weight>
      </Group:Group1/RowCount>1<//Group:Group1/RowCount>
    </Row>
    <Row>
      <Site>Site1</Site>
      <Unit>Unit1</Unit>
      </Group:Group2/Name>Name1<//Group:Group2/Name>
      </Group:Group2/Number>2<//Group:Group2/Number>
      </Group:Group2/Weight>1.2<//Group:Group2/Weight>
      </Group:Group2/RowCount>1<//Group:Group2/RowCount>
      </Group:Group1/Name>Name1<//Group:Group1/Name>
      </Group:Group1/Number>1<//Group:Group1/Number>
      </Group:Group1/Weight>1.1<//Group:Group1/Weight>
      </Group:Group1/RowCount>1<//Group:Group1/RowCount>
    </Row>
    <Row>
      <Site>Site3</Site>
      <Unit>Unit1</Unit>
      </Group:Group2/Name />
      </Group:Group2/Number />
      </Group:Group2/Weight />
      </Group:Group2/RowCount />
      </Group:Group1/Name>Name1<//Group:Group1/Name>
      </Group:Group1/Number>5<//Group:Group1/Number>
      </Group:Group1/Weight>2.1<//Group:Group1/Weight>
      </Group:Group1/RowCount>1<//Group:Group1/RowCount>
    </Row>
    <Row>
      <Site>Site5</Site>
      <Unit>Unit6</Unit>
      </Group:Group2/Name />
      </Group:Group2/Number />
      </Group:Group2/Weight />
      </Group:Group2/RowCount />
      </Group:Group1/Name>Name1<//Group:Group1/Name>
      </Group:Group1/Number>6<//Group:Group1/Number>
      </Group:Group1/Weight>5.1<//Group:Group1/Weight>
      </Group:Group1/RowCount>1<//Group:Group1/RowCount>
    </Row>
  </Rows>
</Table>";

		[Fact]
		public void TestGroupSiteThenUnitSortBoth()
		{
			var td = GetPivoterTestData();
			var sf = td.Fields.Single(f => f.Name == "Site");
			sf.Area = Area.Row;
			sf.GroupIndex = 0;
			sf.SortOrder = SortOrder.Asc;
			var su = td.Fields.Single(f => f.Name == "Unit");
			su.Area = Area.Row;
			su.GroupIndex = 1;
			su.SortOrder = SortOrder.Desc;
			var sg = td.Fields.Single(f => f.Name == "Group");
			sg.Area = Area.Column;
			sg.GroupIndex = 0;
			sg.SortOrder = SortOrder.Desc;

			var data = td.GetGroupedData_FastIntersect();
			var pr = new Presentation<Test1Row>(data);
			var nest = pr.GetTable_NestedDict();
			var json = ToJson(nest);
			Assert.Equal(TestGroupSiteThenUnitSortBoth_nested, json);

			var flat = pr.GetTable_FlatDict();
			flat.Columns = null;
			flat.ColumnGroups = null;
			flat.RowGroups = null;
			var flat_json = ToJson(flat);
			Assert.Equal(TestGroupSiteThenUnitSortBoth_flat, flat_json);

			var xml_nest = XmlSerialize(nest);
			Assert.Equal(TestGroupSiteThenUnitSortBoth_xml_nest, xml_nest);

			var xml_flat = XmlSerialize(flat);
			Assert.Equal(TestGroupSiteThenUnitSortBoth_xml_flat, xml_flat);

		}


		private static Pivoter<Test1Row> GetPivoterTestData()
		{
			var r1 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 1, Weight = 1.1 };
			var r2 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group2", Name = "Name1", Number = 2, Weight = 1.2 };
			var r3 = new Test1Row { Site = "Site3", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 5, Weight = 2.1 };
			var r4 = new Test1Row { Site = "Site1", Unit = "Unit2", Group = "Group1", Name = "Name3", Number = 4, Weight = 1.4 };
			var r5 = new Test1Row { Site = "Site5", Unit = "Unit6", Group = "Group1", Name = "Name1", Number = 6, Weight = 5.1 };
			var rows = new[] { r1, r2, r3, r4, r5 };

			var p1 = new Field<Test1Row, string>(nameof(Test1Row.Site), rows => Aggregators.CommaList(rows, r => r.Site));
			var p2 = new Field<Test1Row, string>(nameof(Test1Row.Unit), rows => Aggregators.CommaList(rows, r => r.Unit));
			var p3 = new Field<Test1Row, string>(nameof(Test1Row.Group), rows => Aggregators.CommaList(rows, r => r.Group));
			var p4 = new Field<Test1Row, string>(nameof(Test1Row.Name), rows => Aggregators.CommaList(rows, r => r.Name));
			var p5 = new Field<Test1Row, int>(nameof(Test1Row.Number), rows => rows.Sum(r => r.Number));
			var p6 = new Field<Test1Row, double>(nameof(Test1Row.Weight), rows => rows.Sum(r => r.Weight));
			var p7 = new Field<Test1Row, int>("RowCount", rows => rows.Count());
			var props = new Field[] { p1, p2, p3, p4, p5, p6, p7 };

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

		public static string XmlSerialize(object obj)
		{
			//XmlWriter x = null;

			
			//return "";
			//// Using System.Text.Json to parse JSON
			//JsonDocument jsonDocument = JsonDocument.Parse(jsonData);

			//// Creating XDocument and adding elements and attributes
			//XDocument xDocument = new XDocument(
			//	new XElement("Root",
			//		jsonDocument.RootElement.EnumerateObject()
			//			.Select(prop => new XElement(prop.Name, prop.Value.ToString()))
			//	)
			//);

			//// Output the XML
			//string xmlOutput = xDocument.ToString();
			//return xmlOutput;

			XmlSerializer xsSubmit = new XmlSerializer(obj.GetType());
			using (var sww = new ExtentedStringWriter(Encoding.UTF8))
			{
				using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
				{
					xsSubmit.Serialize(writer, obj);
					return sww.ToString();
				}
			}


		}

		public sealed class ExtentedStringWriter : StringWriter
		{
			private readonly Encoding stringWriterEncoding;

			public ExtentedStringWriter(Encoding desiredEncoding)
				: base()
			{
				this.stringWriterEncoding = desiredEncoding;
			}

			public ExtentedStringWriter(StringBuilder builder, Encoding desiredEncoding)
				: base(builder)
			{
				this.stringWriterEncoding = desiredEncoding;
			}

			public override Encoding Encoding
			{
				get
				{
					return this.stringWriterEncoding;
				}
			}
		}
	}
}