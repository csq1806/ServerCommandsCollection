using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigureCLI.Commands
{
	class CreateRDP : ICommand
	{
		public void AddCommand<T>(CommandLineApplication<T> app) where T : class
		{
			app.Command("crdp", config =>
			{
				var rdpName = config.Option("-name", string.Empty, CommandOptionType.SingleValue);
				var userName = config.Option("-un", string.Empty, CommandOptionType.SingleValue);
				var password = config.Option("-pw", string.Empty, CommandOptionType.SingleValue);
				var ip = config.Option("-ip", string.Empty, CommandOptionType.SingleValue);
				var domain = config.Option("-domain", string.Empty, CommandOptionType.SingleOrNoValue);
				config.OnExecuteAsync(async token =>
				{
					bool verified = true;
					if (!rdpName.HasValue()) verified = false;
					if (!userName.HasValue()) verified = false;
					if (!password.HasValue()) verified = false;
					if (!ip.HasValue()) verified = false;
					if (domain.HasValue() && string.IsNullOrWhiteSpace(rdpName.Value())) verified = false;
					if (!verified)
					{
						Console.WriteLine("Arguments are not correct!");
						return 501;
					}
					var dir = Directory.GetCurrentDirectory();
					if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

					string encryptedPassword = GetEncryptedPassword(password.Value());

					string filePath = System.IO.Path.Combine(dir, $"{rdpName.Value()}.rdp");
					if (File.Exists(filePath)) File.Delete(filePath);

					using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine($"full address:s:{ip.Value()}");
						sb.AppendLine($"username:s:{userName.Value()}");
						if (domain.HasValue()) sb.AppendLine($"domain:s:{domain.Value()}");
						sb.AppendLine($"password 51:b:{encryptedPassword}");
						byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
						await fs.WriteAsync(bytes, 0, bytes.Length);
					}

					return 0;
				});
			});
		}

		private string GetEncryptedPassword(string password)
		{
			RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
			Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
			runspace.Open();
			RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);

			Pipeline pipeline = runspace.CreatePipeline();

			Command cmd = new Command("ConvertFrom-SecureString");
			SecureString secureString = new SecureString();
			password.ToCharArray().ToList().ForEach(p => secureString.AppendChar(p));

			CommandParameter param = new CommandParameter("SecureString", secureString);
			cmd.Parameters.Add(param);
			pipeline.Commands.Add(cmd);

			// Execute PowerShell script
			var result = pipeline.Invoke();
			return result.First().ToString();
		}
	}
}
