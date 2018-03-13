using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yuka.Script;
using Yuka.Script.Data;
using Yuka.Script.Source;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class YkcFormat : Format {
		public override string Extension => ".ykc";
		public override string Description => "Yuka container";
		public override FormatType Type => FormatType.None;
	}
}