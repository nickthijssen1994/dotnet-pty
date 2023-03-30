namespace Terminal.Starter.Json.Models
{
	public interface IArgumentModel
	{
		string name { get; set; }
		string? description { get; set; }
		string valueType { get; set; }
	}
}