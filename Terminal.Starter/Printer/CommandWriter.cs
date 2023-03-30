using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.Linq;

namespace Terminal.Starter.Printer
{
	public class CommandWriter : IHelpBuilder
	{
		private const string Indent = "  ";
		private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };
		private static readonly char[] _argumentDelimiters = { ':', '=' };

		public CommandWriter(IConsole console, int maxWidth = int.MaxValue)
		{
			Console = console ?? throw new ArgumentNullException(nameof(console));
			if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth), "Max width must be positive");
			MaxWidth = maxWidth;
		}

		private Dictionary<ISymbol, Customization> Customizations { get; }
			= new();

		protected IConsole Console { get; }
		public int MaxWidth { get; }

		public virtual void Write(ICommand command)
		{
			if (command is null) throw new ArgumentNullException(nameof(command));

			if (command.IsHidden) return;

			AddSynopsis(command);
			AddUsage(command);
			AddCommandArguments(command);
			AddOptions(command);
			AddSubcommands(command);
			AddAdditionalArguments(command);
		}

		public virtual void Write(IEnumerable<Command> commands)
		{
			foreach (var command in commands)
			{
				Write(command);
			}
		}

		private void WriteToFile(string input)
		{
			Console.Out.Write(input);
		}

		private void WriteLineToFile(string input)
		{
			Console.Out.WriteLine(input);
		}

		private void WriteEmptyLineToFile()
		{
			Console.Out.WriteLine();
		}

		protected virtual void AddSynopsis(ICommand command)
		{
			WriteHeading(command.Name, command.Description, command.Aliases);
			WriteEmptyLineToFile();
		}

		protected virtual void AddUsage(ICommand command)
		{
			var description = GetUsage(command);
			WriteHeading(Resources.Instance.HelpUsageTile(), description);
			WriteEmptyLineToFile();
		}

		protected string GetUsage(ICommand command)
		{
			return string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));

			IEnumerable<string> GetUsageParts()
			{
				var parentCommands =
					command
						.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
						.Reverse();

				var displayOptionTitle = command.Options.Any(x => !x.IsHidden);

				foreach (var parentCommand in parentCommands)
				{
					yield return parentCommand.Name;

					if (displayOptionTitle)
					{
						yield return Resources.Instance.HelpUsageOptionsTile();
						displayOptionTitle = true;
					}

					yield return FormatArgumentUsage(parentCommand.Arguments);
				}

				var hasCommandWithHelp = command.Children
					.OfType<ICommand>()
					.Any(x => !x.IsHidden);

				if (hasCommandWithHelp) yield return Resources.Instance.HelpUsageCommandTile();

				if (!command.TreatUnmatchedTokensAsErrors)
					yield return Resources.Instance.HelpUsageAdditionalArguments();
			}
		}

		protected virtual void AddCommandArguments(ICommand command)
		{
			var commandArguments = GetCommandArguments(command).ToArray();

			if (commandArguments.Length > 0)
			{
				WriteHeading(Resources.Instance.HelpArgumentsTitle(), null);
				RenderAsColumns(commandArguments);
				WriteEmptyLineToFile();
			}
		}

		protected IEnumerable<HelpItem> GetCommandArguments(ICommand command)
		{
			//TODO: This shows all parent arguments not just the first level
			return command.RecurseWhileNotNull(c => c.Parents.FirstOrDefaultOfType<ICommand>())
				.Reverse()
				.SelectMany(GetArguments)
				.Distinct();

			IEnumerable<HelpItem> GetArguments(ICommand command)
			{
				var arguments = command.Arguments.Where(x => !x.IsHidden).ToList();
				foreach (var argument in arguments)
				{
					var argumentDescriptor = GetArgumentDescriptor(argument);

					yield return new HelpItem(argumentDescriptor,
						string.Join(" ", GetArgumentDescription(command, argument)));
				}
			}

			IEnumerable<string> GetArgumentDescription(IIdentifierSymbol parent, IArgument argument)
			{
				var description = argument.Description;
				if (!string.IsNullOrWhiteSpace(description)) yield return description!;

				if (argument.HasDefaultValue) yield return $"[{GetArgumentDefaultValue(parent, argument, true)}]";
			}
		}

		protected virtual void AddOptions(ICommand command)
		{
			var options = GetOptions(command).ToArray();

			if (options.Length > 0)
			{
				WriteHeading(Resources.Instance.HelpOptionsTitle(), null);
				RenderAsColumns(options);
				WriteEmptyLineToFile();
			}
		}

		protected IEnumerable<HelpItem> GetOptions(ICommand command)
		{
			return command.Options.Where(x => !x.IsHidden).Select(GetHelpItem);
		}

		protected virtual void AddSubcommands(ICommand command)
		{
			var subcommands = GetSubcommands(command).ToArray();

			if (subcommands.Length > 0)
			{
				WriteHeading(Resources.Instance.HelpCommandsTitle(), null);
				RenderAsColumns(subcommands);
				WriteEmptyLineToFile();
			}
		}

		protected IEnumerable<HelpItem> GetSubcommands(ICommand command)
		{
			return command.Children.OfType<ICommand>().Where(x => !x.IsHidden).Select(GetHelpItem);
		}

		protected virtual void AddAdditionalArguments(ICommand command)
		{
			if (command.TreatUnmatchedTokensAsErrors) return;

			WriteHeading(Resources.Instance.HelpAdditionalArgumentsTitle(),
				Resources.Instance.HelpAdditionalArgumentsDescription());
		}

		protected void WriteHeading(string descriptor, string? description)
		{
			if (!string.IsNullOrWhiteSpace(descriptor)) WriteLineToFile(descriptor);

			if (!string.IsNullOrWhiteSpace(description))
			{
				var maxWidth = MaxWidth - Indent.Length;
				foreach (var part in WrapItem(description!, maxWidth))
				{
					WriteToFile(Indent);
					WriteLineToFile(part);
				}
			}
		}

		protected void WriteHeading(string descriptor, string? description, IReadOnlyCollection<string> aliases)
		{
			if (aliases.Count == 1 && !string.IsNullOrWhiteSpace(descriptor))
			{
				WriteLineToFile(descriptor);
			}
			else
			{
				foreach (var alias in aliases)
				{
					WriteToFile(alias);
					WriteToFile(Indent);
				}

				WriteEmptyLineToFile();
			}

			if (!string.IsNullOrWhiteSpace(description))
			{
				var maxWidth = MaxWidth - Indent.Length;
				foreach (var part in WrapItem(description!, maxWidth))
				{
					WriteToFile(Indent);
					WriteLineToFile(part);
				}
			}
		}

		private string FormatArgumentUsage(IReadOnlyList<IArgument> arguments)
		{
			var sb = StringBuilderPool.Default.Rent();

			try
			{
				var end = default(Stack<char>);

				for (var i = 0; i < arguments.Count; i++)
				{
					var argument = arguments[i];
					if (argument.IsHidden) continue;

					var arityIndicator =
						argument.Arity.MaximumNumberOfValues > 1
							? "..."
							: "";

					var isOptional = IsOptional(argument);

					if (isOptional)
					{
						sb.Append($"[<{argument.Name}>{arityIndicator}");
						(end ??= new Stack<char>()).Push(']');
					}
					else
					{
						sb.Append($"<{argument.Name}>{arityIndicator}");
					}

					sb.Append(' ');
				}

				if (sb.Length > 0)
				{
					sb.Length--;

					if (end is { })
						while (end.Count > 0)
						{
							sb.Append(end.Pop());
						}
				}

				return sb.ToString();
			}
			finally
			{
				StringBuilderPool.Default.ReturnToPool(sb);
			}

			bool IsMultiParented(IArgument argument)
			{
				return argument is Argument a &&
				       a.Parents.Count > 1;
			}

			bool IsOptional(IArgument argument)
			{
				return IsMultiParented(argument) ||
				       argument.Arity.MinimumNumberOfValues == 0;
			}
		}

		protected void RenderAsColumns(params HelpItem[] items)
		{
			if (items.Length == 0) return;
			var windowWidth = MaxWidth;

			var firstColumnWidth = items.Select(x => x.Descriptor.Length).Max();
			var secondColumnWidth = items.Select(x => x.Description.Length).Max();

			if (firstColumnWidth + secondColumnWidth + Indent.Length + Indent.Length > windowWidth)
			{
				var firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
				if (firstColumnWidth > firstColumnMaxWidth)
					firstColumnWidth =
						items.SelectMany(x => WrapItem(x.Descriptor, firstColumnMaxWidth).Select(x => x.Length)).Max();

				secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;
			}

			foreach (var (descriptor, description) in items)
			{
				var descriptorParts = WrapItem(descriptor, firstColumnWidth);
				var descriptionParts = WrapItem(description, secondColumnWidth);

				foreach (var (first, second) in ZipWithEmpty(descriptorParts, descriptionParts))
				{
					WriteToFile($"{Indent}{first}");
					if (!string.IsNullOrWhiteSpace(second))
					{
						var padSize = firstColumnWidth - first.Length;
						var padding = "";
						if (padSize > 0) padding = new string(' ', padSize);

						WriteToFile($"{padding}{Indent}{second}");
					}

					WriteEmptyLineToFile();
				}
			}

			static IEnumerable<(string, string)> ZipWithEmpty(IEnumerable<string> first, IEnumerable<string> second)
			{
				using var enum1 = first.GetEnumerator();
				using var enum2 = second.GetEnumerator();
				bool hasFirst = false, hasSecond = false;
				while ((hasFirst = enum1.MoveNext()) | (hasSecond = enum2.MoveNext()))
				{
					yield return (hasFirst ? enum1.Current : "", hasSecond ? enum2.Current : "");
				}
			}
		}

		private static IEnumerable<string> WrapItem(string item, int maxWidth)
		{
			if (string.IsNullOrWhiteSpace(item)) yield break;
			//First handle existing new lines
			var parts = item.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

			foreach (var part in parts)
			{
				if (part.Length <= maxWidth)
					yield return part;
				else
					//Long item, wrap it based on the width
					for (var i = 0; i < part.Length;)
					{
						if (part.Length - i < maxWidth)
						{
							yield return part.Substring(i);
							break;
						}

						var length = -1;
						for (var j = 0; j + i < part.Length && j < maxWidth; j++)
						{
							if (char.IsWhiteSpace(part[i + j])) length = j + 1;
						}

						if (length == -1) length = maxWidth;

						yield return part.Substring(i, length);

						i += length;
					}
			}
		}

		internal static (string? Prefix, string Alias) SplitPrefix(string rawAlias)
		{
			for (var i = 0; i < _optionPrefixStrings.Length; i++)
			{
				var prefix = _optionPrefixStrings[i];
				if (rawAlias.StartsWith(prefix, StringComparison.Ordinal))
					return (prefix, rawAlias.Substring(prefix.Length));
			}

			return (null, rawAlias);
		}

		protected HelpItem GetHelpItem(IIdentifierSymbol symbol)
		{
			string descriptor;
			if (Customizations.TryGetValue(symbol, out var customization) &&
			    customization.GetDescriptor?.Invoke() is { } setDescriptor)
			{
				descriptor = setDescriptor;
			}
			else
			{
				var rawAliases = symbol.Aliases
					.Select(SplitPrefix)
					.OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
					.ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
					.GroupBy(t => t.Alias)
					.Select(t => t.First())
					.Select(t => $"{t.Prefix}{t.Alias}");

				descriptor = string.Join(", ", rawAliases);

				foreach (var argument in symbol.Arguments())
				{
					if (!argument.IsHidden)
					{
						var argumentDescriptor = GetArgumentDescriptor(argument);
						if (!string.IsNullOrWhiteSpace(argumentDescriptor)) descriptor += $" {argumentDescriptor}";
					}
				}

				if (symbol is IOption option &&
				    option.IsRequired)
					descriptor += $" {Resources.Instance.HelpOptionsRequired()}";
			}

			return new HelpItem(descriptor, GetDescription(symbol));
		}

		protected string GetDescription(IIdentifierSymbol symbol)
		{
			return string.Join(" ", GetDescriptionParts(symbol));

			IEnumerable<string> GetDescriptionParts(IIdentifierSymbol symbol)
			{
				var description = symbol.Description;
				if (!string.IsNullOrWhiteSpace(description)) yield return description!;

				var argumentsDescription = GetArgumentsDescription(symbol);
				if (!string.IsNullOrWhiteSpace(argumentsDescription)) yield return argumentsDescription;
			}

			string GetArgumentsDescription(IIdentifierSymbol symbol)
			{
				IEnumerable<IArgument> arguments = symbol.Arguments();
				var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

				if (defaultArguments.Length == 0) return "";

				var isSingleArgument = defaultArguments.Length == 1;
				var argumentDefaultValues = defaultArguments
					.Select(argument => GetArgumentDefaultValue(symbol, argument, isSingleArgument));
				return $"[{string.Join(", ", argumentDefaultValues)}]";
			}
		}

		private string GetArgumentDefaultValue(IIdentifierSymbol parent, IArgument argument, bool displayArgumentName)
		{
			string? defaultValue;
			if (Customizations.TryGetValue(parent, out var customization) &&
			    customization.GetDefaultValue?.Invoke() is { } parentSetDefaultValue)
			{
				defaultValue = parentSetDefaultValue;
			}
			else if (Customizations.TryGetValue(argument, out customization) &&
			         customization.GetDefaultValue?.Invoke() is { } setDefaultValue)
			{
				defaultValue = setDefaultValue;
			}
			else
			{
				var argumentDefaultValue = argument.GetDefaultValue();
				if (argumentDefaultValue is IEnumerable enumerable && !(argumentDefaultValue is string))
					defaultValue = string.Join("|", enumerable.OfType<object>().ToArray());
				else
					defaultValue = argumentDefaultValue?.ToString();
			}

			var name = displayArgumentName ? Resources.Instance.HelpArgumentDefaultValueTitle() : argument.Name;

			return $"{name}: {defaultValue}";
		}

		protected string GetArgumentDescriptor(IArgument argument)
		{
			if (Customizations.TryGetValue(argument, out var customization) &&
			    customization.GetDescriptor?.Invoke() is { } setDescriptor)
				return setDescriptor;

			if (argument.ValueType == typeof(bool) ||
			    argument.ValueType == typeof(bool?))
				return "";

			string descriptor;
			var suggestions = argument.GetSuggestions().ToArray();
			if (suggestions.Length > 0)
				descriptor = string.Join("|", suggestions);
			else
				descriptor = argument.Name;

			if (!string.IsNullOrWhiteSpace(descriptor)) return $"<{descriptor}>";

			return descriptor;
		}

		private class Customization
		{
			public Customization(Func<string?>? getDescriptor,
				Func<string?>? getDefaultValue)
			{
				GetDescriptor = getDescriptor;
				GetDefaultValue = getDefaultValue;
			}

			public Func<string?>? GetDescriptor { get; }
			public Func<string?>? GetDefaultValue { get; }
		}
	}
}