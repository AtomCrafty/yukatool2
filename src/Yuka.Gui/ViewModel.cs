using System.ComponentModel;

namespace Yuka.Gui {
	public class ViewModel : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChangedEvent(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
	}
}
