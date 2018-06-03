using System.IO;
using Yuka.Util;

namespace Yuka.IO.Formats {

	public class RawFormat : Format {
		public override string Id => "raw";
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

		// overwrite default behavior so the file extension isn't deleted
		public override void Write(byte[] bytes, string baseName, FileSystem fs, FileList files) {
			using(var stream = fs.CreateFile(baseName)) {
				files?.Add(baseName, Format.Raw);
				Write(bytes, stream);
			}
		}

		public override void Write(byte[] bytes, Stream s) {
			s.WriteBytes(bytes);
		}
	}
}
