using System;
using System.Windows.Input;

namespace Yuka.Gui {
	public class ActionCommand : ICommand {

		public event EventHandler CanExecuteChanged;
		protected readonly Action Action;
		protected bool _enabled;

		public ActionCommand(Action action, bool enabled = true) {
			Action = action ?? throw new ArgumentNullException(nameof(action));
			_enabled = enabled;
		}

		public bool IsEnabled {
			get => _enabled;
			set {
				if(value) Enable();
				else Disable();
			}
		}

		public void Enable() {
			if(_enabled) return;
			_enabled = true;
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Disable() {
			if(!_enabled) return;
			_enabled = false;
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool CanExecute(object parameter) => _enabled;
		public void Execute(object parameter) => Action();
	}
}
