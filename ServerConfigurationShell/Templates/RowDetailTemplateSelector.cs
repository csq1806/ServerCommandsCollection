using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ServerConfigurationShell.Templates
{
	public class RowDetailTemplateSelector : DataTemplateSelector
	{
		public DataTemplate AssociatedVPNTemplate { get; set; }
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var configuration = item as Configuration;
			if (configuration == null) return null;
			if (!string.IsNullOrWhiteSpace(configuration.AssociatedVPNName))
				return AssociatedVPNTemplate;
			return null;
		}
	}
}
