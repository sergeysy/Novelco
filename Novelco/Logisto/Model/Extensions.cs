using System.Collections.Generic;
using System.Linq;

namespace Logisto.Models
{
	public static class Extensions
	{
		public static void AddIfNotNull<T>(this List<T> list, T item)
		{
			if (item != null)
				list.Add(item);
		}

		public static bool In<T>(this T item, params T[] list)
		{
			return list.Contains(item);
		}
	}
}