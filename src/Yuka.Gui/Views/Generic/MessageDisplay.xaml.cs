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

namespace Yuka.Gui.Views.Generic {
	/// <summary>
	/// Interaktionslogik für LoadingMessage.xaml
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
