namespace Yuka.IO.Formats {

	public class YkiFormat : Format {
		public override string Extension => ".yki";
		public override string Description => "Intermediate Yuka script instruction list";
		public override FormatType Type => FormatType.Unpacked;
	}

}