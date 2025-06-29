## 0.0.3
* Rename Pivoter -> PivotBuider
* Rename Presentation -> TableBuilder
* Make the Pivoter2\PivoterPtb and co. internal, its only used for regression testing.
* GetTable_NestedKeyValueList_VariableColumns: add argument (bool createEmptyIntersects = false) back again
* Rename GetGroupedData_FastIntersect -> GetGroupedData
* Rename GetTable_Array -> GetObjectArrayTable
* Rename createEmptyIntersects -> padEmptyIntersects
* Rename GetTable_FlatKeyValueList_CompleteColumns -> GetKeyValueListTable
* Rename GetTable_NestedKeyValueList_VariableColumns -> GetNestedKeyValueListTable
* Rename GetTableCore => GetTable

## 0.0.2
* Change new Field syntax, from
  `new Field<CsvRow, string>(nameof(CsvRow.Region), rows => Aggregators.CommaList(rows, row => row.Region)`
  to
  `new Field<CsvRow, string>(nameof(CsvRow.Region), row => row.Region, Aggregators.CommaList)`
* Add SortMode/GroupMode
* Add new Field ctor to support DisplayValue different from DataValue, example:
  `new Field<CsvRow, DateTime, string>(nameof(CsvRow.Time), r => r.Time, Enumerable.Max, t => t.ToString("o"));`
  and
  `new Field<CsvRow, int, string>(nameof(CsvRow.RowId), r => r.RowId, v => v.ToString(), Aggregators.CommaList);`
* FIX: Presentation2: change root detection to `grp.Field == null`

## 0.0.1
* First release.
* Dynamic group, aggregate, sort, pivot data. 
* Output to json, xml, csv or via DataTable.
* Csv or json with or without header row.
* Primary implementation: Pivoter.GetGroupedData_FastIntersect(). Made by osexpert.
* Secondary/alternative implementation: Pivoter2.GetGroupedData_PivotTableBuilder(). Based on/uses https://github.com/Kazinix/PivotTable
