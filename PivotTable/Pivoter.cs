using System.ComponentModel;
using System.Data;

namespace osexpert.PivotTable
{
	/// <summary>
	/// Group and aggregate rows
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	public class Pivoter<TRow> where TRow : class // notnull
	{
		List<Field> _fields;
		IEnumerable<TRow> _rows;
		Dictionary<string, PropertyDescriptor> _props;

		// TODO: change to dict? the same logic apply here, can only be one field per fieldname
		public List<Field> Fields => _fields;

		public IReadOnlyDictionary<string, PropertyDescriptor> Props => _props;

		public Pivoter(IEnumerable<TRow> rows, IEnumerable<PropertyDescriptor> props)
		: this(rows, props, Field.CreateFieldsFromProperties(props))
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
			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;
			_fields = fields.ToList();
			_props = props.ToDictionary(pd => pd.Name);
		}

		private void Validate()
		{
			if (_fields.Any(f => f.FieldType == FieldType.ColGroup) && _fields.Any(f => f.FieldType == FieldType.Data && f.SortOrder != SortOrder.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.Any(f => f.FieldName.StartsWith('/')))
				throw new ArgumentException("FieldName can not start with reserved char '/'");

			if (_props.Values.Any(p => p.Name.StartsWith('/')))
				throw new ArgumentException("Property.Name can not start with reserved char '/'");

			if (_fields.GroupBy(f => f.FieldName).Any(g => g.Count() > 1))
				throw new ArgumentException("More than one field with same fieldName");




		}

		private List<List<Group<TRow>>> GroupRows(IEnumerable<Field> fields, enRootType rootType)//, bool sort = false)
		{
			List<Group<TRow>> lastGroups = new List<Group<TRow>>();
			lastGroups.Add(new Group<TRow> { Rows = _rows, RootType = rootType });

			var res = GroupRows(lastGroups, fields);//, sort: sort);
//			if (!res.Any())
	//			return new List<List<Group<TRow>>>() { lastGroups };
			return res;
		}

		private List<List<Group<TRow>>> GroupRows(List<Group<TRow>> lastGroups, IEnumerable<Field> fields, bool freeOriginalLastGroupsMem = true)//, bool sort = false)
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
					var subGroups = go.Rows.GroupBy(r => getter.GetValue(r.Yield()), gf.GroupComparer).Select(g => new Group<TRow>()
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

					//if (sort)
					//{
					//	throw new Exception("never called");
					//	allSubGroups.AddRange(subGroups.OrderBy(sg => sg.Key)); // displayText or value?
					//}
					//else
					allSubGroups.AddRange(subGroups);
				}

				listRes.Add(allSubGroups);

				lastGroups = allSubGroups;
			}

			//return lastGroups;
			if (!listRes.Any())
				listRes.Add(originalLastGroups);
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



		/// <summary>
		/// For a 5 million rows example, this takes 4 min 19sec. So 13 times slower than FastIntersect.
		/// Used only for testing\benchmarking, as this code is shorter and easier than FastIntersect.
		/// </summary>
		/// <returns></returns>
		public GroupedData<TRow> GetGroupedData_SlowIntersect(bool createEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder, enRootType.Row);
			List<List<Group<TRow>>> allColGroups = GroupRows(colFieldsInGroupOrder, enRootType.Col);

			if (!allRowGroups.Any())
			{
				var list = new List<Group<TRow>>();
				list.Add(new Group<TRow>() { RootType = enRootType.Row, Rows = _rows });
				allRowGroups.Add( list);
			}

			List<Group<TRow>> lastRowGroups = allRowGroups.Last();
			List<Group<TRow>> lastColGroups = allColGroups.Last();

			foreach (var lastRowGroup in lastRowGroups)
			{
				lastRowGroup.IntersectData = new();

				foreach (var lastColGroup in lastColGroups)
				{
					var intersectRows = lastRowGroup.Rows.Intersect(lastColGroup.Rows).ToList();

					if (intersectRows.Any() || createEmptyIntersects)
					{
						var data = new object?[dataFields.Length];

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


			return new GroupedData<TRow>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				dataFields = dataFields,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups,
				fields = _fields,
				props = _props
			};
		}

		/// <summary>
		/// For a 5 million rows example, this takes 19sec. So 13 times faster than SlowIntersect.
		/// </summary>
		/// <returns></returns>
		public GroupedData<TRow> GetGroupedData_FastIntersect(bool createEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder, enRootType.Row);
			List<List<Group<TRow>>> allRowThenColGroups = GroupRows(allRowGroups.Last(), colFieldsInGroupOrder);

			List<Group<TRow>> lastRowThenColGroups = allRowThenColGroups.Last();

			Dictionary<(Group<TRow>?, object?), Group<TRow>>[] htSynthMergedAllColGroups = new Dictionary<(Group<TRow>?, object?), Group<TRow>>[colFieldsInGroupOrder.Length];

			var rootColGroup = new Group<TRow> { Rows = _rows, RootType = enRootType.Col };

			foreach (var lastRowThenColGroup in lastRowThenColGroups)
			{
				Group<TRow> lastColGroup = null;
				if (!colFieldsInGroupOrder.Any())
				{
					// no col grouping, use root
					lastColGroup = rootColGroup;
				}
				else
				{
					// has col grouping
					lastColGroup = CloneColGroups(rootColGroup, lastRowThenColGroup, htSynthMergedAllColGroups);
				}

				Group<TRow> lastRowG = GetLastRowGroup(lastRowThenColGroup);

				int dataFieldIdx = 0;
				foreach (var dataField in dataFields)
				{
					var getter = _props[dataField.FieldName];

					var theValue = getter.GetValue(lastRowThenColGroup.Rows);

					lastRowG.IntersectData ??= new();

					if (!lastRowG.IntersectData.TryGetValue(lastColGroup, out var idata))
					{
						idata = new object?[dataFields.Length];
						lastRowG.IntersectData.Add(lastColGroup, idata);
					}

					idata[dataFieldIdx] = theValue;

					// theValue is the intersect of lastRowG (eg /site/Site1) and lastG (eg /feedType/type1)

					dataFieldIdx++;
				}
			}

			//var syntLastColGroups = htSynthMergedLastColGroups.Values.ToList(); // TOLIST needed?
			var allColGroups = htSynthMergedAllColGroups
				.Select(g => g == null ? new List<Group<TRow>>() : g.Values.ToList()).ToList();

			var b1 = colFieldsInGroupOrder.Any();
			var b2 = allColGroups.Any();
			if (b1 != b2) throw new Exception("they should be the same...");

			if (!b2)
			{
				allColGroups.Add(new List<Group<TRow>>() { rootColGroup }); 
			}

			// because of the was we group, groups without rows are not intersected. So fill these groups with default data here.
			if (createEmptyIntersects)
			{
				object?[] defaultValues = null;
				foreach (var lastRowGroup in allRowGroups.Last())
				{
					foreach (var lastColGroup in allColGroups.Last())
					{
						if (!lastRowGroup.IntersectData.ContainsKey(lastColGroup))
						{
							// write default values
							if (defaultValues == null)
							{
								// aggregate with no rows = default value
								var defVals = new object?[dataFields.Length];
								int i = 0;
								foreach (var df in dataFields)
								{
									defVals[i++] = _props[df.FieldName].GetValue(Enumerable.Empty<TRow>());
								}
								defaultValues = defVals;
							}
							//Array.Copy(defaultValues, 0, row, startIdx, defaultValues.Length);
							lastRowGroup.IntersectData.Add(lastColGroup, defaultValues);
						}
					}
				}
			}

			return new GroupedData<TRow>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				allRowGroups = allRowGroups,
				allColGroups = allColGroups,
				fields = _fields,
				props = _props
			};
		}


		/// <summary>
		/// Return only col groups, so need to strip off the row groups in front.
		/// </summary>
		private Group<TRow> CloneColGroups(Group<TRow> rootColGroup, Group<TRow> lastRowThenColGroup, Dictionary<(Group<TRow>?, object?), Group<TRow>>[] lookupGroups)
		{
			Stack<Group<TRow>> st = new();

			var current = lastRowThenColGroup;
			do
			{
				st.Push(current);

				current = current.ParentGroup;
			} while (current != null && current.FieldType == FieldType.ColGroup);

			//return new GroupingKey<object>(st.ToArray());
			Group<TRow>? curr = rootColGroup;
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

		private Group<TRow> GetLastRowGroup(Group<TRow> lastG)
		{
			// FIXME: handle IsRoot

			var current = lastG;
			while (current.ParentGroup != null && current.FieldType != FieldType.RowGroup)
			{
				current = current.ParentGroup;
			}

			return current;
		}

		//public void Sort(GroupedData<TRow> data)
		//{
		//	//			data.rowFieldsInGroupOrder
		//	var comparer = Comparer<object>.Default;
		//	foreach (var grpLevel in data.allRowGroups)
		//	{
		//		var first = grpLevel.First();
		//		if (first.Field.SortOrder != SortOrder.None)
		//		{
		//			bool asc = first.Field.SortOrder == SortOrder.Asc;
		//			grpLevel.Sort((a, b) => asc ? comparer.Compare(a.Key, b.Key) : comparer.Compare(b.Key, a.Key));
		//		}
		//	}
		//}
	}

	public class GroupedData<TRow> where TRow : class
	{
		public Field[] rowFieldsInGroupOrder;
		public Field[] colFieldsInGroupOrder;

		public Field[] dataFields;

		public List<List<Group<TRow>>> allRowGroups;
		public List<List<Group<TRow>>> allColGroups;

		public List<Field> fields;
		public Dictionary<string, PropertyDescriptor> props;
	}
}
