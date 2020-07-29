using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigureCLI
{
	interface ICommand
	{
		void AddCommand<T>(CommandLineApplication<T> app) where T : class;
	}
}
