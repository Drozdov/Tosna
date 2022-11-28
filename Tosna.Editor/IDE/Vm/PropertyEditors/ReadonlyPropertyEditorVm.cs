namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class ReadonlyPropertyEditorVm : IPropertyEditorVm
	{
		public string PublicName { get; }

		public string Value { get; }

		public ReadonlyPropertyEditorVm(string publicName, string value)
		{
			PublicName = publicName;
			Value = value;
		}
	}
}
