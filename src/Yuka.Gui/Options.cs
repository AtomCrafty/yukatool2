using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Yuka.Gui {
	public static class Options {
#if DEBUG
		public static bool VerboseLogging = true;
#else
		public static bool VerboseLogging = false;
#endif
		public static bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

		public static bool AlwaysUseHexPreview = false;
		public static bool NeverShowHexPreview = false;
		public static long HexPreviewMaxFileSize = 1024 * 1024; // 1 MB

		public static bool EnableCollectorLogging = true;
		public static bool EnableFileLogging = true;
		public static string LogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "yuka.log");

		public static Dictionary<string, bool> RememberedConfirmations = new Dictionary<string, bool>();
	}
}
