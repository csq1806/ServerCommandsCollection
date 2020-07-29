using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace ServerConfigurationShell
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();
			VPNName = "VPN Japan";
			NetworkName = "Ethernet";
			UserName = "JeffreyChen";
			Password = "Qohlj81102";
			DNS = "8.8.8.8";
			ConnectToVPNCommand = new DelegateCommand(OnConnectToVPNCommand);
			DisconnectToVPNCommand = new DelegateCommand(OnDisconnectToVPNCommand);

			DataContext = this;
		}

		private void OnDisconnectToVPNCommand(object obj)
		{
			IsBusy = true;
			Task.Run(() =>
			{
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = $"vpn --disconnect --dns -net \"{NetworkName}\"",
					FileName = @"CLI\sc.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
					IsBusy = false;
					//if (process.ExitCode != 0) return;
				}
			});
		}

		private void OnConnectToVPNCommand(object obj)
		{
			IsBusy = true;
			Task.Run(() =>
			{
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = $"vpn --sstp \"{VPNName}\" --dns:\"{DNS}\" -un \"{UserName}\" -pw \"{Password}\" -net \"{NetworkName}\"",
					FileName = @"CLI\sc.exe"
				};
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
					IsBusy = false;
					//if (process.ExitCode != 0) return;
				}
			});
		}

		public string VPNName { get; set; }
		public string NetworkName { get; set; }
		public string DNS { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public ICommand ConnectToVPNCommand { get; set; }
		public ICommand DisconnectToVPNCommand { get; set; }

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


		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var eventHandler = this.PropertyChanged;
			if (eventHandler != null)
			{
				eventHandler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
