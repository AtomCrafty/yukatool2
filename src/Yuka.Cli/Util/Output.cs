using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yuka.Cli.Util {
	public static class Output {

		private static readonly BlockingCollection<(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)> WriteQueue = new BlockingCollection<(string, ConsoleColor, ConsoleColor)>();
		private static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;
		private static readonly ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;

		private static readonly AutoResetEvent QueueEmpty = new AutoResetEvent(false);

		static Output() {
			Task.Run(() => {
				foreach(var (text, foregroundColor, backgroundColor) in WriteQueue.GetConsumingEnumerable()) {
					Console.ForegroundColor = foregroundColor;
					Console.BackgroundColor = backgroundColor;
					Console.Write(text);
					Console.ResetColor();
				}
				QueueEmpty.Set();
			});
		}

		public static void Flush() {
			WriteQueue.CompleteAdding();
			QueueEmpty.WaitOne();
		}

		#region Text output methods

		#region Write

		public static void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
			=> WriteQueue.Add((text, foregroundColor, backgroundColor));

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

		public static void WriteLine()
			=> Write(Environment.NewLine);

		#endregion

		public static void WriteCaption(string text)
			=> WriteLine(text, ConsoleColor.Yellow);

		public static void WriteColored(string text) {
			var foregroundColor = DefaultForegroundColor;
			var backgroundColor = DefaultBackgroundColor;
			var sb = new StringBuilder(text.Length);

			for(int i = 0; i < text.Length; i++) {
				char ch = text[i];
				switch(ch) {

					case '\a' when i < text.Length - 2:
						if(sb.Length > 0) {
							Write(sb.ToString(), foregroundColor, backgroundColor);
							sb.Clear();
						}
						foregroundColor = text[++i] == '-' ? DefaultForegroundColor : (ConsoleColor)int.Parse(text[i].ToString(), NumberStyles.HexNumber);
						break;

					case '\b' when i < text.Length - 2:
						if(sb.Length > 0) {
							Write(sb.ToString(), foregroundColor, backgroundColor);
							sb.Clear();
						}
						backgroundColor = text[++i] == '-' ? DefaultBackgroundColor : (ConsoleColor)int.Parse(text[i].ToString(), NumberStyles.HexNumber);
						break;

					default:
						sb.Append(ch);
						break;
				}
			}
			if(sb.Length > 0) Write(sb.ToString(), foregroundColor, backgroundColor);
		}

		public static void WriteLineColored(string text)
			=> WriteColored(text + Environment.NewLine);

		#endregion
	}
}
