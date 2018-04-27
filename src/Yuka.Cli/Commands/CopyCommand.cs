using System;
using System.Collections.Generic;
using System.Linq;
using Yuka.Cli.Util;
using Yuka.IO;

namespace Yuka.Cli.Commands {
	public class CopyCommand : Command {
		public CopyCommand(CommandParameters parameters) : base(parameters) {
		}

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

		// reminder: when changing these, also change all inheriting classes
		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source", null, "Source location"),
			('d', "destination", null, "Destination location"),
			('f', "format", "keep", "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('r', "raw", null, "Short form of \ac--format=keep\a-, overwrites the format flag if set"),
			('m', "move", "false", "Delete each fileName after successfully copying it"),
			('o', "overwrite", "false", "Delete existing destination archive/folder"),
			('q', "quiet", null, "Disable user-friendly output"),
			('v', "verbose", null, "Whether to enable detailed output"),
			('w', "wait", null, "Whether to wait after the command finished")
		};

		// copy modes
		protected FormatType _prefererredFormatType;
		protected bool _rawCopy, _deleteAfterCopy, _overwriteExisting;

		public override bool Execute() {
			try {
				var (sourcePath, destinationPath, filters) = GetPaths();
				SetCopyModes();

				CheckPaths(sourcePath, destinationPath);

				int fileCount;
				using(var sourceFs = FileSystem.OpenExisting(sourcePath)) {
					using(var destinationFs = FileSystem.OpenOrCreate(destinationPath, sourceFs is SingleFileSystem, _overwriteExisting)) {
						// collect files
						var files = new List<string>();
						foreach(string filter in filters) {
							files.AddRange(sourceFs.GetFiles(filter));
						}

						// call copy loop
						fileCount = CopyFiles(sourceFs, destinationFs, files.Distinct(), _rawCopy, _deleteAfterCopy);
					}
				}

				Success($"Successfully copied {fileCount} files");
			}
			catch(Exception e) when(!System.Diagnostics.Debugger.IsAttached) {
				Error($"{e.GetType().Name}: {e.Message}");
			}

			Wait(false);
			return true;
		}

		protected virtual (string sourcePath, string destinationPath, string[] filters) GetPaths() {
			string sourcePath, destinationPath;
			string[] filters = { "*.*" };

			switch(Arguments.Length) {
				case 0:
					sourcePath = Parameters.GetString("source", 's', null);
					destinationPath = Parameters.GetString("destination", 'd', null) ?? DeriveDestinationPath(sourcePath);
					break;
				case 1:
					sourcePath = Arguments[0];
					destinationPath = Parameters.GetString("destination", 'd', null) ?? DeriveDestinationPath(sourcePath);
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

			return (sourcePath, destinationPath, filters);
		}

		protected virtual string DeriveDestinationPath(string sourcePath) {
			return sourcePath;
		}

		protected virtual void CheckPaths(string sourcePath, string destinationPath) {
			if(sourcePath == null) {
				throw new ArgumentNullException(nameof(sourcePath), "No source path specified");
			}
			if(destinationPath == null) {
				throw new ArgumentNullException(nameof(destinationPath), "No destination path specified");
			}
		}

		protected virtual string FormatTypeFallback => "keep";

		protected virtual void SetCopyModes() {

			string format = Parameters.GetString("format", 'f', FormatTypeFallback).ToLower();
			_rawCopy = Parameters.GetBool("raw", 'r', false);
			_deleteAfterCopy = Parameters.GetBool("move", 'm', false);
			_overwriteExisting = Parameters.GetBool("overwrite", 'o', false);

			switch(format) {
				case "keep":
					_prefererredFormatType = FormatType.None;
					_rawCopy = true;
					break;
				case "packed":
					_prefererredFormatType = FormatType.Packed;
					break;
				case "unpacked":
					_prefererredFormatType = FormatType.Unpacked;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, "Format must be one of the following: keep, packed, unpacked");
			}
		}

		protected virtual FormatPreference GetOutputFormat(object obj, string fileName, Format fileFormat) {
			return new FormatPreference(null, _prefererredFormatType);
		}

		protected virtual (object obj, Format fileFormat) ReadFile(FileSystem sourceFs, string fileName) {
			return FileReader.DecodeObject(fileName, sourceFs, true);
		}

		protected virtual Format WriteFile(object obj, Format inputFormat, FileSystem destinationFs, string fileName) {
			return FileWriter.EncodeObject(obj, fileName, destinationFs, GetOutputFormat(obj, fileName, inputFormat));
		}

		protected virtual int CopyFiles(FileSystem sourceFs, FileSystem destinationFs, IEnumerable<string> files, bool rawCopy, bool deleteAfterCopy) {
			bool verbose = Parameters.GetBool("verbose", 'v', false);
			int fileCount = 0;

			// main loop
			foreach(string fileName in files) {
				if(rawCopy) {
					Log($"Copying \ae{fileName}");
					using(var sourceStream = sourceFs.OpenFile(fileName)) {
						using(var destinationStream = destinationFs.CreateFile(fileName)) {
							sourceStream.CopyTo(destinationStream);
							fileCount++;
						}
					}
				}
				else {

					var (obj, fileFormat) = ReadFile(sourceFs, fileName);

					if(obj == null) {
						if(verbose) Output.WriteLine($"Skipping {fileName}", ConsoleColor.Yellow);
						continue;
					}

					Log($"Decoded \ae{fileName}\a- to \ab{obj.GetType().Name}");

					var outputFormat = WriteFile(obj, fileFormat, destinationFs, fileName);

					Log($"Encoded \ab{obj.GetType().Name}\a- ({outputFormat.GetType().Name})");

					if(deleteAfterCopy) {
						// delete auxiliary files (csv, frm, ani, etc...)
						foreach(string secondaryFile in fileFormat.GetSecondaryFiles(sourceFs, fileName)) {
							if(verbose) Output.WriteLine($"Deleting {secondaryFile}", ConsoleColor.Red);
							sourceFs.DeleteFile(secondaryFile);
						}

						if(verbose) Output.WriteLine($"Deleting {fileName}", ConsoleColor.Red);
						sourceFs.DeleteFile(fileName);
					}

					fileCount++;
				}
			}

			return fileCount;
		}
	}
}