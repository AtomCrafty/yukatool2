using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.IO {

	[JsonConverter(typeof(FormatConverter))]
	public abstract class Format {

		public abstract string Id { get; }
		public abstract string Extension { get; }
		public abstract string Description { get; }
		public abstract FormatType Type { get; }
		public virtual string Name => GetType().Name.Replace("Format", "");

		public virtual FileCategory GetFileCategory(FileSystem fs, string fileName) => FileCategory.Primary;
		public virtual IEnumerable<string> GetSecondaryFiles(FileSystem fs, string fileName) => Array.Empty<string>();

		#region Format instances

		// graphics
		public static readonly PngFormat Png = new PngFormat();
		public static readonly GnpFormat Gnp = new GnpFormat();
		public static readonly BmpFormat Bmp = new BmpFormat();
		public static readonly YkgFormat Ykg = new YkgFormat();
		// animation
		public static readonly AniFormat Ani = new AniFormat();
		public static readonly FrmFormat Frm = new FrmFormat();
		// script
		public static readonly YksFormat Yks = new YksFormat();
		public static readonly YkiFormat Yki = new YkiFormat();
		public static readonly YkdFormat Ykd = new YkdFormat();
		public static readonly CsvFormat Csv = new CsvFormat();
		// other
		public static readonly YkcFormat Ykc = new YkcFormat();
		public static readonly TxtFormat Txt = new TxtFormat();
		public static readonly RawFormat Raw = new RawFormat();

		public static readonly List<Format> RegisteredFormats = new List<Format> {
			Png, Gnp, Bmp, Ykg,
			Ani, Frm,
			Yks, Yki, Ykd, Csv,
			Ykc, Txt, Raw
		};

		#endregion

		public static Format[] GraphicsFormats = { Png, Bmp, Gnp, Ykg };
		public static Format[] AnimationFormats = { Ani, Frm };

		public static Format ById(string id) {
			return RegisteredFormats.FirstOrDefault(f => f.Id == id);
		}

		public static Format ForFile(FileSystem fs, string fileName) {
			using(var s = fs.OpenFile(fileName)) {
				var readers = FileReader.FindReaders(fileName, s.NewReader());
				return readers.FirstOrDefault()?.Format;
			}
		}

		public static Format GuessFromFileName(string name) {
			string extension = Path.GetExtension(name)?.ToLower();
			foreach(var format in RegisteredFormats) {
				if(format.Extension == extension) return format;
			}
			return extension.IsOneOf(Txt.TextExtensions) ? (Format)Txt : Raw;
		}
	}

	public class FormatPreference : IComparer<FileWriter>, IComparer<Format> {

		public Format[] AllowedFormats;
		public Format PreferredFormat;
		public FormatType PreferredType;

		private FormatPreference() { }

		public FormatPreference(Format allowedFormat) {
			AllowedFormats = new[] { allowedFormat };
			PreferredFormat = allowedFormat;
			PreferredType = allowedFormat.Type;
		}

		public FormatPreference(Format preferredFormat, FormatType preferredType, params Format[] allowedFormats) {
			AllowedFormats = allowedFormats.NullIfEmpty();
			PreferredFormat = preferredFormat;
			PreferredType = preferredType;
		}

		public int Compare(Format a, Format b) {

			Debug.Assert(a != null, nameof(a) + " != null");
			Debug.Assert(b != null, nameof(b) + " != null");

			int aScore = 0;
			int bScore = 0;

			if(a == PreferredFormat) aScore += 100;
			if(b == PreferredFormat) bScore += 100;

			if(PreferredType != FormatType.None) {
				if(a.Type == PreferredType) aScore += 10;
				if(b.Type == PreferredType) bScore += 10;
			}

			return bScore.CompareTo(aScore);
		}

		public int Compare(FileWriter a, FileWriter b) {
			return Compare(a?.Format, b?.Format);
		}

		public bool Allows(Format f) {
			return AllowedFormats == null || AllowedFormats.Contains(f);
		}

		public bool Allows(FileWriter w) {
			return Allows(w.Format);
		}

		public static FormatPreference Default => new FormatPreference { AllowedFormats = null, PreferredFormat = null, PreferredType = FormatType.None };
		public static FormatPreference Packed => new FormatPreference { AllowedFormats = null, PreferredFormat = null, PreferredType = FormatType.Packed };
		public static FormatPreference Unpacked => new FormatPreference { AllowedFormats = null, PreferredFormat = null, PreferredType = FormatType.Unpacked };
		public static FormatPreference DefaultGraphics => new FormatPreference { AllowedFormats = Format.GraphicsFormats, PreferredFormat = Format.Png };
		public static FormatPreference DefaultAnimation => new FormatPreference { AllowedFormats = Format.AnimationFormats, PreferredFormat = Format.Ani };
	}

	public enum FormatType {
		None, Packed, Unpacked
	}

	public enum FileCategory {
		Primary, Secondary, Ignore
	}

	public sealed class FormatConverter : JsonConverter<Format> {

		public override void WriteJson(JsonWriter writer, Format value, JsonSerializer serializer) {
			var token = JToken.FromObject(value.Id);
			Debug.Assert(token.Type == JTokenType.String);
			token.WriteTo(writer);
		}

		public override Format ReadJson(JsonReader reader, Type objectType, Format existingValue, bool hasExistingValue, JsonSerializer serializer) {
			return Format.ById(reader.Value as string);
		}
	}
}
