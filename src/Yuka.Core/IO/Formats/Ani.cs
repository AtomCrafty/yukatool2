using System.IO;
using System.Text;
using Newtonsoft.Json;
using Yuka.Graphics;
using Yuka.Util;

namespace Yuka.IO.Formats {

	public class AniFormat : Format {
		public override string Id => "ani";
		public override string Extension => ".ani";
		public override string Description => "Human-readable frame animation data";
		public override FormatType Type => FormatType.Unpacked;

		public override FileCategory GetFileCategory(FileSystem fs, string fileName) {
			// when a png or bmp with the same name exists, this ani belongs to it
			return fs.FileExists(fileName.WithExtension(Png.Extension))
				 || fs.FileExists(fileName.WithExtension(Bmp.Extension)) ? FileCategory.Secondary : FileCategory.Primary;
		}
	}

	public class AniAnimationReader : FileReader<Animation> {

		public override Format Format => Format.Ani;

		private readonly JsonSerializer _serializer = new JsonSerializer();

		public override bool CanRead(string name, BinaryReader r) {
			return Path.GetExtension(name)?.ToLower() == ".ani";
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
			using(var streamWriter = new StreamWriter(s, Encoding.UTF8, 1024, true)) {
				using(var jsonTextWriter = new JsonTextWriter(streamWriter)) {
					_serializer.Serialize(jsonTextWriter, ani);
					//jsonTextWriter.Flush();
				}
			}
		}
	}
}