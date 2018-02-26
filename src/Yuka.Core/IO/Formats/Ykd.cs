using System.IO;
using Yuka.Script;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkdFormat : Format {
		public override string Extension => ".ykd";
		public override string Description => "Decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;
	}
}