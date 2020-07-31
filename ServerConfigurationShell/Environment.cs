using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerConfigurationShell
{
	public class Environment
	{
		public Environment()
		{

		}

		async public Task Initialize()
		{
			await Task.Run(async () =>
			{
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.json");
				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					byte[] buffer = new byte[fs.Length];
					await fs.ReadAsync(buffer, 0, (int)fs.Length);
					string json = Encoding.UTF8.GetString(buffer);
					VPNConfigurations = JsonConvert.DeserializeObject<List<Configuration>>(json);
				}
			});
		}

		public List<Configuration> VPNConfigurations { get; set; }

		async public Task Save()
		{
			await Task.Run(async () =>
			{
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.json");
				if (File.Exists(filePath)) File.Delete(filePath);
				using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					var json = JsonConvert.SerializeObject(VPNConfigurations);
					byte[] buffer = Encoding.UTF8.GetBytes(json);
					await fs.WriteAsync(buffer, 0, (int)buffer.Length);
				}
			});
		}
	}

	public class Configuration : ModelBase
	{
		public string Name { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string NetworkName { get; set; }
		public string DNS { get; set; }
		
		[JsonIgnore]
		private bool isConnected;
		[JsonIgnore]
		public bool IsConnected
		{
			get { return isConnected; }
			set
			{
				isConnected = value;
				OnPropertyChanged();
			}
		}

	}
}
