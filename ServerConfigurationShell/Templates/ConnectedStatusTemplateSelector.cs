using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ServerConfigurationShell.Templates
{
	class ConnectedStatusTemplateSelector : DataTemplateSelector
	{
		public DataTemplate SonicContentTemplate { get; set; }
		public DataTemplate NormalContentTemplate { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null) return null;
			Configuration configuration = item as Configuration;
			if (configuration.IsSonicWallVPN) return SonicContentTemplate;
			else return NormalContentTemplate;
		}
	}
}
