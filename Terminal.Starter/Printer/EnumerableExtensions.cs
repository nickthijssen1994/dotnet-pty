using System;
using System.Collections;
using System.Collections.Generic;

namespace Terminal.Starter.Printer
{
	/// <summary>
	///     Provides a set of <see langword="static" /> methods for querying objects that implement <see cref="IEnumerable" />.
	/// </summary>
	internal static class EnumerableExtensions
	{
		internal static IEnumerable<T> FlattenBreadthFirst<T>(
			this IEnumerable<T> source,
			Func<T, IEnumerable<T>> children)
		{
			var queue = new Queue<T>();

			foreach (var item in source)
			{
				queue.Enqueue(item);
			}

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();

				foreach (var option in children(current))
				{
					queue.Enqueue(option);
				}

				yield return current;
			}
		}

		internal static IEnumerable<T> RecurseWhileNotNull<T>(
			this T? source,
			Func<T, T?> next)
			where T : class
		{
			if (source is null) yield break;

			yield return source;

			while ((source = next(source)) != null)
			{
				yield return source;
			}
		}
	}
}