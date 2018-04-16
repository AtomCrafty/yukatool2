using PropertyChanged;

namespace Yuka.Gui.Jobs {
	[AddINotifyPropertyChangedInterface]
	public abstract class Job {

		public double Progress { get; protected set; }
		public string Description { get; protected set; }

		public abstract void Execute();
	}
}
