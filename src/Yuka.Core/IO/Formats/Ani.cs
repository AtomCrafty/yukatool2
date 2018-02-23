using System.IO;
using Newtonsoft.Json;
using Yuka.Graphics;

namespace Yuka.IO.Formats {

	public class AniFormat : Format {
		public override string Extension => ".ani";
		public override string Description => "Human-readable frame animation data";
		public override FormatType Type => FormatType.Unpacked;
	}

	public class AniAnimationReader : FileReader<Animation> {

		public override Format Format => Format.Ani;

		private readonly JsonSerializer _serializer = new JsonSerializer();

		public override bool CanRead(string name, BinaryReader r) {
			return ('.' + name.ToLower()).EndsWith(".ani");
		}

		public override Animation Read(string name, Stream s) {
			return _serializer.Deserialize<Animation>(new JsonTextReader(new StreamReader(s)));
		}
	}

	public class AniAnimationWriter : FileWriter<Animation> {

		public override Format Format => Format.Ani;

		private readonly JsonSerializer _serializer = new JsonSerializer { Formatting = Formatting.Indented };

		public override bool CanWrite(object obj) {
			return obj is Animation;
		}

		public override void Write(Animation ani, Stream s) {
			ani.EnsureDecoded();
			_serializer.Serialize(new JsonTextWriter(new StreamWriter(s)), ani);
		}
	}
}