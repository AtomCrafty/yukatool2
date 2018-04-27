using System.ComponentModel;
using PropertyChanged;

namespace Yuka.Gui.Jobs {
	[AddINotifyPropertyChangedInterface]
	public abstract class Job {

		public double Progress { get; protected set; }
		protected string _desc;

		[Localizable(true)]
		public string Description {
			get => _desc;
			protected set {
				_desc = value;
				Log.Note(_desc, "");
			}
		}

		public abstract void Execute();
	}
}
