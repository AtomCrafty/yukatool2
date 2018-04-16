using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using PropertyChanged;
using Yuka.Gui.Config;
using Yuka.Gui.Properties;

namespace Yuka.Gui {
	public static class Log {

		public static LoggerEndPoint.File FileEndPoint { get; private set; }
		public static LoggerEndPoint.Collector CollectorEndPoint { get; private set; }

		public static readonly List<LoggerEndPoint> EndPoints = new List<LoggerEndPoint>();

		static Log() => UpdateEndPoints();

		public static void UpdateEndPoints() {
			if(Config.Config.Current.EnableCollectorLogging) {
				CollectorEndPoint = CollectorEndPoint ?? new LoggerEndPoint.Collector();
				EndPoints.Add(CollectorEndPoint);
			}
			else if(CollectorEndPoint != null) {
				EndPoints.Remove(CollectorEndPoint);
			}

			if(Config.Config.Current.EnableFileLogging) {
				try {
					FileEndPoint = new LoggerEndPoint.File(new StreamWriter(File.Open(Config.Config.Current.LogFilePath, FileMode.Append, FileAccess.Write)));
					EndPoints.Add(FileEndPoint);
				}
				catch(IOException e) {
					Fail(string.Format(Resources.System_LogFileCannotBeOpened, e.GetType().Name, e.Message), Resources.Tag_System);
					Fail(e.StackTrace, Resources.Tag_System);
				}
			}
			else if(FileEndPoint != null) {
				EndPoints.Remove(FileEndPoint);
				FileEndPoint.Close();
				FileEndPoint = null;
			}

			if(Config.Config.Current.IsInDesignMode) {
				Debug(Resources.System_LoggingSessionStarted, Resources.Tag_System);
				Note(Resources.System_LoggingSessionStarted, Resources.Tag_System);
				Info(Resources.System_LoggingSessionStarted, Resources.Tag_System);
				Warn(Resources.System_LoggingSessionStarted, Resources.Tag_System);
				Fail(Resources.System_LoggingSessionStarted, Resources.Tag_System);
			}
			else {
				Info(Resources.System_LoggingSessionStarted, Resources.Tag_System);
			}
		}

		//[Conditional("DEBUG")] // not conditional to support verbose logging in release builds
		public static void Debug([Localizable(true)] string message, string tag)
			=> Write(message, LogSeverity.Debug, tag);

		public static void Note([Localizable(true)] string message, string tag)
			=> Write(message, LogSeverity.Note, tag);

		public static void Info([Localizable(true)] string message, string tag)
			=> Write(message, LogSeverity.Info, tag);

		public static void Warn([Localizable(true)] string message, string tag)
			=> Write(message, LogSeverity.Warn, tag);

		public static void Fail([Localizable(true)] string message, string tag)
			=> Write(message, LogSeverity.Error, tag);

		public static void Write([Localizable(true)] string message, LogSeverity severity, string tag)
			=> Write(new LogEntry(message, severity, tag));

		public static void Write(LogEntry entry)
			=> EndPoints.ForEach(endpoint => endpoint.Write(entry));

		public static void Clear()
			=> CollectorEndPoint?.Entries.Clear();
	}

	public class LogEntry {
		public string Tag { get; }
		public string Message { get; }
		public LogSeverity Severity { get; }
		public DateTime Time { get; } = DateTime.Now;

		public LogEntry(string message, LogSeverity severity, string tag = null) {
			Tag = tag ?? Resources.Tag_General;
			Message = message;
			Severity = severity;
		}

		public override string ToString() {
			return $"{Time.ToString(CultureInfo.CurrentCulture)}: [{Severity}] [{Tag}] {Message}";
		}
	}

	public enum LogSeverity {
		Debug, Note, Info, Warn, Error
	}

	public abstract class LoggerEndPoint {
		public abstract void Write(LogEntry entry);

		public class File : LoggerEndPoint {
			protected readonly TextWriter Writer;

			public File(TextWriter writer) {
				Writer = writer;
			}

			public override void Write(LogEntry entry) {
				Writer?.WriteLine(entry.ToString());
			}

			public void Close() {
				Writer.Flush();
				Writer.Close();
				GC.SuppressFinalize(this);
			}

			~File() {
				Close();
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class Collector : LoggerEndPoint {
			public ObservableCollection<LogEntry> Entries { get; } = new ObservableCollection<LogEntry>();

			public override void Write(LogEntry entry) {
				Application.Current.Dispatcher.Invoke(() => Entries.Add(entry));
				//Application.Current.Dispatcher.Invoke(() => Entries.Insert(0, entry));
			}
		}
	}
}