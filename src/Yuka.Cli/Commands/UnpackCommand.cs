using System;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Cli.Commands {
	public class UnpackCommand : CopyCommand {

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
			('s', "source", null, "Source archive"),
			('d', "destination", null, "Destination folder"),
			('f', "format", "unpacked", "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('r', "raw", null, "Short form of \ac--format=keep\a-, overwrites the format flag if set"),
			('o', "overwrite", "false", "Delete existing destination folder"),
			('q', "quiet", null, "Disable user-friendly output"),
			('v', "verbose", null, "Whether to enable detailed output"),
			('w', "wait", null, "Whether to wait after the command finished")
		};

		protected override string DeriveDestinationPath(string sourcePath) {
			return sourcePath.WithoutExtension();
		}

		protected override void CheckPaths(string sourcePath, string destinationPath) {
			base.CheckPaths(sourcePath, destinationPath);

			// make sure the source is an archive
			if(!sourcePath.EndsWith(Format.Ykc.Extension, StringComparison.CurrentCultureIgnoreCase)) {
				throw new ArgumentException("Source is not an archive", nameof(sourcePath));
			}
		}

		protected override void SetCopyModes() {
			base.SetCopyModes();

			// --move flag always false
			_deleteAfterCopy = false;
		}
	}
}