using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PivotDataTable
{
	/// <summary>
	/// Group and aggregate rows
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	public class Pivoter<TRow> where TRow : class // class notnull
	{
		List<Field> _fields;
		IEnumerable<TRow> _rows;

		public List<Field> Fields => _fields;

		public Pivoter(IEnumerable<TRow> rows, IEnumerable<Field> fields)
		{
			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;
			_fields = fields.ToList();
			//_props = props.ToDictionary(pd => pd.Name);
		}

		private void Validate()
		{
			if (_fields.Any(f => f.Area == Area.Column) && _fields.Any(f => f.Area == Area.Data && f.SortOrder != SortOrder.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.GroupBy(f => f.Name).Any(g => g.Count() > 1))
				throw new ArgumentException("More than one field with same fieldName");
		}

		private List<List<Group<TRow>>> GroupRows(IEnumerable<Field> fields, RootType rootType)//, bool sort = false)
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
				//var getter = _props[gf.FieldName];

				var allSubGroups = new List<Group<TRow>>();

				foreach (var go in lastGroups)
				{
					var subGroups = go.Rows.GroupBy(r => gf.GetValue(r.Yield()), gf.GroupComparer).Select(g => new Group<TRow>()
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
			return _fields.Where(f => f.Area == Area.Data);//.OrderBy(f => f.Index);
		}

		//private IEnumerable<Field> GetGroupFields()
		//{
		//	return _fields.Where(f => f.FieldType == FieldType.RowGroup).OrderBy(f => f.GroupIndex)
		//		.Concat(_fields.Where(f => f.FieldType == FieldType.ColGroup).OrderBy(f => f.GroupIndex));
		//}



		public GroupedData<TRow, Lazy<KeyValueList>> GetGroupedData_FastIntersect2()//bool createEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Row).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Column).OrderBy(f => f.GroupIndex).ToArray();

			var ptb = new PivotTableBuilder<TRow, Lazy<KeyValueList>>(_rows, rows =>
			{
				// only leafs
				//if ((agg_ctx == AggregateContext.Row_ColumnAggregates || agg_ctx == AggregateContext.Row_Aggregates) && group is IGroup<KeyValueList?> g && !g.Children.Any())
				{
					return new Lazy<KeyValueList>(() =>
					{
						KeyValueList res = new();
						foreach (var dataField in dataFields)
						{
							var theValue = dataField.GetValue(rows);
							res.Add(dataField.Name, theValue);
						}
						return res;
					});
				}

				//return null;
			});

			foreach (var rowF in rowFieldsInGroupOrder)
			{
				ptb.AddRow((row => rowF.GetValue(row.Yield()), rowF));
			}
			foreach (var colF in colFieldsInGroupOrder)
			{
				ptb.AddColumn((col => colF.GetValue(col.Yield()), colF));
			}
			var rbl = ptb.Build();

			// flip so we get the last groups (they have no children)
			var lastRows = GetLast(rbl.Rows).ToList();// Flatten(rbl.Rows).Where(r => !r.Children.Any()).ToList();
			var lastCols = GetLast(rbl.ColumnAggregates).ToList();// Flatten(rbl.ColumnAggregates).Where(r => !r.Children.Any()).ToList();


			return new GroupedData<TRow, Lazy<KeyValueList>>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				//allRowGroups = allRowGroups,
				//allColGroups = allColGroups,
				PT = rbl,
				fields = _fields,
				//props = _props
				PT_lastCols = lastCols,
				PT_lastRows = lastRows
			};
		}

		/// <summary>
		/// return groups without children (Last groups)
		/// </summary>
		static IEnumerable<IGroup<TAgg>> GetLast<TAgg>(IEnumerable<IGroup<TAgg>> source)
		{
			return source.TopogicalSequenceDFS<IGroup<TAgg>>(d => d.Children).Where(r => !r.Children.Any());
		}


		//static IEnumerable<IGroup<TAgg>> Flatten<TAgg>(IEnumerable<IGroup<TAgg>> collection)
		//{
		//	foreach (var o in collection)
		//	{
		//		foreach (var t in Flatten(o.Children))
		//			yield return t;

		//		yield return o;
		//		//if (o. is IEnumerable<IGroup<TAgg>> oo)// oo && !(o is T))
		//		//{
		//		//	foreach (var t in Flatten(oo))
		//		//		yield return t;
		//		//}
		//		//else
		//		//	yield return o;
		//	}
		//}

		/// <summary>
		/// For a 5 million rows example, this takes 19sec. So 13 times faster than SlowIntersect.
		/// </summary>
		/// <returns></returns>
		public GroupedData<TRow, KeyValueList> GetGroupedData_FastIntersect()//bool createEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Row).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Column).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder, RootType.Row);
			List<List<Group<TRow>>> allRowThenColGroups = GroupRows(allRowGroups.Last(), colFieldsInGroupOrder);

			List<Group<TRow>> lastRowThenColGroups = allRowThenColGroups.Last();

			Dictionary<(Group<TRow>?, object?), Group<TRow>>[] htSynthMergedAllColGroups = new Dictionary<(Group<TRow>?, object?), Group<TRow>>[colFieldsInGroupOrder.Length];

			var rootColGroup = new Group<TRow> { Rows = _rows, RootType = RootType.Col };

			foreach (var lastRowThenColGroup in lastRowThenColGroups)
			{
				Group<TRow> lastColGroup = null!;
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
					//var getter =  _props[dataField.FieldName];

					var theValue = dataField.GetValue(lastRowThenColGroup.Rows);

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

				// free mem
				lastRowThenColGroup.Rows = null!;
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

			return new GroupedData<TRow, KeyValueList>()
			{
				colFieldsInGroupOrder = colFieldsInGroupOrder,
				rowFieldsInGroupOrder = rowFieldsInGroupOrder,
				dataFields = dataFields,
				lastRowGroups = allRowGroups.Last(),
				lastColGroups = allColGroups.Last(),
				fields = _fields,
				//props = _props
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
			} while (current != null && current.FieldType == Area.Column);

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
			while (current.ParentGroup != null && current.FieldType != Area.Row)
			{
				current = current.ParentGroup;
			}

			return current;
		}

	}

	public class GroupedData<TRow, TAggregates> where TRow : class
	{
		public Field[] rowFieldsInGroupOrder = null!;
		public Field[] colFieldsInGroupOrder = null!;

		public Field[] dataFields = null!;

		public List<Group<TRow>> lastRowGroups = null!;
		public List<Group<TRow>> lastColGroups = null!;

		public List<Field> fields = null!;
		//public Dictionary<string, PropertyDescriptor> props = null!;

		public PivotTable<TAggregates> PT = null!;
		public IEnumerable<IGroup<TAggregates>> PT_lastCols = null!;
		public IEnumerable<IGroup<TAggregates>> PT_lastRows = null!;
	}
}
