using System.IO;
using System.Reflection;

namespace Yuka.Gui {
	public static class Options {
		public static bool AlwaysUseHexPreview = false;

		public static bool EnableCollectorLogging = true;
		public static bool EnableFileLogging = true;
		public static string LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "yuka.log");
	}
}
