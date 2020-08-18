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
		}

		public ICommand SaveCommand { get; set; }
		public ICommand CancelCommand { get; set; }

		public string VPNName { get; set; }
		public string IPAddress { get; set; }
		public string PresharedKey { get; set; }
		public string NetworkName { get; set; }
		public string DNS { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool IsSonicVPN { get; set; }

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
