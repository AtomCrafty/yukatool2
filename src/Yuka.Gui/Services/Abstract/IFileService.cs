namespace Yuka.Gui.Services.Abstract {
	public interface IFileService : IService {
		string SelectDirectory(string initialDirectory);
		string SelectFile(string initialDirectory, string[] filters = null);
		string SelectArchiveFile(string initialDirectory);
	}
}
