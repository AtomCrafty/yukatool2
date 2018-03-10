using System;
using System.IO;
using System.Reflection;
using Yuka.Cli.Util;
using Yuka.Script;

namespace Yuka.Cli.Commands {
	public class HelpCommand : Command {

		public static readonly string AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location).ToLower();

		public HelpCommand(CommandParameters parameters) : base(parameters) {
		}

		public override string Name => "help";
		public override string[] Description => new[]{
			""
		};

		public override (string syntax, string description)[] Usage => new[] {
			("help", "Display general information about yukatool"),
			("help <command>", "Display the help page of a specific command")
		};

		public override bool Execute() {
			DisplayVersionInformation();

			if(Arguments.Length > 0) {
				var command = Command.CreateFromName(Arguments[0], CommandParameters.Empty);
				if(command != null) {
					var _ = command.Usage;

					return true;
				}
			}

			DisplayGeneralHelpPage();
			return true;
		}

		public static void DisplayVersionInformation() {
			var cliVersion = Assembly.GetEntryAssembly().GetName().Version;
			var coreVersion = Assembly.GetAssembly(typeof(YukaScript)).GetName().Version;
			Output.WriteLine($"YukaTool Cli v{cliVersion.ToString(3)} running YukaTool Core v{coreVersion.ToString(3)}", ConsoleColor.Yellow);
		}

		public static void DisplayGeneralHelpPage() {

		}
	}
}