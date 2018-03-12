using System;
using System.IO;
using System.Reflection;
using Yuka.Cli.Util;
using Yuka.IO;
using Yuka.Script;

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
			('s', "source", "false", "Source archive"),
			('d', "destination", "false", "Destination folder"),
			('f', "format", "unpacked", "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('r', "raw", null, "Short form of \ac--format=keep\a-, overwrites the format flag if set"),
			('o', "overwrite", "false", "Delete existing destination archive/folder"),
			('q', "quiet", null, "Disable user-friendly output"),
			('v', "verbose", null, "Whether to enable detailed output"),
			('w', "wait", null, "Whether to wait after the command finished")
		};


		protected override void CheckPaths(string sourcePath, string destinationPath) {
			base.CheckPaths(sourcePath, destinationPath);

			// make sure the source is an archive TODO YkcFormat
			if(sourcePath.EndsWith(".ykc", StringComparison.CurrentCultureIgnoreCase)) {
				throw new ArgumentException("Source is not an archive", nameof(sourcePath));
			}
		}

		protected override (FormatPreference formatPreference, bool rawCopy, bool deleteAfterCopy) GetCopyModes() {

			string format = Parameters.GetString("format", 'f', "unpacked").ToLower();
			bool rawCopy = Parameters.GetBool("raw", 'r', false);

			switch(format) {
				case "keep":
					return (new FormatPreference(null, FormatType.None), rawCopy: true, deleteAfterCopy: false);
				case "packed":
					return (new FormatPreference(null, FormatType.Packed), rawCopy, deleteAfterCopy: false);
				case "unpacked":
					return (new FormatPreference(null, FormatType.Unpacked), rawCopy, deleteAfterCopy: false);
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, "Format must be one of the following: keep, packed, unpacked");
			}
		}
	}
}