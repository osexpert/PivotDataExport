# PivotDataExport
Dynamic group, aggregate, sort, pivot data. 
Output to json, xml, csv or via DataTable.
Csv or json with or without header row.

Built on an idea: receive field definitions via api. Fetch, process and return data as result from api,
alternatively queue it up and save a file somewhere, that later can be downloaded.
So the idea is a dynamic report/export engine that can power a single api (per topic/data source).
This hypotetical api is not a part of this project, but such api could use PivotDataExport as an engine.

Example:

Test data used:
https://excelbianalytics.com/wp/wp-content/uploads/2020/09/5m-Sales-Records.zip

Read data:

    List<CsvRow> salesRecords = null!;
    using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
      var records = csv.GetRecords<CsvRow>();
      salesRecords = records.ToList();
    }
    
Create fields:
    
    var fields = new List<Field>();
    
    fields.Add(new Field<CsvRow, string>(nameof(CsvRow.Region), row => row.Region, Aggregators.CommaList)
    {
      GroupIndex = 0,
      Area = Area.Row,
      SortOrder = SortOrder.Asc
    });
    fields.Add(new Field<CsvRow, string>(nameof(CsvRow.Country), row => row.Country, Aggregators.CommaList)
    {
      GroupIndex = 1,
      Area = Area.Row,
      SortOrder = SortOrder.Desc
    });
    
    fields.Add(new Field<CsvRow, string>(nameof(CsvRow.ItemType), row => row.ItemType, Aggregators.CommaList)
    {
      GroupIndex = 0,
      Area = Area.Column,
      SortOrder = SortOrder.Desc
    });
    fields.Add(new Field<CsvRow, string>(nameof(CsvRow.SalesChannel), row => row.SalesChannel, Aggregators.CommaList)
    {
      GroupIndex = 1,
      Area = Area.Column,
      SortOrder = SortOrder.Asc
    });
    
    fields.Add(new Field<CsvRow, long>(nameof(CsvRow.UnitsSold), row => row.UnitsSold, Enumerable.Sum);

Group, aggregate, sort:

	var pb = new PivotBuilder<CsvRow>(salesRecords, fields);
	var data = pb.GetGroupedData();
	var tb = new TableBuilder<CsvRow>(data);
Get table:
	
	var nested_kv_tbl = tb.GetTable_NestedKeyValueList_VariableColumns();

Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv.json", FileMode.Create))
	{
		JsonSerializer.Serialize(f, nested_kv_tbl, new JsonSerializerOptions { WriteIndented = true });
	}

Produce:

    {
      "Rows": [
        {
          "Region": "Asia",
          "Country": "Vietnam",
          "ItemTypeList": [
            {
              "ItemType": "Vegetables",
              "SalesChannelList": [
                {
                  "SalesChannel": "Offline",
                  "UnitsSold": 5820281
                },
                {
                  "SalesChannel": "Online",
                  "UnitsSold": 5501045
                }
              ]
            },
			...

Code:


	using (var f = File.Open(@"d:\pivottest\test5mill_nested_kv.xml", FileMode.Create))
	{
		nested_kv_tbl.WriteXml(f);
	}

Produce:

    <?xml version="1.0" encoding="utf-8"?>
    <Table>
      <Rows>
        <Row>
          <Region>Asia</Region>
          <Country>Vietnam</Country>
          <ItemTypeList>
            <Entry>
              <ItemType>Vegetables</ItemType>
              <SalesChannelList>
                <Entry>
                  <SalesChannel>Offline</SalesChannel>
                  <UnitsSold>5820281</UnitsSold>
                </Entry>
                <Entry>
                  <SalesChannel>Online</SalesChannel>
                  <UnitsSold>5501045</UnitsSold>
                </Entry>
              </SalesChannelList>
            </Entry>
    		...

Code:

	// not supported
	//using (var f = File.Open(@"d:\test5mill_nested_kv.csv", FileMode.Create))
	//{
	//	nested_kv_tbl.WriteCsv(f);
	//}

Get table:

	var flat_kv_tbl = tb.GetTable_FlatKeyValueList_CompleteColumns();

Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.json", FileMode.Create))
	{
		JsonSerializer.Serialize(f, flat_kv_tbl, new JsonSerializerOptions { WriteIndented = true });
	}
Produce:

    {
      "Rows": [
        {
          "Region": "Asia",
          "Country": "Vietnam",
          "/ItemType:Vegetables/SalesChannel:Offline/UnitsSold": 5820281,
          "/ItemType:Vegetables/SalesChannel:Online/UnitsSold": 5501045,
          "/ItemType:Snacks/SalesChannel:Offline/UnitsSold": 5479038,
          "/ItemType:Snacks/SalesChannel:Online/UnitsSold": 5797606,
          "/ItemType:Personal Care/SalesChannel:Offline/UnitsSold": 5561844,
          "/ItemType:Personal Care/SalesChannel:Online/UnitsSold": 5606394,
          "/ItemType:Office Supplies/SalesChannel:Offline/UnitsSold": 5512339,
          "/ItemType:Office Supplies/SalesChannel:Online/UnitsSold": 5647800,
          "/ItemType:Meat/SalesChannel:Offline/UnitsSold": 5538231,
          "/ItemType:Meat/SalesChannel:Online/UnitsSold": 5593779,
          "/ItemType:Household/SalesChannel:Offline/UnitsSold": 5616672,
          "/ItemType:Household/SalesChannel:Online/UnitsSold": 5651216,
          "/ItemType:Fruits/SalesChannel:Offline/UnitsSold": 5712683,
          "/ItemType:Fruits/SalesChannel:Online/UnitsSold": 5767442,
          "/ItemType:Cosmetics/SalesChannel:Offline/UnitsSold": 5514074,
          "/ItemType:Cosmetics/SalesChannel:Online/UnitsSold": 5692946,
          "/ItemType:Clothes/SalesChannel:Offline/UnitsSold": 5463956,
          "/ItemType:Clothes/SalesChannel:Online/UnitsSold": 5678553,
          "/ItemType:Cereal/SalesChannel:Offline/UnitsSold": 5704591,
          "/ItemType:Cereal/SalesChannel:Online/UnitsSold": 5522784,
          "/ItemType:Beverages/SalesChannel:Offline/UnitsSold": 5462035,
          "/ItemType:Beverages/SalesChannel:Online/UnitsSold": 5643938,
          "/ItemType:Baby Food/SalesChannel:Offline/UnitsSold": 5738685,
          "/ItemType:Baby Food/SalesChannel:Online/UnitsSold": 5468720
        },
        ...

  Code:
  
	using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.xml", FileMode.Create))
	{
		flat_kv_tbl.WriteXml(f);
	}
Produce:

    <?xml version="1.0" encoding="utf-8"?>
    <Table>
      <Rows>
        <Row>
          <Region>Asia</Region>
          <Country>Vietnam</Country>
          </ItemType:Vegetables/SalesChannel:Offline/UnitsSold>5820281<//ItemType:Vegetables/SalesChannel:Offline/UnitsSold>
          </ItemType:Vegetables/SalesChannel:Online/UnitsSold>5501045<//ItemType:Vegetables/SalesChannel:Online/UnitsSold>
          </ItemType:Snacks/SalesChannel:Offline/UnitsSold>5479038<//ItemType:Snacks/SalesChannel:Offline/UnitsSold>
          </ItemType:Snacks/SalesChannel:Online/UnitsSold>5797606<//ItemType:Snacks/SalesChannel:Online/UnitsSold>
          </ItemType:Personal Care/SalesChannel:Offline/UnitsSold>5561844<//ItemType:Personal Care/SalesChannel:Offline/UnitsSold>
          </ItemType:Personal Care/SalesChannel:Online/UnitsSold>5606394<//ItemType:Personal Care/SalesChannel:Online/UnitsSold>
          </ItemType:Office Supplies/SalesChannel:Offline/UnitsSold>5512339<//ItemType:Office Supplies/SalesChannel:Offline/UnitsSold>
          </ItemType:Office Supplies/SalesChannel:Online/UnitsSold>5647800<//ItemType:Office Supplies/SalesChannel:Online/UnitsSold>
          </ItemType:Meat/SalesChannel:Offline/UnitsSold>5538231<//ItemType:Meat/SalesChannel:Offline/UnitsSold>
          </ItemType:Meat/SalesChannel:Online/UnitsSold>5593779<//ItemType:Meat/SalesChannel:Online/UnitsSold>
          </ItemType:Household/SalesChannel:Offline/UnitsSold>5616672<//ItemType:Household/SalesChannel:Offline/UnitsSold>
          </ItemType:Household/SalesChannel:Online/UnitsSold>5651216<//ItemType:Household/SalesChannel:Online/UnitsSold>
          </ItemType:Fruits/SalesChannel:Offline/UnitsSold>5712683<//ItemType:Fruits/SalesChannel:Offline/UnitsSold>
          </ItemType:Fruits/SalesChannel:Online/UnitsSold>5767442<//ItemType:Fruits/SalesChannel:Online/UnitsSold>
          </ItemType:Cosmetics/SalesChannel:Offline/UnitsSold>5514074<//ItemType:Cosmetics/SalesChannel:Offline/UnitsSold>
          </ItemType:Cosmetics/SalesChannel:Online/UnitsSold>5692946<//ItemType:Cosmetics/SalesChannel:Online/UnitsSold>
          </ItemType:Clothes/SalesChannel:Offline/UnitsSold>5463956<//ItemType:Clothes/SalesChannel:Offline/UnitsSold>
          </ItemType:Clothes/SalesChannel:Online/UnitsSold>5678553<//ItemType:Clothes/SalesChannel:Online/UnitsSold>
          </ItemType:Cereal/SalesChannel:Offline/UnitsSold>5704591<//ItemType:Cereal/SalesChannel:Offline/UnitsSold>
          </ItemType:Cereal/SalesChannel:Online/UnitsSold>5522784<//ItemType:Cereal/SalesChannel:Online/UnitsSold>
          </ItemType:Beverages/SalesChannel:Offline/UnitsSold>5462035<//ItemType:Beverages/SalesChannel:Offline/UnitsSold>
          </ItemType:Beverages/SalesChannel:Online/UnitsSold>5643938<//ItemType:Beverages/SalesChannel:Online/UnitsSold>
          </ItemType:Baby Food/SalesChannel:Offline/UnitsSold>5738685<//ItemType:Baby Food/SalesChannel:Offline/UnitsSold>
          </ItemType:Baby Food/SalesChannel:Online/UnitsSold>5468720<//ItemType:Baby Food/SalesChannel:Online/UnitsSold>
        </Row>
        ...


Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_flat_kv.csv", FileMode.Create))
	{
		flat_kv_tbl.WriteCsv(f);
	}
Produce:

    Region;Country;/ItemType:Vegetables/SalesChannel:Offline/UnitsSold;/ItemType:Vegetables/SalesChannel:Online/UnitsSold;/ItemType:Snacks/SalesChannel:Offline/UnitsSold;/ItemType:Snacks/SalesChannel:Online/UnitsSold;/ItemType:Personal Care/SalesChannel:Offline/UnitsSold;/ItemType:Personal Care/SalesChannel:Online/UnitsSold;/ItemType:Office Supplies/SalesChannel:Offline/UnitsSold;/ItemType:Office Supplies/SalesChannel:Online/UnitsSold;/ItemType:Meat/SalesChannel:Offline/UnitsSold;/ItemType:Meat/SalesChannel:Online/UnitsSold;/ItemType:Household/SalesChannel:Offline/UnitsSold;/ItemType:Household/SalesChannel:Online/UnitsSold;/ItemType:Fruits/SalesChannel:Offline/UnitsSold;/ItemType:Fruits/SalesChannel:Online/UnitsSold;/ItemType:Cosmetics/SalesChannel:Offline/UnitsSold;/ItemType:Cosmetics/SalesChannel:Online/UnitsSold;/ItemType:Clothes/SalesChannel:Offline/UnitsSold;/ItemType:Clothes/SalesChannel:Online/UnitsSold;/ItemType:Cereal/SalesChannel:Offline/UnitsSold;/ItemType:Cereal/SalesChannel:Online/UnitsSold;/ItemType:Beverages/SalesChannel:Offline/UnitsSold;/ItemType:Beverages/SalesChannel:Online/UnitsSold;/ItemType:Baby Food/SalesChannel:Offline/UnitsSold;/ItemType:Baby Food/SalesChannel:Online/UnitsSold
    Asia;Vietnam;5820281;5501045;5479038;5797606;5561844;5606394;5512339;5647800;5538231;5593779;5616672;5651216;5712683;5767442;5514074;5692946;5463956;5678553;5704591;5522784;5462035;5643938;5738685;5468720
    ...

Get table:

	var array_tbl = tb.GetTable_Array();

Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_array.json", FileMode.Create))
	{
		JsonSerializer.Serialize(f, array_tbl.AddHeaderRowClone(), new JsonSerializerOptions { WriteIndented = true });
	}
Produce:

    {
      "Rows": [
        [
          "Region",
          "Country",
          "/ItemType:Vegetables/SalesChannel:Offline/UnitsSold",
          "/ItemType:Vegetables/SalesChannel:Online/UnitsSold",
          "/ItemType:Snacks/SalesChannel:Offline/UnitsSold",
          "/ItemType:Snacks/SalesChannel:Online/UnitsSold",
          "/ItemType:Personal Care/SalesChannel:Offline/UnitsSold",
          "/ItemType:Personal Care/SalesChannel:Online/UnitsSold",
          "/ItemType:Office Supplies/SalesChannel:Offline/UnitsSold",
          "/ItemType:Office Supplies/SalesChannel:Online/UnitsSold",
          "/ItemType:Meat/SalesChannel:Offline/UnitsSold",
          "/ItemType:Meat/SalesChannel:Online/UnitsSold",
          "/ItemType:Household/SalesChannel:Offline/UnitsSold",
          "/ItemType:Household/SalesChannel:Online/UnitsSold",
          "/ItemType:Fruits/SalesChannel:Offline/UnitsSold",
          "/ItemType:Fruits/SalesChannel:Online/UnitsSold",
          "/ItemType:Cosmetics/SalesChannel:Offline/UnitsSold",
          "/ItemType:Cosmetics/SalesChannel:Online/UnitsSold",
          "/ItemType:Clothes/SalesChannel:Offline/UnitsSold",
          "/ItemType:Clothes/SalesChannel:Online/UnitsSold",
          "/ItemType:Cereal/SalesChannel:Offline/UnitsSold",
          "/ItemType:Cereal/SalesChannel:Online/UnitsSold",
          "/ItemType:Beverages/SalesChannel:Offline/UnitsSold",
          "/ItemType:Beverages/SalesChannel:Online/UnitsSold",
          "/ItemType:Baby Food/SalesChannel:Offline/UnitsSold",
          "/ItemType:Baby Food/SalesChannel:Online/UnitsSold"
        ],
        [
          "Asia",
          "Vietnam",
          5820281,
          5501045,
          5479038,
          5797606,
          5561844,
          5606394,
          5512339,
          5647800,
          5538231,
          5593779,
          5616672,
          5651216,
          5712683,
          5767442,
          5514074,
          5692946,
          5463956,
          5678553,
          5704591,
          5522784,
          5462035,
          5643938,
          5738685,
          5468720
        ],
        ...
Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_array.xml", FileMode.Create))
	{
		array_tbl.WriteXml(f);
	}
Produce:

    <?xml version="1.0" encoding="utf-8"?>
    <Table>
      <Rows>
        <Row>
          <Region>Asia</Region>
          <Country>Vietnam</Country>
          </ItemType:Vegetables/SalesChannel:Offline/UnitsSold>5820281<//ItemType:Vegetables/SalesChannel:Offline/UnitsSold>
          </ItemType:Vegetables/SalesChannel:Online/UnitsSold>5501045<//ItemType:Vegetables/SalesChannel:Online/UnitsSold>
          </ItemType:Snacks/SalesChannel:Offline/UnitsSold>5479038<//ItemType:Snacks/SalesChannel:Offline/UnitsSold>
          </ItemType:Snacks/SalesChannel:Online/UnitsSold>5797606<//ItemType:Snacks/SalesChannel:Online/UnitsSold>
          </ItemType:Personal Care/SalesChannel:Offline/UnitsSold>5561844<//ItemType:Personal Care/SalesChannel:Offline/UnitsSold>
          </ItemType:Personal Care/SalesChannel:Online/UnitsSold>5606394<//ItemType:Personal Care/SalesChannel:Online/UnitsSold>
          </ItemType:Office Supplies/SalesChannel:Offline/UnitsSold>5512339<//ItemType:Office Supplies/SalesChannel:Offline/UnitsSold>
          </ItemType:Office Supplies/SalesChannel:Online/UnitsSold>5647800<//ItemType:Office Supplies/SalesChannel:Online/UnitsSold>
          </ItemType:Meat/SalesChannel:Offline/UnitsSold>5538231<//ItemType:Meat/SalesChannel:Offline/UnitsSold>
          </ItemType:Meat/SalesChannel:Online/UnitsSold>5593779<//ItemType:Meat/SalesChannel:Online/UnitsSold>
          </ItemType:Household/SalesChannel:Offline/UnitsSold>5616672<//ItemType:Household/SalesChannel:Offline/UnitsSold>
          </ItemType:Household/SalesChannel:Online/UnitsSold>5651216<//ItemType:Household/SalesChannel:Online/UnitsSold>
          </ItemType:Fruits/SalesChannel:Offline/UnitsSold>5712683<//ItemType:Fruits/SalesChannel:Offline/UnitsSold>
          </ItemType:Fruits/SalesChannel:Online/UnitsSold>5767442<//ItemType:Fruits/SalesChannel:Online/UnitsSold>
          </ItemType:Cosmetics/SalesChannel:Offline/UnitsSold>5514074<//ItemType:Cosmetics/SalesChannel:Offline/UnitsSold>
          </ItemType:Cosmetics/SalesChannel:Online/UnitsSold>5692946<//ItemType:Cosmetics/SalesChannel:Online/UnitsSold>
          </ItemType:Clothes/SalesChannel:Offline/UnitsSold>5463956<//ItemType:Clothes/SalesChannel:Offline/UnitsSold>
          </ItemType:Clothes/SalesChannel:Online/UnitsSold>5678553<//ItemType:Clothes/SalesChannel:Online/UnitsSold>
          </ItemType:Cereal/SalesChannel:Offline/UnitsSold>5704591<//ItemType:Cereal/SalesChannel:Offline/UnitsSold>
          </ItemType:Cereal/SalesChannel:Online/UnitsSold>5522784<//ItemType:Cereal/SalesChannel:Online/UnitsSold>
          </ItemType:Beverages/SalesChannel:Offline/UnitsSold>5462035<//ItemType:Beverages/SalesChannel:Offline/UnitsSold>
          </ItemType:Beverages/SalesChannel:Online/UnitsSold>5643938<//ItemType:Beverages/SalesChannel:Online/UnitsSold>
          </ItemType:Baby Food/SalesChannel:Offline/UnitsSold>5738685<//ItemType:Baby Food/SalesChannel:Offline/UnitsSold>
          </ItemType:Baby Food/SalesChannel:Online/UnitsSold>5468720<//ItemType:Baby Food/SalesChannel:Online/UnitsSold>
        </Row>
        ...

Code:

	using (var f = File.Open(@"d:\pivottest\test5mill_array.csv", FileMode.Create))
	{
		array_tbl.WriteCsv(f);
	}
Produce:

    Region;Country;/ItemType:Vegetables/SalesChannel:Offline/UnitsSold;/ItemType:Vegetables/SalesChannel:Online/UnitsSold;/ItemType:Snacks/SalesChannel:Offline/UnitsSold;/ItemType:Snacks/SalesChannel:Online/UnitsSold;/ItemType:Personal Care/SalesChannel:Offline/UnitsSold;/ItemType:Personal Care/SalesChannel:Online/UnitsSold;/ItemType:Office Supplies/SalesChannel:Offline/UnitsSold;/ItemType:Office Supplies/SalesChannel:Online/UnitsSold;/ItemType:Meat/SalesChannel:Offline/UnitsSold;/ItemType:Meat/SalesChannel:Online/UnitsSold;/ItemType:Household/SalesChannel:Offline/UnitsSold;/ItemType:Household/SalesChannel:Online/UnitsSold;/ItemType:Fruits/SalesChannel:Offline/UnitsSold;/ItemType:Fruits/SalesChannel:Online/UnitsSold;/ItemType:Cosmetics/SalesChannel:Offline/UnitsSold;/ItemType:Cosmetics/SalesChannel:Online/UnitsSold;/ItemType:Clothes/SalesChannel:Offline/UnitsSold;/ItemType:Clothes/SalesChannel:Online/UnitsSold;/ItemType:Cereal/SalesChannel:Offline/UnitsSold;/ItemType:Cereal/SalesChannel:Online/UnitsSold;/ItemType:Beverages/SalesChannel:Offline/UnitsSold;/ItemType:Beverages/SalesChannel:Online/UnitsSold;/ItemType:Baby Food/SalesChannel:Offline/UnitsSold;/ItemType:Baby Food/SalesChannel:Online/UnitsSold
    Asia;Vietnam;5820281;5501045;5479038;5797606;5561844;5606394;5512339;5647800;5538231;5593779;5616672;5651216;5712683;5767442;5514074;5692946;5463956;5678553;5704591;5522784;5462035;5643938;5738685;5468720
    ...

