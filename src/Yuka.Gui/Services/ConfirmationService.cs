using System.ComponentModel;
using System.Windows;
using Ookii.Dialogs.Wpf;
using Yuka.Gui.Config;
using Yuka.Gui.Properties;
using Yuka.Gui.Services.Abstract;

namespace Yuka.Gui.Services {
	public class ConfirmationService : IConfirmationService {
		protected readonly Window ParentWindow;

		public ConfirmationService(Window parentWindow) {
			ParentWindow = parentWindow;
		}

		public bool? GetRememberedConfirmation(string id) {
			return id != null && Config.Config.Current.RememberedConfirmations.ContainsKey(id) ? (bool?)Config.Config.Current.RememberedConfirmations[id] : null;
		}

		public void RememberConfirmation(string id, bool choice) {
			if(id != null) Config.Config.Current.RememberedConfirmations[id] = choice;
		}

		public bool ConfirmAndRemember(string id, [Localizable(true)]string message = null, [Localizable(true)]string description = null, [Localizable(true)]string title = null, DialogIcon icon = DialogIcon.Information, bool allowCancellation = true) {
			var remembered = GetRememberedConfirmation(id);
			if(remembered != null) {
				Log.Note(string.Format(Resources.UI_ConfirmationUsingRememberedValue, remembered, id), Resources.Tag_System);
				return remembered.Value;
			}

			if(TaskDialog.OSSupportsTaskDialogs) {
				using(var dialog = new TaskDialog {
					AllowDialogCancellation = allowCancellation,
					WindowTitle = title ?? Resources.ResourceManager.GetString($"UI_Confirmation_{id}_Title") ?? Resources.UI_ConfirmationDefaultWindowTitle,
					MainIcon = (TaskDialogIcon)icon,
					MainInstruction = message ?? Resources.ResourceManager.GetString($"UI_Confirmation_{id}_Message") ?? id,
					Content = description ?? Resources.ResourceManager.GetString($"UI_Confirmation_{id}_Description"),
					VerificationText = Resources.UI_ConfirmationRememberSelection
				}) {
					dialog.Buttons.Add(new TaskDialogButton {
						ButtonType = ButtonType.Yes
					});
					dialog.Buttons.Add(new TaskDialogButton {
						ButtonType = ButtonType.No
					});
					bool choice = dialog.ShowDialog(ParentWindow)?.ButtonType == ButtonType.Yes;
					if(dialog.IsVerificationChecked) RememberConfirmation(id, choice);
					return choice;
				}
			}

			// TODO fallback confirmation dialog
			return false;
		}

		public bool Confirm([Localizable(true)]string message, [Localizable(true)]string description = null, [Localizable(true)]string title = null, DialogIcon icon = DialogIcon.Information, bool allowCancellation = true) {
			if(TaskDialog.OSSupportsTaskDialogs) {
				using(var dialog = new TaskDialog {
					AllowDialogCancellation = allowCancellation,
					WindowTitle = title ?? Resources.UI_ConfirmationDefaultWindowTitle,
					MainIcon = (TaskDialogIcon)icon,
					MainInstruction = message,
					Content = description
				}) {
					dialog.Buttons.Add(new TaskDialogButton {
						ButtonType = ButtonType.Yes
					});
					dialog.Buttons.Add(new TaskDialogButton {
						ButtonType = ButtonType.No
					});
					return dialog.ShowDialog(ParentWindow)?.ButtonType == ButtonType.Yes;
				}
			}

			// TODO fallback confirmation dialog
			return false;
		}
	}
}