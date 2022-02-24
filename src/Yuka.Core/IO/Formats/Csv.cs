using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using Yuka.Script.Data;
using Yuka.Util;
using static Yuka.IO.Format;

namespace Yuka.IO.Formats {

	public class CsvFormat : Format {
		public override string Id => "csv";
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

			using var reader = new CsvReader(new StreamReader(s), CultureInfo.InvariantCulture, true);
			if(!reader.Read()) return null;
			if(!reader.ReadHeader()) return null;

			int keyColumn = -1;
			int speakerColumn = -1;
			int commentColumn = -1;
			int fallbackColumn = -1;
			var textColumns = new List<int>();
			var textColumnNames = new List<string>();

			var textColumnNameRegex = new Regex(Options.CsvTextColumnNameRegex);

			// identify columns
			for(int i = 0; i < reader.HeaderRecord.Length; i++) {
				string field = reader.HeaderRecord[i];
				if(field.Equals(Options.CsvIdColumnName, StringComparison.CurrentCultureIgnoreCase)) {
					keyColumn = i;
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
						textColumns.Add(i);
						textColumnNames.Add(match.Groups[0].Value);
					}
				}
			}

			if(keyColumn == -1) throw new FormatException($"No {Options.CsvIdColumnName} column found in '{name}'");
			table.Stages = textColumnNames;

			while(reader.Read()) {
				string key = reader.GetField(keyColumn);
				var category = StringTableEntry.GetCategoryForKey(key);

				if(string.IsNullOrWhiteSpace(key) || key.StartsWith(Options.CsvIgnorePrefix) || category == null)
					continue;

				string speaker = speakerColumn != -1 ? reader.GetField(speakerColumn) : null;
				string comment = commentColumn != -1 ? reader.GetField(commentColumn) : null;
				string fallback = fallbackColumn != -1 ? reader.GetField(fallbackColumn) : null;
				var text = textColumns.Select(reader.GetField).ToArray();

				table[key] = new StringTableEntry(category.Value, key) {
					Speaker = speaker,
					Comment = comment,
					Fallback = fallback,
					Text = text
				};
			}

			return table;
		}
	}

	public class CsvStringTableWriter : FileWriter<StringTable> {

		public override Format Format => Csv;

		public override bool CanWrite(object obj) {
			return obj is StringTable;
		}

		public override void Write(StringTable table, Stream s) {
			using var writer = new CsvWriter(new StreamWriter(s), CultureInfo.InvariantCulture, true);

			var textColumnNameRegex = new Regex(Options.CsvTextColumnNameRegex);
			var columnTypes = Options.CsvGeneratedColumns.Select(column => {
				if(column.Equals(Options.CsvIdColumnName, StringComparison.InvariantCultureIgnoreCase))
					return CsvColumnType.Id;
				if(column.Equals(Options.CsvSpeakerColumnName, StringComparison.InvariantCultureIgnoreCase))
					return CsvColumnType.Speaker;
				if(column.Equals(Options.CsvCommentColumnName, StringComparison.InvariantCultureIgnoreCase))
					return CsvColumnType.Comment;
				if(column.Equals(Options.CsvFallbackColumnName, StringComparison.InvariantCultureIgnoreCase))
					return CsvColumnType.Fallback;
				if(textColumnNameRegex.IsMatch(column))
					return CsvColumnType.Text;
				return CsvColumnType.None;
			}).ToList();

			// write header record
			foreach(string column in Options.CsvGeneratedColumns) {
				writer.WriteField(column);
			}
			writer.NextRecord();

			// write name records
			writer.NextRecord();
			writer.WriteField("#Names");
			writer.NextRecord();
			foreach(var record in table.Names) {
				WriteRecord(record, columnTypes, writer);
			}

			// write line records
			writer.NextRecord();
			writer.WriteField("#Lines");
			writer.NextRecord();
			foreach(var record in table.NonNames) {
				WriteRecord(record, columnTypes, writer);
			}

			writer.Flush();
		}

		public void WriteRecord(StringTableEntry record, IEnumerable<CsvColumnType> columnTypes, CsvWriter writer) {
			int writtenTextColumns = 0;

			foreach(var type in columnTypes) {
				switch(type) {
					case CsvColumnType.Id:
						writer.WriteField(record.Key);
						break;
					case CsvColumnType.Speaker:
						writer.WriteField(record.Speaker);
						break;
					case CsvColumnType.Comment:
						writer.WriteField(record.Comment);
						break;
					case CsvColumnType.Fallback:
						writer.WriteField(record.Fallback);
						break;
					case CsvColumnType.Text when writtenTextColumns < record.Text.Length:
						writer.WriteField(record.Text[writtenTextColumns++]);
						break;
					default:
						writer.WriteField(null);
						break;
				}
			}
			writer.NextRecord();
		}
	}

	public enum CsvColumnType {
		None, Id, Speaker, Comment, Fallback, Text
	}
}