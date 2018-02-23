using System.IO;
using Yuka.Util;

namespace Yuka.IO.Formats {

	public class RawFormat : Format {
		public override string Extension => null;
		public override string Description => "Arbitrary binary data";
		public override FormatType Type => FormatType.None;
	}

	public class RawFileReader : FileReader<byte[]> {

		public override Format Format => Format.Raw;

		public override bool CanRead(string name, BinaryReader r) {
			return true;
		}

		public override byte[] Read(string name, Stream s) {
			return s.NewReader().ReadToEnd();
		}
	}

	public class RawFileWriter : FileWriter<byte[]> {

		public override Format Format => Format.Raw;

		public override bool CanWrite(object obj) {
			return obj is byte[];
		}

		public override void Write(byte[] bytes, Stream s) {
			s.WriteBytes(bytes);
		}
	}
}
