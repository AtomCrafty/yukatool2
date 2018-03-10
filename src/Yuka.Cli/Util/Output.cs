using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Yuka.Cli.Util {
	public static class Output {

		private static readonly BlockingCollection<(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)> WriteBuffer = new BlockingCollection<(string, ConsoleColor, ConsoleColor)>();
		private static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
		private static readonly ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;

		static Output() {
			Task.Run(() => {
				while(true) {
					var (text, foregroundColor, backgroundColor) = WriteBuffer.Take();
					Console.ForegroundColor = foregroundColor;
					Console.BackgroundColor = backgroundColor;
					Console.Write(text);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}

		#region Text output methods

		#region Write

		public static void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
			=> WriteBuffer.Add((text, foregroundColor, backgroundColor));

		public static void Write(string text, ConsoleColor foregroundColor)
			=> Write(text, foregroundColor, DefaultBackgroundColor);

		public static void Write(string text)
			=> Write(text, DefaultForegroundColor);

		#endregion

		#region WriteLine

		public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
			=> Write(text + Environment.NewLine, foregroundColor, backgroundColor);

		public static void WriteLine(string text, ConsoleColor foregroundColor)
			=> Write(text + Environment.NewLine, foregroundColor, DefaultBackgroundColor);

		public static void WriteLine(string text)
			=> Write(text + Environment.NewLine, DefaultForegroundColor);

		#endregion

		#endregion
	}
}
