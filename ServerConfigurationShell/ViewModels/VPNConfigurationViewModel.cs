using Prism.Commands;
using ServerConfigurationShell.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace ServerConfigurationShell.ViewModels
{
	public class VPNConfigurationViewModel : ViewModelBase
	{
		private Environment environment;
		public VPNConfigurationViewModel(
			ServerConfigurationShell.Views.VPNConfiguration view,
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
				IPAddress = IPAddress,
				PresharedKey = PresharedKey,
				Name = VPNName,
				NetworkName = NetworkName,
				Password = Security.Encrypt(Password),
				UserName = UserName,
				Type = IsSonicVPN ? ConfigurationType.SonicWallVPN : ConfigurationType.VPN
			};

			if (config.Type == ConfigurationType.SonicWallVPN)
			{
				if (!NetworkHelper.IsNetworkExist(config.Name))
				{
					Telerik.Windows.Controls.RadWindow.Alert("Please install SonicWall VPN Client.");
				}
			}
			else
			{
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = $"cvpn -l2tp -name \"{config.Name}\" -ip \"{config.IPAddress}\" " +
						$"-prekey \"{config.PresharedKey}\" -un:\"{config.UserName}\" " +
						$"-pw:\"{Security.Decrypt(config.Password)}\"",
					FileName = @"CLI\sc.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
					if (process.ExitCode != 0)
					{
						Telerik.Windows.Controls.RadWindow.Alert("Generate L2TP VPN failed.");
						IsBusy = false;
						return;
					}
				}
			}

			EventAggregator.GetEvent<AddConfigurationEvent>().Publish(config);
			environment.Configurations.Add(config);
			await environment.Save();
			IsBusy = false;
			Telerik.Windows.Controls.RadWindow.Alert("Save succeed!");
		}

		public ICommand SaveCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public void SetConfigurationValue(Configuration configuration, bool isEdit)
		{
			this.IsEdit = isEdit;
			OriginalVPNName = VPNName = configuration.Name;
			IPAddress = configuration.IPAddress;
			UserName = configuration.UserName;
			PresharedKey = configuration.PresharedKey;
			NetworkName = configuration.NetworkName;
			DNS = configuration.DNS;
			IsSonicVPN = configuration.Type == ConfigurationType.SonicWallVPN;
			Password = Security.Decrypt(configuration.Password);
		}
		public bool IsEdit { get; set; }
		public string OriginalVPNName { get; private set; }

		private string vpnName;

		public string VPNName
		{
			get { return vpnName; }
			set
			{
				vpnName = value;
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
		private string presharedKey;

		public string PresharedKey
		{
			get { return presharedKey; }
			set
			{
				presharedKey = value;
				OnPropertyChanged();
			}
		}
		private string networkName;

		public string NetworkName
		{
			get { return networkName; }
			set
			{
				networkName = value;
				OnPropertyChanged();
			}
		}
		private string dns;

		public string DNS
		{
			get { return dns; }
			set
			{
				dns = value;
				OnPropertyChanged();
			}
		}
		private bool isSonicVPN;

		public bool IsSonicVPN
		{
			get { return isSonicVPN; }
			set
			{
				isSonicVPN = value;
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
