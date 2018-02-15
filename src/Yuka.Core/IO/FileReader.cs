using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.IO {
	public abstract class FileReader {
		public abstract Format Format { get; }
		public abstract object ReadObject(string name, Stream s);
		public abstract object ReadObject(string name, FileSystem fs);

		// CanWrite must reset the stream position to where it was!
		public abstract bool CanRead(string name, BinaryReader r);

		#region Static methods

		private static readonly List<FileReader> Readers = new List<FileReader> {
			new AniAnimationReader(),
			new FrmAnimationReader(),
			new PngGraphicReader(),
			new GnpGraphicReader(),
			new BmpGraphicReader(),
			new YkgGraphicReader(),
			new GnpBitmapReader(),
			new PngBitmapReader(),
			new YksScriptReader()
		};

		public static IEnumerable<FileReader> FindReaders(string name, BinaryReader r, Predicate<FileReader> predicate = null) {
			return Readers.Where(reader => (predicate?.Invoke(reader) ?? true) && reader.CanRead(name, r));
		}

		public static List<FileReader<T>> FindReaders<T>(string name, Stream s) where T : class {
			return FindReaders(name, s.NewReader(), reader => reader is FileReader<T>).Cast<FileReader<T>>().ToList();
		}

		public static T Decode<T>(string name, FileSystem fs) where T : class {
			List<FileReader<T>> readers;
			using(var s = fs.OpenFile(name)) {
				readers = FindReaders<T>(name, s);
			}

			if(readers.Any()) {
				return readers.First().Read(name, fs);
			}

			// TODO Warning
			Console.WriteLine("No suitable reader found for type " + typeof(T).Name);
			return null;
		}

		public static T Decode<T>(string name, Stream s) where T : class {
			var readers = FindReaders<T>(name, s);

			if(readers.Any()) {
				return readers.First().Read(name, s);
			}

			// TODO Warning
			Console.WriteLine("No suitable reader found for type " + typeof(T).Name);
			return null;
		}

		public static T Decode<T>(string name, byte[] data) where T : class {
			using(var ms = new MemoryStream(data)) {
				return Decode<T>(name, ms);
			}
		}

		#endregion
	}

	public abstract class FileReader<T> : FileReader where T : class {

		public abstract T Read(string name, Stream s);
		public virtual T Read(string name, FileSystem fs) {
			using(var s = fs.OpenFile(name)) {
				return Read(name, s);
			}
		}

		public override object ReadObject(string name, Stream s) => Read(name, s);
		public override object ReadObject(string name, FileSystem fs) => Read(name, fs);
	}
}


/*
 * FileReader<T> : FileReader
 *   RawFileReader : FileReader<byte[]>
 *   GraphicReader : FileReader<Graphic>
 *     YkgGraphicReader : GraphicReader
 *     Reader : GraphicReader
 *     BmpReader : GraphicReader
 *   ScriptReader : FileReader<YukaScript>
 *     YksReader : GraphicReader
 *     YkdReader : GraphicReader
 */
