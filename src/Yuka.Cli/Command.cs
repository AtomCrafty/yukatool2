using System;
using System.Collections.Generic;
using System.Linq;
using Yuka.Cli.Commands;
using Yuka.Util;

namespace Yuka.Cli {
	public abstract class Command {

		public readonly CommandParameters Parameters;
		public readonly string[] Arguments;
		public abstract string Name { get; }
		public abstract string[] Description { get; }

		public abstract (string syntax, string description)[] Usage { get; }

		protected Command(CommandParameters parameters) {
			Parameters = parameters;
			Arguments = parameters.Arguments.Skip(1).ToArray();
		}

		public abstract bool Execute();

		#region Static

		private static readonly Dictionary<string, Func<CommandParameters, Command>> CommandFactories = new Dictionary<string, Func<CommandParameters, Command>> {
			{"help", cl => new HelpCommand(cl) }
		};

		public static Command CreateFromName(string name, CommandParameters parameters) {
			return CommandFactories.TryGet(name, param => null)(parameters);
		}

		#endregion
	}
}
