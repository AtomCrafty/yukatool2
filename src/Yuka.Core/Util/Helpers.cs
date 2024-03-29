﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuka.Util {
	public static class Helpers {

		public static bool IsOneOf<T>(this T value, params T[] options) {
			return options.Contains(value);
		}

		public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue fallback = default(TValue)) {
			return dict.ContainsKey(key) ? dict[key] : fallback;
		}

		public static TValue Fetch<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> producer) {
			return dict.ContainsKey(key) ? dict[key] : producer();
		}

		public static bool Matches(this byte[] a, byte[] b) {
			if(a == b) return true;
			if(a == null || b == null) return false;
			if(a.Length != b.Length) return false;
			return !a.Where((t, i) => t != b[i]).Any();
		}

		public static bool StartsWith(this byte[] a, byte[] b) {
			if(a == b) return true;
			if(a == null || b == null) return false;
			if(a.Length < b.Length) return false;
			return !b.Where((t, i) => t != a[i]).Any();
		}

		public static BinaryReader Seek(this BinaryReader r, long offset) {
			r.BaseStream.Seek(offset, SeekOrigin.Begin);
			return r;
		}

		public static void CopyRangeTo(this Stream input, Stream output, long start, long bytes) {
			input.Seek(start);
			input.CopyBytesTo(output, bytes);
		}

		public static void CopyBytesTo(this Stream input, Stream output, long bytes) {
			var buffer = new byte[Math.Min(32768, (int)bytes)];
			int read;
			while(bytes > 0 &&
				  (read = input.Read(buffer, 0, Math.Min(buffer.Length, (int)bytes))) > 0) {
				output.Write(buffer, 0, read);
				bytes -= read;
			}
		}

		public static BinaryReader NewReader(this Stream s) {
			return new BinaryReader(s);
		}

		public static BinaryWriter NewWriter(this Stream s) {
			return new BinaryWriter(s);
		}

		public static string ReadNullTerminatedString(this BinaryReader r, Encoding encoding = null) {
			var bytes = new List<byte>();

			// performance optimization for debug builds
			long len = r.BaseStream.Length;
			long pos = r.BaseStream.Position;

			while(pos++ < len) {
				byte b = r.ReadByte();
				if(b == 0) break;
				bytes.Add(b);
			}

			return (encoding ?? EncodingUtils.ShiftJis).GetString(bytes.ToArray());
		}

		public static void WriteNullTerminatedString(this BinaryWriter w, string str, Encoding encoding = null) {
			w.Write((encoding ?? EncodingUtils.ShiftJis).GetBytes(str));
			w.Write((byte)0);
		}

		public static int GetNullTerminatedStringLength(string str, Encoding encoding = null) {
			return (encoding ?? EncodingUtils.ShiftJis).GetByteCount(str) + 1;
		}

		public static string ReadString(this BinaryReader r, uint length, Encoding encoding = null) {
			var bytes = r.ReadBytes((int)length);
			return (encoding ?? EncodingUtils.ShiftJis).GetString(bytes).TrimEnd('\0');
		}

		public static bool IsNullOrEmpty<T>(this T[] array) {
			return array == null || array.Length == 0;
		}

		public static T[] NullIfEmpty<T>(this T[] array) {
			return array.IsNullOrEmpty() ? null : array;
		}

		public static string WithExtension(this string path, string extension) {
			return Path.ChangeExtension(path, extension);
		}

		public static string WithoutExtension(this string path) {
			return Path.ChangeExtension(path, "")?.TrimEnd('.');
		}

		public static T Seek<T>(this T s, long offset) where T : Stream {
			s.Seek(offset, SeekOrigin.Begin);
			return s;
		}

		public static T WriteBytes<T>(this T s, byte[] bytes) where T : Stream {
			s.Write(bytes, 0, bytes.Length);
			return s;
		}

		public static byte[] ReadToEnd(this BinaryReader r) {
			return r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position));
		}

		public static BinaryReader Skip(this BinaryReader r, long offset) {
			r.BaseStream.Position += offset;
			return r;
		}

		public static BinaryWriter Skip(this BinaryWriter w, long offset) {
			w.BaseStream.Position += offset;
			return w;
		}

		public static T Clamp<T>(this T val, T max) where T : IComparable<T> {
			return val.CompareTo(default(T)) < 0 ? default(T) : val.CompareTo(max) > 0 ? max : val;
		}

		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
			return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
		}

		public static string EscapeIdentifier(this string value) {
			return string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
		}

		public static string Escape(this string value) {
			return value.Replace(@"\", @"\\").Replace(@"""", @"\""").Replace("\n", @"\n").Replace("\r", @"\r").Replace("\t", @"\t");
		}

		private static readonly Regex UnescapeRegex = new Regex(@"\\(.)");

		public static string Unescape(this string value) {
			return UnescapeRegex.Replace(value, match => {
				switch(match.Groups[1].Value[0]) {
					case '\\': return "\\";
					case 'r': return "\r";
					case 'n': return "\n";
					case 't': return "\t";
					case '"': return "\"";
					default: return match.Groups[1].Value;
				}
			});
		}
	}
}
