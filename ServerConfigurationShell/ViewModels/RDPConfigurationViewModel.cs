using Prism.Commands;
using ServerConfigurationShell.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
			if (IsEdit) RemoveConfigurationByName(OriginalRDPName);
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
			if (IsEdit) OriginalRDPName = RDPName;
			await environment.Save();
			IsBusy = false;
			Telerik.Windows.Controls.RadWindow.Alert("Save succeed!");
		}

		private void RemoveConfigurationByName(string rdpName)
		{
			var config = environment.Configurations.FirstOrDefault(p => p.Name == rdpName);
			if (config == null) return;
			environment.Configurations.Remove(config);
			EventAggregator.GetEvent<RemoveConfigurationEvent>().Publish(rdpName);

			var rdpFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rdps", $"{rdpName}.rdp");
			if (File.Exists(rdpFilePath)) File.Delete(rdpFilePath);
		}

		public ICommand SaveCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public void SetConfigurationValue(Configuration configuration, bool isEdit)
		{
			this.IsEdit = isEdit;
			OriginalRDPName = RDPName = configuration.Name;
			IPAddress = configuration.IPAddress;
			UserName = configuration.UserName;
			Password = Security.Decrypt(configuration.Password);
			Domain = configuration.Domain;
			SelectedAssociatedVPN = AvailableVPNs.FirstOrDefault(p => p.Name == (configuration.AssociatedVPNName ?? "None"));
		}

		public bool IsEdit { get; set; }
		public string OriginalRDPName { get; set; }

		private string rdpName;

		public string RDPName
		{
			get { return rdpName; }
			set
			{
				rdpName = value;
				OnPropertyChanged();
			}
		}
		private string ipAddress;

		public string IPAddress
		{
			get { return ipAddress; }
			set
			{
				ipAddress = value;
				OnPropertyChanged();
			}
		}
		private string userName;

		public string UserName
		{
			get { return userName; }
			set
			{
				userName = value;
				OnPropertyChanged();
			}
		}
		private string password;

		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				OnPropertyChanged();
			}
		}
		private string domain;

		public string Domain
		{
			get { return domain; }
			set
			{
				domain = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Configuration> AvailableVPNs { get; set; }
		private Configuration selectedAssociatedVPN;

		public Configuration SelectedAssociatedVPN
		{
			get { return selectedAssociatedVPN; }
			set
			{
				selectedAssociatedVPN = value;
				OnPropertyChanged();
			}
		}

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
