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

#endif