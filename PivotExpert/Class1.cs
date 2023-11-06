#define WRITE_OA

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using NotVisualBasic;
using NotVisualBasic.FileIO;


namespace PivotExpert
{
	//	Region,              Country,Item Type,   Sales Channel,Order Priority, Order Date,Order ID, Ship Date,Units Sold, Unit Price,Unit Cost, Total Revenue,Total Cost, Total Profit
	//Australia and Oceania, Palau, Office Supplies,Online,     H,              3/6/2016,517073523,  3/26/2016,2401,651.21,524.96,1563555.21,1260428.96,303126.25
	public class CsvRow
	{
		[Index(0)]
		public string Region { get; set; }
		[Index(1)]
		public string Country { get; set; }
		[Index(2)]
		public string ItemType { get; set; }
		[Index(3)]
		public string SalesChannel { get; set; }
		[Index(4)]
		public string OrderPriority { get; set; }
		[Index(5)]
		public DateTime OrderDate { get; set; }
		[Index(6)]
		public string OrderID { get; set; }
		[Index(7)]
		public DateTime ShipDate { get; set; }
		[Index(8)]
		public long UnitsSold { get; set; }
		[Index(9)]
		public double UnitPrice { get; set; }
		[Index(10)]
		public double UnitCost { get; set; }
		[Index(11)]
		public double TotalRevenue { get; set; }
		[Index(12)]
		public double TotalCost { get; set; }
		[Index(13)]
		public double TotalProfit { get; set; }
		
	}

	public class Table<TRow>
	{
		public List<TableColumn> RowGroups { get; set; }
		public List<TableColumn> ColumnGroups { get; set; }
		public List<TableColumn> Columns { get; set; }

		public List<TRow> Rows { get; set; }
		public TRow? GrandTotalRow { get; set; }

		//public void ChangeTypeToName(bool fullName = false)
		//{
		//	foreach (var c in this.Columns)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//	foreach (var c in this.ColumnGroups)
		//	{
		//		if (c.DataType is Type t)
		//			c.DataType = fullName ? (t.FullName ?? t.Name) : t.Name;
		//	}
		//}

	}

	public class TableColumn
	{
		public string Name { get; set; }

		[JsonIgnore]
		public Type DataType { get; set; }

		public string TypeName => DataType.Name;

		public FieldType FieldType { get; set; }
		public int GroupIndex { get; set; }

		public Sorting Sorting { get; set; }
		public int SortIndex { get; set; }

		public object?[]? GroupValues { get; set; }
	}

	//public class TableRow
	//{
	//	public object?[] Values { get; set; }
	//}

	//public class CsvRowDataFetcher<TRow> : ITypedList
	//{
	//	PropertyDescriptorCollection _props;

	//	public CsvRowDataFetcher()
 //       {
	//		_props = TypeDescriptor.GetProperties(typeof(TRow));
	//	}
	//	public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
	//	{
	//		return _props;
	//	}

	//	public string GetListName(PropertyDescriptor[] listAccessors)
	//	{
	//		return "";
	//	}
	//}

	public class Tester
	{
		public static void Main()
		{
			Version v = null;

			string fff = "" + v;

			var t = new Tester();
			t.Test();
		}

		public void Test()
		{
			//	List<ExpandoObject> lx = new();//
			//			var x = new ExpandoObject();
			//		x.TryAdd("Test/:\"ttt", "hello");
			//			lx.Add(x);

			//List<Dictionary<string, object?>> listtt = new();

			//Dictionary<string, object?> row = new();
			//row.Add("Test/:\"ttt", "hello");
			//row.Add("Testttt", 42);
			//row.Add("ATestttt", 43);
			//listtt.Add(row);

			//using (var f = File.Open(@"d:\testwrite.json", FileMode.Create))
			//{
			//	JsonSerializer.Serialize(f, listtt, new JsonSerializerOptions { WriteIndented = true });
			//}

			//var datas = new CsvTextFieldParser(@"d:\5m Sales Records.csv");

			//while (!datas.EndOfData)
			//{
			//	var fields = datas.ReadFields();
			//}

			List<CsvRow> allRTows = null;

			using (var reader = new StreamReader(@"d:\5m Sales Records.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				var records = csv.GetRecords<CsvRow>();

				allRTows = records.ToList();
			}

			//var props = TypeDescriptor.GetProperties(typeof(CsvRow));

			var fieldsss = CreateFieldsFromType<CsvRow>();// (props);



			// TODO: Should maybe had a way to set index after all? That was independent of order by ienumerable?
	//		MoveToTop(fieldsss, "OrderDate");
//			MoveToTop(fieldsss, "ItemType");
			//MoveToTop(fieldsss, "OrderID");
			//MoveToTop(fieldsss, "ItemType");

			GetField(fieldsss, "Region").FieldType = FieldType.RowGroup;
			GetField(fieldsss, "Region").GroupIndex = 1;
			GetField(fieldsss, "Region").Sorting = Sorting.Asc;
			GetField(fieldsss, "Region").SortIndex = 1;

			GetField(fieldsss, "Country").FieldType = FieldType.RowGroup;
			GetField(fieldsss, "Country").GroupIndex = 2;
			GetField(fieldsss, "Country").Sorting = Sorting.Asc;
			GetField(fieldsss, "Country").SortIndex = 2;

			GetField(fieldsss, "SalesChannel").FieldType = FieldType.ColGroup;
			GetField(fieldsss, "SalesChannel").Sorting = Sorting.Asc;
			GetField(fieldsss, "SalesChannel").SortIndex = 1;
			GetField(fieldsss, "SalesChannel").GroupIndex = 3;

			GetField(fieldsss, "ItemType").FieldType = FieldType.ColGroup;
			GetField(fieldsss, "ItemType").Sorting = Sorting.Asc;
			GetField(fieldsss, "ItemType").SortIndex = 0;
			GetField(fieldsss, "ItemType").GroupIndex = 0;

			//GetField(fieldsss, "Country").Area = Area.Group;
			//GetField(fieldsss, "Country").Sort = Sort.Asc;

			//GetField(fieldsss, "ShipDate").Sort = Sort.Desc;

			fieldsss.Add(new Field { FieldType = FieldType.Data, FieldName = "RowCount", Sorting = Sorting.None, DataType = typeof(int), SortIndex = 0 });

			var props = new List<PropertyDescriptor>();

			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.Region), rows => GetCommaList(rows, row => row.Region)));
			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.Country), rows => GetCommaList(rows, row => row.Country)));
			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.ItemType), rows => GetCommaList(rows, row => row.ItemType)));
			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.SalesChannel), rows => GetCommaList(rows, row => row.SalesChannel)));
			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.OrderPriority), rows => GetCommaList(rows, row => row.OrderPriority)));

			props.Add(new ColumnFunc<CsvRow, DateTime>(nameof(CsvRow.OrderDate), rows => rows.Max(r => r.OrderDate)));

			props.Add(new ColumnFunc<CsvRow, string>(nameof(CsvRow.OrderID), rows => GetSingleOrCount(rows, row => row.OrderID)));
			props.Add(new ColumnFunc<CsvRow, int>("RowCount", rows => rows.Count()));

			props.Add(new ColumnFunc<CsvRow, DateTime>(nameof(CsvRow.ShipDate), rows => rows.Max(r => r.ShipDate)));

		
			props.Add(new ColumnFunc<CsvRow, long>(nameof(CsvRow.UnitsSold), rows => rows.Sum(r => r.UnitsSold)));


			props.Add(new ColumnFunc<CsvRow, double>(nameof(CsvRow.UnitPrice), rows => rows.Sum(r => r.UnitPrice)));


			props.Add(new ColumnFunc<CsvRow, double>(nameof(CsvRow.UnitCost), rows => rows.Sum(r => r.UnitCost)));


			props.Add(new ColumnFunc<CsvRow, double>(nameof(CsvRow.TotalRevenue), rows => rows.Sum(r => r.TotalRevenue)));


			props.Add(new ColumnFunc<CsvRow, double>(nameof(CsvRow.TotalCost), rows => rows.Sum(r => r.TotalCost)));

			props.Add(new ColumnFunc<CsvRow, double>(nameof(CsvRow.TotalProfit), rows => rows.Sum(r => r.TotalProfit)));

			var sw = Stopwatch.StartNew();

//			TypeValue: object, name, fullname

			var pp = new Pivoter<CsvRow>(fieldsss, allRTows, new PropertyDescriptorCollection(props.ToArray()));





			var tblll = pp.GetTableFastIntersect_DictArrNEsted();


			using (var f = File.Open(@"d:\testdt5mill2_fast_nested_min.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tblll, new JsonSerializerOptions { WriteIndented = true });
			}





			var fast = pp.GetTableFastIntersect();
			var slow = pp.GetTableSlowIntersect();


			var tbl = pp.GetTableFastIntersect_DictArr();
			
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, tbl, new JsonSerializerOptions { WriteIndented = true });
			}


			var datat = pp.GetDataTable();

			datat.WriteXml(@"d:\testdt5mill2_fast.xml");


			var dt = pp.GetTableSlowIntersect_objectArr();
//			dt.ChangeTypeToName();

			//var dt = pp.GetTableSlowIntersect();

			sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_slow.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dt, new JsonSerializerOptions { WriteIndented=true});
			}



			dt = null;


			var dtF = pp.GetTableFastIntersect_objectArr();
	//		dtF.ChangeTypeToName();


			//var dt = pp.GetTableSlowIntersect();

			//sw.Stop();

			//dt.WriteXml(@"d:\testdt5mill.xml");
			using (var f = File.Open(@"d:\testdt5mill2_fast.json", FileMode.Create))
			{
				JsonSerializer.Serialize(f, dtF, new JsonSerializerOptions { WriteIndented = true });
			}





			//var dt = pp.GetTable();








			// 37 sec without DT or object arrays
			// 35 sec with DT??
			// 186 rows

			// 58sec,  3.6GB, Count = 2097153
			// DT: 2min,  5.4GB, Count = 2097153
			// SLOW: 4.37, 4GB

			var pdc = new PropertyDescriptorCollection(new PropertyDescriptor[]
			{
				new SiteNameCol(),
				new UnitNameCol(),
				new SpeciesNameCol().Col,
				new IndCountCol().Col
			});
			
			var list = new Rows<Row>(pdc);

			// TODO: optin: RowNumber auto column? 1 to n?
			// TODO: optin: top row fieldnames? (or caption?)

			list.Add(new Row() { IndCount = 11, SiteName = "S1", UnitName = "U1", SpecName = "Frog"});
			list.Add(new Row() { IndCount = 13, SiteName = "S1", UnitName = "U1", SpecName = "Hog" });
			list.Add(new Row() { IndCount = 22, SiteName = "S1", UnitName = "U2", SpecName = "Bird" });
			list.Add(new Row() { IndCount = 33, SiteName = "S2", UnitName = "U1", SpecName = "Dog" });
			list.Add(new Row() { IndCount = 44, SiteName = "S3", UnitName = "U1", SpecName = "Human" });
			list.Add(new Row() { IndCount = 111, SiteName = "S1", UnitName = "U11", SpecName = "Frog" });
			list.Add(new Row() { IndCount = 222, SiteName = "S1", UnitName = "U22", SpecName = "Bird" });
			list.Add(new Row() { IndCount = 333, SiteName = "S2", UnitName = "U11", SpecName = "Dog" });
			list.Add(new Row() { IndCount = 444, SiteName = "S3", UnitName = "U11", SpecName = "Human" });


			var siteF = new FieldGen<string>() { FieldType = FieldType.Data, FieldName = "SiteName", Sorting = Sorting.Asc, SortIndex = 0 };

			

//			var unitF = new FieldGen<string>() { Area = Area.Group, FieldName = "UnitName"  };
			var specF = new FieldGen<string>() { FieldType = FieldType.Data, FieldName = "SpeciesName", Sorting = Sorting.Desc, SortIndex = 1 };
			var indF = new FieldGen<int>() { FieldType = FieldType.Data, FieldName = "IndCount" };
			var ff = new Field[] { specF,   indF, siteF };

			var p = new Pivoter<Row>(ff, list, new PropertyDescriptorCollection(props.ToArray()));
			//p.GetTable();
			// TODO: dt can be slow? add option to use different construct? and then need different sorting?
		}

		private string GetCommaList(IEnumerable<CsvRow> rows, Func<CsvRow, string> value)
		{
			int constrainedCount = rows.Take(2).Count();
			if (constrainedCount == 0)
				return "";
			else if (constrainedCount == 1)
				return value(rows.Single());
			else
				return string.Join(", ", rows.Select(value).Distinct().OrderBy(v => v));
		}

		private string GetSingleOrCount(IEnumerable<CsvRow> rows, Func<CsvRow, string> value)
		{
			int constrainedCount = rows.Take(2).Count();
			if (constrainedCount == 0)
				return "";
			else if (constrainedCount == 1)
				return value(rows.Single());
			else
				return $"Count: {rows.Count()}";
		}



		private List<Field> CreateFieldsFromType<T>()
		{
			return typeof(T).GetProperties().Select(pd => new Field { FieldType = FieldType.Data, FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}

		private Field GetField(List<Field> fieldsss, string v)
		{
			return fieldsss.Where(f => f.FieldName == v).Single();
		}

		private void MoveToTop(List<Field> fieldsss, string field)
		{
			var sing = fieldsss.Where(f => f.FieldName == field).Single();
			fieldsss.Remove(sing);
			fieldsss.Insert(0, sing);
		}

		private List<Field> CreateFieldsFromProps(PropertyDescriptorCollection props)
		{
			return props.Cast<PropertyDescriptor>().Select(pd => new Field { FieldType = FieldType.Data, FieldName = pd.Name, DataType = pd.PropertyType }).ToList();
		}

		public class SiteNameCol : Column<Row, string>
		{
            public SiteNameCol() : base("SiteName")
            {
                
            }

			protected override string GetRowValue(IEnumerable<Row> rows)
			{
				return string.Join(", ", rows.Select(r => r.SiteName).Distinct().OrderBy(s => s));
			}
		}

		public class UnitNameCol : Column<Row, string>
		{
			public UnitNameCol() : base("UnitName")
			{

			}

			protected override string GetRowValue(IEnumerable<Row> rows)
			{
				return string.Join(", ", rows.Select(r => r.UnitName).Distinct().OrderBy(s => s));
			}
		}

		public class SpeciesNameCol
		{
			public ColumnFunc<Row, string> Col;

			public SpeciesNameCol()
			{
				Col = new ColumnFunc<Row, string>("SpeciesName", GetRowValue);
			}

			string GetRowValue(IEnumerable<Row> rows)
			{
				return string.Join(", ", rows.Select(r => r.SpecName).Distinct().OrderBy(s => s));
			}
		}

		public class IndCountCol
		{
			public ColumnFunc<Row, int> Col;

			public IndCountCol()
			{
				Col = new ColumnFunc<Row, int>("IndCount", GetRowValue);
			}

			int GetRowValue(IEnumerable<Row> rows)
			{
				return rows.Select(r => r.IndCount).Sum();
			}
		}


		public abstract class Column<RT, PT> : PropertyDescriptor
		{
			//Type _propType;
	//		Func<object?, object> _getVal;

			public Column(string propName)//, Type propType)//, Func<object?, object> getVal)
				: base(propName, null)
			{
				//_propType = propType;
//				_getVal = getVal;
			}

			public override object? GetValue(object? component)
			{
	//			if (component is RT r)
//					return GetRowValue(r);
				if (component is IEnumerable<RT> rows)
					return GetRowValue(rows);
				else
					throw new Exception("unk type");
			}

			protected abstract PT GetRowValue(IEnumerable<RT> rows);

			//private object GetRowValue(RT r)
			//{
			//	throw new NotImplementedException();
			//}

			//public abstract object GetRowValue()

			public override Type PropertyType => typeof(PT);

			public override void ResetValue(object component)
			{
				// Not relevant.
			}

			public override void SetValue(object? component, object? value) => throw new NotImplementedException();

			public override bool ShouldSerializeValue(object component) => true;
			public override bool CanResetValue(object component) => false;

			public override Type ComponentType => typeof(RT);
			public override bool IsReadOnly => true;
		}


		public class ColumnFunc<TRow, TProp> : PropertyDescriptor
		{
			//Type _propType;
			Func<IEnumerable<TRow>, TProp> _getVal;

			public ColumnFunc(string propName, Func<IEnumerable<TRow>, TProp> getVal)
				: base(propName, null)
			{
				//_propType = propType;
				_getVal = getVal;
			}

			public override object? GetValue(object? component)
			{
				//			if (component is RT r)
				//					return GetRowValue(r);
				if (component is IEnumerable<TRow> rows)
					return _getVal(rows);//: GetRowValue(rows);
				else
					throw new Exception("wrong type");
			}

			public override Type PropertyType => typeof(TProp);

			public override void ResetValue(object component)
			{
				// Not relevant.
			}

			public override void SetValue(object? component, object? value) => throw new NotImplementedException();

			public override bool ShouldSerializeValue(object component) => true;
			public override bool CanResetValue(object component) => false;

			public override Type ComponentType => typeof(IEnumerable<TRow>);
			public override bool IsReadOnly => true;
		}


		class Rows<T> : List<T>, ITypedList
		{
			PropertyDescriptorCollection _pdc;

			public Rows(PropertyDescriptorCollection pdc)
            {
				_pdc = pdc;
            }

            public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
			{
				return _pdc;
			}

			public string GetListName(PropertyDescriptor[] listAccessors)
			{
				return "";
			}
		}

		public class Row
		{
			//public int RowId;
			public string SiteName;
			public string UnitName;
			public string SpecName;
			public int IndCount;

			//public Row(int rowId)
   //         {
			//	RowId = rowId;
   //         }
        }
	}



	public class Field
	{
		public IEqualityComparer<object?> Comparer = EqualityComparer<object?>.Default;

		// FIXME: kind of pointless...could simply used passed order
		//public int Index { get; set; }  // 0, 1, 2

		public Type DataType;

		public string FieldName { get; set; }

		public FieldType FieldType { get; set; } // Group, Data, etc.?

		public Sorting Sorting;
		public int SortIndex;

		public int GroupIndex;

		internal TableColumn ToTableColumn()
		{
			return new(){
				Name = FieldName,
				DataType = DataType,
				FieldType = FieldType,
				Sorting = Sorting,
				SortIndex = SortIndex,
				GroupIndex = GroupIndex
			};

		}

		internal TableColumn ToTableColumn(string combNAme, object?[] groupVals)
		{
			return new()
			{
				Name = combNAme,
				FieldType = FieldType,
				DataType = DataType,
				GroupIndex = GroupIndex,
				SortIndex = SortIndex,
				Sorting = Sorting,
				GroupValues = groupVals
			};

		}

		// FUNC to get display text?
		// Compare\equal by value or text?

		// datavalue (groupin)
		// displayvalue
		// sorting value

		// TOTAL value stored here?

		// Should we cache the value for every row?
		//internal int idx;
	}

	public class FieldGen<T> : Field
	{
        public FieldGen()
        {
			DataType = typeof(T);
        }
    }

	public enum FieldType
	{
		Data = 0,
		RowGroup = 1,
		ColGroup = 2,
	}
	public enum Sorting
	{
		None = 0,
		Asc = 1,
		Desc = 2
	}


	//class IntersectData
	//{
	//	Dictionary<Field, object?> values = new();

	//	internal void Add(Field dataField, object? theValue)
	//	{
	//		values.Add(dataField, theValue);
	//	}
	//}

	public class Group<T> where T : class
	{
		/// <summary>
		/// If true, not a group but all the rows
		/// </summary>
		public bool IsRoot;


		// Denne er vel ikke satt hvis gruppa har IntersectData?
		public object? Key; // data or display? this is raw value. the field funcs decide the groupings via funcs.

		//public IEnumerable<Group<T>> Groups;
		
		public IEnumerable<T> Rows;

		public Field Field;

		public Group<T>? ParentGroup;

		public Dictionary<Group<T>, object?[]> IntersectData { get; internal set; }

		public object?[] RowData { get; internal set; }

		internal object? GetKeyByField(Field colField)
		{
			var current = this;
			do
			{
				if (current.Field == colField)
					return current.Key;
				current = current.ParentGroup;

			} while (current != null);

			throw new Exception("Bug");
		}
	}

	public class Pivoter<T> where T : class // notnull
	{
		IEnumerable<Field> _fields;
		IEnumerable<T> _list;
		Dictionary<string, PropertyDescriptor> _props;

		//public Pivoter(IEnumerable<Field> fields, IEnumerable<T> list) : this(fields, list, TypeDescriptor.GetProperties(typeof(T)))
		//{

		//}

		public Pivoter(IEnumerable<Field> fields, IEnumerable<T> list, ITypedList typedList) : this(fields, list, typedList.GetItemProperties(null!))
		{

		}

		public Pivoter(IEnumerable<Field> fields, IEnumerable<T> list, PropertyDescriptorCollection pdc)
		{
			if (fields.Any(f => f.FieldType == FieldType.ColGroup) && fields.Any(f => f.FieldType == FieldType.Data && f.Sorting != Sorting.None))
				throw new ArgumentException("Can not sort on data fields if groping on columns");

			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_list = list;

			_fields = fields;

			// pdc: aggregator\data getter
			_props = pdc.Cast<PropertyDescriptor>().ToDictionary(pd => pd.Name);

			foreach (var field in _fields)
			{
				var prop = _props[field.FieldName];
			}
		}

		//		public Table GetTableOnlyWorkedForRowGroups(bool addGrandTotalRow = false)
		//		{
		//			int idx = 0;
		//			foreach (var f in _fields)
		//				f.idx = idx++;

		//			var dataFields = GetDataFields().ToArray();

		//			//var lastGrr = GroupRowsAllAtOnce(GetGroupFields());
		//			List<Group<T>> lastGroups = GroupRows(GetGroupFields());

		//			List<object?[]> rows_o = new();

		//			var colCount = _fields.Count();

		//			//if (lastGroups != null)
		//			{
		//				foreach (var group in lastGroups)
		//				{
		//					var row_o = new object?[colCount];

		//					foreach (Field dataField in dataFields)
		//					{
		//						var getter = _props[dataField.FieldName];
		//						var theValue = getter.GetValue(group.Rows);
		//						row_o[dataField.idx] = theValue;
		//					}

		//					var parent = group;
		//					do
		//					{
		//						row_o[parent.Field.idx] = parent.Key;
		//						parent = parent.ParentGroup;
		//					} while (parent != null && !parent.IsRoot);

		//					rows_o.Add(row_o);
		//				}

		////				lastGroups = null!; // free mem
		//			}
		//			//else // no groups, total sum
		//			//{
		//			//	var row_o = new object?[colCount];

		//			//	foreach (Field dataField in dataFields)
		//			//	{
		//			//		var getter = _props[dataField.FieldName];
		//			//		var theValue = getter.GetValue(_list);
		//			//		row_o[dataField.idx] = theValue;
		//			//	}

		//			//	rows_o.Add(row_o);
		//			//}
		//			// else...a mode for output 1:1?
		//			//{
		//			//	foreach (var l in _list)
		//			//	{
		//			//		var r = res.NewRow();

		//			//		foreach (Field dataField in GetDataFields())
		//			//		{
		//			//			var getter = _props[dataField.FieldName];
		//			//			// TODO: get value from multiple ROWS
		//			//			var theValue = getter.GetValue(l.Yield());

		//			//			r[dataField.FieldName] = theValue;
		//			//		}
		//			//	}
		//			//}


		//			SortRows(ref rows_o);

		//			Table t = new();
		//			t.Columns = _fields.Select(f => new TableColumn { 
		//				Name = f.FieldName, 
		//				DataType = f.DataType.Name, 
		//				Sorting = f.Sorting,
		//				SortIndex = f.SortIndex,
		//				FieldType = f.FieldType,
		//				GroupIndex = f.GroupIndex
		//			}).ToList();
		//			t.Rows = rows_o;


		//			// add sum row after sort, always want it last.
		//			// only add if we have groups, else it will always be only sum, we dont need double.
		//			// TODO: should we sum group fields too??
		//			// TODO: should write "Grand total" or "*" into grouped cols?
		//			if (addGrandTotalRow)
		//			{
		//				if (lastGroups.First().IsRoot)
		//				{
		//					// its the same...
		//					t.GrandTotalRow = rows_o.Single();
		//				}
		//				else // if (lastGroups != null)
		//				{
		//					var row_o = new object?[colCount];

		//					foreach (var dataField in dataFields)
		//					{
		//						var getter = _props[dataField.FieldName];
		//						var theValue = getter.GetValue(_list);
		//						row_o[dataField.idx] = theValue;
		//					}

		//					t.GrandTotalRow = row_o;
		//				}
		//			}

		//			// Transform to rows
		//			//if (lastGroups != null)
		//			{
		//				var ggg = lastGroups.GroupBy(lg => GetAllColGroupLevels(lg));
		//				var lol = ggg.ToList();
		//			}

		//			return t;
		//		}



		//private GroupingKey<object?> GetAllColGroupLevels(Group<T> lg)
		//{
		//	// while parent
		//	Stack<object?> st = new();

		//	var parent = lg;//.ParentGroup;
		//	do
		//	{
		//		st.Push(parent.Key);

		//		parent = parent.ParentGroup;
		//	} while (parent != null && !parent.IsRoot && parent.Field.FieldType == FieldType.ColGroup);

		//	return new GroupingKey<object?>(st.ToArray());
		//}

		//private void SortRows(ref List<object?[]> rows)
		//{
		//	var sortFields = _fields.Where(f => f.Sorting != Sorting.None).OrderBy(f => f.SortIndex);
		//	if (sortFields.Any())
		//	{
		//		IOrderedEnumerable<object?[]> sorter = null!;
		//		foreach (var sf in sortFields)
		//		{
		//			if (sorter == null)
		//				sorter = sf.Sorting == Sorting.Asc ? rows.OrderBy(r => r[sf.idx]) : rows.OrderByDescending(r => r[sf.idx]);
		//			else
		//				sorter = sf.Sorting == Sorting.Asc ? sorter.ThenBy(r => r[sf.idx]) : sorter.ThenByDescending(r => r[sf.idx]);
		//		}
		//		rows = sorter.ToList();
		//	}
		//}

		private List<object?[]> SortRowsNew(List<object?[]> rows, List<TableColumn> tableCols)
		{
			var sortFields = _fields
				.Where(f => f.FieldType != FieldType.ColGroup) // sorting col groups mean sorting the columns themself (the labels)
				.Where(f => f.Sorting != Sorting.None)
				.OrderBy(f => f.SortIndex);

			if (sortFields.Any())
			{
				IOrderedEnumerable<object?[]> sorter = null!;
				foreach (var sf in sortFields)
				{
					// TODO lookup idx from filedname
					var sortCol = tableCols.Single(tc => tc.Name == sf.FieldName);
					var idx = tableCols.IndexOf(sortCol);

					if (sorter == null)
						sorter = sf.Sorting == Sorting.Asc ? rows.OrderBy(r => r[idx]) : rows.OrderByDescending(r => r[idx]);
					else
						sorter = sf.Sorting == Sorting.Asc ? sorter.ThenBy(r => r[idx]) : sorter.ThenByDescending(r => r[idx]);
				}
				rows = sorter.ToList();
			}

			return rows;
		}




		private List<List<Group<T>>> GroupRows(IEnumerable<Field> fields, bool sort = false)
		{
			List<Group<T>> lastGroups = new List<Group<T>>();
			lastGroups.Add(new Group<T> { Rows = _list, IsRoot = true });

			return GroupRows(lastGroups, fields, sort: sort);
		}

		private List<List<Group<T>>> GroupRows(List<Group<T>> lastGroups, IEnumerable<Field> fields, bool freeOriginalLastGroupsMem = true, bool sort = false)
		{
			List<List<Group<T>>> listRes = new();

			List<Group<T>> originalLastGroups = lastGroups;

			//			List<Group<T>> lastGroups = new List<Group<T>>();
			//		lastGroups.Add(new Group<T> { Rows = _list, IsRoot = true });

			foreach (Field gf in fields)
			{
				var getter = _props[gf.FieldName];

				var allSubGroups = new List<Group<T>>();

				foreach (var go in lastGroups)
				{
					var subGroups = go.Rows.GroupBy(r => getter.GetValue(r.Yield()), gf.Comparer).Select(g => new Group<T>()
					{
						Key = g.Key,
						Rows = g,
						Field = gf,
						ParentGroup = go
					});

					if (lastGroups == originalLastGroups)
					{
						if (freeOriginalLastGroupsMem)
							go.Rows = null!; // free mem, no longer needed now we have divided rows futher down in sub groups
					}
					else
					{
						go.Rows = null!; // free mem, no longer needed now we have divided rows futher down in sub groups
					}

					if (sort)
						allSubGroups.AddRange(subGroups.OrderBy(sg => sg.Key)); // displayText or value?
					else
						allSubGroups.AddRange(subGroups);
				}

				listRes.Add(allSubGroups);

				lastGroups = allSubGroups;
			}

			//return lastGroups;
			return listRes;
		}

		public DataTable GetDataTable()
		{
			var t = GetTableFastIntersect_objectArr();

			DataTable res = new("row");

			foreach (var f in t.Columns)
			{
				res.Columns.Add(f.Name, (Type)f.DataType);
			}

			res.BeginLoadData();
			foreach (var oarr in t.Rows)
				res.LoadDataRow(oarr, fAcceptChanges: false /* ?? */);

			if (t.GrandTotalRow != null)
				res.LoadDataRow(t.GrandTotalRow, fAcceptChanges: false /* ?? */);

			res.EndLoadData();

			return res;
		}

		private IEnumerable<Field> GetDataFields()
		{
			return _fields.Where(f => f.FieldType == FieldType.Data);//.OrderBy(f => f.Index);
		}

		//private IEnumerable<Field> GetGroupFields()
		//{
		//	return _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex)
		//		.Concat(_fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex));
		//}

		public Table<Dictionary<string, object?>> GetTableSlowIntersect_DictArr()
		{
			var s1d = GetTableSlowIntersect();
			return Next<Dictionary<string, object?>>(s1d, (rows, tcols) =>
			{
				List<Dictionary<string, object?>> dictRows = new();

				foreach (var row in rows)
				{
					Dictionary<string, object?> dictRow = new();
					foreach (var v in row.Zip(tcols))
						dictRow.Add(v.Second.Name, v.First);

					dictRows.Add(dictRow);
				}

				return dictRows;
			});
		}


		public Table<object?[]> GetTableSlowIntersect_objectArr()
		{
			var s1d = GetTableSlowIntersect();
			return Next(s1d, (rows, tcols) => rows);
		}

		public Step1Data GetTableSlowIntersect()
		{
			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<T>>> allRowGroups = GroupRows(rowFieldsInGroupOrder);
			List<List<Group<T>>> allColGroups = GroupRows(colFieldsInGroupOrder);

			List<Group<T>> lastRowGroups = allRowGroups.Last();
			List<Group<T>> lastColGroups = allColGroups.Last();

			foreach (var lastRowGroup in lastRowGroups)
			{
				lastRowGroup.IntersectData = new();

				foreach (var lastColGroup in lastColGroups)
				{
					//var lastG_groupKey = GetAllColGroupLevels(lastColGroup);

					var data = new object?[dataFields.Length];

					var intersectRows = lastRowGroup.Rows.Intersect(lastColGroup.Rows).ToList();

					int dataFieldIdx = 0;
					foreach (var dataField in dataFields)
					{
						var prop = _props[dataField.FieldName];
						var theValue = prop.GetValue(intersectRows);

						data[dataFieldIdx] = theValue;
						dataFieldIdx++;
					}

					lastRowGroup.IntersectData.Add(lastColGroup, data);
				}
			}

			//var colFieldsInSortOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup)
			//	.Where(f => f.Sorting != Sorting.None)
			//	.OrderBy(f => f.SortIndex).ToArray();

			//var lastColGroupsSorted = SortColGroups(lastColGroups, colFieldsInSortOrder).ToList();

			return new Step1Data()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				dataFields = dataFields,
				//lastColGroupsSorted = lastColGroupsSorted,
				//lastRowGroups = lastRowGroups,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups
			};
		}



		private List<TableColumn> CreateTableCols(Field[] dataFields, Field[] rowGroupFields, List<Group<T>> lastColGroups /* sorted */)
		{
			List<TableColumn> tablecols = new();
			// fill rowGroups
			//			int colCount = rowGroupFields.Length + (lastColGroups.Count * dataFields.Length);
			tablecols.AddRange(rowGroupFields.Select(f => f.ToTableColumn()));

			if (lastColGroups.Any())
			{
				List<TableColumn> tablecols_after = new();
				foreach (var gr in lastColGroups)
				{
					Stack<TableGroup> tgs = new();

					var parent = gr;
					do
					{
						tgs.Push(new TableGroup { Name = parent.Field.FieldName,
							//DataType = parent.Field.DataType,
							Value = parent.Key });

						parent = parent.ParentGroup;
					} while (parent != null && !parent.IsRoot);

					foreach (var dataField in dataFields)
					{
						// /fdfd:34/gfgfg:fdfd/gfgfgfggf
						var middle = string.Join('/', tgs.Select(tg => $"{Escape(tg.Name)}:{Escape(Convert.ToString(tg.Value) ?? string.Empty)}"));
						var combNAme = $"/{middle}/{Escape(dataField.FieldName)}";

						tablecols_after.Add(dataField.ToTableColumn(combNAme, tgs.Select(tg => tg.Value).ToArray()));
					}
				}

				//			tablecols_after = SortColGroupsCols(tablecols_after, colGroupFields);

				tablecols.AddRange(tablecols_after);
			}
			else
			{
				tablecols.AddRange(dataFields.Select(df => df.ToTableColumn()));
			}

			return tablecols;
		}


		/// <summary>
		///  : 	%3A
		///	/ 	%2F
		///   % 	%25
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static string Escape(string str)
		{
			StringBuilder sb = new();
			foreach (var c in str)
			{
				if (c == '/')
					sb.Append("%2F");
				else if (c == ':')
					sb.Append("%3A");
				else if (c == '%')
					sb.Append("%25");
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		//public class PathElement
		//{
		//	public string Name;
		//	public object Value;
		//}

		//public static PathElement[] SplitPathName(string str)
		//{
		//	if (!str.StartsWith('/'))
		//		throw new ArgumentException("Must start with '/'");

		//	var parts = str.Split('/');
		//	// make sure first part is empty

		//	PathElement[] res = new PathElement[parts.Length - 1];



		//}

		/// <summary>
		/// You have a column name.
		/// First, split it by '/'. Now have the groups.
		/// For every group, split by ':'. Now have the group name and the value.
		/// Next, Unescape the group name and the value.
		/// 
		/// 
		///  : 	%3A
		///	/ 	%2F
		///   % 	%25
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Unescape(string str)
		{
			StringBuilder sb = new();
			bool foundPerc = false;
			bool foundChar1 = false;
			char char1 = 'X';

			foreach (var c in str)
			{
				if (foundPerc)
				{
					if (foundChar1)
					{
						// now we have char2
						if (char1 == '3' && c == 'A')
						{
							sb.Append(':');
						}
						else if (char1 == '2' && c == 'F')
						{
							sb.Append('/');
						}
						else if (char1 == '2' && c == '5')
						{
							sb.Append('%');
						}
						else
						{
							throw new Exception($"Invalid escape code '%{char1}{c}'");
						}

						// reset
						foundPerc = false;
						foundChar1 = false;
						char1 = 'X';
					}
					else
					{
						foundChar1 = true;
						char1 = c;
					}
				}
				else if (c == '%')
				{
					foundPerc = true;
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public Table<object?[]> GetTableFastIntersect_objectArr()
		{
			var s1d = GetTableFastIntersect();
			return Next(s1d, (rows, tcols) => rows);
			//return GetTableFastIntersect((rows, tcols) => rows);
		}


		/*
		 * Example data
		 *     {
      "Region": "Asia",
      "Country": "Maldives",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderPriority": "C, H, L, M",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderDate": "2020-09-08T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderID": "102625882, 103654563, 105711925, 106797683, 107769286, 108995378, 110081136, 111109817, 113167178, 115224540, 116450631, 117479312, 118565070, 119536674, 120622432, 122679793, 123905885, 124934566, 126991927, 129049289, 130135047, 134447181, 136504542, 140816676, 141902434, 143959796, 148271930, 150329291, 151415050, 153612744, 155727183, 157784545, 159841907, 161067998, 162096679, 163182437, 164154040, 165239799, 167297160, 168523252, 169551932, 171609294, 173666656, 174752414, 175921428, 175978505, 177007186, 179064548, 181121909, 185434043, 186519801, 188577163, 192889297, 194946658, 196032416, 200344550, 202401912, 206714046, 207799804, 209857165, 213140618, 214169299, 216226661, 218284022, 219510114, 220538794, 220595872, 221624553, 223681914, 225739276, 227994048, 230051410, 231137168, 233194530, 237506663, 239564025, 240649783, 244961917, 247019279, 251331412, 252417171, 254474532, 258786666, 260844027, 261929786, 262901389, 264127480, 265213239, 266241919, 268299281, 270356643, 271582734, 272611415, 274668776, 275754535, 277811896, 279037988, 280066668, 282124030, 284181392, 285267150, 286493241, 289579284, 291636645, 295948779, 297034537, 299091899, 303404033, 305461394, 306547152, 310859286, 312916648, 316200101, 317228782, 318314540, 319286143, 320371901, 323655354, 324684035, 326741397, 328798758, 329884517, 331053531, 331110608, 332139289, 334196650, 336254012, 340566146, 341651904, 343709266, 348021399, 350078761, 351164519, 355476653, 357534015, 361846148, 362931907, 363903510, 364989268, 368272721, 369301402, 371358764, 373416125, 374642217, 375670897, 375727975, 376756656, 378814017, 380871379, 382097470, 383126151, 385183513, 386269271, 388326632, 389552724, 390581405, 392638766, 394696128, 395781886, 400094020, 402151381, 406463515, 407549273, 408520877, 409606635, 413918769, 415976130, 419259583, 420288264, 420345342, 421374022, 423431384, 425488746, 426714837, 427743518, 429800879, 430886638, 432943999, 434170091, 435198771, 437256133, 439313495, 440399253, 441568267, 441625344, 442654025, 444711387, 446768748, 451080882, 452166640, 454224002, 458536136, 460593497, 465991389, 468048751, 471332204, 472360885, 474418246, 475504004, 478787457, 479816138, 481873500, 483930861, 485156953, 486185634, 486242711, 487271392, 489328753, 491386115, 492612206, 493640887, 495698249, 496784007, 498841369, 503153502, 505210864, 506296622, 510608756, 512666118, 516978251, 518064010, 519035613, 520121371, 524433505, 526490867, 528548228, 529774320, 530803000, 530860078, 531888759, 533946120, 536003482, 537229573, 538258254, 540315616, 541401374, 543458735, 544684827, 545713508, 547770869, 549828231, 550913989, 552140080, 555226123, 557283484, 561595618, 562681376, 563652980, 564738738, 569050872, 571108233, 575420367, 576506125, 578563487, 581846940, 582875621, 584932982, 586018741, 589302194, 590330874, 592388236, 594445598, 595531356, 596700370, 596757447, 597786128, 599843490, 601900851, 606212985, 607298743, 608270347, 609356105, 613668239, 615725600, 620037734, 621123492, 623180854, 627492988, 629550349, 630636107, 633919560, 634948241, 637005603, 639062964, 640289056, 641317737, 641374814, 642403495, 644460856, 646518218, 647744309, 648772990, 650830352, 651916110, 653973472, 655199563, 656228244, 658285605, 660342967, 664655101, 665740859, 667798221, 672110354, 674167716, 675253474, 679565608, 681622970, 684906423, 685935103, 685992181, 687020862, 689078223, 691135585, 692361676, 693390357, 695447719, 696533477, 698590838, 699816930, 700845611, 702902972, 704960334, 706046092, 707215106, 708300864, 710358226, 712415587, 716727721, 718785083, 719870841, 724182975, 726240336, 730552470, 731638228, 733695590, 736979043, 738007724, 740065085, 741150844, 744434297, 745462977, 747520339, 749577701, 750803792, 751832473, 752918231, 754975593, 757032954, 758259046, 759287726, 761345088, 762430846, 763402450, 764488208, 768800342, 770857703, 775169837, 776255595, 778312957, 782625091, 784682452, 785768210, 790080344, 792137706, 794195067, 795421159, 796449840, 796506917, 797535598, 799592959, 801650321, 802876412, 803905093, 805962455, 807048213, 808019816, 809105575, 810331666, 811360347, 813417708, 815475070, 819787204, 820872962, 822930324, 827242457, 829299819, 830385577, 834697711, 836755073, 841067206, 842152965, 844210326, 847493779, 848522460, 850579822, 851665580, 853863275, 854949033, 855977714, 858035075, 860092437, 862347209, 863432967, 864404571, 865490329, 867547690, 871859824, 873917186, 875002944, 879315078, 881372439, 885684573, 886770331, 888827693, 893139827, 895197188, 896282947, 898480641, 899566400, 900595080, 902652442, 904709804, 905935895, 906964576, 908050334, 910107696, 912165057, 913391149, 914419829, 916477191, 918534553, 919620311, 920846402, 921875083, 923932445, 925989806, 930301940, 931387698, 933445060, 937757194, 939814555, 940900313, 945212447, 947269809, 950553262, 951581943, 952667701, 954725062, 956782424, 958008515, 959037196, 961094558, 962180316, 963151919, 964237678, 965463769, 966492450, 968549811, 970607173, 972861945, 974919307, 976005065, 978062427, 982374560, 984431922, 985517680, 989829814, 991887176, 996199309, 997285068, 999342429",
      "/ItemType:Baby Food/SalesChannel:Offline/ShipDate": "2020-10-23T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Offline/UnitsSold": 5678787,
      "/ItemType:Baby Food/SalesChannel:Offline/UnitPrice": 283871.3600000013,
      "/ItemType:Baby Food/SalesChannel:Offline/UnitCost": 177275.04000000245,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalRevenue": 1449680745.3600004,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalCost": 905312223.5399997,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalProfit": 544368521.82,
      "/ItemType:Baby Food/SalesChannel:Offline/RowCount": 1112,
      "/ItemType:Baby Food/SalesChannel:Online/OrderPriority": "C, H, L, M",
      "/ItemType:Baby Food/SalesChannel:Online/OrderDate": "2020-09-10T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Online/OrderID": "100371110, 104683244, 106740605, 107826364, 112138497, 114195859, 118507993, 119593751, 121651113, 125963246, 128020608, 129106366, 130077970, 131304061, 132389819, 133418500, 135475862, 137533223, 138759315, 139787995, 140873754, 141845357, 142931115, 144988477, 146214568, 147243249, 149300611, 151357972, 152443730, 153669822, 154698503, 156755864, 158813226, 163125360, 164211118, 166268479, 170580613, 172637975, 173723733, 178035867, 180093228, 183376681, 184405362, 185491120, 186462724, 187548482, 190831935, 191860616, 193917977, 195975339, 197061097, 198230111, 198287189, 199315869, 201373231, 203430593, 205685365, 207742726, 208828485, 210885846, 215197980, 217255342, 218341100, 222653234, 224710595, 229022729, 230108487, 232165849, 235449302, 236477982, 238535344, 240592706, 241818797, 242904555, 243933236, 245990598, 248047959, 249274051, 250302731, 252360093, 253445851, 255503213, 256729304, 257757985, 259815347, 261872708, 262958467, 267270600, 269327962, 273640096, 274725854, 276783215, 281095349, 283152711, 284238469, 286436164, 287521922, 288550603, 290607964, 292665326, 293891417, 294920098, 296005856, 296977460, 298063218, 300120580, 301346671, 302375352, 304432713, 306490075, 307575833, 308744847, 308801925, 309830605, 311887967, 313945329, 318257462, 319343221, 321400582, 325712716, 327770078, 328855836, 333167970, 335225331, 338508784, 339537465, 340623223, 341594827, 342680585, 345964038, 346992719, 349050080, 351107442, 352333533, 353362214, 353419291, 354447972, 356505334, 358562695, 360817468, 362874829, 363960587, 366017949, 370330083, 372387444, 373473203, 377785336, 379842698, 384154832, 385240590, 386212193, 387297952, 391610085, 393667447, 395724809, 396950900, 397979581, 398036658, 399065339, 401122701, 403180062, 404406154, 405434834, 407492196, 408577954, 410635316, 411861407, 412890088, 414947450, 417004811, 418090569, 419316661, 422402703, 424460065, 428772199, 429857957, 431915318, 436227452, 438284814, 442596948, 443682706, 445740067, 449023520, 450052201, 452109563, 453195321, 456478774, 457507455, 459564816, 461622178, 462707936, 463876950, 463934028, 464962708, 467020070, 469077432, 473389565, 474475324, 476532685, 480844819, 482902181, 483987939, 488300073, 490357434, 494669568, 496726930, 497812688, 501096141, 502124822, 504182183, 506239545, 507465636, 508494317, 508551394, 509580075, 511637437, 513694798, 514920890, 515949571, 518006932, 519092690, 521150052, 522376143, 523404824, 525462186, 527519547, 528605306, 532917439, 534974801, 539286935, 540372693, 541344296, 542430055, 546742188, 548799550, 552083003, 553111684, 553168761, 554197442, 556254804, 558312165, 559538257, 560566937, 562624299, 563710057, 565767419, 566993510, 568022191, 570079553, 572136914, 573222672, 574391686, 574448764, 575477445, 577534806, 579592168, 583904302, 584990060, 585961663, 587047421, 591359555, 593416917, 597729051, 598814809, 600872170, 604155623, 605184304, 607241666, 608327424, 611610877, 612639558, 614696919, 616754281, 617980372, 619009053, 619066131, 620094811, 622152173, 624209535, 625435626, 626464307, 628521668, 629607427, 630579030, 631664788, 635976922, 638034284, 642346417, 643432176, 645489537, 649801671, 651859033, 652944791, 657256925, 659314286, 661371648, 662597739, 663626420, 663683497, 664712178, 666769540, 668826901, 670052993, 671081674, 673139035, 674224793, 676282155, 677508246, 678536927, 680594289, 682651650, 688049542, 690106904, 694419038, 696476399, 697562158, 701874291, 703931653, 708243787, 709329545, 711386907, 714670360, 715699040, 717756402, 718842160, 722125613, 723154294, 725211656, 727269017, 728354775, 729523789, 730609548, 732666909, 734724271, 739036405, 740122163, 741093766, 742179524, 746491658, 748549020, 752861154, 753946912, 756004273, 760316407, 762373769, 763459527, 766742980, 767771661, 769829022, 771886384, 773112475, 774141156, 774198234, 775226914, 777284276, 779341638, 780567729, 781596410, 783653771, 784739530, 785711133, 786796891, 788022983, 789051663, 791109025, 793166387, 797478520, 798564279, 800621640, 804933774, 806991136, 808076894, 812389028, 814446389, 817729842, 818758523, 818815600, 819844281, 821901643, 823959004, 825185096, 826213777, 828271138, 829356896, 830328500, 831414258, 832640349, 833669030, 835726392, 837783753, 840038526, 841124284, 842095887, 843181645, 845239007, 849551141, 851608502, 852694261, 857006394, 859063756, 863375890, 864461648, 866519010, 869802463, 870831143, 872888505, 873974263, 876171958, 877257716, 878286397, 880343759, 882401120, 883627212, 884655892, 885741651, 887799012, 889856374, 891082465, 892111146, 894168508, 896225869, 897311627, 901623761, 903681123, 907993257, 909079015, 911136376, 915448510, 917505872, 918591630, 920789325, 922903764, 924961125, 927018487, 928244578, 929273259, 930359017, 932416379, 934473741, 935699832, 936728513, 938785874, 940843236, 941928994, 943155086, 944183766, 946241128, 948298490, 952610623, 953696382, 955753743, 960065877, 962123239, 963208997, 967521131, 969578492, 973890626, 974976384, 977033746, 980317199, 981345880, 983403241, 984488999, 985460603, 986686694, 987772452, 988801133, 990858495, 992915856, 995170629, 996256387, 997227990, 998313748",
      "/ItemType:Baby Food/SalesChannel:Online/ShipDate": "2020-10-26T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Online/UnitsSold": 5475800,
      "/ItemType:Baby Food/SalesChannel:Online/UnitPrice": 289742.80000000197,
		 * 
		 * */

		public Table<IDictionary<string, object?>> GetTableFastIntersect_DictArr()
		{
			var s1d = GetTableFastIntersect();

			return Next<IDictionary<string, object?>>(s1d, (rows, tcols) =>
			{
				List<IDictionary<string, object?>> dictRows = new();

				foreach (var row in rows)
				{
					//Dictionary<string, object?> dictRow = new();
					//foreach (var v in row.Zip(tcols))
					//	dictRow.Add(v.Second.Name, v.First);

					var dictRow = new WrapperObj(row, tcols);

					dictRows.Add(dictRow);
				}

				return dictRows;
			});
		}



		/*
		 * ONly nested one level for now
		 * Example data;:
		 * {
		 *   "Country": "Maldives",
      "/ItemType:Baby Food/SalesChannel:Offline":
      {
        "OrderPriority": "C, H, L, M",
        "OrderDate": "2020-09-08T00:00:00",
        "OrderID": "102625882, 103654563, 105711925, 106797683, 107769286, 108995378, 110081136, 111109817, 113167178, 115224540, 116450631, 117479312, 118565070, 119536674, 120622432, 122679793, 123905885, 124934566, 126991927, 129049289, 130135047, 134447181, 136504542, 140816676, 141902434, 143959796, 148271930, 150329291, 151415050, 153612744, 155727183, 157784545, 159841907, 161067998, 162096679, 163182437, 164154040, 165239799, 167297160, 168523252, 169551932, 171609294, 173666656, 174752414, 175921428, 175978505, 177007186, 179064548, 181121909, 185434043, 186519801, 188577163, 192889297, 194946658, 196032416, 200344550, 202401912, 206714046, 207799804, 209857165, 213140618, 214169299, 216226661, 218284022, 219510114, 220538794, 220595872, 221624553, 223681914, 225739276, 227994048, 230051410, 231137168, 233194530, 237506663, 239564025, 240649783, 244961917, 247019279, 251331412, 252417171, 254474532, 258786666, 260844027, 261929786, 262901389, 264127480, 265213239, 266241919, 268299281, 270356643, 271582734, 272611415, 274668776, 275754535, 277811896, 279037988, 280066668, 282124030, 284181392, 285267150, 286493241, 289579284, 291636645, 295948779, 297034537, 299091899, 303404033, 305461394, 306547152, 310859286, 312916648, 316200101, 317228782, 318314540, 319286143, 320371901, 323655354, 324684035, 326741397, 328798758, 329884517, 331053531, 331110608, 332139289, 334196650, 336254012, 340566146, 341651904, 343709266, 348021399, 350078761, 351164519, 355476653, 357534015, 361846148, 362931907, 363903510, 364989268, 368272721, 369301402, 371358764, 373416125, 374642217, 375670897, 375727975, 376756656, 378814017, 380871379, 382097470, 383126151, 385183513, 386269271, 388326632, 389552724, 390581405, 392638766, 394696128, 395781886, 400094020, 402151381, 406463515, 407549273, 408520877, 409606635, 413918769, 415976130, 419259583, 420288264, 420345342, 421374022, 423431384, 425488746, 426714837, 427743518, 429800879, 430886638, 432943999, 434170091, 435198771, 437256133, 439313495, 440399253, 441568267, 441625344, 442654025, 444711387, 446768748, 451080882, 452166640, 454224002, 458536136, 460593497, 465991389, 468048751, 471332204, 472360885, 474418246, 475504004, 478787457, 479816138, 481873500, 483930861, 485156953, 486185634, 486242711, 487271392, 489328753, 491386115, 492612206, 493640887, 495698249, 496784007, 498841369, 503153502, 505210864, 506296622, 510608756, 512666118, 516978251, 518064010, 519035613, 520121371, 524433505, 526490867, 528548228, 529774320, 530803000, 530860078, 531888759, 533946120, 536003482, 537229573, 538258254, 540315616, 541401374, 543458735, 544684827, 545713508, 547770869, 549828231, 550913989, 552140080, 555226123, 557283484, 561595618, 562681376, 563652980, 564738738, 569050872, 571108233, 575420367, 576506125, 578563487, 581846940, 582875621, 584932982, 586018741, 589302194, 590330874, 592388236, 594445598, 595531356, 596700370, 596757447, 597786128, 599843490, 601900851, 606212985, 607298743, 608270347, 609356105, 613668239, 615725600, 620037734, 621123492, 623180854, 627492988, 629550349, 630636107, 633919560, 634948241, 637005603, 639062964, 640289056, 641317737, 641374814, 642403495, 644460856, 646518218, 647744309, 648772990, 650830352, 651916110, 653973472, 655199563, 656228244, 658285605, 660342967, 664655101, 665740859, 667798221, 672110354, 674167716, 675253474, 679565608, 681622970, 684906423, 685935103, 685992181, 687020862, 689078223, 691135585, 692361676, 693390357, 695447719, 696533477, 698590838, 699816930, 700845611, 702902972, 704960334, 706046092, 707215106, 708300864, 710358226, 712415587, 716727721, 718785083, 719870841, 724182975, 726240336, 730552470, 731638228, 733695590, 736979043, 738007724, 740065085, 741150844, 744434297, 745462977, 747520339, 749577701, 750803792, 751832473, 752918231, 754975593, 757032954, 758259046, 759287726, 761345088, 762430846, 763402450, 764488208, 768800342, 770857703, 775169837, 776255595, 778312957, 782625091, 784682452, 785768210, 790080344, 792137706, 794195067, 795421159, 796449840, 796506917, 797535598, 799592959, 801650321, 802876412, 803905093, 805962455, 807048213, 808019816, 809105575, 810331666, 811360347, 813417708, 815475070, 819787204, 820872962, 822930324, 827242457, 829299819, 830385577, 834697711, 836755073, 841067206, 842152965, 844210326, 847493779, 848522460, 850579822, 851665580, 853863275, 854949033, 855977714, 858035075, 860092437, 862347209, 863432967, 864404571, 865490329, 867547690, 871859824, 873917186, 875002944, 879315078, 881372439, 885684573, 886770331, 888827693, 893139827, 895197188, 896282947, 898480641, 899566400, 900595080, 902652442, 904709804, 905935895, 906964576, 908050334, 910107696, 912165057, 913391149, 914419829, 916477191, 918534553, 919620311, 920846402, 921875083, 923932445, 925989806, 930301940, 931387698, 933445060, 937757194, 939814555, 940900313, 945212447, 947269809, 950553262, 951581943, 952667701, 954725062, 956782424, 958008515, 959037196, 961094558, 962180316, 963151919, 964237678, 965463769, 966492450, 968549811, 970607173, 972861945, 974919307, 976005065, 978062427, 982374560, 984431922, 985517680, 989829814, 991887176, 996199309, 997285068, 999342429",
        "ShipDate": "2020-10-23T00:00:00",
        "UnitsSold": 5678787,
        "UnitPrice": 283871.3600000013,
        "UnitCost": 177275.04000000245,
        "TotalRevenue": 1449680745.3600004,
        "TotalCost": 905312223.5399997,
        "TotalProfit": 544368521.82,
        "RowCount": 1112,
      }
		 * 
		 * */
		public Table<KeyValueClass<T>> GetTableFastIntersect_DictArrNEsted()
		{
			var s1d = GetTableFastIntersect();

			return NextDictNested(s1d);
			//return Next<IDictionary<string, object?>>(s1d, (rows, tcols) =>
			//{
			//	List<IDictionary<string, object?>> dictRows = new();

			//	foreach (var row in rows)
			//	{
			//		//Dictionary<string, object?> dictRow = new();
			//		//foreach (var v in row.Zip(tcols))
			//		//	dictRow.Add(v.Second.Name, v.First);

			//		var dictRow = new WrapperObj(row, tcols);

			//		dictRows.Add(dictRow);
			//	}

			//	return dictRows;
			//});
		}

		private Table<KeyValueClass<T>> NextDictNested(Pivoter<T>.Step1Data s1d)
		{
			var lastRowGroups = s1d.allRowGroups.Last();
			var firstColGroups = s1d.allColGroups.First();
			var lastColGroups = s1d.allColGroups.Last();

			var colFieldsInSortOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup)
				.Where(f => f.Sorting != Sorting.None)
				.OrderBy(f => f.SortIndex).ToArray();

			var lastColGroupsSorted = SortColGroups(lastColGroups, colFieldsInSortOrder, ele => ele).ToList();


			// create a new GetFullRows that create nested objects





			var tableCols = CreateTableCols(s1d.dataFields, s1d.rowFieldsInGroupOrder, lastColGroupsSorted);
			//			rowsss = SortRowsNew(rowsss, tableCols);

			Table<KeyValueClass<T>> t = new Table<KeyValueClass<T>>();

			//	
			//	
			//			t.Rows = toRows(rowsss, tableCols);

			var rowsss = GetFullRowsNestedDict(s1d.dataFields, s1d.rowFieldsInGroupOrder, lastRowGroups,
				lastColGroupsSorted, firstColGroups, tableCols);

			t.Rows = rowsss;
			t.Columns = tableCols;
			t.RowGroups = s1d.rowFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();
			t.ColumnGroups = s1d.colFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();


			return t;
		}

		class WrapperObj : IDictionary<string, object?>
		{
			object?[] _row;
			List<TableColumn> _tcols;

			public WrapperObj(object?[] row, List<TableColumn> tcols)
			{
				_row = row;
				_tcols = tcols;
			}

			public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
			{
				var a = _row.Zip(_tcols).Select(a => new KeyValuePair<string, object?>(a.Second.Name, a.First));
				return a.GetEnumerator();
				//throw new NotImplementedException();
			}


			public object? this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ICollection<string> Keys => throw new NotImplementedException();

			public ICollection<object?> Values => throw new NotImplementedException();

			public int Count => throw new NotImplementedException();

			public bool IsReadOnly => throw new NotImplementedException();

			public void Add(string key, object? value)
			{
				throw new NotImplementedException();
			}

			public void Add(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(string key)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}



			public bool Remove(string key)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}



		class WrapperObjNested : IDictionary<string, object?>
		{
			public object?[] _row;
			public List<TableColumn> _tcols;

			public WrapperObjNested(object?[] row, List<TableColumn> tcols)
			{
				_row = row;
				_tcols = tcols;
			}

			public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
			{
				var a = _row.Zip(_tcols).Select(a => new KeyValuePair<string, object?>(a.Second.Name, a.First));
				return a.GetEnumerator();
				//throw new NotImplementedException();
			}


			public object? this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ICollection<string> Keys => throw new NotImplementedException();

			public ICollection<object?> Values => throw new NotImplementedException();

			public int Count => throw new NotImplementedException();

			public bool IsReadOnly => throw new NotImplementedException();

			public void Add(string key, object? value)
			{
				throw new NotImplementedException();
			}

			public void Add(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(string key)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}



			public bool Remove(string key)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}



		public class Step1Data
		{
			public Field[] rowFieldsInGroupOrder;
			public Field[] colFieldsInGroupOrder;

			public Field[] dataFields;

			//			public List<Group<T>> lastRowGroups;
			//		public List<Group<T>> lastColGroupsSorted;

			public List<List<Group<T>>> allRowGroups;
			public List<List<Group<T>>> allColGroups;
		}

		public Step1Data GetTableFastIntersect()
		{
			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<T>>> allRowGroups = GroupRows(rowFieldsInGroupOrder);
			List<List<Group<T>>> allRowThenColGroups = GroupRows(allRowGroups.Last(), colFieldsInGroupOrder);

			List<Group<T>> lastRowGroups = allRowGroups.Last();// GroupRows(rowFieldsInGroupOrder);
			List<Group<T>> lastRowThenColGroups = allRowThenColGroups.Last();// GroupRows(lastRowGroups, colFieldsInGroupOrder);

			// ha like mangle slike som vi har grouping levels
			//Dictionary<GroupingKey<object?>, Group<T>> htSynthMergedLastColGroups = new();

			Dictionary<(Group<T>?, object?), Group<T>>[] htSynthMergedAllColGroups = new Dictionary<(Group<T>?, object?), Group<T>>[colFieldsInGroupOrder.Length];


			foreach (var lastRowThenColGroup in lastRowThenColGroups)
			{
				if (colFieldsInGroupOrder.Length == 0)//lastRowAndColGroups == lastRowGroups)
				{
					// no col grouping
					lastRowThenColGroup.RowData = new object?[dataFields.Length];
				}

				var lastColGroup = CloneColGroups(lastRowThenColGroup, htSynthMergedAllColGroups);

				int dataFieldIdx = 0;
				foreach (var dataField in dataFields)
				{
					var getter = _props[dataField.FieldName];

					var theValue = getter.GetValue(lastRowThenColGroup.Rows);

					//					if (lastRowG == lastG)
					if (colFieldsInGroupOrder.Length == 0)//lastRowAndColGroups == lastRowGroups)
					{
						// no col grouping
						lastRowThenColGroup.RowData[dataFieldIdx] = theValue;
					}
					else
					{
						// has col groups
						Group<T> lastRowG = GetLastRowGroup(lastRowThenColGroup);

						lastRowG.IntersectData ??= new();

						if (!lastRowG.IntersectData.TryGetValue(lastColGroup, out var idata))
						{
							idata = new object?[dataFields.Length];
							lastRowG.IntersectData.Add(lastColGroup, idata);
						}

						idata[dataFieldIdx] = theValue;

						// theValue is the intersect of lastRowG (eg /site/Site1) and lastG (eg /feedType/type1)
					}

					dataFieldIdx++;
				}
			}

			//var syntLastColGroups = htSynthMergedLastColGroups.Values.ToList(); // TOLIST needed?
			var allColGroups = htSynthMergedAllColGroups
				.Select(g => g == null ? new List<Group<T>>() : g.Values.ToList()).ToList();



			return new Step1Data()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups
				//lastColGroupsSorted = lastColGroupsSorted,
				//lastRowGroups = lastRowGroups,

			};
		}


		public Table<TRow> Next<TRow>(Step1Data s1d, Func<List<object?[]>, List<TableColumn>, List<TRow>> toRows)
		{
			var lastRowGroups = s1d.allRowGroups.Last();
			var lastColGroups = s1d.allColGroups.Last();

			var colFieldsInSortOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup)
				.Where(f => f.Sorting != Sorting.None)
				.OrderBy(f => f.SortIndex).ToArray();

			var lastColGroupsSorted = SortColGroups(lastColGroups, colFieldsInSortOrder).ToList();


			// TODO: when writing to json, instead of writing full rows we could write objects........
			// I guess the method could have ended at this point....and some other code could work on this.
			// The code below work on it to produce flat tables.
			// But some other code could produce json nested objects...

			List<object?[]> rowsss = GetFullRows(s1d.dataFields, s1d.rowFieldsInGroupOrder, lastRowGroups, lastColGroupsSorted);

			//}
			// else...a mode for output 1:1?
			//{
			//	foreach (var l in _list)
			//	{
			//		var r = res.NewRow();

			//		foreach (Field dataField in GetDataFields())
			//		{
			//			var getter = _props[dataField.FieldName];
			//			// TODO: get value from multiple ROWS
			//			var theValue = getter.GetValue(l.Yield());

			//			r[dataField.FieldName] = theValue;
			//		}
			//	}
			//}


			var tableCols = CreateTableCols(s1d.dataFields, s1d.rowFieldsInGroupOrder, lastColGroupsSorted);

			rowsss = SortRowsNew(rowsss, tableCols);

			Table<TRow> t = new Table<TRow>();
			t.Rows = toRows(rowsss, tableCols);

			t.Columns = tableCols;
			t.RowGroups = s1d.rowFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();
			t.ColumnGroups = s1d.colFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();


			return t;
		}

		private Group<T> CloneColGroups(Group<T> lg, Dictionary<(Group<T>?, object?), Group<T>>[] lookupGroups)
		{
			Stack<Group<T>> st = new();

			var parent = lg;//.ParentGroup;
			do
			{
				st.Push(parent);

				parent = parent.ParentGroup;
			} while (parent != null && parent.Field.FieldType == FieldType.ColGroup);

			//return new GroupingKey<object>(st.ToArray());
			Group<T>? curr = null;
			int lvl = 0;
			while (st.Any())
			{
				var g = st.Pop();

				// TODO: lookup existing groups, based on level (or field) and key
				// Teh same level will always have same field so its same same.

				lookupGroups[lvl] ??= new();

				if (!lookupGroups[lvl].TryGetValue((curr, g.Key), out var res))
				{
					res = new Group<T>();
					res.Field = g.Field;
					res.Key = g.Key;
					res.ParentGroup = curr;
					if (g.Rows != null)
					{
						//	res.Rows = g.Rows;//.ToList(); // clone? mem vs speed?
						res.Rows = g.Rows.ToList(); // clone? mem vs speed?
					}
					lookupGroups[lvl].Add((curr, g.Key), res);
				}
				else
				{
					// TODO: dedup needed???
					if (g.Rows != null)
					{
						//if (res.Rows is List<T> exRows)
						//	exRows.AddRange(g.Rows);
						if (res.Rows != null)//is List<T> exRows)
						{
							//res.Rows = res.Rows.Concat(g.Rows); // mem vs speed ?
							((List<T>)res.Rows).AddRange(g.Rows); // mem vs speed ?
						}
						else
						{
							//res.Rows = g.Rows;// new List<T>(g.Rows);//.ToList(); // clone?
							res.Rows = g.Rows.ToList();

						}
					}
				}

				curr = res;
				lvl++;
			}

			return curr!;
		}

		private List<KeyValueClass<T>> GetFullRowsNestedDict(Field[] dataFields,
			Field[] rowFieldsInGroupOrder,
			List<Group<T>> lastRowGroups,
			List<Group<T>> lastColGroups, /* sorted */
			List<Group<T>> firstColGroups, /* not sorted */
			List<TableColumn> tableCols)
		{
			List<KeyValueClass<T>> rows = new();

			// funke dette uten colGroups?
			int colCount = rowFieldsInGroupOrder.Length;
			//			if (lastColGroups.Any())
			colCount += firstColGroups.Count;// * dataFields.Length);
											 //		else
											 //		colCount += dataFields.Length;

			Dictionary<Group<T>, int> grpStartIdx = new();
			int totalStartIdx = rowFieldsInGroupOrder.Length;

			// FIXME: vi må starte med firstColGroups her!!!!!!!!!! deretter nøste oss nedover.

			foreach (var colGrp in firstColGroups)
			{
				grpStartIdx.Add(colGrp, totalStartIdx);

				totalStartIdx++;

				//totalStartIdx += dataFields.Length;
				// produce name for the colGrp
				// produce starting index in row for colGrp
			}

			var colFieldsInSortOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup)
	.Where(f => f.Sorting != Sorting.None)
	.OrderBy(f => f.SortIndex).ToArray();

			Dictionary<Group<T>, string> groupNameLookup = new();

			object?[] defaultValues = null;// new object?[dataFields.Length];

			foreach (var lastRowGroup in lastRowGroups)
			{
				//object?[] aRow = new object?[colCount];
				KeyValueClass<T> row = new KeyValueClass<T>();
				rows.Add(row);

				// TODO: write rowGroup values
				//int rowFieldIdx = 0;
				//foreach (var rowField in rowFieldsInGroupOrder)
				//{
				//	row[rowFieldIdx] = lastRowGroup.GetKeyByField(rowField);
				//	rowFieldIdx++;
				//}

				var current = lastRowGroup;
				//			int par_idx = rowFieldsInGroupOrder.Length - 1;
				do
				{
					//					aRow[par_idx] = parent.Key;
					row.Add(current.Field.FieldName, current.Key);
					current = current.ParentGroup;
					//					par_idx--;
				} while (current != null && !current.IsRoot);

				if (lastColGroups.Any())
				{

					//KeyValuePair<Group<T>, object?[]>
					foreach (var idata in SortColGroups(lastRowGroup.IntersectData, colFieldsInSortOrder, ele => ele.Key))
					{
						var lastColGroup = idata.Key;
						var values = idata.Value;

						//KeyValueClass<T> sub_row = GetCreateSubRow(lastColGroup, row);

						//// sub_row



						KeyValueClass<T> sub_row = new KeyValueClass<T>();
						foreach (var z in dataFields.Zip(values))
						{
							sub_row.Add(z.First.FieldName, z.Second);
						}

						string grpNamePath = GetGrpNamePath(lastColGroup, groupNameLookup);
						row.Add(grpNamePath, sub_row);
					}

					//					Stack<WrapperObjNested>

					// this produce one row in the table
					//foreach (var lastColGrp in lastColGroups)
					//{
					//	var startIdx = grpStartIdx[lastColGrp];

					//	if (lastRowGroup.IntersectData.TryGetValue(lastColGrp, out var values))
					//	{

					//		KeyValueClass sub_row = GetCreateSubRow(lastColGrp, row);

					//		foreach (var z in dataFields.Zip(values))
					//		{
					//			sub_row.Add(z.First.FieldName, z.Second);
					//		}

					//		// write values
					//		//Array.Copy(values, 0, row, startIdx, values.Length);

					//		//							colGrp.ParentGroup

					//		// push this on the parent somehow...

					//		//WrapperObjNested nrow = new WrapperObjNested(aRow, tableCols);


					//		//							aRow[startIdx] = nrow;
					//	}
					//}
				}
				else
				{
					throw new NotImplementedException();
					//Array.Copy(lastRowGroup.RowData, 0, aRow, totalStartIdx, lastRowGroup.RowData.Length);
				}

				//WrapperObjNested row = new WrapperObjNested(aRow, tableCols);
				//rows.Add(row);
			}

			return rows;
		}

		private string GetGrpNamePath(Group<T> lastColGroup, Dictionary<Group<T>, string> dict)
		{
			if (!dict.TryGetValue(lastColGroup, out var name))
			{

				Stack<TableGroup> tgs = new();

				var current = lastColGroup;
				do
				{
					tgs.Push(new TableGroup
					{
						Name = current.Field.FieldName,
						//DataType = parent.Field.DataType,
						Value = current.Key
					});

					current = current.ParentGroup;
				} while (current != null && !current.IsRoot);

				//			foreach (var dataField in dataFields)
				{
					// /fdfd:34/gfgfg:fdfd/gfgfgfggf
					name = string.Join('/', tgs.Select(tg => $"{Escape(tg.Name)}:{Escape(Convert.ToString(tg.Value) ?? string.Empty)}"));
					//var combNAme = $"/{middle}/{Escape(dataField.FieldName)}";

					//					tablecols_after.Add(dataField.ToTableColumn(combNAme, tgs.Select(tg => tg.Value).ToArray()));
				}

				dict.Add(lastColGroup, name);
			}
			return name;
		}

		private Pivoter<T>.KeyValueClass<T> GetCreateSubRow(Group<T> lastColGrp, Pivoter<T>.KeyValueClass<T> row)
		{
			Stack<Group<T>> st = new();

			var current = lastColGrp;
			do
			{
				st.Push(current);
				current = current.ParentGroup;
			} while (current != null && !current.IsRoot);


			while (st.Any())
			{
				var grp = st.Pop();
				//Pivoter<T>.KeyValueClass<T>  sr = GetCreateSingleSubRow(pop, row);


				if (row.Group == null)
				{
					// root
					//					row.TryGetSubRow(grp.Field.f)
				}
				else if (row.Group == grp)
				{

				}
				else
				{
					var srow = new KeyValueClass<T>();
					srow.Group = grp;
					//srow.Add(grp.Key.ToString(), );
					//row.Add(grp.Field.FieldName, srow);
					row.Add(grp.Key?.ToString() ?? "", srow);

				}


			}

			//var last = row.Last();
			//while (last != null && st.Any())
			//{
			//	var p = st.Pop();
			//	if (p.Field.FieldName == last.Value.Key)
			//	{
			//		//last = ((KeyValueClass)last.Value.Value!).Last();
			//		continue;
			//	}
			//}

			throw new NotImplementedException();
		}

		private Pivoter<T>.KeyValueClass<T> GetCreateSingleSubRow(Group<T> grp, Pivoter<T>.KeyValueClass<T> row)
		{
			//return row.GetOrCreate(grp);
			throw new NotImplementedException();
		}

		public class KeyValueClass<T> : IDictionary<string, object?>
			where T : class
		{
			public Group<T> Group = null!;

			List<KeyValuePair<string, object?>> _list = new();

			public object? this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ICollection<string> Keys => throw new NotImplementedException();

			public ICollection<object?> Values => throw new NotImplementedException();

			public int Count => throw new NotImplementedException();

			public bool IsReadOnly => throw new NotImplementedException();

			public void Add(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(string key)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public bool Remove(string key)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<string, object?> item)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
			{
				throw new NotImplementedException();
			}

			internal void Add(string fieldName, object? key)
			{
				var kv = new KeyValuePair<string, object?>(fieldName, key);
				_list.Add(kv);
			}

			//internal KeyValueClass<T> GetOrCreate(Group<T> grp)
			//{
			//	if (Group == null)
			//		throw new Exception();
			//	if (Group == grp)
			//		return this;

			//}

			internal KeyValuePair<string, object?>? Last()
			{
				return _list.Last();
			}

			void IDictionary<string, object?>.Add(string key, object? value)
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		private List<object?[]> GetFullRows(Field[] dataFields, Field[] rowFieldsInGroupOrder, List<Group<T>> lastRowGroups, List<Group<T>> lastColGroups /* sorted */)
		{
			List<object?[]> rows = new();

			// funke dette uten colGroups?
			int colCount = rowFieldsInGroupOrder.Length;
			if (lastColGroups.Any())
				colCount += (lastColGroups.Count * dataFields.Length);
			else
				colCount += dataFields.Length;

			Dictionary<Group<T>, int> grpStartIdx = new();
			int totalStartIdx = rowFieldsInGroupOrder.Length;
			foreach (var colGrp in lastColGroups)
			{
				grpStartIdx.Add(colGrp, totalStartIdx);

				totalStartIdx += dataFields.Length;
				// produce name for the colGrp
				// produce starting index in row for colGrp
			}

			object?[] defaultValues = null;// new object?[dataFields.Length];

			foreach (var lastRowGroup in lastRowGroups)
			{
				var row = new object?[colCount];

				// TODO: write rowGroup values
				//int rowFieldIdx = 0;
				//foreach (var rowField in rowFieldsInGroupOrder)
				//{
				//	row[rowFieldIdx] = lastRowGroup.GetKeyByField(rowField);
				//	rowFieldIdx++;
				//}

				var current = lastRowGroup;
				int par_idx = rowFieldsInGroupOrder.Length - 1;
				do
				{
					row[par_idx] = current.Key;
					current = current.ParentGroup;
					par_idx--;
				} while (current != null && !current.IsRoot);

				if (lastColGroups.Any())
				{
					// this produce one row in the table
					foreach (var lastColGroup in lastColGroups)
					{
						var startIdx = grpStartIdx[lastColGroup];

						if (lastRowGroup.IntersectData.TryGetValue(lastColGroup, out var values))
						{
							// write values
							Array.Copy(values, 0, row, startIdx, values.Length);
						}
						else
						{
							// write default values
							if (defaultValues == null)
							{
								throw new NotImplementedException();
							}
							Array.Copy(defaultValues, 0, row, startIdx, defaultValues.Length);
						}

					}
				}
				else
				{
					Array.Copy(lastRowGroup.RowData, 0, row, totalStartIdx, lastRowGroup.RowData.Length);
				}

				rows.Add(row);
			}

			return rows;
		}

		//private List<Group<T>> SortColGroupsOrg(List<Group<T>> colGrops, Field[] colFields)
		//{
		//	//.OrderBy(a => a.Key.Groups[0]).ThenBy(a => a.Key.Groups[1]).ToList();

		//	//var sortFields = _fields.Where(f => f.Grouping == Grouping.Col)
		//	//	.Where(f => f.Sorting != Sorting.None)
		//	//	.OrderBy(f => f.SortIndex)
		//	//	.ToArray();

		//	if (colFields.Any())
		//	{
		//		IOrderedEnumerable<Pivot.Group<T>> sorter = null!;

		//		int colFieldIdx = 0;
		//		foreach (var colField in colFields)
		//		{
		//			int colFieldIdx_local_capture = colFieldIdx;

		//			if (sorter == null)
		//				sorter = colField.Sorting == Sorting.Asc ?
		//					colGrops.OrderBy(r => r.GetKeyByField(colField))//.Key.Groups[colFieldIdx_local_capture]) 
		//					: colGrops.OrderByDescending(r => r.GetKeyByField(colField));
		//			else
		//				sorter = colField.Sorting == Sorting.Asc ?
		//					sorter.ThenBy(r => r.GetKeyByField(colField))
		//					: sorter.ThenByDescending(r => r.GetKeyByField(colField));

		//			colFieldIdx++;
		//		}

		//		colGrops = sorter.ToList(); // tolist needed?
		//	}

		//	return colGrops;

		//}

		private IEnumerable<TEle> SortColGroups<TEle>(IEnumerable<TEle> colGrops, Field[] colFields) where TEle : Group<T>
		{
			return SortColGroups<TEle>(colGrops, colFields, ele => ele);
		}
		

		//KeyValuePair<Group<T>, object?[]>
		private IEnumerable<TEle> SortColGroups<TEle>(IEnumerable<TEle> colGrops, Field[] colFields, Func<TEle, Group<T>> getGroup)
		{
			//.OrderBy(a => a.Key.Groups[0]).ThenBy(a => a.Key.Groups[1]).ToList();

			//var sortFields = _fields.Where(f => f.Grouping == Grouping.Col)
			//	.Where(f => f.Sorting != Sorting.None)
			//	.OrderBy(f => f.SortIndex)
			//	.ToArray();

			if (colFields.Any())
			{
				IOrderedEnumerable<TEle> sorter = null!;

				int colFieldIdx = 0;
				foreach (var colField in colFields)
				{
					int colFieldIdx_local_capture = colFieldIdx;

					if (sorter == null)
						sorter = colField.Sorting == Sorting.Asc ?
							colGrops.OrderBy(r => getGroup(r).GetKeyByField(colField))//.Key.Groups[colFieldIdx_local_capture]) 
							: colGrops.OrderByDescending(r => getGroup(r).GetKeyByField(colField));
					else
						sorter = colField.Sorting == Sorting.Asc ?
							sorter.ThenBy(r => getGroup(r).GetKeyByField(colField))
							: sorter.ThenByDescending(r => getGroup(r).GetKeyByField(colField));

					colFieldIdx++;
				}

				colGrops = sorter.ToList(); // tolist needed?
			}

			return colGrops;

		}


		private Group<T> GetLastRowGroup(Group<T> lastG)
		{
			// FIXME: handle IsRoot

			var current = lastG;
			while (current.ParentGroup != null && current.Field.FieldType != FieldType.RowGroup)
			{
				current = current.ParentGroup;
			}

			return current;
		}
	}

	public class TableGroup
	{
		public string Name { get; set; }
		//public object DataType { get; set; }

		public object? Value { get; set; }

		//public TableGroup Parent { get; set; }
	}

	public static class CollExt
	{
		public static IEnumerable<T> Yield<T>(this T t)
		{
			// Alternative: return new[] { t }; is somewhat faster, 10 vs 12 seconds on 2 million calls, but maybe more heavy on memory?
			yield return t;
		}
	}

	public class GroupingKey<T> : IEquatable<GroupingKey<T>>
	{
		public T[] Groups { get; init; }
		static EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

		public GroupingKey(T[] groups)
		{
			Groups = groups;
		}

		public override int GetHashCode()
		{
			var hc = new HashCode();
			foreach (var g in Groups)
				hc.Add(g);
			return hc.ToHashCode();
		}

		public override bool Equals(object? other)
		{
			return Equals(other as GroupingKey<T>);
		}

		public bool Equals(GroupingKey<T>? other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (Groups.Length != other.Groups.Length)
				return false;

			for (int i = 0; i < Groups.Length; i++)
			{
				if (!equalityComparer.Equals(Groups[i], other.Groups[i]))
					return false;
			}

			return true;
		}

		public override string ToString()
		{
			string[] array = new string[Groups.Length];
			for (int i = 0; i < Groups.Length; i++)
				array[i] = $"Group{i} = {Groups[i]}";

			return $"{{ {string.Join(", ", array)} }}";
		}
	}
}
