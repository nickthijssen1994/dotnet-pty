using System.CommandLine;
using Terminal.Starter.Json.Models;

namespace Terminal.Starter.Models
{
	public class ArgumentModel : IArgumentModel
	{
		public ArgumentModel(Argument argument)
		{
			name = argument.Name;
			description = argument.Description;
			valueType = argument.ArgumentType.Name;
		}

		public string name { get; set; }
		public string? description { get; set; }
		public string valueType { get; set; }
	}
}