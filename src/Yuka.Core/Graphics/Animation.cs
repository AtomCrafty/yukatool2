using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Yuka.IO;
using Yuka.Util;

namespace Yuka.Graphics {

	[Serializable]
	public class Animation {

		// only one may be non-null at a time
		[NonSerialized]
		public byte[] FrameData;
		public List<Frame> Frames;

		public bool IsDecoded {
			get {
				Debug.Assert(Frames == null | FrameData == null);
				return Frames != null;
			}
		}

		public void EnsureDecoded() {
			if(IsDecoded) return;

			Debug.Assert(Frames == null);
			Decode();
			Debug.Assert(FrameData == null);
		}

		public void EnsureEncoded() {
			if(!IsDecoded) return;

			Debug.Assert(FrameData == null);
			Encode();
			Debug.Assert(Frames == null);
		}

		public void Decode() {
			if(IsDecoded) return;
			Debug.Assert(Frames == null);

			using(var r = new BinaryReader(new MemoryStream(FrameData))) {
				Frames = new List<Frame>();

				for(int i = 0; i < r.BaseStream.Length / Format.Frm.FrameSize; i++) {
					Frames.Add(new Frame {
						X = r.ReadUInt32(),
						Y = r.ReadUInt32(),
						W = r.ReadUInt32(),
						H = r.ReadUInt32(),
						Duration = r.ReadUInt32(),
						Type = (FrameType)r.ReadUInt32(),
						Unknown = r.ReadUInt32(),
						Action = (FrameAction)r.ReadUInt32()
					});
				}
			}

			FrameData = null;
		}

		public void Encode() {
			if(!IsDecoded || Frames == null) return;
			Debug.Assert(FrameData == null);

			FrameData = new byte[Frames.Count * Format.Frm.FrameSize];

			using(var w = new MemoryStream(FrameData).NewWriter()) {
				foreach(var frame in Frames) {
					w.Write(frame.X);
					w.Write(frame.Y);
					w.Write(frame.W);
					w.Write(frame.H);
					w.Write(frame.Duration);
					w.Write((uint)frame.Type);
					w.Write(frame.Unknown);
					w.Write((uint)frame.Action);
				}
			}

			Frames = null;
		}

		public static Animation FromFrameData(byte[] frameData) {
			return frameData.IsNullOrEmpty() ? null : new Animation { FrameData = frameData };
		}

		public class Frame {
			public uint X;
			public uint Y;
			public uint W;
			public uint H;
			public uint Duration;
			public FrameType Type;
			public uint Unknown;
			public FrameAction Action;
		}

		[Flags]
		[JsonConverter(typeof(StringEnumConverter))]
		public enum FrameType : uint {
			None = 0,
			Reverse = 1,
			Loop = 2
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public enum FrameAction : uint {
			None,
			SetFrame,
			SetState
		}
	}
}
