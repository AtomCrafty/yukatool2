using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yuka.Gui.Views.Files.Preview {
	/// <summary>
	/// Interaction logic for YukaGraphicPreview.xaml
	/// </summary>
	public partial class ImageFilePreview {
		public ImageFilePreview() {
			InitializeComponent();
		}

		private void ZoomSlider_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
			ZoomSlider.Value = 1.0;
		}

		private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			if(!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

			ZoomSlider.Value += Math.Sign(e.Delta) * 0.1;
			e.Handled = true;
		}
	}
}
