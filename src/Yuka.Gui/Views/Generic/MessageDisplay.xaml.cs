using System.Windows;
using System.Windows.Media;

namespace Yuka.Gui.Views.Generic {
	/// <summary>
	/// Interaction logic for LoadingMessage.xaml
	/// </summary>
	public partial class MessageDisplay {

		public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageDisplay), new UIPropertyMetadata(string.Empty));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MessageDisplay));

		public string Message {
			get => (string)GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		public ImageSource Icon {
			get => (ImageSource)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public MessageDisplay() {
			InitializeComponent();
		}
	}
}
