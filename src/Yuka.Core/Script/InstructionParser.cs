using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Yuka.Script.Data;
using Yuka.Script.Instructions;
using Yuka.Util;

namespace Yuka.Script {
	public class InstructionParser {
		protected static readonly Regex StringLiteralRegex = new Regex("\"(?<str>(?:\\\\.|[^\\\"])*)\"\\s*,?\\s*");
		protected static readonly Regex IntLiteralRegex = new Regex("(?<int>-?\\d+)\\s*,?\\s*");
		protected static readonly Regex LabelLiteralRegex = new Regex(":(?<label>[^\\s,)]+)\\s*,?\\s*");
		protected static readonly Regex VariableRegex = new Regex("(?:(?<type>\\$|(?:Global)?(?:Flag|String):)(?<id>\\d+))\\s*,?\\s*");
		protected static readonly Regex CallRegex = new Regex("^(?<func>[\\w=]+)\\(\\s*(?<args>.*)\\)\\s*(?:#.*)?$");
		protected static readonly Regex ArgumentRegex = new Regex($"{StringLiteralRegex}|{IntLiteralRegex}|{LabelLiteralRegex}|{VariableRegex}");
		protected static readonly Regex LabelRegex = new Regex("^:(?:(?<id>\\d+):)?(?<name>\\S+)(?:\\s+\\[(?<link>\\d+)\\])?$");

		protected readonly Stream Stream;
		protected InstructionList _instructions;
		protected DataSet _dataSet;

		public InstructionParser(Stream stream) {
			Stream = stream;
		}

		public InstructionList Parse() {
			if(_instructions != null) return _instructions;

			var reader = new StreamReader(Stream);
			_instructions = new InstructionList();
			_dataSet = new DataSet();

			string line;
			while((line = reader.ReadLine()) != null) _instructions.Add(ParseInstruction(line.Trim()));

			_instructions.MaxLocals = _dataSet.MaxLocals;
			return _instructions;
		}

		public Instruction ParseInstruction(string line) {

			// label instruction
			if(line.StartsWith(":")) return ParseLabelInstruction(line);

			// call instruction
			var match = CallRegex.Match(line);
			if(match.Success) {
				string function = match.Groups["func"].Value;
				string arguments = match.Groups["args"].Value;
				return CreateCallInstruction(function, ParseArgumentList(arguments));
			}

			// target instruction
			match = VariableRegex.Match(line);
			if(match.Success) return CreateTargetInstruction(match.Groups["type"].Value, match.Groups["id"].Value);

			throw new FormatException($"Unrecocnized instruction: '{line}'");
		}

		public DataElement[] ParseArgumentList(string line) {
			var arguments = new List<DataElement>();
			var argMatches = ArgumentRegex.Matches(line.Trim());
			foreach(Match argMatch in argMatches) {
				if(argMatch.Groups["str"].Success) {
					arguments.Add(_dataSet.GetStringConstant(argMatch.Groups["str"].Value.Unescape()));
					continue;
				}

				if(argMatch.Groups["int"].Success) {
					arguments.Add(_dataSet.GetIntConstant(int.Parse(argMatch.Groups["int"].Value)));
					continue;
				}

				if(argMatch.Groups["label"].Success) {
					arguments.Add(_dataSet.GetLabel(argMatch.Groups["label"].Value));
					continue;
				}

				if(argMatch.Groups["type"].Success) {
					if(!int.TryParse(argMatch.Groups["id"].Value, out int id)) id = 0;
					arguments.Add(_dataSet.GetVariable(argMatch.Groups["type"].Value, id));
					continue;
				}

				throw new FormatException($"Unrecognized argument type: '{argMatch.Value}'");
			}

			if(argMatches.Count > 0 && argMatches[argMatches.Count - 1].Index + argMatches[argMatches.Count - 1].Length < line.Length)
				throw new FormatException($"Malformed call instruction: '{line}' (malformed arguments)");

			return arguments.ToArray();
		}

		protected LabelInstruction ParseLabelInstruction(string line) {
			var match = LabelRegex.Match(line);
			if(!match.Success) throw new FormatException($"Malformed label instruction: '{line}' (regex mismatch)");
			switch(match.Groups.Count) {
				case 3:
					// :id:name
					return CreateLabelInstruction(match.Groups["name"].Value, match.Groups["id"].Value);
				case 4:
					// :id:name [link]
					return CreateLabelInstruction(match.Groups["name"].Value, match.Groups["id"].Value, match.Groups["link"].Value);
				default:
					throw new FormatException($"Malformed label instruction: '{line}' (group count {match.Groups.Count})");
			}
		}

		protected LabelInstruction CreateLabelInstruction(string name, string id = null, string link = null) {
			return new LabelInstruction(_dataSet.GetLabel(name, id, link), _instructions);
		}

		public CallInstruction CreateCallInstruction(string function, DataElement[] arguments) {
			return new CallInstruction(_dataSet.GetFunction(function), arguments, _instructions);
		}

		private TargetInstruction CreateTargetInstruction(string type, string idString) {
			if(!int.TryParse(idString, out int id)) id = 0;
			return new TargetInstruction(_dataSet.GetVariable(type, id), _instructions);
		}
	}
}