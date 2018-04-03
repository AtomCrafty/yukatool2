using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Yuka.Gui {
	public class ViewModel : INotifyPropertyChanged {
#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
