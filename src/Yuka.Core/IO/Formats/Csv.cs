namespace Yuka.IO.Formats {

	public class CsvFormat : Format {
		public override string Extension => ".csv";
		public override string Description => "Comma separated values; String table for a decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;
	}
}