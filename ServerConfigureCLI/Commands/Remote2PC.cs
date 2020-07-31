using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigureCLI.Commands
{
	class Remote2PC : ICommand
	{
		public void AddCommand<T>(CommandLineApplication<T> app) where T : class
		{
			app.Command("remote", config =>
			{
				var filePath = config.Option("-p", string.Empty, CommandOptionType.SingleValue);
				config.OnExecuteAsync(async token =>
				{
					if (!File.Exists(filePath.Value()))
					{
						return 504;
					}
					var startInfo = new ProcessStartInfo
					{
						CreateNoWindow = false,
						UseShellExecute = false,
						WindowStyle = ProcessWindowStyle.Hidden,
						Arguments = $"\"{filePath.Value()}\"",
						FileName = "mstsc"
					};
					using (Process process = Process.Start(startInfo))
					{
						process.WaitForExit();
						return process.ExitCode;
					}
				});
			});
		}
	}
}
