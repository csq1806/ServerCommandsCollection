using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConfigurationShell
{
	public static class LinqExtensions
	{
		public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> _LinqResult)
		{
			return new ObservableCollection<T>(_LinqResult);
		}

		public static HashSet<T> ToHashset<T>(this IEnumerable<T> _LinqResult)
		{
			return new HashSet<T>(_LinqResult);
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T element in source)
				action(element);
		}
	}
}
