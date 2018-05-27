using System.ComponentModel;

namespace Yuka.Gui.Services.Abstract {
	public interface IFileService : IService {
		string SelectDirectory(string initialDirectory = null, [Localizable(true)] string description = null);
		string OpenFile(string filter = null, string ext = null, string title = null, string initialDirectory = null);
		string SaveFile(string filter = null, string ext = null, string title = null, string initialDirectory = null);

		string SelectArchiveFile(string initialDirectory = null);
	}
}
