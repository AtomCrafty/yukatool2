using System.IO;
using System.Linq;

namespace Yuka.Util {
	public class XorStream : Stream {

		public readonly Stream BaseStream;
		private readonly byte _key;

		public override bool CanRead => BaseStream.CanRead;
		public override bool CanSeek => BaseStream.CanSeek;
		public override bool CanWrite => BaseStream.CanWrite;
		public override long Length => BaseStream.Length;
		public override long Position {
			get => BaseStream.Position;
			set => BaseStream.Position = value;
		}

		public XorStream(Stream baseStream, byte key) {
			BaseStream = baseStream;
			_key = key;
		}

		public override void Flush() => BaseStream.Flush();
		public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
		public override void SetLength(long value) => BaseStream.SetLength(value);

		public override int Read(byte[] buffer, int offset, int count) {
			int result = BaseStream.Read(buffer, offset, count);

			for(int i = offset; i < offset + count; i++) {
				buffer[i] ^= _key;
			}

			return result;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			// clone array, so we don't modify the original
			buffer = buffer.ToArray();

			for(int i = offset; i < offset + count; i++) {
				buffer[i] ^= _key;
			}

			BaseStream.Write(buffer, offset, count);
		}
	}
}