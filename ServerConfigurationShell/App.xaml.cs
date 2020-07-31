using log4net;
using log4net.Config;
using Prism.Events;
using Prism.Ioc;
using ServerConfigurationShell.Services;
using ServerConfigurationShell.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Telerik.Windows.Controls;

namespace ServerConfigurationShell
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Prism.Unity.PrismApplication
	{
		protected override Window CreateShell()
		{
			Container.Resolve<IEventAggregator>();
			var vm = Container.Resolve<MainWindowViewModel>();
			return vm.View;
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterInstance(LogManager.GetLogger(Assembly.GetExecutingAssembly(), DateTime.Now.ToShortTimeString()));
			RegistServicesAndStart(containerRegistry);
			containerRegistry.RegisterSingleton<Environment>();
		}

		private void RegistServicesAndStart(IContainerRegistry containerRegistry)
		{
			Assembly ass = Assembly.GetExecutingAssembly();
			Type[] types = ass.GetTypes();
			foreach (var type in types)
			{
				if (type.GetInterface("IService") != null)
				{
					containerRegistry.RegisterSingleton(type);
					IService service = Container.Resolve(type) as IService;
					if (service.AutoStart) service.Start();
				}
			}
		}

		public override void Initialize()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			StyleManager.ApplicationTheme = new Windows8Theme();
			var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

			base.Initialize();
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ev = (Exception)e.ExceptionObject;
			string errorMsg = ev.ToString();
			RadWindow.Alert("未处理的异常:\r\n" + errorMsg);
		}
	}
}
