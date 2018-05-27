using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;
using PropertyChanged;
using Yuka.Gui.Properties;

namespace Yuka.Gui.Configuration {
	[Serializable]
	[AddINotifyPropertyChangedInterface]
	public sealed class Config {
		#region Management

		public static Config Current { get; } = new Config();
		private Config() { }

		public static void Save(string path) {
			Log.Info(Resources.Config_Saving, Resources.Tag_Config);
			using(var writer = new JsonTextWriter(new StreamWriter(File.Create(path)))) {
				new JsonSerializer { Formatting = Formatting.Indented }.Serialize(writer, Current);
			}
		}

		public static void Load(string path) {
			Log.Info(Resources.Config_Loading, Resources.Tag_Config);
			using(var reader = new JsonTextReader(new StreamReader(File.OpenRead(path)))) {
				Current.Update(new JsonSerializer().Deserialize<Config>(reader));
			}
		}

		public static void Reset() {
			Log.Info(Resources.Config_Resetting, Resources.Tag_Config);
			Current.Update(new Config());
		}

		private void Update(Config config) {
			foreach(var property in typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
				if(property.CanRead && property.CanWrite && property.CustomAttributes.All(a => a.AttributeType != typeof(JsonIgnoreAttribute))) {
					property.SetValue(this, property.GetValue(config));
				}
			}
		}

		#endregion

		#region Properties
		[JsonIgnore]
		public bool IsInDesignMode { get; set; } = DesignerProperties.GetIsInDesignMode(new DependencyObject());
#if DEBUG
		private static bool _isDebugBuild = true;
#else
		private static bool _isDebugBuild = false;
#endif

		public bool IsDebugBuild => _isDebugBuild;
		public bool VerboseLogging { get; set; } = _isDebugBuild;

		// preview settings
		public bool AlwaysUseHexPreview { get; set; } = false;
		public bool NeverShowHexPreview { get; set; } = false;
		public long HexPreviewMaxFileSize { get; set; } = 1024 * 1024; // 1 MB
		public bool DeletePreviewOnItemDeselect { get; set; } = true;
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

		#endregion
	}
}
