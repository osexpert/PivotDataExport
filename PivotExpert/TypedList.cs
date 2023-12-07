using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PivotExpert
{

	class RowList<TRow> : List<TRow>, ITypedList
	{
		PropertyDescriptorCollection _props;

		public RowList(PropertyDescriptorCollection props)
		{
			_props = props;
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) => _props;
		public string GetListName(PropertyDescriptor[] listAccessors) => string.Empty;
	}


	public class PropertyColumn<TRow, TProp> : PropertyDescriptor
	{
		readonly Func<IEnumerable<TRow>, TProp> _getValue;

		public PropertyColumn(string propName, Func<IEnumerable<TRow>, TProp> getValue)
			: base(propName, null)
		{
			_getValue = getValue;
		}

		public PropertyColumn(string propName)
			: base(propName, null)
		{
			_getValue = GetValue;
		}

		public override object? GetValue(object? component)
		{
			if (component is IEnumerable<TRow> rows)
			{
				return _getValue(rows);
			}
			else
				throw new InvalidOperationException("wrong component type");
		}

		protected virtual TProp GetValue(IEnumerable<TRow> rows)
			=> throw new InvalidOperationException("GetValue(IEnumerable<TRow> rows) must be overridden");

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


}
