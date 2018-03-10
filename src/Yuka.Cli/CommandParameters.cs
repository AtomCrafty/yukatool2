using System;
using System.Collections.Generic;

namespace Yuka.Cli {
	public class CommandParameters {

		public readonly List<string> Arguments = new List<string>();
		public readonly Dictionary<string, string> Flags = new Dictionary<string, string>();

		public CommandParameters(string[] cliArgs) {
			foreach(string argument in cliArgs) {
				if(argument.StartsWith("-")) {
					ParseFlag(argument);
				}
				else {
					Arguments.Add(argument);
				}
			}
		}

		public void ParseFlag(string flag) {
			var parts = flag.Split(new[] { '=' }, 2);

			string key = parts[0];
			string value = parts.Length == 2 ? parts[1] : "true";
			if(value.StartsWith("\"") && value.EndsWith("\"")) {
				value = value.Substring(1, value.Length - 2);
			}

			SetFlag(key, value);
		}

		public void SetFlag(string key, string value) {
			if(key.StartsWith("--") || key.Length == 2) {
				Flags[key] = value;
			}
			else {
				// expand -abc to -a -b -c
				foreach(char shorthand in key.Substring(1)) {
					Flags["-" + shorthand] = value;
				}
			}
		}

		#region Getters

		#region Booleans

		public bool GetBoolFlagByKey(string key, bool fallback) {
			if(!Flags.ContainsKey(key)) return fallback;

			return bool.TryParse(Flags[key], out bool value) ? value : fallback;
		}

		public bool GetBoolFlag(string name, bool fallback) {
			return GetBoolFlagByKey("--" + name, fallback);
		}

		public bool GetBoolFlag(char shorthand, bool fallback) {
			return GetBoolFlagByKey("-" + shorthand, fallback);
		}

		public bool GetBoolFlag(string name, char shorthand, bool fallback) {
			return GetBoolFlag(name, GetBoolFlag(shorthand, fallback));
		}

		#endregion

		#region Integers

		public int GetIntFlagByKey(string key, int fallback) {
			if(!Flags.ContainsKey(key)) return fallback;

			return int.TryParse(Flags[key], out int value) ? value : fallback;
		}

		public int GetIntFlag(string name, int fallback) {
			return GetIntFlagByKey("--" + name, fallback);
		}

		public int GetIntFlag(char shorthand, int fallback) {
			return GetIntFlagByKey("-" + shorthand, fallback);
		}

		public int GetIntFlag(string name, char shorthand, int fallback) {
			return GetIntFlag(name, GetIntFlag(shorthand, fallback));
		}

		#endregion

		#region Strings

		public string GetStringFlagByKey(string key, string fallback) {
			return Flags.ContainsKey(key) ? Flags[key] : fallback;
		}

		public string GetStringFlag(string name, string fallback) {
			return GetStringFlagByKey("--" + name, fallback);
		}

		public string GetStringFlag(char shorthand, string fallback) {
			return GetStringFlagByKey("-" + shorthand, fallback);
		}

		public string GetStringFlag(string name, char shorthand, string fallback) {
			return GetStringFlag(name, GetStringFlag(shorthand, fallback));
		}

		#endregion

		#endregion

		#region Static

		public static CommandParameters Empty => new CommandParameters(Array.Empty<string>());

		public static CommandParameters ParseArguments(string[] args) {
			return new CommandParameters(args);
		}

		#endregion
	}
}
