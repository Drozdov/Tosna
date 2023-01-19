using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Tosna.Core;
using Tosna.Core.Documents.Xml;
using Tosna.Editor.IDE;
using Tosna.Editor.IDE.Interfaces;
using Tosna.Editor.IDE.Vm;
using Tosna.Editor.Wpf.Demo.Domain;
using Tosna.Extensions.Serialization;
using Tosna.Parsers.Xml.V2;

namespace Tosna.Editor.Wpf.Demo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly IFilesSelector filesSelector = new FilesSelector();
		private readonly FilesManager filesManager;
		private readonly XmlIdeVm xmlIdeVm;
		private readonly ILogger logger;

		public MainWindow()
		{
			InitializeComponent();

			WindowState = WindowState.Maximized;

			var serializingElementsManager = new SerializingElementsManager();
			var serializingTypesResolver = new SerializingTypesResolver(serializingElementsManager);
			filesManager = new FilesManager(serializingElementsManager, serializingTypesResolver,
				new ExtendedXmlDocumentReaderV2Factory(), new XmlDocumentWriterFactory());
			logger = new Logger();
			xmlIdeVm = new XmlIdeVm(filesManager, filesSelector, new ConfirmationRequester(), logger);
			DataContext = xmlIdeVm;
		}
		
		protected override void OnClosing(CancelEventArgs e)
		{
			xmlIdeVm?.Dispose();
			base.OnClosing(e);
		}

		private async void OnCreateNewEnvironmentRequest(object sender, RoutedEventArgs args)
		{
			try
			{
				if (!filesSelector.CreateFile(null, out var fileName)) return;
				EnvironmentIo.CreateAndSaveDefaultEnvironment(fileName);
				await filesManager.AddFilesWithDependencies(new[] { fileName });
				xmlIdeVm.HierarchyVm.RefreshAll();
			}
			catch (Exception e)
			{
				logger.LogError(e.Message, e);
			}
		}

		private void OnWeatherForecastRequest(object sender, RoutedEventArgs args)
		{
			WeatherStationsEnvironment weatherEnvironment;
			
			if (!filesSelector.SelectFiles(null, out var files)) return;

			try
			{
				if (!EnvironmentIo.TryReadEnvironment(files, out weatherEnvironment))
				{
					MessageBox.Show(
						"Weather environment not found",
						"Could not load configuration",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message,
					"Could not load configuration",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				return;
			}


			var infoBuilder = new StringBuilder();
			weatherEnvironment.Init();
			
			foreach (var station in weatherEnvironment.WeatherStations)
			{
				infoBuilder.Append(station.Name);
				infoBuilder.Append(Environment.NewLine);

				var temperatureInfo = "-";
				foreach (var thermometer in station.Thermometers)
				{
					if (thermometer.TryGetTemperature(out var temperatureCelsius))
					{
						temperatureInfo = $"{temperatureCelsius:F1}°";
						break;
					}
				}

				infoBuilder.Append($"Temperature: {temperatureInfo}");
				infoBuilder.Append(Environment.NewLine);
				
				var pressureInfo = "-";
				foreach (var barometer in station.Barometers)
				{
					if (barometer.TryGetPressure(out var pressureHPa))
					{
						pressureInfo = $"{pressureHPa:F1} HPa";
						break;
					}
				}

				infoBuilder.Append($"Pressure: {pressureInfo}");
				infoBuilder.Append(Environment.NewLine);
				
				var anemometerInfo = "-";
				foreach (var barometer in station.Anemometers)
				{
					if (barometer.TryGetWindSpeed(out var windSpeedMPerSecond, out var azimuthDegrees))
					{
						anemometerInfo = $"{windSpeedMPerSecond:F1} m/s, {azimuthDegrees:F1}°";
						break;
					}
				}

				infoBuilder.Append($"Wind speed: {anemometerInfo}");
				infoBuilder.Append(Environment.NewLine);
				infoBuilder.Append(Environment.NewLine);
			}

			var weatherInfo = infoBuilder.ToString();

			MessageBox.Show(weatherInfo, "Weather forecast");
		}

		#region Nested

		private class FilesSelector : IFilesSelector
		{
			public bool CreateFile(string initialDirectory, out string fileName)
			{
				var dialog = new SaveFileDialog { InitialDirectory = initialDirectory, FileName = "NewFile.xml" };
				if (dialog.ShowDialog() == true)
				{
					fileName = dialog.FileName;
					return true;
				}

				fileName = null;
				return false;
			}

			public bool SelectFiles(string initialDirectory, out string[] files)
			{
				var dialog = new OpenFileDialog{Multiselect = true, InitialDirectory = initialDirectory};
				if (dialog.ShowDialog() == true)
				{
					files = dialog.FileNames;
					return true;
				}

				files = null;
				return false;
			}
		}

		private class ConfirmationRequester : IConfirmationRequester
		{
			public ConfirmationAnswer ConfirmOperation(string question)
			{
				var resultMessageBox = MessageBox.Show(question, "Tosna", MessageBoxButton.YesNoCancel,
					MessageBoxImage.None);

				switch (resultMessageBox)
				{
					case MessageBoxResult.Yes:
						return ConfirmationAnswer.Yes;

					case MessageBoxResult.No:
						return ConfirmationAnswer.No;

					case MessageBoxResult.Cancel:
						return ConfirmationAnswer.Cancel;

					default:
						return ConfirmationAnswer.Cancel;
				}
			}
		}

		private class Logger : ILogger
		{
			public void LogMessage(LogMessageType messageType, string message)
			{
				Console.WriteLine($"[{messageType.GetMessageTypeStr()}] {message}");
			}

			public void LogMessage(LogMessageType messageType, string message, Exception e)
			{
				LogMessage(messageType, message);
				LogMessage(LogMessageType.Debug, e.Message);
				if (e.StackTrace != null)
				{
					LogMessage(LogMessageType.Debug, e.StackTrace);
				}
			}
		}

		#endregion

	}
}