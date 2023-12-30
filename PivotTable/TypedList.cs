#if false
using System.ComponentModel;

namespace osexpert.PivotTable
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

	//public abstract class PropertyColumn : PropertyDescriptor
	//{
	//	public PropertyColumn(string name, Attribute[] attrs) : base(name, attrs)
	//	{
	//	}
	//}

	/// <summary>
	/// rename DataField ?
	/// FieldProperty? _props
	/// FieldData _datas
	/// FieldStore _Stores
	/// _props
	/// _datas
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	/// <typeparam name="TProp"></typeparam>
	public class Property<TRow, TProp> : PropertyDescriptor
	{
		readonly Func<IEnumerable<TRow>, TProp> _getValue;

		public Property(string fieldName, Func<IEnumerable<TRow>, TProp> getValue)
			: base(fieldName, null)
		{
			_getValue = getValue;
		}

		public Property(string fieldName)
			: base(fieldName, null)
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

#endif