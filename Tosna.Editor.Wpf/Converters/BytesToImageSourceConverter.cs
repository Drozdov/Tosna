using System;
using System.Globalization;
using System.Windows.Data;

namespace Tosna.Editor.Wpf.Converters
{
	public class BytesToImageSourceConverter : IValueConverter
	{
		public static BytesToImageSourceConverter Instance { get; } = new BytesToImageSourceConverter();
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}