using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yuka.IO.Formats;
using Yuka.Util;

namespace Yuka.IO {
	public abstract class Format {

		public abstract string Extension { get; }
		public abstract string Description { get; }
		public abstract FormatType Type { get; }

		public virtual FileCategory GetFileType(FileSystem fs, string fileName) => FileCategory.Primary;
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
		public static readonly RawFormat Raw = new RawFormat();

		#endregion

		public static Format[] GraphicsFormats = { Png, Bmp, Gnp, Ykg };
		public static Format[] AnimationFormats = { Ani, Frm };

		public static Format ForFile(FileSystem fs, string fileName) {
			using(var s = fs.OpenFile(fileName)) {
				var readers = FileReader.FindReaders(fileName, s.NewReader());
				return readers.FirstOrDefault()?.Format;
			}
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
		public static FormatPreference DefaultAnimation => new FormatPreference { AllowedFormats = Format.AnimationFormats, PreferredFormat = Format.Frm };
	}

	public enum FormatType {
		None, Packed, Unpacked
	}

	public enum FileCategory {
		Primary, Secondary, Ignore
	}
}
