using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.Wpf.XmlEditor.AutoCompletion
{
	public class RefUnfinishedTypeCompletionData : ICompletionData
	{
		private readonly UnfinishedTypeCompletionDataProvider problem;
		private readonly string typeName;
		private readonly string referenceId;
		private readonly string referencePath;

		public RefUnfinishedTypeCompletionData(UnfinishedTypeCompletionDataProvider problem, string typeName, string referenceId, string referencePath)
		{
			this.problem = problem;
			this.typeName = typeName;
			this.referenceId = referenceId;
			this.referencePath = referencePath;
		}

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			var newData = referencePath != null
				? $"<{typeName} Reference.Id=\"{referenceId}\" Reference.Path=\"{referencePath}\" />"
				: $"<{typeName} Reference.Id=\"{referenceId}\" />";

			var startOffset = textArea.Document.GetOffset(new TextLocation(problem.Line, problem.ColumnStart));
			var endOffset = completionSegment.EndOffset;

			textArea.Document.Replace(new TextSegment
			{
				StartOffset = startOffset + 1,
				EndOffset = endOffset
			}, newData);
		}

		public ImageSource Image => null;

		public string Text => $"{typeName} {referenceId}";

		public object Content
		{
			get
			{
				var panel = new StackPanel{Orientation = Orientation.Vertical};

				var topTextBlock = new TextBlock{Margin = new Thickness(1)};
				topTextBlock.Inlines.Add(new Run("ref ") { Foreground = new SolidColorBrush { Color = Color.FromRgb(255, 0, 255) } });
				var commonPrefix = new string(problem.UnfinishedPrefix
					.TakeWhile((c, i) => string.Equals("" + c, "" + typeName[i], StringComparison.InvariantCultureIgnoreCase)).ToArray());
				topTextBlock.Inlines.Add(new Run(commonPrefix) { FontWeight = FontWeights.Bold });
				topTextBlock.Inlines.Add(typeName.Substring(commonPrefix.Length));
				panel.Children.Add(topTextBlock);

				var bottomTextBlock =  new TextBlock { Margin = new Thickness(1), FontSize = 10};
				bottomTextBlock.Inlines.Add(new Run($"{referenceId}")
				{
					Foreground = new SolidColorBrush { Color = Color.FromRgb(0, 0, 128) }
				});
				if (referencePath != null)
				{
					bottomTextBlock.Inlines.Add(new Run($"; {referencePath}")
					{
						Foreground = new SolidColorBrush {Color = Color.FromRgb(0, 0, 128)}
					});
				}
				else
				{
					bottomTextBlock.Inlines.Add(new Run("; ")
					{
						Foreground = new SolidColorBrush { Color = Color.FromRgb(0, 0, 128) }
					});
					bottomTextBlock.Inlines.Add(new Run("local")
					{
						Foreground = new SolidColorBrush { Color = Color.FromRgb(0, 0, 128) },
						FontWeight = FontWeights.Bold
					});
				}
				panel.Children.Add(bottomTextBlock);

				return panel;
			}
		}

		public object Description => Text;

		public double Priority => referencePath == null ? 1 : 3;
	}
}
