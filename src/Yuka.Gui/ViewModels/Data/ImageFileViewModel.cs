using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Yuka.Graphics;

namespace Yuka.Gui.ViewModels.Data {
	public class ImageFileViewModel : FileViewModel {

		public YukaGraphic Graphic { get; protected set; }

		protected BitmapImage _previewImage;
		public BitmapImage PreviewImage {
			get {
				if(_previewImage != null) return _previewImage;

				Graphic.MergeChannels();
				Graphic.EnsureEncoded();

				if(Graphic.ColorData == null) return null;

				using(var stream = new MemoryStream(Graphic.ColorData)) {
					_previewImage = new BitmapImage();
					_previewImage.BeginInit();
					_previewImage.StreamSource = stream;
					_previewImage.CacheOption = BitmapCacheOption.OnLoad;
					_previewImage.EndInit();
					_previewImage.Freeze();
				}
				return _previewImage;
			}
		}

		public int Width => PreviewImage.PixelWidth;
		public int Height => PreviewImage.PixelHeight;

		public ImageFileViewModel(YukaGraphic graphic) {
			Graphic = graphic;
		}

		public override Dictionary<string, object> FileInfo => new Dictionary<string, object> {
			{"Width", Width },
			{"Height", Height }
		};
	}
}