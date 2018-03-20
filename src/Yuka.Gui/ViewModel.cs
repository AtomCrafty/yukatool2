using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using Yuka.Gui.Annotations;

namespace Yuka.Gui {
	public class ViewModel : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
