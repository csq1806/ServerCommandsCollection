using Prism.Commands;
using ServerConfigurationShell.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace ServerConfigurationShell.ViewModels
{
	public class ConfigurationViewModel : ViewModelBase
	{
		private Environment environment;
		public ConfigurationViewModel(
			ServerConfigurationShell.Views.Configuration view,
			IUnityContainer container,
			Environment environment) :
			base(view, container)
		{
			this.environment = environment;
			SaveCommand = new DelegateCommand(OnSaveCommand);
			CancelCommand = new DelegateCommand(OnCancelCommand);
		}

		private void OnCancelCommand()
		{
			Telerik.Windows.Controls.RadWindow.Confirm("Do you want to close this window?", (s, ev) =>
			{
				if (!ev.DialogResult ?? false) return;
				HideWindow();
			});
		}

		async private void OnSaveCommand()
		{
			IsBusy = true;
			var config = new Configuration
			{
				DNS = DNS,
				Name = VPNName,
				NetworkName = NetworkName,
				Password = Security.Encrypt(Password),
				UserName = UserName,
			};
			EventAggregator.GetEvent<AddConfigurationEvent>().Publish(config);
			environment.VPNConfigurations.Add(config);
			await environment.Save();
			IsBusy = false;
		}

		public ICommand SaveCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public string VPNName { get; set; }
		public string NetworkName { get; set; }
		public string DNS { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		private bool isBusy;

		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				isBusy = value;
				OnPropertyChanged();
			}
		}

	}
}
