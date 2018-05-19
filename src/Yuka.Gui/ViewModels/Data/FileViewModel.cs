using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Yuka.Gui.ViewModels.Data {
	public class FileViewModel : ViewModel {

		#region Attributes

		public ObservableCollection<KeyValuePair<string, object>> Attributes { get; } = new ObservableCollection<KeyValuePair<string, object>>();
		public IEnumerable<string> AttributeNames => Attributes.Select(kvp => kvp.Key);
		public IEnumerable<object> AttributeValues => Attributes.Select(kvp => kvp.Value);

		public void ClearAttributes() => Attributes.Clear();
		public void AddAttribute(string key, object value) => Attributes.Add(new KeyValuePair<string, object>(key, value));

		public FileViewModel WithAttribute(string key, object value) {
			AddAttribute(key, value);
			return this;
		}

		#endregion

		public static readonly FileViewModel Dummy = new DummyFileViewModel();
		public static readonly FileViewModel Pending = new PendingFileViewModel();
		public static FileViewModel Error(Exception e) => new ErrorFileViewModel(e);
	}

	internal sealed class DummyFileViewModel : FileViewModel { }
	internal sealed class PendingFileViewModel : FileViewModel { }
	internal sealed class ErrorFileViewModel : FileViewModel {
		public Exception Exception { get; }
		public string Message => "Failed to load preview"
								 + Environment.NewLine + Exception.GetType().Name + ": " + Exception.Message
								 + (Configuration.Config.Current.DisplayStackTraceOnPreviewError
									? Environment.NewLine
									+ Environment.NewLine
									+ string.Join(Environment.NewLine, new StackTrace(Exception).GetFrames()?.Select(f => f.GetMethod().DeclaringType + "." + f.GetMethod().Name) ?? Array.Empty<string>())
								 : "");

		public ErrorFileViewModel(Exception exception) {
			Exception = exception;
		}
	}
}
