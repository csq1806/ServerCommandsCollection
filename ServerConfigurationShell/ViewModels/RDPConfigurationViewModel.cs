using Prism.Commands;
using ServerConfigurationShell.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace ServerConfigurationShell.ViewModels
{
	public class RDPConfigurationViewModel : ViewModelBase
	{
		private Environment environment;
		public RDPConfigurationViewModel(
			ServerConfigurationShell.Views.RDPConfiguration view,
			IUnityContainer container,
			Environment environment) : base(view, container)
		{
			this.environment = environment;
			AvailableVPNs = new ObservableCollection<Configuration>(this.environment.Configurations.Where(p => p.Type != ConfigurationType.Remote));
			AvailableVPNs.Insert(0, new Configuration { Name = "None" });
			SelectedAssociatedVPN = AvailableVPNs.First();
			SaveCommand = new DelegateCommand(OnSaveCommand);
			CancelCommand = new DelegateCommand(OnCancelCommand);
			EventAggregator.GetEvent<AddConfigurationEvent>().Subscribe(OnConfigurationChanged);
		}

		private void OnConfigurationChanged(Configuration configuration)
		{
			if (configuration.Type == ConfigurationType.Remote) return;
			AvailableVPNs.Add(configuration);
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
				Name = RDPName,
				IPAddress = IPAddress,
				UserName = UserName,
				Password = Security.Encrypt(Password),
				Type = ConfigurationType.Remote,
				Domain = Domain,
				AssociatedVPNName = SelectedAssociatedVPN.Name == "None" ? null : SelectedAssociatedVPN.Name
			};
			EventAggregator.GetEvent<AddConfigurationEvent>().Publish(config);
			environment.Configurations.Add(config);
			await environment.Save();
			IsBusy = false;
		}

		public ICommand SaveCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public string RDPName { get; set; }
		public string IPAddress { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Domain { get; set; }

		public ObservableCollection<Configuration> AvailableVPNs { get; set; }
		public Configuration SelectedAssociatedVPN { get; set; }

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
