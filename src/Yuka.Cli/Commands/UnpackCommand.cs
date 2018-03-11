using System;
using System.IO;
using System.Reflection;
using Yuka.Cli.Util;
using Yuka.Script;

namespace Yuka.Cli.Commands {
	public class UnpackCommand : Command {
		public static readonly string AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location).ToLower();

		public UnpackCommand(CommandParameters parameters) : base(parameters) { }

		public override string Name => "unpack";

		public override string[] Description => new[] {
			"Unpacks a yuka archive"
		};

		public override (string syntax, string description)[] Usage => new[] {
			("source", "Unpack all files from the \absource\a- archive into a folder with the same name"),
			("source destination", "Unpack all files from the \absource\a- archive into the \abdestination\a- folder"),
			("source destination filter1 filter2 ...", "Unpack all files matching at least one \abfilter\a- from the \absource\a- archive into the \abdestination\a- folder"),
			("", "Specify all parameters with flags")
		};

		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source", "false", "Source archive"),
			('d', "destination", "false", "Destination folder"),
			('r', "raw", "false", "Whether to extract all files as-is, without converting them"),
			('v', "verbose", "false", "Whether to enable detailed output"),
			('w', "wait", "false", "Whether to wait after the command finished")
		};

		public override bool Execute() {




			Wait(false);
			return true;
		}
	}
}