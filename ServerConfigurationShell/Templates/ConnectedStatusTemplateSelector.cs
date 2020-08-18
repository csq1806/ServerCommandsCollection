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
		public DataTemplate VPNContentTemplate { get; set; }
		public DataTemplate RDPContentTemplate { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null) return null;
			Configuration configuration = item as Configuration;
			switch (configuration.Type)
			{
				case ConfigurationType.Remote:
					return RDPContentTemplate;
				case ConfigurationType.VPN:
					return VPNContentTemplate;
				case ConfigurationType.SonicWallVPN:
					return SonicContentTemplate;
				default: return null;
			}
		}
	}
}
