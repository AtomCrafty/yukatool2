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
			new YkdScriptWriter(),
			new RawFileWriter()
		};

		/// <summary>
		/// Returns all registered <code>FileWriter</code>s that can write the object and satisfy the filter.
		/// </summary>
		/// <param name="obj">Object for the <code>FileReader.CanRead</code> check</param>
		/// <param name="filter">A filter to select only some writers</param>
		/// <returns>All registered <code>FileWriter</code>s that can write the object and satisfy the filter</returns>
		public static IEnumerable<FileWriter> FindWriters(object obj, Predicate<FileWriter> filter = null) {
			return Writers.Where(reader => (filter?.Invoke(reader) ?? true) && reader.CanWrite(obj));
		}

		/// <summary>
		/// Returns all registered <code>FileWriter</code>s that can write the object and satisfy both the filter and format preference.
		/// Writers are sorted by descending relevance as determined by the <code>FormatPreference</code>.
		/// </summary>
		/// <param name="obj">Object for the <code>FileReader.CanRead</code> check</param>
		/// <param name="pref">A format preference used to filter and rank writers</param>
		/// <param name="filter">A filter to select only some writers</param>
		/// <returns>All registered <code>FileWriter</code>s that can write the object and satisfy both the filter and format preference</returns>
		public static List<FileWriter> FindWriters(object obj, FormatPreference pref, Predicate<FileWriter> filter = null) {
			var writers = FindWriters(obj, fw => (filter?.Invoke(fw) ?? true) && (pref ?? FormatPreference.Default).Allows(fw)).ToList();
			writers.Sort(pref ?? FormatPreference.Default);
			return writers;
		}

		/// <summary>
		/// Returns all registered <code>FileWriter&lt;T&gt;</code>s with that can write the object and satisfy the format preference.
		/// </summary>
		/// <param name="obj">Object for the <code>FileReader.CanRead</code> check</param>
		/// <param name="pref">A format preference used to filter and rank writers</param>
		/// <returns>All registered <code>FileWriter</code>s that can write the object and satisfy the format preference</returns>
		public static IEnumerable<FileWriter<T>> FindWriters<T>(object obj, FormatPreference pref) where T : class {
			return FindWriters(obj, pref, fw => fw is FileWriter<T>).Cast<FileWriter<T>>();
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

		public static void EncodeObject(object obj, Stream s, FormatPreference pref) {
			pref = pref ?? FormatPreference.Default;
			var writers = FindWriters(obj, pref);
			if(!writers.Any()) throw new InvalidOperationException("No writer found for object");

			writers.First().WriteObject(obj, s);
		}

		public static void EncodeObject(object obj, string baseName, FileSystem fs, FormatPreference pref) {
			pref = pref ?? FormatPreference.Default;
			var writers = FindWriters(obj, pref);
			if(!writers.Any()) throw new InvalidOperationException("No writer found for object");

			writers.First().WriteObject(obj, baseName, fs);
		}
		#endregion
	}

	public abstract class FileWriter<T> : FileWriter {
		public override void WriteObject(object obj, Stream s) => Write((T)obj, s);
		public override void WriteObject(object obj, string name, FileSystem fs) => Write((T)obj, name, fs);

		// The stream is expected to point to the end of the written data when this method returns.
		public abstract void Write(T obj, Stream s);
		public virtual void Write(T obj, string baseName, FileSystem fs) {
			using(var s = fs.CreateFile(baseName.WithExtension(Format.Extension))) {
				Write(obj, s);
				s.Flush();
			}
		}
	}
}
