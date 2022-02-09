using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Yuka.Cli.Util;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Cli.Commands {
	public class CopyCommand : Command {
		public CopyCommand(CommandParameters parameters) : base(parameters) {
		}

		public override string Name => "copy";

		public override string[] Description => new[] {
			"Copies files from one place to another while optionally converting them in some way.",
			"Most other commands use copy internally."
		};

		public override (string syntax, string description)[] Usage => new[] {
			("source", "Applies the selected conversion to the \abspecified files\a-"),
			("source destination", "Copies all files from \absource\a- to \abdestination\a- while applying the selected conversion"),
			("source destination filter1 filter2 ...", "Copies all files matching at least one \abfilter\a- from \absource\a- to\n\abdestination\a- while applying the selected conversion"),
			("", "Specify all parameters with flags")
		};

		// reminder: when changing these, also change all inheriting classes
		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source",          null,       "Source location"),
			('d', "destination",     null,       "Destination location"),
			('f', "format",          "keep",     "The preferred output format (valid values: \abkeep\a-, \abpacked\a-, \abunpacked\a-)"),
			('r', "raw",             null,       "Short form of \ac--format=keep\a-, overwrites the format flag if set"),
			('m', "move",            "false",    "Delete each fileName after successfully copying it"),
			(' ', "manifest",        "false",    "Generate a manifest file"),
			('i', "ignore-manifest", "false",    "Ignore the manifest, if it exists"),
			(' ', "normalize-case",  "true",     "Convert all file names to lower case"),
			('o', "overwrite",       "false",    "Delete existing destination archive/folder"),
			('q', "quiet",           null,       "Disable user-friendly output"),
			('v', "verbose",         null,       "Whether to enable detailed output"),
			('w', "wait",            null,       "Whether to wait after the command finished")
		};

		// copy modes
		protected FormatType _prefererredFormatType;
		protected bool _rawCopy, _deleteAfterCopy, _overwriteExisting, _generateManifest, _ignoreManifest, _normalizeCase;
		protected Manifest _inputManifest;

		public override bool Execute() {
			try {
				var (sourcePath, destinationPath, filters) = GetPaths();
				SetCopyModes();

				CheckPaths(sourcePath, destinationPath);

				Manifest outputManifest;
				using(var sourceFs = FileSystem.OpenExisting(sourcePath)) {
					_inputManifest = _ignoreManifest ? null : ReadManifest(sourceFs);

					using(var destinationFs = FileSystem.OpenOrCreate(destinationPath, sourceFs is SingleFileSystem, _overwriteExisting)) {
						// collect files
						var files = filters.SelectMany(sourceFs.GetFiles).Distinct();

						// call copy loop
						outputManifest = CopyFiles(sourceFs, destinationFs, files, _rawCopy, _deleteAfterCopy);
						if(_generateManifest) WriteManifest(outputManifest, destinationFs);
					}
				}

				Success($"Successfully copied {outputManifest.Count} files");
			}
			catch(Exception e) when(!System.Diagnostics.Debugger.IsAttached) {
				Error($"{e.GetType().Name}: {e.Message}");
			}

			Wait(false);
			return true;
		}

		protected virtual void WriteManifest(Manifest manifest, FileSystem fs) {
			using(var writer = new JsonTextWriter(new StreamWriter(fs.CreateFile(".manifest")))) {
				new JsonSerializer { Formatting = Formatting.Indented }.Serialize(writer, manifest);
			}
		}

		protected virtual Manifest ReadManifest(FileSystem fs) {
			if(!fs.FileExists(".manifest")) return null;
			using(var reader = new JsonTextReader(new StreamReader(fs.OpenFile(".manifest")))) {
				return new JsonSerializer { Formatting = Formatting.Indented }.Deserialize<Manifest>(reader);
			}
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
			return sourcePath + "-copy";
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
			_generateManifest = Parameters.GetBool("manifest", false);
			_ignoreManifest = Parameters.GetBool("ignore-manifest", 'i', false);
			_normalizeCase = Parameters.GetBool("normalize-case", ' ', false);
			_overwriteExisting = Parameters.GetBool("overwrite", 'o', false);

			EncodingUtils.SetShiftJisTunnelFile(Parameters.GetString("tunnel-file", 't', null));

			switch(format) {
				case "keep":
					_prefererredFormatType = FormatType.None;
					_rawCopy = true;
					break;
				case "pack":
				case "packed":
					_prefererredFormatType = FormatType.Packed;
					break;
				case "unpack":
				case "unpacked":
					_prefererredFormatType = FormatType.Unpacked;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, "Format must be one of the following: keep, pack, packed, unpack, unpacked");
			}
		}

		protected virtual FormatPreference GetOutputFormat(object obj, string fileName, Format fileFormat) {
			if(_inputManifest != null) {
				foreach(var (source, target) in _inputManifest) {
					// find the entry that produced this file
					if(target.Any(pair => pair.Name == fileName)) {
						// return the original format
						return new FormatPreference(source[0].Format, _prefererredFormatType);
					}
				}
			}
			return new FormatPreference(null, _prefererredFormatType);
		}

		protected virtual (object obj, Format fileFormat) ReadFile(FileSystem sourceFs, string fileName, FileList files) {
			return FileReader.DecodeObject(fileName, sourceFs, files, true);
		}

		protected virtual void WriteFile(object obj, Format inputFormat, FileSystem destinationFs, string fileName, FileList files) {
			FileWriter.EncodeObject(obj, fileName, destinationFs, GetOutputFormat(obj, fileName, inputFormat), files);
		}

		protected virtual Manifest CopyFiles(FileSystem sourceFs, FileSystem destinationFs, IEnumerable<string> files, bool rawCopy, bool deleteAfterCopy) {
			bool verbose = Parameters.GetBool("verbose", 'v', false);

			var manifest = new Manifest();

			// main loop
			foreach(string file in files) {
				if(file == ".manifest") continue;
				string fileName = _normalizeCase ? file.ToLower() : file;

				if(rawCopy) {
					Log($"Copying \ae{fileName}");
					PerformRawCopy(sourceFs, destinationFs, fileName, manifest);
				}
				else {

					var readFiles = new FileList();
					var writtenFiles = new FileList();

					var (obj, fileFormat) = ReadFile(sourceFs, fileName, readFiles);

					if(obj == null) {
						//if(verbose) Output.WriteLine($"Skipping {fileName}", ConsoleColor.Yellow);
						continue;
					}
					//if(verbose) Output.WriteLine($"Processing {fileName}", ConsoleColor.Yellow);

					Log($"Decoded [{string.Join(", ", readFiles.Select(pair => $"\ab{pair.Format.Name} \ae{pair.Name}\a-"))}] to \ab{obj.GetType().Name}");

					try {
						WriteFile(obj, fileFormat, destinationFs, fileName, writtenFiles);
						manifest.Add(readFiles, writtenFiles);

						Log($"Encoded [{string.Join(", ", writtenFiles.Select(pair => $"\ab{pair.Format.Name} \ae{pair.Name}\a-"))}]");
						Log("");

						if(deleteAfterCopy) {
							// delete auxiliary files (csv, frm, ani, etc...)
							foreach(string secondaryFile in fileFormat.GetSecondaryFiles(sourceFs, fileName)) {
								if(verbose) Output.WriteLine($"Deleting {secondaryFile}", ConsoleColor.Red);
								sourceFs.DeleteFile(secondaryFile);
							}

							if(verbose) Output.WriteLine($"Deleting {fileName}", ConsoleColor.Red);
							sourceFs.DeleteFile(fileName);
						}
					}
					catch(Exception e) {
						Error($"Encountered {e.GetType().Name} while encoding: {e.Message}");
						Log("Falling back to raw copy");
						PerformRawCopy(sourceFs, destinationFs, fileName, manifest);
					}
				}
			}

			EncodingUtils.WriteShiftJisTunnelFile();

			return manifest;
		}

		private static void PerformRawCopy(FileSystem sourceFs, FileSystem destinationFs, string fileName, Manifest manifest) {
			using(var sourceStream = sourceFs.OpenFile(fileName)) {
				using(var destinationStream = destinationFs.CreateFile(fileName)) {
					sourceStream.CopyTo(destinationStream);

					manifest.Add(
						new FileList { (fileName, Format.Raw) },
						new FileList { (fileName, Format.Raw) }
					);
				}
			}
		}
	}
}