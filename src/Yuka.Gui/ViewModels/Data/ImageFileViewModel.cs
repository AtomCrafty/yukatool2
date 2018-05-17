using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Yuka.Graphics;

namespace Yuka.Gui.ViewModels.Data {
	public class ImageFileViewModel : FileViewModel {

		protected YukaGraphic _graphic;
		public YukaGraphic Graphic {
			get => _graphic;
			protected set {
				if(_graphic == value) return;
				_graphic = value;
				UpdatePreviewImage();
			}
		}
		public BitmapImage PreviewImage { get; protected set; }

		public int Width => PreviewImage.PixelWidth;
		public int Height => PreviewImage.PixelHeight;

		public ImageFileViewModel(YukaGraphic graphic) {
			Graphic = graphic;
		}

		public void UpdatePreviewImage() {
			if(Graphic == null) return;

			Graphic.MergeChannels();
			Graphic.EnsureEncoded();

			if(Graphic.ColorData == null) return;

			var image = new BitmapImage();
			using(var stream = new MemoryStream(Graphic.ColorData)) {
				image.BeginInit();
				image.StreamSource = stream;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.EndInit();
				image.Freeze();
			}
			PreviewImage = image;

			ClearAttributes();
			AddAttribute("Width", Width + " px");
			AddAttribute("Height", Height + " px");
			if(Graphic.Animation != null) {
				Graphic.Animation.Decode();
				AddAttribute("States", Graphic.Animation.Frames.Count);
			}
		}
	}
}