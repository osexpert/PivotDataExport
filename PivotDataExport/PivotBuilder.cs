﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace PivotDataExport
{
	/// <summary>
	/// Group and aggregate rows (fast intersect)
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	public class PivotBuilder<TRow> where TRow : class // class notnull
	{
		List<Field<TRow>> _fields;
		IEnumerable<TRow> _rows;

		public List<Field<TRow>> Fields => _fields;

		public PivotBuilder(IEnumerable<TRow> rows, IEnumerable<Field<TRow>> fields)
		{
			//			if (list is not IEnumerable<T>)
			//			throw new ArgumentException("list must be IEnumerable<T>");

			//	_list = (IEnumerable<T>)list;
			_rows = rows;
			_fields = fields.ToList();
		}

		private void Validate()
		{
			if (_fields.Any(f => f.Area == Area.Column) && _fields.Any(f => f.Area == Area.Data && f.SortOrder != SortOrder.None))
				throw new ArgumentException("Can not sort on data fields if grouping on columns");

			if (_fields.GroupBy(f => f.Name).Any(g => g.Count() > 1))
				throw new ArgumentException("More than one field with same fieldName");
		}

		private List<List<Group<TRow>>> GroupRows(IEnumerable<Field<TRow>> fields, RootType rootType)
		{
			var lastGroups = new List<Group<TRow>>();
			lastGroups.Add(new Group<TRow> { Rows = _rows, RootType = rootType });

			var res = GroupRows(lastGroups, fields);
//			if (!res.Any())
	//			return new List<List<Group<TRow>>>() { lastGroups };
			return res;
		}

		private List<List<Group<TRow>>> GroupRows(List<Group<TRow>> lastGroups, IEnumerable<Field<TRow>> fields, bool freeOriginalLastGroupsMem = true)
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

			foreach (Field<TRow> gf in fields)
			{
				var allSubGroups = new List<Group<TRow>>();

				foreach (var go in lastGroups)
				{
					var subGroups = go.Rows.GroupBy(r => gf.GetGroupValue(gf.GetRowValue(r)), gf.GroupComparer).Select(g => new Group<TRow>()
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

		private IEnumerable<Field<TRow>> GetDataFields()
		{
			return _fields.Where(f => f.Area == Area.Data);//.OrderBy(f => f.Index);
		}

		/// <summary>
		/// GetGroupedData (fast intersect)
		/// </summary>
		/// <returns></returns>
		public GroupedData<TRow> GetGroupedData()//bool padEmptyIntersects = false)
		{
			Validate();

			var dataFields = GetDataFields().ToArray();

			var rowFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Row).OrderBy(f => f.GroupIndex).ToArray();
			var colFieldsInGroupOrder = _fields.Where(f => f.Area == Area.Column).OrderBy(f => f.GroupIndex).ToArray();

			List<List<Group<TRow>>> allRowGroups = GroupRows(rowFieldsInGroupOrder, RootType.Row);
			List<List<Group<TRow>>> allRowThenColGroups = GroupRows(allRowGroups.Last(), colFieldsInGroupOrder);

			List<Group<TRow>> lastRowThenColGroups = allRowThenColGroups.Last();

			Dictionary<(Group<TRow>?, object?), Group<TRow>>[] htSynthMergedAllColGroups = new Dictionary<(Group<TRow>?, object?), Group<TRow>>[colFieldsInGroupOrder.Length];

			var rootColGroup = new Group<TRow> { Rows = _rows, RootType = RootType.Column };

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
					var theValue = dataField.GetRowsValue(lastRowThenColGroup.Rows);

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

			return new GroupedData<TRow>()
			{
				ColFieldsInGroupOrder = colFieldsInGroupOrder,
				RowFieldsInGroupOrder = rowFieldsInGroupOrder,
				DataFields = dataFields,
				LastRowGroups = allRowGroups.Last(),
				LastColGroups = allColGroups.Last(),
				Fields = _fields
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
				// The same level will always have same field so its same same.

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

	public class GroupedData<TRow> where TRow : class
	{
		public Field<TRow>[] RowFieldsInGroupOrder = null!;
		public Field<TRow>[] ColFieldsInGroupOrder = null!;

		public Field<TRow>[] DataFields = null!;

		public List<Group<TRow>> LastRowGroups = null!;
		public List<Group<TRow>> LastColGroups = null!;

		public List<Field<TRow>> Fields = null!;
	}
}
