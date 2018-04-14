using Yuka.Gui.Services;
using Yuka.IO;

namespace Yuka.Gui.Jobs {
	public class ExportAllJob {
		protected readonly FileSystem Source;
		protected readonly string TargetFolder;
		protected readonly bool RawCopy;

		public ExportAllJob(FileSystem source, string targetFolder) {
			Source = source;
			TargetFolder = targetFolder;
			RawCopy = true;
		}

		public void Execute() {
			using(var dstFs = FileSystem.NewFolder(TargetFolder)) {

				if(dstFs.GetFiles().Length > 0 && !Service.Get<ConfirmationService>().ConfirmAndRemember("ExportIntoNonEmptyFolder")) return;

				foreach(string file in Source.GetFiles()) {
					if(RawCopy) {
						using(var srcFile = Source.OpenFile(file))
						using(var dstFile = dstFs.CreateFile(file)) {

							srcFile.CopyTo(dstFile);
						}
					}
					// TODO convert-copy
				}
			}
		}
	}
}
