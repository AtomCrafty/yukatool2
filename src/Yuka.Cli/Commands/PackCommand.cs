using System;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Cli.Commands {
	public class PackCommand : CopyCommand {

		public PackCommand(CommandParameters parameters) : base(parameters) { }

		public override string Name => "pack";

		public override string[] Description => new[] {
			"Packs a folder into a yuka archive"
		};

		public override (string syntax, string description)[] Usage => new[] {
			("source", "Pack all files from the \absource\a- folder into an archive with the same name"),
			("source destination", "Pack all files from the \absource\a- folder into the \abdestination\a- archive"),
			("source destination filter1 filter2 ...", "Unpack all files matching at least one \abfilter\a- from the \absource\a- archive into the \abdestination\a- folder"),
			("", "Specify all parameters with flags")
		};

		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source", null, "Source folder"),
			('d', "destination", null, "Destination archive"),
			('f', "format", "packed", "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('r', "raw", null, "Short form of \ac--format=keep\a-, overwrites the format flag if set"),
			('o', "overwrite", "true", "Delete existing destination archive"),
			('a', "append", null, "Appends files to an existing archive, equal to \ac--overwrite=false"),
			('q', "quiet", null, "Disable user-friendly output"),
			('v', "verbose", null, "Whether to enable detailed output"),
			('w', "wait", null, "Whether to wait after the command finished")
		};

		protected override string DeriveDestinationPath(string sourcePath) {
			return sourcePath.WithExtension(Format.Ykc.Extension);
		}

		protected override void CheckPaths(string sourcePath, string destinationPath) {
			base.CheckPaths(sourcePath, destinationPath);

			// make sure the destination is an archive
			if(!destinationPath.EndsWith(Format.Ykc.Extension, StringComparison.CurrentCultureIgnoreCase)) {
				throw new ArgumentException("Destination is not an archive", nameof(destinationPath));
			}
		}

		protected override string FormatTypeFallback => "packed";

		protected override void SetCopyModes() {
			base.SetCopyModes();

			// --move flag always false
			_deleteAfterCopy = false;
			_overwriteExisting = Parameters.GetBool("overwrite", 'o', true) && !Parameters.GetBool("append", 'a', false);
		}
	}
}