using System.Collections.Generic;

namespace Terminal.Starter.Json.Models
{
	public interface ICommandModel
	{
		string name { get; set; }
		string? description { get; set; }
		List<string> aliases { get; set; }
		List<IOptionModel>? options { get; set; }
		List<IArgumentModel>? arguments { get; set; }
		List<ICommandModel>? commands { get; set; }
	}
}