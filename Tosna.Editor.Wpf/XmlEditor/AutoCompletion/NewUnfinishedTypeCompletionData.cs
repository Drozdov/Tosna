using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Editor.Helpers.Xml;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.Wpf.XmlEditor.AutoCompletion
{
	public class NewUnfinishedTypeCompletionData : ICompletionData
	{
		private readonly UnfinishedTypeCompletionDataProvider problem;
		private readonly Type destinationType;
		private readonly string typeName;

		public NewUnfinishedTypeCompletionData(UnfinishedTypeCompletionDataProvider problem, Type destinationType, string typeName)
		{
			this.problem = problem;
			this.destinationType = destinationType;
			this.typeName = typeName;
		}

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			var completor = new XmlCompletor(problem.SerializingElementsManager, problem.TypesResolver);

			var newData = completor.GetDefaultContent(destinationType, new string(' ', problem.ColumnStart)).TrimStart();

			var startOffset = textArea.Document.GetOffset(new TextLocation(problem.Line, problem.ColumnStart));
			var endOffset = completionSegment.EndOffset;

			textArea.Document.Replace(new TextSegment
			{
				StartOffset = startOffset + 1,
				EndOffset = endOffset
			}, newData);

		}

		public ImageSource Image => null;

		public string Text => typeName;

		public object Content
		{
			get
			{
				var tb = new TextBlock();
				tb.Inlines.Add(new Run("new ") {Foreground = new SolidColorBrush {Color = Color.FromRgb(0, 0, 255)}});
				var commonPrefix = new string(problem.UnfinishedPrefix
					.TakeWhile((c, i) => string.Equals("" + c, "" + typeName[i], StringComparison.InvariantCultureIgnoreCase)).ToArray());
				tb.Inlines.Add(new Run(commonPrefix) { FontWeight = FontWeights.Bold });
				tb.Inlines.Add(typeName.Substring(commonPrefix.Length));
				return tb;
			}
		}

		public object Description => Text;

		public double Priority => typeName.Contains(".") ? 4 : 2;
	}
}
