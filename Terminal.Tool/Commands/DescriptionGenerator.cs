﻿using System;
using System.Collections.Generic;

namespace Terminal.Tool.Commands
{
	public static class DescriptionGenerator
	{
		private static readonly string[] _verbs =
		{
			"Downloading", "Rerouting", "Retriculating", "Collapsing", "Folding", "Solving", "Colliding", "Measuring"
		};

		private static readonly string[] _nouns =
			{ "internet", "splines", "space", "capacitators", "quarks", "algorithms", "data structures", "spacetime" };

		private static readonly Random _random;
		private static readonly HashSet<string> _used;

		static DescriptionGenerator()
		{
			_random = new Random(DateTime.Now.Millisecond);
			_used = new HashSet<string>();
		}

		public static bool TryGenerate(out string name)
		{
			var iterations = 0;
			while (iterations < 25)
			{
				name = Generate();
				if (!_used.Contains(name))
				{
					_used.Add(name);
					return true;
				}

				iterations++;
			}

			name = Generate();
			return false;
		}

		public static string Generate()
		{
			return _verbs[_random.Next(0, _verbs.Length)]
			       + " " + _nouns[_random.Next(0, _nouns.Length)];
		}
	}
}