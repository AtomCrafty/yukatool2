using Ookii.Dialogs.Wpf;

namespace Yuka.Gui.Services.Abstract {
	public interface IConfirmationService : IService {
	}

	public enum DialogIcon {
		None = TaskDialogIcon.Custom,
		Shield = TaskDialogIcon.Shield,
		Information = TaskDialogIcon.Information,
		Error = TaskDialogIcon.Error,
		Warning = TaskDialogIcon.Warning
	}
}