using log4net;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace ServerConfigurationShell
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		private SynchronizationContext current = SynchronizationContext.Current;
		protected IUnityContainer Container;
		protected ILog Logger => Container.Resolve<ILog>();
		protected IEventAggregator EventAggregator => Container.Resolve<IEventAggregator>();

		public ViewModelBase(
			Window view,
			IUnityContainer container)
		{
			if (view != null) this.View = view;
			this.Container = container;
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

		private Window view;

		public Window View
		{
			get { return view; }
			set
			{
				if (view != value)
				{
					view = value;
					view.DataContext = this;
				}
			}
		}

		public virtual void HideWindow()
		{
			view.Close();
		}

		public virtual void ShowWindow()
		{
			View.Show();
		}

		public virtual void ShowDialogWindow()
		{
			view.ShowDialog();
		}

		protected void InvokeOnUIThread(Action action)
		{
			current.Send(new SendOrPostCallback(p => action.Invoke()), null);
		}

		protected void InvokeOnUIThreadAsync(Action action)
		{
			current.Post(new SendOrPostCallback(p => action.Invoke()), null);
		}

		protected void InvokeOnUIThread<T>(Action<T> action, T payload)
		{
			current.Send(new SendOrPostCallback(p => action.Invoke(payload)), payload);
		}

		protected void InvokeOnUIThreadAsync<T>(Action<T> action, T payload)
		{
			current.Post(new SendOrPostCallback(p => action.Invoke(payload)), payload);
		}
	}
}
