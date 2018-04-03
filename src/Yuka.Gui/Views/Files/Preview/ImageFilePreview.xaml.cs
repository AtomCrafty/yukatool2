using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yuka.Gui.Views.Files.Preview {
	/// <summary>
	/// Interaktionslogik für YukaGraphicPreview.xaml
	/// </summary>
	public partial class ImageFilePreview : UserControl {
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
