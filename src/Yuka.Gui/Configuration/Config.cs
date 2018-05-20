using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;
using PropertyChanged;

namespace Yuka.Gui.Configuration {
	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public sealed class Config {
		public static Config Current { get; private set; } = new Config();
		private Config() { }

		public static void Save(string path) {
			using(var writer = new JsonTextWriter(new StreamWriter(File.Create(path)))) {
				new JsonSerializer { Formatting = Formatting.Indented }.Serialize(writer, Current);
			}
		}

		public static void Load(string path) {
			using(var reader = new JsonTextReader(new StreamReader(File.OpenRead(path)))) {
				var newConfig = new JsonSerializer().Deserialize<Config>(reader);
				var curConfig = Current;
				foreach(var property in typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
					if(property.CustomAttributes.All(a => a.AttributeType != typeof(JsonIgnoreAttribute))) {
						property.SetValue(curConfig, property.GetValue(newConfig));
					}
				}
			}
		}

#if DEBUG
		public bool VerboseLogging { get; set; } = true;
#else
		public bool VerboseLogging { get; set; } = false;
#endif
		[JsonIgnore]
		public bool IsInDesignMode { get; set; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());

		// preview settings
		public bool AlwaysUseHexPreview { get; set; } = false;
		public bool NeverShowHexPreview { get; set; } = false;
		public long HexPreviewMaxFileSize { get; set; } = 1024 * 1024; // 1 MB
		public bool DeletePreviewOnItemDeselect { get; set; } = false;
		public bool DisplayStackTraceOnPreviewError { get; set; } = true;

		// logging settings
		public bool EnableCollectorLogging { get; set; } = true;
		public bool EnableFileLogging { get; set; } = true;
		public string LogFilePath { get; set; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "yuka.log");

		public Dictionary<string, bool> RememberedConfirmations { get; set; } = new Dictionary<string, bool>();

		// format conversions
		public FormatMapper ExportFormatMapping { get; set; } = new FormatMapper();
		public FormatMapper ImportFormatMapping { get; set; } = new FormatMapper();
		public bool ConvertOnExport { get; set; } = false;
		public bool ConvertOnImport { get; set; } = false;
	}
}
