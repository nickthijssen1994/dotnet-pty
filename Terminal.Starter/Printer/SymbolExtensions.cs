using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Terminal.Starter.Printer
{
	public static class SymbolExtensions
	{
		internal static IReadOnlyList<IArgument> Arguments(this ISymbol symbol)
		{
			switch (symbol)
			{
				case IOption option:
					return new[]
					{
						option.Argument
					};
				case ICommand command:
					return command.Arguments;
				case IArgument argument:
					return new[]
					{
						argument
					};
				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<string?> GetSuggestions(this ISymbol symbol, string? textToMatch = null)
		{
			return symbol.GetSuggestions(null, textToMatch);
		}
	}
}