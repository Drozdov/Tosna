using System;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class EnumPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeDescriptedField property;

		public EnumPropertyEditorVm(SimpleTypeDescriptedField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;

			Values = Enum.GetValues(property.Value.GetType());
		}

		public string PublicName { get; set; }

		public object Values { get; }

		public object Value
		{
			get => property.Value;
			set => property.Value = value;
		}
	}
}
