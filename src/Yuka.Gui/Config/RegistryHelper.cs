using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Yuka.Gui.Config {
	public class RegistryHelper {
		internal static string AppPaths = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
		internal static string ClassPath = @"HKEY_CLASSES_ROOT\Applications\";

		public static void RegisterApp() {
			string appName = Assembly.GetEntryAssembly().GetName().Name + ".exe";
			string assemblyLocation = Assembly.GetEntryAssembly().Location;
			string assemblyPath = Path.GetDirectoryName(assemblyLocation) ?? "";

			string yukaAppPath = AppPaths + appName;
			Registry.SetValue(yukaAppPath, string.Empty, assemblyLocation, RegistryValueKind.String);
			Registry.SetValue(yukaAppPath, "Path", assemblyPath, RegistryValueKind.String);
			Registry.SetValue(yukaAppPath, "FriendlyAppName", "YukaTool GUI Wrapper", RegistryValueKind.String);

			string yukaClassPath = ClassPath + appName;
			Registry.SetValue(yukaClassPath + @"\SupportedTypes", ".ykc", string.Empty, RegistryValueKind.String);
			Registry.SetValue(yukaClassPath + @"\shell\open\command", string.Empty, $@"""{assemblyLocation}"" ""%1""", RegistryValueKind.String);
		}
	}
}