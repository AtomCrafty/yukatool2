using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuka.IO.Formats {
	public class TxtFormat : Format {
		public override string Extension => null;
		public override string Description => "Unrecognized text data";
		public override FormatType Type => FormatType.None;
	}
}
