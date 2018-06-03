using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Yuka.Cli.Commands;
using Yuka.Cli.Util;
using Yuka.Container;
using Yuka.Graphics;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script;
using Yuka.Script.Data;
using Yuka.Script.Source;
using Yuka.Util;

namespace Yuka.Cli {

#if true

	public class Program {
		public static void Main(string[] args) {
			if(!Command.TryRun(args)) {
				new HelpCommand(CommandParameters.Empty).Execute();
			}
			Output.Flush();
		}
	}

#else

	public static class Tests {

		public static void Main() {
			Disassemble();
			//DecompileGame();
			//CompileGame();
		}

		public static void Disassemble() {
			const string path = @"S:\Games\Visual Novels\Imouto Sama FD\data01\yks\all\all_00030.yks";
			var fs = FileSystem.FromFile(path);
			string name = Path.GetFileName(path);

			var script = FileReader.Decode<YukaScript>(name, fs);
			FileWriter.Encode(script, name.WithExtension(Format.Yki.Extension), fs, new FormatPreference(Format.Yki));
		}

		public static void CompileGame() {
			using(FileSystem srcFs = FileSystem.FromFolder(@"C:\Temp\CopyTest\decompiled"), dstFs = FileSystem.NewFolder(@"C:\Temp\CopyTest\compiled")) {
				foreach(string file in srcFs.GetFiles("*.ykd")) {
					Console.WriteLine(file);

					var script = FileReader.Decode<YukaScript>(file, srcFs);
					FileWriter.Encode(script, file, dstFs, new FormatPreference(Format.Yks));

				}
			}
			//Console.ReadLine();
		}

		public static void DecompileGame() {
			using(FileSystem srcFs = FileSystem.FromFolder(@"C:\Temp\CopyTest\compiled"), dstFs = FileSystem.NewFolder(@"C:\Temp\CopyTest\decompiled")) {
				foreach(string file in srcFs.GetFiles("*.yks")) {
					Console.WriteLine(file);

					var script = FileReader.Decode<YukaScript>(file, srcFs);
					FileWriter.Encode(script, file, dstFs, new FormatPreference(Format.Ykd));

				}
			}
			//Console.ReadLine();
		}

		public static void Parser() {
			using(var fs = FileSystem.FromFolder(@"S:\Games\Visual Novels\Lover Able Unpacked")) {
				foreach(string file in fs.GetFiles("*.ykd")) {
					Console.WriteLine(file);
					string outPath = Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileName(file).WithExtension(".parsed.yks"));

					var script = FileReader.Decode<YukaScript>(file, fs);
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Yks));
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Yki));
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Ykd));

				}
			}
			Console.ReadLine();
		}

		public static void Recompile() {
			using(var fs = FileSystem.FromFolder(@"S:\Games\Visual Novels\Lover Able Unpacked")) {
				foreach(string file in fs.GetFiles("*root_select.org.yks")) {
					Console.WriteLine(file);
					string outPath = Path.Combine(Path.GetDirectoryName(file) ?? "", "root_select.yks");

					var script = FileReader.Decode<YukaScript>(file, fs);
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Ykd));
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Yki));
					FileWriter.Encode(script, outPath, fs, new FormatPreference(Format.Yks));

				}
			}
			Console.ReadLine();
		}

		public static void Lexer() {
			using(var fs = FileSystem.FromFolder(@"S:\Games\Visual Novels\Lover Able Unpacked")) {
				using(var reader = new StreamReader(fs.OpenFile("start.ykd"))) {
					var lexer = new Lexer(reader, "start.ykd");

					Token token;
					while((token = lexer.LexToken()) != null) {
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.Write(token.Source);
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine($" ({token.Kind})");
					}

					Console.ResetColor();
					Console.ReadLine();
				}
			}
		}

		public static void ScriptTest() {
			using(var fs = FileSystem.FromFolder(@"C:\Temp\CopyTest")) {
				var script = FileReader.Decode<YukaScript>("root_select-org.yks", fs);
				FileWriter.Encode(script, "root_select.yki", fs, new FormatPreference(Format.Yki));
				script = FileReader.Decode<YukaScript>("root_select.yki", fs);
				script.Decompile();
				FileWriter.Encode(script, "root_select.ykd", fs, new FormatPreference(Format.Ykd));
				script.Compile();
				FileWriter.Encode(script, "root_select.yks", fs, new FormatPreference(Format.Yks));
			}
		}

		public static void InstructionParser() {
			using(var fs = FileSystem.FromFolder(@"C:\Temp\CopyTest")) {
				var script = FileReader.Decode<YukaScript>("debug_before.yki", fs);
				FileWriter.Encode(script, "debug_after.yki", fs, new FormatPreference(Format.Yki));
			}
		}

		public static void ScriptBenchmark() {
			const int iterations = 10000;

			var sw = new Stopwatch();
			sw.Start();
			using(var fs = FileSystem.FromFolder(@"C:\Temp\CopyTest")) {
				for(int i = 0; i < iterations; i++) {

					// Disassemble
					var script = FileReader.Decode<YukaScript>("debug.yks", fs);

					// Decompile
					script.Decompile();

					// Compile
					script.Compile();

					// Assemble
					FileWriter.Encode(script, "output.yks", fs, new FormatPreference(Format.Yks));
				}
			}
			sw.Stop();
			Console.WriteLine($"Processed {iterations} scripts in {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"Average time per script: {(double)sw.ElapsedMilliseconds / iterations:0.##} ms");
			Console.ReadLine();
		}

		public static void CsvWrite() {
			using(var fs = FileSystem.FromFolder(@"C:\Temp\CopyTest")) {

				var table = FileReader.Decode<StringTable>("kaho_01.csv", fs);
				FileWriter.Encode(table, "kaho_01_export.csv", fs, new FormatPreference(Format.Csv));

				table = FileReader.Decode<StringTable>("kaho_01_export.csv", fs);
				FileWriter.Encode(table, "kaho_01_export_2.csv", fs, new FormatPreference(Format.Csv));

			}
		}

		public static void CsvRead() {
			using(var fs = FileSystem.FromFolder(@"S:\Games\Visual Novels\Lover Able\data01-src\yks\story\common")) {

				var table = FileReader.Decode<StringTable>("chapter1-1.csv", fs);
				Console.WriteLine($"Imported {table.Count} records");

			}
		}

		public static void ReAssembleStart() {

			using(var fs = FileSystem.FromArchive(@"C:\Temp\CopyTest\data01.ykc")) {
				var script = FileReader.Decode<YukaScript>("start.yks", fs);

				//FileWriter.Encode(script, "start.yks", fs, new FormatPreference(Format.Yks));

				using(var dir = FileSystem.FromFolder(@"C:\Temp\CopyTest")) {
					FileWriter.Encode(script, "start.yks", dir, new FormatPreference(Format.Yks));
				}
			}

			Console.ReadLine();
		}

		public static void Assemble() {
			const string path = @"C:\Temp\CopyTest\debug.yks";
			var fs = FileSystem.FromFile(path);
			var fn = Path.GetFileName(path);

			var sw = new Stopwatch();
			sw.Start();

			int l = 1;
			for(int i = 0; i < l; i++) {
				var script = FileReader.Decode<YukaScript>(fn, fs);
				FileWriter.Encode(script, fn.WithoutExtension() + "_new", fs, new FormatPreference(Format.Yks));
			}
			sw.Stop();
			Console.WriteLine();
			Console.WriteLine($"Processed {l} scripts in {sw.ElapsedMilliseconds:0.##} ms");
			Console.WriteLine($"Average time per script: {sw.ElapsedMilliseconds / (float)l:0.##} ms");

			Console.ReadLine();
		}

		public static void Decompile() {
			const string path = @"C:\Temp\CopyTest\debug.yks";
			var fs = FileSystem.FromFile(path);
			var fn = Path.GetFileName(path);

			var sw = new Stopwatch();
			sw.Start();

			int l = 10000;
			for(int i = 0; i < l; i++) {
				var script = FileReader.Decode<YukaScript>(fn, fs);
				if(i == 0) {
					FileWriter.Encode(script, fn.WithExtension(Format.Yki.Extension), fs, new FormatPreference(Format.Yki));
				}
				script.Decompile();
				Console.Write($"\r{(i + 1) * 100f / l}%  ");
			}
			sw.Stop();
			Console.WriteLine();
			Console.WriteLine($"Processed {l} scripts in {sw.ElapsedMilliseconds:0.##} ms");
			Console.WriteLine($"Average time per script: {sw.ElapsedMilliseconds / (float)l:0.##} ms");

			Console.ReadLine();
		}

		public static void SemiramisExtract() {
			foreach(string arcPath in Directory.GetFiles(@"S:\Games\Visual Novels\Semiramis no Tenbin", "*.ykc")) {
				var arc = FileSystem.FromArchive(arcPath);
				var dir = FileSystem.NewFolder(arcPath.WithoutExtension());

				foreach(string file in arc.GetFiles()) {
					using(Stream source = arc.OpenFile(file), destination = dir.CreateFile(file)) {
						source.CopyTo(destination);
					}
				}

				dir.Dispose();
				arc.Dispose();
			}
		}

		public static void YkgWrite() {
			const string path = @"C:\Temp\CopyTest\system.png";
			string name = Path.GetFileName(path);
			var fs = FileSystem.FromFile(path);

			var ykg = FileReader.Decode<YukaGraphic>(name, fs);
			// ykg is the only packed format for Yuka.Graphics.Graphic
			FileWriter.Encode(ykg, "system2.ykg", fs, new FormatPreference(null, FormatType.Packed));
		}

		public static void AlphaCopy2() {
			const string path = @"C:\Temp\CopyTest\system2.ykg";
			string name = Path.GetFileName(path);
			var fs = FileSystem.FromFile(path);

			var ykg = FileReader.Decode<YukaGraphic>(name, fs);
			Options.YkgMergeAlphaChannelOnExport = true;
			FileWriter.Encode(ykg, name, fs, FormatPreference.DefaultGraphics);
		}

		public static void UnpackGfx() {
			const string path = "data02.ykc"; //@"C:\Temp\CopyTest\data02.ykc";

			var srcFs = FileSystem.FromArchive(path);
			var dstFs = FileSystem.NewFolder(path.WithoutExtension());

			var sw = new Stopwatch();
			sw.Start();
			int fileCount = 0;

			foreach(string file in srcFs.GetFiles("*.ykg")) {
				//Console.WriteLine("Converting " + file);
				fileCount++;

				// read ykg
				var ykg = FileReader.Decode<YukaGraphic>(file, srcFs);

				// write png
				if(ykg != null) FileWriter.Encode(ykg, file, dstFs, FormatPreference.DefaultGraphics);
				else Console.WriteLine("Skipping file " + file);
			}

			dstFs.Dispose();
			srcFs.Dispose();

			sw.Stop();
			Console.WriteLine($"Converted {fileCount} files in {sw.ElapsedMilliseconds / 1000f:0.##} seconds");
			Console.ReadKey();
		}

		public static void ByteStream() {

			var bytes = new byte[1];
			using(var ms = new MemoryStream(bytes)) {
				ms.Seek(0).WriteByte(1);
			}

			Console.WriteLine(bytes[0]);
			Console.ReadKey();
		}

		public static void JsonFormat() {
			var ani = new Animation {
				Frames = new List<Animation.Frame> {
					new Animation.Frame(),
					new Animation.Frame(),
					new Animation.Frame(),
					new Animation.Frame()
				}
			};
			var sb = new StringBuilder();
			new JsonSerializer { Formatting = Formatting.Indented }.Serialize(new JsonTextWriter(new StringWriter(sb)), ani);
			Console.WriteLine(sb);
			Console.ReadKey();
		}

		public static void YkgRead() {
			var path = "data02/system.ykg";

			using(var s = new FileStream(path, FileMode.Open)) {
				var reader = new YkgGraphicReader();
				var ykg = reader.Read(path, s);
				ykg.Decode();
				ykg.ColorBitmap.Save("system.png");
			}
		}

		public static void AlphaCopy() {
			var src = new Bitmap("src.png");
			var dst = new Bitmap("dst.png");

			BitmapUtils.CopyAlphaChannel(src, dst);

			dst.Save("out1.png");
		}

		public static void Unpack() {
			string arcPath = @"C:\Temp\CopyTest\data02.ykc"; //@"S:\Games\Visual Novels\Semiramis no Tenbin\data04.ykc";
			string dirPath = Path.ChangeExtension(arcPath, "");

			var arc = FileSystem.FromArchive(arcPath);
			var dir = FileSystem.NewFolder(dirPath);

			foreach(string file in arc.GetFiles()) {
				Console.WriteLine($"Extracting {file}");
				using(Stream src = arc.OpenFile(file), dst = dir.CreateFile(file)) {
					src.CopyTo(dst);
				}
			}

			dir.Dispose();
			arc.Dispose();

			Console.ReadKey();
		}

		public static void Pack() {
			string dirPath = "data02";
			string arcPath = Path.ChangeExtension(dirPath, "ykc");

			var dir = FileSystem.FromFolder(dirPath);
			var arc = FileSystem.NewArchive(arcPath, ArchiveSaveMode.Explicit);

			foreach(string file in dir.GetFiles()) {
				Console.WriteLine($"Packing {file}");
				using(Stream src = dir.OpenFile(file), dst = arc.CreateFile(file)) {
					src.CopyTo(dst);
				}
			}

			arc.Flush();
			arc.Dispose();
			dir.Dispose();

			Console.ReadKey();
		}

		public static void FileSystemRead() {
			var dir = FileSystem.FromFolder(@"S:\Games\Visual Novels\Lover Able\data01-bin");
			var arc = FileSystem.FromArchive(@"S:\Games\Visual Novels\Lover Able\data01.ykc");

			using(Stream f1 = dir.OpenFile("start.yks"), f2 = arc.OpenFile("start.yks")) {
				var buffer1 = new byte[1024];
				var buffer2 = new byte[1024];

				f1.Read(buffer1, 0, buffer1.Length);
				f2.Read(buffer2, 0, buffer2.Length);

				Console.WriteLine(buffer1.Matches(buffer2) ? "Files match" : "Files don't match");
			}

			arc.Dispose();
			dir.Dispose();
			Console.ReadKey();
		}

		public static void ArchiveReadWrite() {
			using(var arc = Archive.Load("data01.ykc")) {
				using(var fs = arc.OpenFile("default.ini", true)) {
					fs.CopyTo(Console.OpenStandardOutput());

					fs.SetLength(0);
					fs.Seek(0);
					fs.Write(Encoding.ASCII.GetBytes("Test :D\n"), 0, 8);
					fs.Flush();
				}

				arc.Flush(); // flush changes to disk and re-open archive (all streams will be closed)

				using(var fs = arc.OpenFile("default.ini")) {
					fs.CopyTo(Console.OpenStandardOutput());
				}
			}

			File.Delete("test.ykc");
			using(var arc = Archive.Create("test.ykc")) {
				using(var fs = arc.CreateFile("file1.txt")) {
					var data = Encoding.ASCII.GetBytes("file 1 content\n");
					fs.Write(data, 0, data.Length);
				}

				using(var fs = arc.CreateFile("file2.txt")) {
					var data = Encoding.ASCII.GetBytes("file 2 content\n");
					fs.Write(data, 0, data.Length);
				}

				arc.Flush();
			}

			Console.ReadKey();
		}
	}

#endif
}