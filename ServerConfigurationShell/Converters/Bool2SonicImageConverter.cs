using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ServerConfigurationShell.Converters
{
	public class Bool2SonicImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return bool.Parse(value.ToString()) ? "/ServerConfigurationShell;component/assets/sonic.png" :
				"/ServerConfigurationShell;component/assets/sonic-disconnect.png";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
