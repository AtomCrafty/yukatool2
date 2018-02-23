namespace Yuka.IO.Formats {

	public class YkdFormat : Format {
		public override string Extension => ".ykd";
		public override string Description => "Decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;
	}
}