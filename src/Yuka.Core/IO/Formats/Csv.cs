using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yuka.Script.Data;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class CsvFormat : Format {
		public override string Extension => ".csv";
		public override string Description => "String table for a decompiled Yuka script";
		public override FormatType Type => FormatType.Unpacked;

		public override FileCategory GetFileCategory(FileSystem fs, string fileName) {
			if(fs.FileExists(fileName.WithExtension(Ykd.Extension))) {
				// this csv file contains the string table for a decompiled script
				return FileCategory.Secondary;
			}
			// it is a standalone csv file
			return FileCategory.Primary;
		}
	}

	public class CsvStringTableReader : FileReader<StringTable> {

		public override Format Format => Csv;

		public override bool CanRead(string name, BinaryReader r) {
			return name.EndsWith(Csv.Extension);
		}

		public override StringTable Read(string name, Stream s) {
			var table = new StringTable();

			var records = ReadTable(new StreamReader(s));
			if(records.Count == 0) return null;

			int columnCount = records[0].Count;

			int idColumn = -1;
			int speakerColumn = -1;
			int commentColumn = -1;
			int fallbackColumn = -1;
			var textColumns = new Dictionary<int, string>();

			var textColumnNameRegex = new Regex(Options.CsvTextColumnNameRegex);

			// identify columns
			for(int i = 0; i < records[0].Count; i++) {
				string field = records[0][i];
				if(field.Equals(Options.CsvIdColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					idColumn = i;
				}
				else if(field.Equals(Options.CsvSpeakerColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					speakerColumn = i;
				}
				else if(field.Equals(Options.CsvCommentColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					commentColumn = i;
				}
				else if(field.Equals(Options.CsvFallbackColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					fallbackColumn = i;
				}
				else {
					var match = textColumnNameRegex.Match(field);
					if(match.Success) {
						textColumns[i] = match.Value;
					}
					else {
						Console.WriteLine($"Unrecognized column '{field}', ignoring");
					}
				}
			}

			if(idColumn == -1) throw new FormatException($"No ID column found in '{name}'");
			table.Stages = textColumns.Values.ToList();

			for(int i = 1; i < records.Count; i++) {
				var record = records[i];
				if(record.Count != columnCount
				   || string.IsNullOrWhiteSpace(record[idColumn])
				   || record[idColumn].StartsWith(Options.CsvIgnorePrefix))
					continue;

				string key = record[idColumn];
				string speaker = speakerColumn != -1 ? record[speakerColumn] : null;
				string comment = commentColumn != -1 ? record[commentColumn] : null;
				string fallback = fallbackColumn != -1 ? record[fallbackColumn] : null;

				var text = textColumns.Keys.Select(k => record[k]).ToArray();

				table[key] = new StringTableEntry {
					Key = key,
					Speaker = speaker,
					Comment = comment,
					Fallback = fallback,
					Text = text
				};
			}

			return table;
		}

		public List<List<string>> ReadTable(TextReader reader) {
			var list = new List<List<string>>();

			// read records until end of file is reached
			while(reader.Peek() != -1) list.Add(ReadRecord(reader));

			return list;
		}

		public List<string> ReadRecord(TextReader reader) {

			// skip empty lines
			while(reader.Peek() == '\r' || reader.Peek() == '\n') reader.Read();

			// read at least one field
			var list = new List<string> { ReadField(reader) };
			while(true) {
				switch(reader.Peek()) {

					// end of record reached
					case '\r':
					case '\n':
					case -1:
						// skip line break
						while(reader.Peek() == '\r' || reader.Peek() == '\n') reader.Read();
						return list;

					// field separator
					case ',':
						reader.Read(); // skip comma
						list.Add(ReadField(reader));
						break;

					default:
						throw new FormatException($"Unexpected '{(char)reader.Peek()}' in csv record, expected comma or line break or end of file");
				}
			}
		}

		public string ReadField(TextReader reader) {
			bool isQuoted = reader.Peek() == '"';
			if(isQuoted) reader.Read(); // skip leading quote

			var sb = new StringBuilder();

			while(true) {
				switch(reader.Peek()) {

					case '"' when isQuoted:
						reader.Read(); // consume quote
						if(reader.Peek() == '"') {
							sb.Append((char)reader.Read());
						}
						else {
							return sb.ToString();
						}
						break;

					case '"' when Options.CsvStrict:
						throw new FormatException("Unexpected quote in unquoted csv field");

					// field/record separator
					case ',' when !isQuoted:
					case '\r' when !isQuoted:
					case '\n' when !isQuoted:
					case -1 when !isQuoted:
						return sb.ToString();

					case -1:
						throw new FormatException("Unexpected end of file; expected closing quote");

					// consume character and append it to string builder
					default:
						sb.Append((char)reader.Read());
						break;
				}
			}
		}
	}

	public class CsvStringTableWriter : FileWriter<StringTable> {

		public override Format Format => Csv;

		public override bool CanWrite(object obj) {
			return obj is StringTable;
		}

		public override void Write(StringTable table, Stream s) {
			var writer = new StreamWriter(s);

			var columnTypes = new CsvColumnType[Options.CsvGeneratedColumns.Length];
			var textColumnNameRegex = new Regex(Options.CsvTextColumnNameRegex);

			bool firstColumn = true;
			for(int i = 0; i < Options.CsvGeneratedColumns.Length; i++) {

				// write column names
				string column = Options.CsvGeneratedColumns[i];
				if(!firstColumn) writer.Write(',');
				WriteField(column, writer);

				// identify column types
				if(column.Equals(Options.CsvIdColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					columnTypes[i] = CsvColumnType.Id;
				}
				else if(column.Equals(Options.CsvSpeakerColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					columnTypes[i] = CsvColumnType.Speaker;
				}
				else if(column.Equals(Options.CsvCommentColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					columnTypes[i] = CsvColumnType.Comment;
				}
				else if(column.Equals(Options.CsvFallbackColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					columnTypes[i] = CsvColumnType.Fallback;
				}
				else {
					var match = textColumnNameRegex.Match(column);
					if(match.Success) {
						columnTypes[i] = CsvColumnType.Text;
					}
				}

				firstColumn = false;
			}
			writer.WriteLine();

			// write name records
			WriteFakeRecord(columnTypes, writer);
			WriteFakeRecord(columnTypes, writer, "#Names");
			foreach(var record in table.Names) {
				WriteRecord(record, columnTypes, writer);
			}

			// write line records
			WriteFakeRecord(columnTypes, writer);
			WriteFakeRecord(columnTypes, writer, "#Lines");
			foreach(var record in table.NonNames) {
				WriteRecord(record, columnTypes, writer);
			}

			writer.Flush();
		}

		public void WriteFakeRecord(CsvColumnType[] columnTypes, TextWriter writer, params string[] content) {
			bool firstField = true;
			for(int i = 0; i < columnTypes.Length; i++) {
				if(!firstField) writer.Write(',');
				if(i < content.Length) WriteField(content[i], writer);
				firstField = false;
			}
			writer.WriteLine();
		}

		public void WriteRecord(StringTableEntry record, CsvColumnType[] columnTypes, TextWriter writer) {
			int writtenTextColumns = 0;
			bool firstColumn = true;
			foreach(var type in columnTypes) {

				if(!firstColumn) writer.Write(',');

				switch(type) {
					case CsvColumnType.Id:
						WriteField(record.Key, writer);
						break;
					case CsvColumnType.Speaker when record.Speaker != null:
						WriteField(record.Speaker, writer);
						break;
					case CsvColumnType.Comment when record.Comment != null:
						WriteField(record.Comment, writer);
						break;
					case CsvColumnType.Fallback when record.Fallback != null:
						WriteField(record.Fallback, writer);
						break;
					case CsvColumnType.Text when writtenTextColumns < record.Text.Length:
						WriteField(record.Text[writtenTextColumns++], writer);
						break;
				}

				firstColumn = false;
			}
			writer.WriteLine();
		}

		public void WriteField(string content, TextWriter writer) {
			if(content.Contains(',') || content.Contains('"') || content.Contains('\n') || content.Contains('r')) {
				writer.Write('"' + content.Replace(@"""", @"""""") + '"');
			}
			else {
				writer.Write(content);
			}
		}
	}

	public enum CsvColumnType {
		None, Id, Speaker, Comment, Fallback, Text
	}
}