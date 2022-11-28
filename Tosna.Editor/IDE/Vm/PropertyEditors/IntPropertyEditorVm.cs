namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class IntPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeDescriptedField property;

		public IntPropertyEditorVm(SimpleTypeDescriptedField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public int IntValue
		{
			get => (int)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
