using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Yuka.Container;
using Yuka.Graphics;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.Cli {
	public class Program {
		public static void Main() {
			Tests.UnpackGfx();
		}
	}

	public static class Tests {

		public static void UnpackGfx() {
			const string path = @"C:\Temp\CopyTest\data02.ykc";

			var srcFs = FileSystem.FromArchive(path);
			var dstFs = FileSystem.NewFolder(path.WithoutExtension());

			foreach(string file in srcFs.GetFiles("*.ykg")) {
				Console.WriteLine("Converting " + file);

				using(var srcStream = srcFs.OpenFile(file)) {
					// read ykg
					var ykg = FileReader.Decode<Graphic>(file, srcFs);
					// write png
					if(ykg != null) FileWriter.Encode(ykg, file, dstFs, FormatPreference.DefaultGraphics);
					else Console.WriteLine("Skipping file " + file);
				}
			}

			dstFs.Dispose();
			srcFs.Dispose();
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
			var fs = FileSystem.FromFolder(Path.GetDirectoryName(path));

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
			string arcPath = @"S:\Games\Visual Novels\Semiramis no Tenbin\data04.ykc";
			string dirPath = Path.ChangeExtension(Path.GetFileName(arcPath), "");

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
}