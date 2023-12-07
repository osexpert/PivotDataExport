using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{
	public class Pivoter<TRow> where TRow : class // notnull
	{
		List<Field> _fields;
		IEnumerable<TRow> _rows;
		Dictionary<string, PropertyDescriptor> _props;

		// TODO: change to dict? the same logic apply here, can only be one field per fieldname
		public List<Field> Fields => _fields;

		public IReadOnlyDictionary<string, PropertyDescriptor> Props => _props;

		public Pivoter(IEnumerable<TRow> rows, IEnumerable<PropertyDescriptor> props)
		: this(rows, props, Field.CreateFieldsFromProps(props))
		{

		}

		public Pivoter(IEnumerable<TRow> rows, PropertyDescriptorCollection props, IEnumerable<Field> fields) 
			: this(rows, props.Cast<PropertyDescriptor>(), fields)
		{
		}


		//public Pivoter(IEnumerable<TRow> rows, IEnumerable<Field> fields) : this(rows, fields, TypeDescriptor.GetProperties(typeof(TRow)))
		//{

		//}

		//public Pivoter(IEnumerable<TRow> rows, IEnumerable<Field> fields, ITypedList typedList) : this(rows, fields, typedList.GetItemProperties(null!))
		//{

		//}

		public Pivoter(IEnumerable<TRow> rows, IEnumerable<PropertyDescriptor> props, IEnumerable<Field> fields)
		{

			//if (props.Cast<PropertyDescriptor>().Any(p => p.Name.StartsWith('/')))
			//	throw new ArgumentException("Can not sort on data field if grouping on columns");



			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;

			_fields = fields.ToList();

			// pdc: aggregator\data getter
			_props = props.ToDictionary(pd => pd.Name);

			//foreach (var field in _fields)
			//{
			//	var prop = _props[field.FieldName];
			//}
		}

		private void Validate()
		{
			if (_fields.Any(f => f.FieldType == FieldType.ColGroup) && _fields.Any(f => f.FieldType == FieldType.Data && f.Sorting != Sorting.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.Any(f => f.FieldName.StartsWith('/')))
				throw new ArgumentException("FieldName can not start with reserved char '/'");

			if (_props.Values.Any(p => p.Name.StartsWith('/')))
				throw new ArgumentException("Property.Name can not start with reserved char '/'");

		}

		//public static IEnumerable<Field> CreateDefaultFields(IEnumerable<PropertyDescriptor> props)
		//{
		//	return props.Select(p => new Field
		//	{
		//		FieldName = p.Name,
		//		DataType = p.PropertyType
		//	});
		//}

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





		private List<List<Group<TRow>>> GroupRows(IEnumerable<Field> fields, bool sort = false)
		{
			List<Group<TRow>> lastGroups = new List<Group<TRow>>();
			lastGroups.Add(new Group<TRow> { Rows = _rows, IsRoot = true });

			return GroupRows(lastGroups, fields, sort: sort);
		}

		private List<List<Group<TRow>>> GroupRows(List<Group<TRow>> lastGroups, IEnumerable<Field> fields, bool freeOriginalLastGroupsMem = true, bool sort = false)
		{
			List<List<Group<TRow>>> listRes = new();

			//if (!fields.Any())
			//{
			//	// make sure we include root
			//	listRes.Add(lastGroups);
			//	return listRes;
			//}

			List<Group<TRow>> originalLastGroups = lastGroups;

			//			List<Group<T>> lastGroups = new List<Group<T>>();
			//		lastGroups.Add(new Group<T> { Rows = _list, IsRoot = true });

			foreach (Field gf in fields)
			{
				var getter = _props[gf.FieldName];

				var allSubGroups = new List<Group<TRow>>();

				foreach (var go in lastGroups)
				{
					var subGroups = go.Rows.GroupBy(r => getter.GetValue(r.Yield()), gf.Comparer).Select(g => new Group<TRow>()
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


		private IEnumerable<Field> GetDataFields()
		{
			return _fields.Where(f => f.FieldType == FieldType.Data);//.OrderBy(f => f.Index);
		}

		//private IEnumerable<Field> GetGroupFields()
		//{
		//	return _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex)
		//		.Concat(_fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex));
		//}



		public GroupedData<TRow> GetGroupedData_SlowIntersect()
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder);
			List<List<Group<TRow>>> allColGroups = GroupRows(colFieldsInGroupOrder);


			if (!allRowGroups.Any())
			{
				var list = new List<Group<TRow>>();
				list.Add(new Group<TRow>() { IsRoot = true, Rows = _rows });
				allRowGroups.Add( list);
			}

			List<Group<TRow>> lastRowGroups = allRowGroups.LastOrDefault() ?? new() { new Group<TRow>() { IsRoot = true, Rows = _rows } };
			

			List<Group<TRow>> lastColGroups = allColGroups.LastOrDefault();

			foreach (var lastRowGroup in lastRowGroups)
			{
				if (lastColGroups == null)
				{
					// no col groups

					var data = new object?[dataFields.Length];

					int dataFieldIdx = 0;
					foreach (var dataField in dataFields)
					{
						var prop = _props[dataField.FieldName];
						var theValue = prop.GetValue(lastRowGroup.Rows);

						data[dataFieldIdx] = theValue;
						dataFieldIdx++;
					}

					lastRowGroup.RowData = data; 
				}
				else
				{
					// has col groups

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
			}

			//var colFieldsInSortOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup)
			//	.Where(f => f.Sorting != Sorting.None)
			//	.OrderBy(f => f.SortIndex).ToArray();

			//var lastColGroupsSorted = SortColGroups(lastColGroups, colFieldsInSortOrder).ToList();

			return new GroupedData<TRow>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				dataFields = dataFields,
				//lastColGroupsSorted = lastColGroupsSorted,
				//lastRowGroups = lastRowGroups,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups,
				_fields = _fields,
				_props = _props
			};
		}





		

	










		public GroupedData<TRow> GetGroupedData_FastIntersect()
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder);
			List<List<Group<TRow>>> allRowThenColGroups = colFieldsInGroupOrder.Any() ? GroupRows(allRowGroups.Last(), colFieldsInGroupOrder)
				: allRowGroups;

//			List<Group<TRow>> lastRowGroups = allRowGroups.Last();// GroupRows(rowFieldsInGroupOrder);
			List<Group<TRow>> lastRowThenColGroups = allRowThenColGroups.Last();// GroupRows(lastRowGroups, colFieldsInGroupOrder);

			// ha like mangle slike som vi har grouping levels
			//Dictionary<GroupingKey<object?>, Group<T>> htSynthMergedLastColGroups = new();

			Dictionary<(Group<TRow>?, object?), Group<TRow>>[] htSynthMergedAllColGroups = new Dictionary<(Group<TRow>?, object?), Group<TRow>>[colFieldsInGroupOrder.Length];


			foreach (var lastRowThenColGroup in lastRowThenColGroups)
			{
				Group<TRow> lastColGroup = null;
				if (!colFieldsInGroupOrder.Any())//lastRowAndColGroups == lastRowGroups)
				{
					// no col grouping
					lastRowThenColGroup.RowData = new object?[dataFields.Length];
				}
				else
				{
					// has col grouping
					lastColGroup = CloneColGroups(lastRowThenColGroup, htSynthMergedAllColGroups);
				}

				//if (allRowGroups.Any())
				//{
				//	// need to strip off row groups?
				//	lastColGroup = CloneColGroups(lastRowThenColGroup, htSynthMergedAllColGroups);
				//}
				//else
				//{
				//	// no need to do anything?
				//	lastColGroup = lastRowThenColGroup;
				//}

				int dataFieldIdx = 0;
				foreach (var dataField in dataFields)
				{
					var getter = _props[dataField.FieldName];

					var theValue = getter.GetValue(lastRowThenColGroup.Rows);

					//					if (lastRowG == lastG)
					if (!colFieldsInGroupOrder.Any())//lastRowAndColGroups == lastRowGroups)
					{
						// no col grouping
						lastRowThenColGroup.RowData[dataFieldIdx] = theValue;
					}
					else
					{
						// has col groups
						Group<TRow> lastRowG = GetLastRowGroup(lastRowThenColGroup);

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
				.Select(g => g == null ? new List<Group<TRow>>() : g.Values.ToList()).ToList();



			return new GroupedData<TRow>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups,
				_fields = _fields,
				_props = _props
				//lastColGroupsSorted = lastColGroupsSorted,
				//lastRowGroups = lastRowGroups,

			};
		}


		/// <summary>
		/// Return only col groups, so need to strip off the row groups in front.
		/// </summary>
		private Group<TRow> CloneColGroups(Group<TRow> lastRowThenColGroup, Dictionary<(Group<TRow>?, object?), Group<TRow>>[] lookupGroups)
		{
			Stack<Group<TRow>> st = new();

			var current = lastRowThenColGroup;
			do
			{
				st.Push(current);

				current = current.ParentGroup;
			} while (current != null && current.Field.FieldType == FieldType.ColGroup);

			//return new GroupingKey<object>(st.ToArray());
			Group<TRow>? curr = null;
			int lvl = 0;
			while (st.Any())
			{
				var g = st.Pop();

				// TODO: lookup existing groups, based on level (or field) and key
				// Teh same level will always have same field so its same same.

				lookupGroups[lvl] ??= new();

				if (!lookupGroups[lvl].TryGetValue((curr, g.Key), out var res))
				{
					res = new Group<TRow>();
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
							((List<TRow>)res.Rows).AddRange(g.Rows); // mem vs speed ?
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





		private KeyValueClass<TRow> GetCreateSubRow(Group<TRow> lastColGrp, KeyValueClass<TRow> row)
		{
			Stack<Group<TRow>> st = new();

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
					var srow = new KeyValueClass<TRow>();
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

		private KeyValueClass<TRow> GetCreateSingleSubRow(Group<TRow> grp, KeyValueClass<TRow> row)
		{
			//return row.GetOrCreate(grp);
			throw new NotImplementedException();
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

	

		private Group<TRow> GetLastRowGroup(Group<TRow> lastG)
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

	public class GroupedData<TRow> where TRow : class
	{
		public Field[] rowFieldsInGroupOrder;
		public Field[] colFieldsInGroupOrder;

		public Field[] dataFields;

		//			public List<Group<T>> lastRowGroups;
		//		public List<Group<T>> lastColGroupsSorted;

		public List<List<Group<TRow>>> allRowGroups;
		public List<List<Group<TRow>>> allColGroups;

		public List<Field> _fields;
		public Dictionary<string, PropertyDescriptor> _props;
	}





}
