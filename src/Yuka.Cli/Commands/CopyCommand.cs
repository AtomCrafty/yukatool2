using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Cli.Commands {
	public class CopyCommand : Command {
		public static readonly string AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location).ToLower();

		public CopyCommand(CommandParameters parameters) : base(parameters) { }

		public override string Name => "copy";

		public override string[] Description => new[] {
			"Copies files from one place to another while optionally converting them in some way.",
			"\aacopy\a- forms the basis of most other commands."
		};

		public override (string syntax, string description)[] Usage => new[] {
			("source", "Applies the selected conversion too the \abspecified files\a-"),
			("source destination", "Copies all files from \absource\a- to \abdestination\a- while applying the selected conversion"),
			("source destination filter1 filter2 ...", "Copies all files matching at least one \abfilter\a- from \absource\a- to \abdestination\a- while applying the selected conversion"),
			("", "Specify all parameters with flags")
		};

		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source", "false", "Source archive"),
			('d', "destination", "false", "Destination folder"),
			('f', "format", "keep", "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('m', "move", "false", "Delete each file after successfully copying it"),
			('o', "overwrite", "false", "Delete existing destination archive/folder"),
			('q', "quiet", "false", "Disable user-friendly output"),
			('v', "verbose", "false", "Whether to enable detailed output"),
			('w', "wait", "false", "Whether to wait after the command finished")
		};

		public override bool Execute() {
			string sourcePath, destinationPath;
			string[] filters = { "*.*" };
			int fileCount = 0;

			switch(Arguments.Length) {
				case 0:
					sourcePath = Parameters.GetString("source", 's', null);
					destinationPath = Parameters.GetString("destination", 'd', null);
					break;
				case 1:
					sourcePath = Arguments[0];
					destinationPath = Parameters.GetString("destination", 'd', sourcePath);
					break;
				case 2:
					sourcePath = Arguments[0];
					destinationPath = Arguments[1];
					break;
				default:
					sourcePath = Arguments[0];
					destinationPath = Arguments[1];
					filters = Arguments.Skip(2).ToArray();
					break;
			}

			using(var sourceFs = OpenFileSystem(sourcePath)) {
				bool overwriteExisting = Parameters.GetBool("overwrite", 'o', false);
				using(var destinationFs = OpenOrCreateFileSystem(destinationPath, sourceFs is SingleFileSystem, overwriteExisting)) {

					// collect files
					var files = new List<string>();
					foreach(string filter in filters) {
						files.AddRange(sourceFs.GetFiles(filter));
					}

					// determine output format
					string format = Parameters.GetString("format", 'f', "keep").ToLower();
					if(!format.IsOneOf("keep", "packed", "unpacked")) throw new ArgumentOutOfRangeException(nameof(format), format, "Format must be one of the following: keep, packed, unpacked");
					var formatPreference = new FormatPreference(null, format == "packed" ? FormatType.Packed : format == "unpacked" ? FormatType.Unpacked : FormatType.None);
					bool rawCopy = format == "keep";
					bool deleteAfterCopy = Parameters.GetBool("move", 'm', false);

					// main loop
					foreach(string file in files.Distinct()) {
						// TODO verbose logging

						if(rawCopy) {
							using(var sourceStream = sourceFs.OpenFile(file)) {
								using(var destinationStream = destinationFs.CreateFile(file)) {
									sourceStream.CopyTo(destinationStream);
									destinationStream.Flush();
								}
							}
						}
						else {
							// TODO skip auxiliary files (csv, frm, ani, etc...)

							var obj = FileReader.DecodeObject(file, sourceFs);
							FileWriter.EncodeObject(obj, file, destinationFs, formatPreference);

							if(deleteAfterCopy) {
								// TODO delete auxiliary files (csv, frm, ani, etc...)

								sourceFs.DeleteFile(file);
							}

							fileCount++;
						}
					}
				}
			}

			Success($"Successfully copied {fileCount} files");
			Wait(false);
			return true;
		}

		public static FileSystem OpenFileSystem(string path) {
			if(Directory.Exists(path)) {
				return FileSystem.FromFolder(path);
			}

			if(File.Exists(path)) {
				try {
					return FileSystem.FromArchive(path);
				}
				catch(Exception) {
					return FileSystem.FromFile(path);
				}
			}

			throw new FileNotFoundException(path);
		}

		public static FileSystem CreateFileSystem(string path, bool singleFile, bool deleteExisting = false) {
			// TODO YkcFormat
			if(path.EndsWith(".ykc")) {
				if(deleteExisting && File.Exists(path)) File.Delete(path);
				return FileSystem.NewArchive(path);
			}
			if(singleFile) {
				if(deleteExisting && File.Exists(path)) File.Delete(path);
				return FileSystem.NewFile(path);
			}
			if(deleteExisting && Directory.Exists(path)) File.Delete(path);
			return FileSystem.NewFolder(path);
		}

		public static FileSystem OpenOrCreateFileSystem(string path, bool singleFile, bool overwriteExisting) {
			if(overwriteExisting) return CreateFileSystem(path, singleFile, true);

			try {
				return OpenFileSystem(path);
			}
			catch(FileNotFoundException) {
				return CreateFileSystem(path, singleFile);
			}
		}
	}
}