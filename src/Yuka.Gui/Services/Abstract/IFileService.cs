using System.ComponentModel;

namespace Yuka.Gui.Services.Abstract {
	public interface IFileService : IService {
		string SelectDirectory(string initialDirectory, [Localizable(true)] string description = null);
		string SelectFile(string initialDirectory, string[] filters = null);
		string SelectArchiveFile(string initialDirectory);
	}
}
