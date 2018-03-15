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
			new PngBitmapReader(),
			new GnpBitmapReader(),
			new BmpBitmapReader(),
			new YksScriptReader(),
			new YkdScriptReader(),
			new CsvStringTableReader(),
			new YkiScriptReader(),
			new RawFileReader()
		};

		/// <summary>
		/// Returns all registered <code>FileReader</code>s that can read the file and satisfy the filter.
		/// </summary>
		/// <param name="name">Name for the <code>FileReader.CanRead</code> check</param>
		/// <param name="r"><code>BinaryReader</code> for the <code>FileReader.CanRead</code> check</param>
		/// <param name="filter">A filter to select only some writers</param>
		/// <returns>All registered <code>FileReader</code>s that can read the file and satisfy the filter</returns>
		public static IEnumerable<FileReader> FindReaders(string name, BinaryReader r, Predicate<FileReader> filter = null) {
			return Readers.Where(reader => (filter?.Invoke(reader) ?? true) && reader.CanRead(name, r));
		}

		/// <summary>
		/// Returns all registered <code>FileReader</code>s that can read the file and satisfy the filter.
		/// </summary>
		/// <param name="name">Name for the <code>FileReader.CanRead</code> check</param>
		/// <param name="s"><code>Stream</code> for the <code>FileReader.CanRead</code> check</param>
		/// <returns>All registered <code>FileReader</code>s that can read the file and satisfy the filter</returns>
		public static List<FileReader<T>> FindReaders<T>(string name, Stream s) where T : class {
			return FindReaders(name, s.NewReader(), reader => reader is FileReader<T>).Cast<FileReader<T>>().ToList();
		}

		public static T Decode<T>(string name, FileSystem fs) where T : class {
			List<FileReader<T>> readers;
			using(var s = fs.OpenFile(name)) {
				readers = FindReaders<T>(name, s);
			}
			if(!readers.Any()) throw new InvalidOperationException("No reader found for type " + typeof(T));

			return readers.First().Read(name, fs);
		}

		public static T Decode<T>(string name, Stream s) where T : class {
			var readers = FindReaders<T>(name, s);
			if(!readers.Any()) throw new InvalidOperationException("No reader found for type " + typeof(T));

			return readers.First().Read(name, s);
		}

		public static T Decode<T>(string name, byte[] data) where T : class {
			using(var ms = new MemoryStream(data)) {
				return Decode<T>(name, ms);
			}
		}

		public static (object, Format) DecodeObject(string name, FileSystem fs, bool ignoreSecondary = false) {
			List<FileReader> readers;
			using(var s = fs.OpenFile(name)) {
				readers = FindReaders(name, s.NewReader()).ToList();
			}
			if(!readers.Any()) throw new InvalidOperationException("No reader found for object");

			var fileReader = readers.First();
			var fileFormat = fileReader.Format;
			var fileCategory = fileFormat.GetFileCategory(fs, name);

			// skip auxiliary files (csv, frm, ani, etc...)
			if(fileCategory == FileCategory.Ignore || ignoreSecondary && fileCategory == FileCategory.Secondary) {
				return (null, fileFormat);
			}

			return (fileReader.ReadObject(name, fs), fileFormat);
		}

		public static (object, Format) DecodeObject(string name, Stream s) {
			var readers = FindReaders(name, s.NewReader()).ToList();
			if(!readers.Any()) throw new InvalidOperationException("No reader found for object");

			var fileReader = readers.First();
			return (fileReader.ReadObject(name, s), fileReader.Format);
		}

		public static (object, Format) DecodeObject(string name, byte[] data) {
			using(var ms = new MemoryStream(data)) {
				return DecodeObject(name, ms);
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
