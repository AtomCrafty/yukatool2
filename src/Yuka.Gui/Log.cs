using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using PropertyChanged;

namespace Yuka.Gui {
	public static class Log {

		public static LoggerEndPoint.File FileEndPoint { get; private set; }
		public static LoggerEndPoint.Collector CollectorEndPoint { get; private set; }

		public static readonly List<LoggerEndPoint> EndPoints = new List<LoggerEndPoint>();

		static Log() => UpdateEndPoints();

		public static void UpdateEndPoints() {
			if(Options.EnableCollectorLogging) {
				CollectorEndPoint = CollectorEndPoint ?? new LoggerEndPoint.Collector();
				EndPoints.Add(CollectorEndPoint);
			}
			else if(CollectorEndPoint != null) {
				EndPoints.Remove(CollectorEndPoint);
			}

			if(Options.EnableFileLogging) {
				FileEndPoint = new LoggerEndPoint.File(new StreamWriter(File.Open(Options.LogFilePath, FileMode.Append, FileAccess.Write)));
				EndPoints.Add(FileEndPoint);
			}
			else if(FileEndPoint != null) {
				EndPoints.Remove(FileEndPoint);
				FileEndPoint.Close();
				FileEndPoint = null;
			}
			
			Info("Started logging session", "Logger");
		}

		public static void Note(string message, string tag = null)
			=> Write(message, LogSeverity.Note, tag);

		public static void Info(string message, string tag = null)
			=> Write(message, LogSeverity.Info, tag);

		public static void Warn(string message, string tag = null)
			=> Write(message, LogSeverity.Warn, tag);

		public static void Fail(string message, string tag = null)
			=> Write(message, LogSeverity.Error, tag);

		public static void Write(string message, LogSeverity severity, string tag = null)
			=> Write(new LogEntry(message, severity, tag));

		public static void Write(LogEntry entry)
			=> EndPoints.ForEach(endpoint => endpoint.Write(entry));
	}

	public class LogEntry {
		public string Tag { get; }
		public string Message { get; }
		public LogSeverity Severity { get; }
		public DateTime Time { get; } = DateTime.Now;

		public LogEntry(string message, LogSeverity severity, string tag = "General") {
			Tag = tag;
			Message = message;
			Severity = severity;
		}

		public override string ToString() {
			return $"{Time.ToString(CultureInfo.CurrentCulture)}: [{Severity}] [{Tag}] {Message}";
		}
	}

	public enum LogSeverity {
		Note, Info, Warn, Error
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
				Entries.Add(entry);
			}
		}
	}
}