using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PivotDataExport;

namespace Tests;

[TestClass]
public class UnitTestFullyNested
{
	const string test_json = """
		{
		  "Rows": [
		    {
		      "Site": "Site1",
		      "Unit": "Unit2",
		      "CountryList": [
		        {
		          "Country": "USA",
		          "CompanyList": [
		            {
		              "Company": "Evil corp",
		              "GroupList": [
		                {
		                  "Group": "Group1",
		                  "Name": "Name3",
		                  "Number": 4,
		                  "Weight": 1.4,
		                  "RowId": "5"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site1",
		      "Unit": "Unit1",
		      "CountryList": [
		        {
		          "Country": "Oman",
		          "CompanyList": [
		            {
		              "Company": "VG",
		              "GroupList": [
		                {
		                  "Group": "Group1",
		                  "Name": "Name1",
		                  "Number": 1,
		                  "Weight": 1.1,
		                  "RowId": "1"
		                }
		              ]
		            },
		            {
		              "Company": "Soft",
		              "GroupList": [
		                {
		                  "Group": "Group2",
		                  "Name": "Name1",
		                  "Number": 2,
		                  "Weight": 1.2,
		                  "RowId": "2"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site3",
		      "Unit": "Unit1",
		      "CountryList": [
		        {
		          "Country": "Italy",
		          "CompanyList": [
		            {
		              "Company": "Hard",
		              "GroupList": [
		                {
		                  "Group": "Group1",
		                  "Name": "Name1, Name2",
		                  "Number": 10,
		                  "Weight": 2.1,
		                  "RowId": "3, 4"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site5",
		      "Unit": "Unit6",
		      "CountryList": [
		        {
		          "Country": "Lux",
		          "CompanyList": [
		            {
		              "Company": "Corp",
		              "GroupList": [
		                {
		                  "Group": "Group1",
		                  "Name": "NameLol",
		                  "Number": 42,
		                  "Weight": 5.1,
		                  "RowId": "6"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site5",
		      "Unit": "Unit4",
		      "CountryList": [
		        {
		          "Country": "Nan",
		          "CompanyList": [
		            {
		              "Company": "none",
		              "GroupList": [
		                {
		                  "Group": "Group0",
		                  "Name": "NameBob",
		                  "Number": 69,
		                  "Weight": 5.5,
		                  "RowId": "11"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site5",
		      "Unit": "Unit123",
		      "CountryList": [
		        {
		          "Country": "Nan",
		          "CompanyList": [
		            {
		              "Company": "none",
		              "GroupList": [
		                {
		                  "Group": "Group1000",
		                  "Name": "NameLol",
		                  "Number": 666,
		                  "Weight": 5.91,
		                  "RowId": "10"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site6",
		      "Unit": "Unit0",
		      "CountryList": [
		        {
		          "Country": "Clyx",
		          "CompanyList": [
		            {
		              "Company": "aCorp",
		              "GroupList": [
		                {
		                  "Group": "Group10",
		                  "Name": "NameDole",
		                  "Number": 64,
		                  "Weight": 55.1,
		                  "RowId": "7"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site7",
		      "Unit": "Unit0",
		      "CountryList": [
		        {
		          "Country": "Bman",
		          "CompanyList": [
		            {
		              "Company": "none",
		              "GroupList": [
		                {
		                  "Group": "Group11",
		                  "Name": "NameBill",
		                  "Number": 62,
		                  "Weight": 5.51,
		                  "RowId": "8"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    },
		    {
		      "Site": "Site8",
		      "Unit": "Unit42",
		      "CountryList": [
		        {
		          "Country": "Heman",
		          "CompanyList": [
		            {
		              "Company": "VG",
		              "GroupList": [
		                {
		                  "Group": "Group100",
		                  "Name": "NameHello",
		                  "Number": 0,
		                  "Weight": 95.1,
		                  "RowId": "9"
		                },
		                {
		                  "Group": "Group42",
		                  "Name": "NameJohn",
		                  "Number": 693,
		                  "Weight": 5.56,
		                  "RowId": "12"
		                }
		              ]
		            }
		          ]
		        }
		      ]
		    }
		  ]
		}
		""";

	[TestMethod]
	public void Test()
	{
		(var pivoter, var pivoter2) = GetPivoterTestData();

		var fields = pivoter.Fields.ToDictionary(k => k.Name);

		fields[nameof(Test1Row.Site)].Area = Area.Row;
		fields[nameof(Test1Row.Site)].SortOrder = SortOrder.Asc;
		fields[nameof(Test1Row.Site)].GroupIndex = 0;

		fields[nameof(Test1Row.Unit)].Area = Area.Row;
		fields[nameof(Test1Row.Unit)].SortOrder = SortOrder.Desc;
		fields[nameof(Test1Row.Unit)].GroupIndex = 1;

		fields[nameof(Test1Row.Country)].Area = Area.Column;
		fields[nameof(Test1Row.Country)].GroupIndex = 0;
		fields[nameof(Test1Row.Country)].SortOrder = SortOrder.Asc;

		fields[nameof(Test1Row.Company)].Area = Area.Column;
		fields[nameof(Test1Row.Company)].GroupIndex = 1;
		fields[nameof(Test1Row.Company)].SortOrder = SortOrder.Desc;

		fields[nameof(Test1Row.Group)].Area = Area.Column;
		fields[nameof(Test1Row.Group)].GroupIndex = 2;

		var gdata_fis = pivoter.GetGroupedData();
		var gdata_ptb = pivoter2.GetGroupedData();

		var pres_fis = new TableBuilder<Test1Row>(gdata_fis);
		var pres_ptb = new TableBuilderPtb<Test1Row>(gdata_ptb);

		// FIXME: currently no supporty for SortOrder
		var nested_tbl_ptb = pres_ptb.GetTable_NestedKeyValueList_VariableColumns();

		var nested_tbl_fis = pres_fis.GetNestedKeyValueListTable();

		//			nested.Columns = null;
		//		nested.ColumnGroups = null;
		//	nested.RowGroups = null;

		var json_ptb = ToJson(nested_tbl_ptb);
		var json_fis = ToJson(nested_tbl_fis);

		Assert.AreEqual(test_json, json_ptb);
		Assert.AreEqual(json_ptb, json_fis);
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
		public int RowId { get; set; }
	}

	private static (PivotBuilder<Test1Row>, PivotBuilderPtb<Test1Row>) GetPivoterTestData()
	{
		var r1 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 1, Weight = 1.1, Country = "Oman", Company = "VG" };
		var r2 = new Test1Row { Site = "Site1", Unit = "Unit1", Group = "Group2", Name = "Name1", Number = 2, Weight = 1.2, Country = "Oman", Company = "Soft" };
		var r3 = new Test1Row { Site = "Site3", Unit = "Unit1", Group = "Group1", Name = "Name1", Number = 5, Weight = 2.1, Country = "Italy", Company = "Hard" };
		var r3_ = new Test1Row { Site = "Site3", Unit = "Unit1", Group = "Group1", Name = "Name2", Number = 5, Weight = 2.1, Country = "Italy", Company = "Hard" };
		var r4 = new Test1Row { Site = "Site1", Unit = "Unit2", Group = "Group1", Name = "Name3", Number = 4, Weight = 1.4, Country = "USA", Company = "Evil corp" };
		var r5 = new Test1Row { Site = "Site5", Unit = "Unit6", Group = "Group1", Name = "NameLol", Number = 42, Weight = 5.1, Country = "Lux", Company = "Corp" };
		var r6 = new Test1Row { Site = "Site6", Unit = "Unit0", Group = "Group10", Name = "NameDole", Number = 64, Weight = 55.1, Country = "Clyx", Company = "aCorp" };
		var r7 = new Test1Row { Site = "Site7", Unit = "Unit0", Group = "Group11", Name = "NameBill", Number = 62, Weight = 5.51, Country = "Bman", Company = "none" };
		var r8 = new Test1Row { Site = "Site8", Unit = "Unit42", Group = "Group100", Name = "NameHello", Number = 0, Weight = 95.1, Country = "Heman", Company = "VG" };
		var r9 = new Test1Row { Site = "Site5", Unit = "Unit123", Group = "Group1000", Name = "NameLol", Number = 666, Weight = 5.91, Country = "Nan", Company = "none" };
		var r10 = new Test1Row { Site = "Site5", Unit = "Unit4", Group = "Group0", Name = "NameBob", Number = 69, Weight = 5.5, Country = "Nan", Company = "none" };
		var r11 = new Test1Row { Site = "Site8", Unit = "Unit42", Group = "Group42", Name = "NameJohn", Number = 693, Weight = 5.56, Country = "Heman", Company = "VG" };
		var rows = new[] { r1, r2, r3, r3_, r4, r5, r6, r7, r8, r9, r10, r11 };
		int i = 1;
		foreach (var r in rows)
			r.RowId = i++;

		var p1 = new Field<Test1Row, string>(nameof(Test1Row.Site), r => r.Site, Aggregators.CommaList);
		var p2 = new Field<Test1Row, string>(nameof(Test1Row.Unit), r => r.Unit, Aggregators.CommaList);
		var p3 = new Field<Test1Row, string>(nameof(Test1Row.Group), r => r.Group, Aggregators.CommaList);
		var p4 = new Field<Test1Row, string>(nameof(Test1Row.Name), r => r.Name, Aggregators.CommaList);
		var p5 = new Field<Test1Row, string>(nameof(Test1Row.Country), r => r.Country, Aggregators.CommaList);
		var p6 = new Field<Test1Row, string>(nameof(Test1Row.Company), r => r.Company, Aggregators.CommaList);
		var p7 = new Field<Test1Row, int>(nameof(Test1Row.Number), r => r.Number, Enumerable.Sum);
		var p8 = new Field<Test1Row, double>(nameof(Test1Row.Weight), r => r.Weight, Enumerable.Average);
		var p9 = new Field<Test1Row, int>("RowCount", r => 1, Enumerable.Count);
		var p10 = new Field<Test1Row, int, string>(nameof(Test1Row.RowId), r => r.RowId, Aggregators.CommaList);
		// Another variant that works almost the same, except here rowId is always string. The above will do group and sort on int, but display as string, just like this.
		//var p10_v2 = new Field<Test1Row, string>(nameof(Test1Row.RowId), r => r.RowId.ToString(), Aggregators.CommaList);
		var fields = new Field<Test1Row>[] { p1, p2, p3, p4, p5, p6, p7, p8, p10 };

		var piv = new PivotBuilder<Test1Row>(rows, fields);
		var piv2 = new PivotBuilderPtb<Test1Row>(rows, fields);
		return (piv, piv2);
	}
}