using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigurationShell.Services
{
	public interface IService
	{
		void Start();
		void Stop();
		void Restart();
		bool AutoStart { get; }
	}
}
