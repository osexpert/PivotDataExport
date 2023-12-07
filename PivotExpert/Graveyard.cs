#if false
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



	

//		public abstract class Column<RT, PT> : PropertyDescriptor
//		{
//			//Type _propType;
//	//		Func<object?, object> _getVal;

//			public Column(string propName)//, Type propType)//, Func<object?, object> getVal)
//				: base(propName, null)
//			{
//				//_propType = propType;
////				_getVal = getVal;
//			}

//			public override object? GetValue(object? component)
//			{
//	//			if (component is RT r)
////					return GetRowValue(r);
//				if (component is IEnumerable<RT> rows)
//					return GetRowValue(rows);
//				else
//					throw new Exception("unk type");
//			}

//			protected abstract PT GetRowValue(IEnumerable<RT> rows);

//			//private object GetRowValue(RT r)
//			//{
//			//	throw new NotImplementedException();
//			//}

//			//public abstract object GetRowValue()

//			public override Type PropertyType => typeof(PT);

//			public override void ResetValue(object component)
//			{
//				// Not relevant.
//			}

//			public override void SetValue(object? component, object? value) => throw new NotImplementedException();

//			public override bool ShouldSerializeValue(object component) => true;
//			public override bool CanResetValue(object component) => false;

//			public override Type ComponentType => typeof(RT);
//			public override bool IsReadOnly => true;
//		}












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


#endif