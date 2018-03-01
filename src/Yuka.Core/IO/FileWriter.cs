using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.IO {
	public abstract class FileWriter {

		public abstract Format Format { get; }
		public abstract bool CanWrite(object obj);

		// Write to stream or write to file
		public abstract void WriteObject(object obj, Stream s);
		public abstract void WriteObject(object obj, string name, FileSystem fs);

		#region Static methods

		private static readonly List<FileWriter> Writers = new List<FileWriter> {
			new AniAnimationWriter(),
			new FrmAnimationWriter(),
			new PngGraphicWriter(),
			new BmpGraphicWriter(),
			new YkgGraphicWriter(),
			new GnpGraphicWriter(),
			new PngBitmapWriter(),
			new GnpBitmapWriter(),
			new YkiScriptWriter(),
			new YksScriptWriter(),
			new CsvStringTableWriter(),
			new YkdScriptWriter()
		};

		public static IEnumerable<FileWriter> FindWriters(object obj, Predicate<FileWriter> filter = null) {
			return Writers.Where(reader => (filter?.Invoke(reader) ?? true) && reader.CanWrite(obj));
		}

		public static List<FileWriter<T>> FindWriters<T>(object obj, FormatPreference pref) where T : class {
			var writers = FindWriters(obj, fw => fw is FileWriter<T> && (pref ?? FormatPreference.Default).Allows(fw)).Cast<FileWriter<T>>().ToList();
			writers.Sort(pref ?? FormatPreference.Default);
			return writers;
		}

		public static void Encode<T>(T obj, Stream s, FormatPreference pref) where T : class {
			pref = pref ?? FormatPreference.Default;
			var writers = FindWriters<T>(obj, pref).ToList();
			if(!writers.Any()) throw new InvalidOperationException("No writer found for type " + typeof(T).Name);

			writers.First().Write(obj, s);
		}

		public static void Encode<T>(T obj, string baseName, FileSystem fs, FormatPreference pref) where T : class {
			pref = pref ?? FormatPreference.Default;
			var writers = FindWriters<T>(obj, pref).ToList();
			if(!writers.Any()) throw new InvalidOperationException("No writer found for type " + typeof(T).Name);

			writers.First().Write(obj, baseName, fs);
		}

		#endregion
	}

	public abstract class FileWriter<T> : FileWriter {
		public override void WriteObject(object obj, Stream s) => Write((T)obj, s);
		public override void WriteObject(object obj, string name, FileSystem fs) => Write((T)obj, name, fs);

		// The stream is expected to point to the end of the written data when this method returs.
		public abstract void Write(T obj, Stream s);
		public virtual void Write(T obj, string baseName, FileSystem fs) {
			using(var s = fs.CreateFile(baseName.WithExtension(Format.Extension))) {
				Write(obj, s);
			}
		}
	}
}
