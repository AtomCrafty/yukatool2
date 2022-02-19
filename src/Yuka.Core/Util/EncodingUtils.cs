using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Yuka.Util {
	public class EncodingUtils {
		private static readonly Encoding ShiftJisEncoding = Encoding.GetEncoding("Shift-JIS");
		private static readonly ShiftJisTunnelEncoding ShiftJisTunnelEncoding = new ShiftJisTunnelEncoding();
		private static string _shiftJisTunnelFilePath;

		public static Encoding ShiftJis => _shiftJisTunnelFilePath != null ? ShiftJisTunnelEncoding : ShiftJisEncoding;

		public static void SetShiftJisTunnelFile(string path) {
			_shiftJisTunnelFilePath = path;

			if(path == null) {
				return;
			}

			if(!File.Exists(path)) {
				string directory = Path.GetDirectoryName(path);
				if(!string.IsNullOrWhiteSpace(directory))
					Directory.CreateDirectory(directory);
				File.Create(path).Close();
			}

			ShiftJisTunnelEncoding.CharTable = File.ReadAllText(path);
		}

		public static void WriteShiftJisTunnelFile() {
			if(_shiftJisTunnelFilePath == null)
				return;

			if(!File.Exists(_shiftJisTunnelFilePath)) {
				string directory = Path.GetDirectoryName(_shiftJisTunnelFilePath);
				if(!string.IsNullOrWhiteSpace(directory))
					Directory.CreateDirectory(directory);
			}

			File.WriteAllText(_shiftJisTunnelFilePath, ShiftJisTunnelEncoding.CharTable);
		}
	}

	// Shift-JIS tunnel encoder courtesy of https://github.com/arcusmaximus
	public class ShiftJisTunnelEncoding : Encoding {
		private static readonly Encoding ShiftJisEncoding = GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

		private readonly char[] _charArray = new char[1];
		private readonly Dictionary<char, char> _mappings = new Dictionary<char, char>();
		private readonly List<char> _charTable = new List<char>();

		public string CharTable {
			get => new string(_charTable.ToArray());
			set {
				_charTable.Clear();
				_mappings.Clear();
				foreach(char c in value) {
					GetTunnelChar(c);
				}
			}
		}

		public override int GetByteCount(string str) {
			int byteCount = 0;
			foreach(char ch in str) {
				bool tunneled = _mappings.ContainsKey(ch);

				if(!tunneled) {
					try {
						_charArray[0] = ch;
						byteCount += ShiftJisEncoding.GetByteCount(_charArray);
					}
					catch {
						tunneled = true;
					}
				}

				if(tunneled)
					byteCount += 2;
			}
			return byteCount;
		}

		public override int GetByteCount(char[] chars, int index, int count) {
			return GetByteCount(new string(chars, index, count));
		}

		public override int GetMaxByteCount(int charCount) {
			return charCount * 2;
		}

		public override byte[] GetBytes(string str) {
			byte[] bytes = new byte[GetByteCount(str)];
			GetBytes(str, 0, str.Length, bytes, 0);
			return bytes;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
			return GetBytes(new string(chars, charIndex, charCount), 0, charCount, bytes, byteIndex);
		}

		public override int GetBytes(string str, int startCharIdx, int charCount, byte[] bytes, int startByteIdx) {
			int byteIdx = startByteIdx;
			for(int charIdx = startCharIdx; charIdx < startCharIdx + charCount; charIdx++) {
				bool tunneled = _mappings.TryGetValue(str[charIdx], out char tunnelChar);

				if(!tunneled) {
					tunneled = !TryEncode(str, charIdx, 1, bytes, byteIdx, out int numBytes);
					if(tunneled)
						tunnelChar = GetTunnelChar(str[charIdx]);
					else
						byteIdx += numBytes;
				}

				if(tunneled) {
					bytes[byteIdx++] = (byte)(tunnelChar >> 8);
					bytes[byteIdx++] = (byte)tunnelChar;
				}
			}
			return byteIdx;
		}

		private static bool TryEncode(string str, int charIdx, int numChars, byte[] bytes, int byteIdx, out int numBytes) {
			numBytes = 0;

			try {
				numBytes = ShiftJisEncoding.GetBytes(str, charIdx, numChars, bytes, byteIdx);
				if(bytes[byteIdx] >= 0xF0) {
					numBytes = 0;
					return false;
				}

				return true;
			}
			catch {
				return false;
			}
		}

		public override int GetCharCount(byte[] bytes, int index, int count) {
			return GetCharsInternal(bytes, index, count, null, 0);
		}

		public override int GetMaxCharCount(int byteCount) {
			return byteCount;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
			return GetCharsInternal(bytes, byteIndex, byteCount, chars, charIndex);
		}

		private int GetCharsInternal(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
			int origCharIndex = charIndex;

			int byteIdx = byteIndex;
			while(byteCount < 0 ? bytes[byteIdx] != 0 : byteIdx < byteIndex + byteCount) {
				if(chars != null && charIndex >= chars.Length)
					throw new ArgumentException("Not enough space to fit all decoded characters");

				byte highByte = bytes[byteIdx++];
				byte lowByte = 0;

				if((highByte >= 0x81 && highByte < 0xA0) || (highByte >= 0xE0 && highByte < 0xFD)) {
					int highIdx = highByte < 0xA0 ? highByte - 0x81 : 0x1F + (highByte - 0xE0);

					lowByte = bytes[byteIdx++];
					if(lowByte == 0)
						break;

					if(lowByte < 0x40) {
						int lowIdx = lowByte;
						if(lowIdx > ',')
							lowIdx--;
						if(lowIdx > ' ')
							lowIdx--;
						if(lowIdx > '\r')
							lowIdx--;
						if(lowIdx > '\n')
							lowIdx--;
						if(lowIdx > '\t')
							lowIdx--;

						lowIdx--;

						int index = highIdx * 0x3A + lowIdx;

						if(index < _charTable.Count) {
							if(chars != null)
								chars[charIndex] = _charTable[index];
							charIndex++;
						}
						else {
							throw new Exception("No mapping defined for tunnel index " + index);
						}

						continue;
					}
				}

				int charLength = lowByte == 0 ? 1 : 2;
				ShiftJisEncoding.GetChars(bytes, byteIdx - charLength, charLength, _charArray, 0);
				
				if(chars != null)
					chars[charIndex] = _charArray[0];
				charIndex++;
			}

			return charIndex - origCharIndex;
		}

		private char GetTunnelChar(char origChar) {
			if(char.IsHighSurrogate(origChar) || char.IsLowSurrogate(origChar))
				throw new NotSupportedException("Surrogate chars not supported");

			char sjisChar;
			if(_mappings.TryGetValue(origChar, out sjisChar))
				return sjisChar;

			int sjisIdx = _mappings.Count;
			if(sjisIdx >= 0x3B * 0x3A)
				throw new Exception("SJIS tunnel limit exceeded");

			int highSjisIdx = Math.DivRem(sjisIdx, 0x3A, out int lowSjisIdx);
			int highByte = highSjisIdx < 0x1F ? 0x81 + highSjisIdx : 0xE0 + (highSjisIdx - 0x1F);
			int lowByte = 1 + lowSjisIdx;
			if(lowByte >= '\t')
				lowByte++;
			if(lowByte >= '\n')
				lowByte++;
			if(lowByte >= '\r')
				lowByte++;
			if(lowByte >= ' ')
				lowByte++;
			if(lowByte >= ',')
				lowByte++;

			sjisChar = (char)((highByte << 8) | lowByte);
			_mappings[origChar] = sjisChar;
			_charTable.Add(origChar);
			return sjisChar;
		}
	}
}
