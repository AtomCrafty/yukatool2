using System;
using System.IO;

namespace Yuka.Util {
	public class ReadOnlySubStream : Stream {

		private readonly Stream _baseStream;
		private readonly long _offset;

		public override bool CanRead => true;
		public override bool CanSeek => _baseStream.CanSeek;
		public override bool CanWrite => false;
		public override long Length { get; }
		public override long Position { get; set; }

		public ReadOnlySubStream(Stream baseStream, long offset, long length) {
			_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			if(!baseStream.CanRead) throw new ArgumentException("Base stream must be readable", nameof(baseStream));
			if(!baseStream.CanSeek) throw new ArgumentException("Base stream must be seekable", nameof(baseStream));
			if(offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Must not be negative");
			if(length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "Must not be negative");

			_offset = offset;
			Length = length;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			switch(origin) {
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position += offset;
					break;
				case SeekOrigin.End:
					Position = Length - offset;
					break;
				default: throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
			}
			return Position;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			try {
				// seek to my position in base stream
				_baseStream.Position = Position + _offset;

				// make sure we stay in bounds
				count = count.Clamp((int)(Length - Position));
				return _baseStream.Read(buffer, offset, count);
			}
			finally {
				// set my position to the new value
				Position = _baseStream.Position - _offset;
			}
		}

		public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

		public override void SetLength(long value) => throw new NotImplementedException();

		public override void Flush() => throw new NotImplementedException();
	}
}