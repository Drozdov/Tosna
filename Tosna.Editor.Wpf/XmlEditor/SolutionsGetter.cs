using ICSharpCode.AvalonEdit.Editing;
using Tosna.Core.Helpers.Xml;
using Tosna.Core.Problems;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public static class SolutionsGetter
	{
		public static bool TryGetSolution(
			this MissingMembersCompletionDataProvider provider, TextArea textArea,
			out ComplexSerializerSolution solution)
		{
			var completor = new XmlCompletor(provider.SerializingElementsManager,
				provider.TypesResolver);
			if (!XmlUtils.TryReadXElement(textArea.Document.Text, provider.Line, 0, out var xElement,
				    out var info))
			{
				solution = default(ComplexSerializerSolution);
				return false;
			}

			var newValue = completor
				.GetFilledContent(xElement, provider.Type, new string(' ', provider.Column))
				.TrimStart();
			solution = new ComplexSerializerSolution(newValue, info.LineStart, info.ColumnStart, info.LineEnd,
				info.ColumnEnd);
			return true;
		}
		
		public static bool TryGetSolution(
			this InvalidClosingTagCompletionProvider provider, TextArea textArea,
			out ComplexSerializerSolution solution)
		{
			solution = new ComplexSerializerSolution(
				provider.OpenTagName,
				provider.Line,
				provider.ColumnStart + 3,
				provider.Line,
				provider.ColumnEnd
			);
			return true;
		}

	}
}
