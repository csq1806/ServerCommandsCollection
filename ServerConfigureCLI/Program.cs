using McMaster.Extensions.CommandLineUtils;
using ServerConfigureCLI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace ServerConfigureCLI
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var Container = new UnityContainer();
			Container.RegisterType<ICommand, Connect2VPN>();

			var app = new CommandLineApplication<Program>();

			Assembly.GetExecutingAssembly().GetTypes().All(type =>
			{
				if (type.GetInterface("ICommand") != null)
				{
					ICommand command = Container.Resolve(type) as ICommand;
					command.AddCommand(app);
				}
				return true;
			});

			await app.ExecuteAsync(args);
		}
	}
}
