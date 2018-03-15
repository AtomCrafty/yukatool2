namespace Yuka.IO.Formats {

	public class YkcFormat : Format {
		public override string Extension => ".ykc";
		public override string Description => "Yuka container";
		public override FormatType Type => FormatType.None;
	}
}