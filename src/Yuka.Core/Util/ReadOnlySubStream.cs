using System;
using System.IO;

namespace Yuka.Util {
	public class ReadOnlySubStream : Stream {

		private readonly Stream _baseStream;
		private readonly long _start;
		private readonly long _end;

		public override bool CanRead => true;
		public override bool CanSeek => _baseStream.CanSeek;
		public override bool CanWrite => false;
		public override long Length { get; }
		public override long Position {
			get => _baseStream.Position - _start;
			set => _baseStream.Position = value + _start;
		}

		public ReadOnlySubStream(Stream baseStream, long start, long length) {
			_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			if(!baseStream.CanRead) throw new ArgumentException("Base stream must be readable", nameof(baseStream));
			if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), start, "Must not be negative");
			if(length < 0) throw new ArgumentOutOfRangeException(nameof(length), length, "Must not be negative");

			_start = start;
			_end = start + length;

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
			// make sure we stay in bounds
			count = count.Clamp((int)(Length - Position));

			return _baseStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

		public override void SetLength(long value) => throw new NotImplementedException();

		public override void Flush() => throw new NotImplementedException();
	}
}