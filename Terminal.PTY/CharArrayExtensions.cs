using System.Collections.Generic;

namespace Terminal.PTY
{
	public static class CharArrayExtensions
	{
		public static char[] SubArray(this char[] input, int startIndex, int length)
		{
			var result = new List<char>();
			for (var i = startIndex; i < length; i++)
			{
				result.Add(input[i]);
			}

			return result.ToArray();
		}
	}
}