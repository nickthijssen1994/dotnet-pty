using System;
using System.IO;
using Newtonsoft.Json;
using Terminal.Starter.Json.Models;

namespace Terminal.Starter.Json
{
	public static class JsonExtensions
	{
		public static void Write(IRootCommandModel rootCommandModel)
		{
			var json = JsonConvert.SerializeObject(rootCommandModel, Formatting.Indented);
			WriteToFile(json);
		}

		private static void WriteToFile(string input)
		{
			var fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands.json");
			using StreamWriter file = new(fileLocation, false);
			file.Write(input);
			Console.WriteLine("Exported Commands Description As JSON To: " + fileLocation);
		}
	}
}