using System.Collections.Generic;

namespace Terminal.Starter.Json.Models
{
	public interface IOptionModel
	{
		string name { get; set; }
		string? description { get; set; }
		bool isRequired { get; set; }
		string valueType { get; set; }
		List<string> aliases { get; set; }
	}
}