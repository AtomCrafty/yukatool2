using System;
using System.Collections.Generic;

namespace Yuka.Gui.ViewModels.Data {
	public class FileViewModel : ViewModel {
		public virtual Dictionary<string, object> FileInfo => new Dictionary<string, object>();

		public static readonly FileViewModel Dummy = new DummyFileViewModel();
		public static readonly FileViewModel Pending = new PendingFileViewModel();
		public static FileViewModel Error(Exception e) => new ErrorFileViewModel(e);
	}

	internal sealed class DummyFileViewModel : FileViewModel { }
	internal sealed class PendingFileViewModel : FileViewModel { }
	internal sealed class ErrorFileViewModel : FileViewModel {
		public Exception Exception { get; }
		public string Message => "Failed to load preview: " + Environment.NewLine + Exception.Message;

		public ErrorFileViewModel(Exception exception) {
			Exception = exception;
		}
	}
}
