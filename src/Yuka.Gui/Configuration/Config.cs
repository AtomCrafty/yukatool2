using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using PropertyChanged;

namespace Yuka.Gui.Configuration {
	[AddINotifyPropertyChangedInterface]
	public sealed class Config {
		public static Config Current { get; private set; } = new Config();
		private Config() { }

#if DEBUG
		public bool VerboseLogging { get; set; } = true;
#else
		public bool VerboseLogging  { get; set; } = false;
#endif
		public bool IsInDesignMode { get; set; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());

		// preview settings
		public bool AlwaysUseHexPreview { get; set; } = false;
		public bool NeverShowHexPreview { get; set; } = false;
		public long HexPreviewMaxFileSize { get; set; } = 1024 * 1024; // 1 MB
		public bool DeletePreviewOnItemDeselect { get; set; } = false;

		// logging settings
		public bool EnableCollectorLogging { get; set; } = true;
		public bool EnableFileLogging { get; set; } = true;
		public string LogFilePath { get; set; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "yuka.log");

		public Dictionary<string, bool> RememberedConfirmations { get; set; } = new Dictionary<string, bool>();

		public FormatMapper ExportFormatMapping { get; set; } = new FormatMapper();
		public FormatMapper ImportFormatMapping { get; set; } = new FormatMapper();
	}
}
