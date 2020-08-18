using Prism.Commands;
using ServerConfigurationShell.Events;
using ServerConfigurationShell.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
			RunCLICommand = new DelegateCommand(OnRunCLICommand);
			AddVPNConfigurationCommand = new DelegateCommand(OnAddVPNConfigurationCommand);
			AddRDPConfigurationCommand = new DelegateCommand(OnAddRDPConfigurationCommand);

			EventAggregator.GetEvent<AddConfigurationEvent>().Subscribe(OnAddingConfiguration);

			Task.Run(MonitorVPNStatus);
		}

		private void OnRunCLICommand()
		{
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLI"),
				Arguments = $"/K",
				FileName = @"CMD"
			};
			Process.Start(startInfo);
		}

		private void OnAddVPNConfigurationCommand()
		{
			var viewModel = Container.Resolve<VPNConfigurationViewModel>();
			viewModel.ShowDialogWindow();
		}

		private void OnAddRDPConfigurationCommand()
		{
			var viewModel = Container.Resolve<RDPConfigurationViewModel>();
			viewModel.ShowDialogWindow();
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

		async private void Initialize()
		{
			await environment.Initialize();
			VPNConfigurations = environment.Configurations?.ToObservableCollection() ?? new ObservableCollection<Configuration>();
			if (VPNConfigurations.Count > 0) SelectedConfiguration = VPNConfigurations.First();
		}

		private void OnDisconnectCommand()
		{
			IsBusy = true;
			Task.Run(() =>
			{
				if (SelectedConfiguration.Type == ConfigurationType.Remote) return;

				if (SelectedConfiguration.Type == ConfigurationType.SonicWallVPN)
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
				switch (SelectedConfiguration.Type)
				{
					case ConfigurationType.Remote:
						Connect2Remote();
						break;
					case ConfigurationType.VPN:
						Connect2VPN(SelectedConfiguration);
						break;
					case ConfigurationType.SonicWallVPN:
						Connect2SonicWallVPN();
						break;
					default:
						break;
				}

				IsBusy = false;
			});
		}

		private void Connect2Remote()
		{
			bool connectedToVPN = true;
			Configuration vpnConfig = null;
			if (!string.IsNullOrWhiteSpace(SelectedConfiguration.AssociatedVPNName))
			{
				vpnConfig = VPNConfigurations.FirstOrDefault(p => p.Name == SelectedConfiguration.AssociatedVPNName);
				connectedToVPN = Connect2VPN(vpnConfig);
			}
			if (!connectedToVPN) return;
			if (vpnConfig != null && vpnConfig.Type == ConfigurationType.SonicWallVPN) Thread.Sleep(TimeSpan.FromSeconds(5));

			Connect2RemoteInternal();
		}

		private void Connect2RemoteInternal()
		{
			bool generateRDPSuccess = true;
			var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rdps", $"{SelectedConfiguration.Name}.rdp");
			if (!File.Exists(filePath))
			{
				generateRDPSuccess = GenerateRDPFile();
			}
			if (!generateRDPSuccess) return;

			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"remote -p \"{filePath}\"",
				FileName = @"CLI\sc.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
			}
		}

		private bool GenerateRDPFile()
		{
			var args = $"crdp -name \"{SelectedConfiguration.Name}\" -un \"{SelectedConfiguration.UserName}\" " +
				$"-pw \"{Security.Decrypt(SelectedConfiguration.Password)}\" -ip \"{SelectedConfiguration.IPAddress}\" ";
			if (!string.IsNullOrWhiteSpace(SelectedConfiguration.Domain)) args += $"-domain:\"{SelectedConfiguration.Domain}\"";
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rdps"),
				Arguments = args,
				FileName = @"CLI\sc.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
				return process.ExitCode == 0;
			}
		}

		private bool Connect2VPN(Configuration configuration)
		{
			if (configuration == null) return false;

			ProcessStartInfo startInfo = null;
			if (configuration.Type == ConfigurationType.SonicWallVPN)
			{
				if (!NetworkHelper.IsNetworkExist(configuration.Name))
				{
					Telerik.Windows.Controls.RadWindow.Alert("Please install SonicWall VPN first.");
					return false;
				}
				startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WorkingDirectory = @"C:\Program Files\Dell SonicWALL\Global VPN Client",
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = $"/C swgvc /E \"{configuration.Name}\" /U {configuration.UserName} /P {Security.Decrypt(configuration.Password)}",
					FileName = "cmd.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
					if (process.ExitCode != 0) return false;
				}
				return true;
			}

			string args = $"vpn -name \"{configuration.Name}\" -un \"{configuration.UserName}\" -pw \"{Security.Decrypt(configuration.Password)}\" ";
			if (!string.IsNullOrWhiteSpace(configuration.DNS) && !string.IsNullOrWhiteSpace(configuration.NetworkName))
				args += $"--dns:\"{configuration.DNS}\" -net \"{configuration.NetworkName}\"";
			startInfo = new ProcessStartInfo
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
				if (process.ExitCode != 0) return false;
			}
			return true;
		}

		private void Connect2SonicWallVPN()
		{
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				WorkingDirectory = @"C:\Program Files\Dell SonicWALL\Global VPN Client",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = $"/C swgvc /E \"{SelectedConfiguration.Name}\" /U {SelectedConfiguration.UserName} /P {Security.Decrypt(SelectedConfiguration.Password)}",
				FileName = "cmd.exe"
			};
			using (Process process = Process.Start(startInfo))
			{
				process.WaitForExit();
			}
		}

		public ICommand ConnectCommand { get; set; }
		public ICommand DisconnectCommand { get; set; }
		public ICommand RunCLICommand { get; set; }
		public ICommand AddVPNConfigurationCommand { get; set; }
		public ICommand AddRDPConfigurationCommand { get; set; }

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
