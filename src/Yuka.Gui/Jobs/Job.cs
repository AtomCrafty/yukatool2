using System.ComponentModel;
using PropertyChanged;

namespace Yuka.Gui.Jobs {
	[AddINotifyPropertyChangedInterface]
	public abstract class Job {

		[Localizable(true)]
		public string Status { get; protected set; }
		public double Progress { get; protected set; }

		public abstract void Execute();
	}
}
