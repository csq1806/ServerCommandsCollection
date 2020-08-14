using Prism.Commands;
using ServerConfigurationShell.Events;
using ServerConfigurationShell.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace ServerConfigurationShell.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Environment environment;
		public MainWindowViewModel(
			MainWindow mainWindow,
			Environment environment,
			IUnityContainer container) :
			base(mainWindow, container)
		{
			this.environment = environment;

			Initialize();

			ConnectCommand = new DelegateCommand(OnConnectCommand);
			DisconnectCommand = new DelegateCommand(OnDisconnectCommand);
			AddConfigurationCommand = new DelegateCommand(OnAddConfigurationCommand);

			EventAggregator.GetEvent<AddConfigurationEvent>().Subscribe(OnAddingConfiguration);

			Task.Run(MonitorVPNStatus);
		}

		private void MonitorVPNStatus()
		{
			while (true)
			{
				try
				{
					if (VPNConfigurations != null)
					{
						var nics = NetworkInterface.GetAllNetworkInterfaces();
						var config = VPNConfigurations.FirstOrDefault(p => nics.Any(pp =>
						{
							return pp.Name == p.Name;
						}));
						if (config != null && config.IsConnected)
						{
							Thread.Sleep(3000);
							continue;
						}

						VPNConfigurations.ForEach(p => p.IsConnected = false);
						if (config != null) config.IsConnected = true;
					}
				}
				catch (Exception)
				{
					VPNConfigurations.ForEach(p => p.IsConnected = false);
				}
				Thread.Sleep(3000);
			}
		}

		private void OnAddingConfiguration(Configuration obj)
		{
			VPNConfigurations.Add(obj);
			if (VPNConfigurations.Count == 1) SelectedConfiguration = obj;
		}

		private void OnAddConfigurationCommand()
		{
			var viewModel = Container.Resolve<ConfigurationViewModel>();
			viewModel.ShowDialogWindow();
		}

		async private void Initialize()
		{
			await environment.Initialize();
			VPNConfigurations = environment.VPNConfigurations?.ToObservableCollection() ?? new ObservableCollection<Configuration>();
			if (environment.SonicVPNEnabled) AddSonicWall2VPNConfigurations();
			if (VPNConfigurations.Count > 0) SelectedConfiguration = VPNConfigurations.First();
		}

		private void AddSonicWall2VPNConfigurations()
		{
			VPNConfigurations.Add(new Configuration
			{
				IsConnected = false,
				Name = "Transfinder VPN",
				UserName = "jeffrey.chen",
				Password = "Qohlj81102",
				IsSonicWallVPN = true
			});
		}

		private void OnDisconnectCommand()
		{
			IsBusy = true;
			Task.Run(() =>
			{
				if (SelectedConfiguration.IsSonicWallVPN)
				{
					Disconnect2SonicWallVPN();
					IsBusy = false;
					return;
				}

				string args = "vpn --disconnect ";
				if (!string.IsNullOrWhiteSpace(SelectedConfiguration.NetworkName)) args += $"--dns -net \"{SelectedConfiguration.NetworkName}\"";
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = args,
					FileName = @"CLI\sc.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
					//if (process.ExitCode == 0) SelectedConfiguration.IsConnected = false;
				}
				IsBusy = false;
			});
		}

		private void Disconnect2SonicWallVPN()
		{
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WorkingDirectory = @"C:\Program Files\Dell SonicWALL\Global VPN Client",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"/C swgvc /D \"{SelectedConfiguration.Name}\"",
				FileName = "cmd.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
				IsBusy = false;
			}
		}

		private void OnConnectCommand()
		{
			IsBusy = true;
			Task.Run(() =>
			{
				if (SelectedConfiguration.IsSonicWallVPN)
				{
					Connect2SonicWallVPN();
					IsBusy = false;
					return;
				}

				string args = $"vpn -name \"{SelectedConfiguration.Name}\" -un \"{SelectedConfiguration.UserName}\" -pw \"{Security.Decrypt(SelectedConfiguration.Password)}\" ";
				if (!string.IsNullOrWhiteSpace(SelectedConfiguration.DNS) && !string.IsNullOrWhiteSpace(SelectedConfiguration.NetworkName))
					args += $"--dns:\"{SelectedConfiguration.DNS}\" -net \"{SelectedConfiguration.NetworkName}\"";
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = args,
					FileName = @"CLI\sc.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
				}

				IsBusy = false;
			});
		}

		private void Connect2SonicWallVPN()
		{
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WorkingDirectory = @"C:\Program Files\Dell SonicWALL\Global VPN Client",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"/C swgvc /E \"{SelectedConfiguration.Name}\" /U {SelectedConfiguration.UserName} /P {SelectedConfiguration.Password}",
				FileName = "cmd.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
			}
		}

		public ICommand ConnectCommand { get; set; }
		public ICommand DisconnectCommand { get; set; }
		public ICommand AddConfigurationCommand { get; set; }

		private ObservableCollection<Configuration> vpnConfigurations;

		public ObservableCollection<Configuration> VPNConfigurations
		{
			get { return vpnConfigurations; }
			set
			{
				vpnConfigurations = value;
				OnPropertyChanged();
			}
		}

		private Configuration selectedConfiguration;

		public Configuration SelectedConfiguration
		{
			get { return selectedConfiguration; }
			set
			{
				selectedConfiguration = value;
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
