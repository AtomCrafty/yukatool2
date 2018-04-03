using System.Collections.Generic;

namespace Yuka.Gui.ViewModels.Data {
	public class FileViewModel : ViewModel {
		public virtual Dictionary<string, object> FileInfo => new Dictionary<string, object>();

		public static readonly FileViewModel Dummy = new DummyFileViewModel();
		public static readonly FileViewModel Pending = new PendingFileViewModel();
	}

	internal sealed class DummyFileViewModel : FileViewModel { }
	internal sealed class PendingFileViewModel : FileViewModel { }
}
