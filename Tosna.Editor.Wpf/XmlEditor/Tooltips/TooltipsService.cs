using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using Tosna.Editor.Common;
using Orientation = System.Windows.Controls.Orientation;

namespace Tosna.Editor.Wpf.XmlEditor.Tooltips
{
	public class TooltipsService
	{
		#region Fields

		private readonly TextEditor textEditor;
		private Popup currentTooltip;

		private readonly DispatcherTimer refreshingTimer;
		private DateTime timeToStopRefresh;
		private bool isRefreshing;

		private IReadOnlyCollection<NotificationMeta> tooltipsMetas;

		private NotificationMeta currentTooltipMeta;

		#endregion

		#region Ctor

		public TooltipsService(TextEditor textEditor)
		{
			this.textEditor = textEditor;
			refreshingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.2) };
			refreshingTimer.Tick += delegate
			{
				if (DateTime.Now < timeToStopRefresh)
				{
					Refresh();
				}
				else
				{
					refreshingTimer.Stop();
					isRefreshing = false;
				}
			};
		}

		#endregion

		#region Public

		public void UpdateTooltips(IReadOnlyCollection<NotificationMeta> newTooltipsMetas)
		{
			tooltipsMetas = newTooltipsMetas;
		}

		public void EnqueueMouseMoveSignal()
		{
			if (!isRefreshing)
			{
				isRefreshing = true;
				refreshingTimer.Start();
			}

			timeToStopRefresh = DateTime.Now + TimeSpan.FromSeconds(1);
		}

		#endregion

		#region Private

		private void Refresh()
		{
			if (currentTooltip != null && currentTooltip.IsMouseOver)
			{
				return;
			}

			var point = Mouse.GetPosition(textEditor);

			var nullablePosition = textEditor.GetPositionFromPoint(point);

			if (nullablePosition == null)
			{
				CloseTooltip();
				return;
			}

			var position = nullablePosition.Value;
			var tooltipMeta = tooltipsMetas.FirstOrDefault(meta => TextIntervalCoordinatesChecker.Contains(meta.Coordinates, position.Location));

			if (tooltipMeta == null)
			{
				CloseTooltip();
				return;
			}

			if (currentTooltipMeta == tooltipMeta)
			{
				if (currentTooltip != null)
				{
					currentTooltip.IsOpen = true;
					return;
				}
			}

			currentTooltipMeta = tooltipMeta;

			CloseTooltip();

			currentTooltip = CreateTooltip(tooltipMeta);
		}

		private Popup CreateTooltip(NotificationMeta tooltipMeta)
		{
			var tooltip = new Popup
			{
				Placement = PlacementMode.MousePoint,
				VerticalOffset = 5
			};
			var tooltipPanel = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Background = new SolidColorBrush {Color = Color.FromArgb(255, 230, 230, 205)}
			};
			tooltip.Child = tooltipPanel;

			TextBlock lastTextBlock = null;

			foreach (var message in tooltipMeta.Messages)
			{
				tooltipPanel.Children.Add(lastTextBlock = new TextBlock
				{
					Text = message,
					Margin = new Thickness(4, 4, 4, 0)
				});
			}

			foreach (var action in tooltipMeta.Actions)
			{
				var hyperLinkTextBlock = lastTextBlock = new TextBlock {Margin = new Thickness(4, 4, 4, 0)};
				var hyperlink =
					new Hyperlink(new Run(action.PublicName ?? "Fix")
					{
						Foreground = new SolidColorBrush {Color = Color.FromRgb(0, 0, 255)},
						Cursor = Cursors.Hand
					});
				hyperLinkTextBlock.Inlines.Add(hyperlink);
				tooltipPanel.Children.Add(hyperLinkTextBlock);

				hyperlink.Command = new ActionCommand(() =>
				{
					action.Execute(null);
					CloseTooltip();
				}, () => true);
			}

			if (lastTextBlock != null)
			{
				lastTextBlock.Margin = new Thickness(4);
			}

			return tooltip;
		}

		private void CloseTooltip()
		{
			if (currentTooltip == null)
			{
				return;
			}
			currentTooltip.IsOpen = false;
			currentTooltip = null;
		}

		#endregion
	}
}
