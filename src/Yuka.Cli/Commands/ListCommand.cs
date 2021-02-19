using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.IO;
using Yuka.Cli.Util;

namespace Yuka.Cli.Commands {
	public class ListCommand : Command {
		public ListCommand(CommandParameters parameters) : base(parameters) { }

		public override string Name => "list";

		public override string[] Description => new[] {
			"Lists files in an archive or directory"
		};

		public override (string syntax, string description)[] Usage => new[] {
			("source", "Display all files in '\absource\a-'"),
			("source filter1 filter2 ...", "Display all files in '\absource\a-' that match at least one \abfilter\a-"),
			("", "Specify all parameters with flags")
		};

		public override (char shorthand, string name, string fallback, string description)[] Flags => new[] {
			('s', "source", null,    "Source location"),
			('m', "mode",   "tree",  "The display mode (valid values: \ablist\a-, \abtree\a-)"),
			('t', "tree",   null,    "Present the files as a directory tree, equal to \ac--mode=tree\a-"),
			('l', "list",   null,    "Present the files as a flat list, equal to \ac--mode=list\a-"),
			('w', "wait",   "false", "Whether to wait after listing the files")
		};

		public override bool Execute() {
			if(Parameters.GetBool("quiet", 'q', false)) return true;

			string sourcePath;
			string[] filters = { "*.*" };

			switch(Arguments.Length) {
				case 0:
					sourcePath = Parameters.GetString("source", 's', null);
					break;
				case 1:
					sourcePath = Arguments[0];
					break;
				default:
					sourcePath = Arguments[0];
					filters = Arguments.Skip(1).ToArray();
					break;
			}

			using(var sourceFs = FileSystem.OpenExisting(sourcePath)) {
				var files = filters.SelectMany(sourceFs.GetFiles).Distinct().ToList();

				string mode =
					Parameters.GetBool("tree", 't', false) ? "tree" :
					Parameters.GetBool("list", 'l', false) ? "list" :
					Parameters.GetString("mode", 'm', "tree");


				switch(mode) {
					case "tree":
						DisplayTree(files, sourcePath);
						break;
					case "list":
						DisplayList(files);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(mode), mode,
							"Display mode must be one of the following: tree, list");
				}
			}

			Wait(false);
			return true;
		}

		protected void DisplayList(List<string> files) {
			files.Sort();
			foreach(string file in files) {
				string folderName = Path.GetDirectoryName(file);
				string fileName = Path.GetFileName(file);
				if(folderName == null) {
					Output.WriteLineColored("\ae" + fileName + "\a-");
				}
				else {
					Output.WriteLineColored("\ab" + folderName + "\\\ae" + fileName + "\a-");
				}
			}
		}

		protected void DisplayTree(List<string> files, string rootPath) {
			var root = new FolderEntry(rootPath);
			files.ForEach(root.AddFile);

			Output.WriteLineColored("\ab" + rootPath + "\a-");
			PrintTree(root, "");
		}

		protected void PrintTree(FolderEntry root, string indent) {
			int totalEntries = root.Folders.Count + root.Files.Count;
			for(int i = 0; i < root.Folders.Count; i++) {
				var folder = root.Folders[i];
				Output.Write(indent);
				Output.Write(i == totalEntries - 1 ? "└╴" : "├╴");
				Output.WriteLineColored("\ab" + folder.Name + "\a-");
				PrintTree(folder, indent + (i == totalEntries - 1 ? "  " : "│ "));
			}
			for(int i = 0; i < root.Files.Count; i++) {
				var file = root.Files[i];
				Output.Write(indent);
				Output.Write(i == root.Files.Count - 1 ? "└╴" : "├╴");
				Output.WriteLineColored("\ae" + file.Name + "\a-");
			}
		}

		protected abstract class FileSystemEntry {
			public readonly string EntryPath;
			public string Name => Path.GetFileName(EntryPath);
			protected FileSystemEntry(string path) {
				EntryPath = path;
			}
		}

		protected sealed class FileEntry : FileSystemEntry {
			public FileEntry(string path) : base(path) { }
		}

		protected sealed class FolderEntry : FileSystemEntry {
			public readonly List<FolderEntry> Folders = new List<FolderEntry>();
			public readonly List<FileEntry> Files = new List<FileEntry>();
			public FolderEntry(string path) : base(path) { }

			public FolderEntry GetFolder(string path) {
				if(string.IsNullOrEmpty(path)) return this;
				var parent = GetFolder(Path.GetDirectoryName(path));
				var entry = parent.Folders.Find(child => child.EntryPath == path);
				if(entry == null) {
					entry = new FolderEntry(path);
					parent.Folders.Add(entry);
				}
				return entry;
			}

			public void AddFile(string path) {
				var folder = GetFolder(Path.GetDirectoryName(path));
				folder.Files.Add(new FileEntry(path));
			}
		}
	}
}