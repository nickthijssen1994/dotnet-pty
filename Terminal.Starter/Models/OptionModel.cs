using System.Collections.Generic;
using System.CommandLine;
using Terminal.Starter.Json.Models;

namespace Terminal.Starter.Models
{
	public class OptionModel : IOptionModel
	{
		public OptionModel(Option option)
		{
			name = option.Name;
			isRequired = option.IsRequired;
			description = option.Description;
			valueType = option.ValueType.Name;
			aliases = new List<string>(option.Aliases);
		}

		public string name { get; set; }
		public string? description { get; set; }
		public bool isRequired { get; set; }
		public string valueType { get; set; }
		public List<string> aliases { get; set; }
	}
}