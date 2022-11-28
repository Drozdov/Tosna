namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class DoublePropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeDescriptedField property;

		public DoublePropertyEditorVm(SimpleTypeDescriptedField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public double DoubleValue
		{
			get => (double)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
