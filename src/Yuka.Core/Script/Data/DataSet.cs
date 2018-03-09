using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yuka.IO;
using Yuka.IO.Formats;

namespace Yuka.Script.Data {
	public class DataSet {

		protected readonly Dictionary<string, DataElement.Func> FuncElements = new Dictionary<string, DataElement.Func>();
		protected readonly Dictionary<string, DataElement.CStr> CStrElements = new Dictionary<string, DataElement.CStr>();
		protected readonly Dictionary<int, DataElement.CInt> CIntElements = new Dictionary<int, DataElement.CInt>();
		protected readonly Dictionary<int, DataElement.CInt> CIntPointers = new Dictionary<int, DataElement.CInt>();

		protected readonly Dictionary<string, DataElement> Variables = new Dictionary<string, DataElement>();
		public uint MaxLocals { get; protected set; }

		protected readonly Dictionary<string, DataElement.Ctrl> UniqueLabels = new Dictionary<string, DataElement.Ctrl>();
		protected readonly Dictionary<int, DataElement.Ctrl> BlockLabels = new Dictionary<int, DataElement.Ctrl>();

		protected readonly Dictionary<int, ScriptValue.Int> IntValues = new Dictionary<int, ScriptValue.Int>();
		protected readonly Dictionary<int, ScriptValue.Int> PtrValues = new Dictionary<int, ScriptValue.Int>();
		protected readonly Dictionary<string, ScriptValue.Str> StrValues = new Dictionary<string, ScriptValue.Str>();

		public ScriptValue.Int CreateScriptValue(int value) {
			if(IntValues.ContainsKey(value)) return IntValues[value];
			return IntValues[value] = new ScriptValue.Int(value);
		}

		public ScriptValue.Int CreateScriptValuePointer(int id) {
			if(PtrValues.ContainsKey(id)) return PtrValues[id];
			return PtrValues[id] = new ScriptValue.Int(0) { PointerId = id };
		}

		public ScriptValue.Str CreateScriptValue(string value) {
			if(StrValues.ContainsKey(value)) return StrValues[value];
			return StrValues[value] = new ScriptValue.Str(value);
		}

		public DataElement.Ctrl CreateLabel(string name, string idString = null, string linkString = null) {
			if(!int.TryParse(idString, out int id)) id = -1;
			if(!int.TryParse(linkString, out int link)) link = -1;

			if(Format.Yks.BlockLabels.Contains(name)) {
				// create new label
				var label = new DataElement.Ctrl(CreateScriptValue(name)) { Id = id };
				if(id == -1 || link == -1) throw new FormatException($"Block label must specify an id and a link: ':{id}:{name} [{link}]'");

				// register label
				Debug.Assert(!BlockLabels.ContainsKey(id), $"Duplicate label id: ':{id}:{name} [{link}]'");
				BlockLabels[id] = label;

				// try to link block start and end
				if(BlockLabels.ContainsKey(link)) {
					LinkLabels(BlockLabels[link], label);
				}
				return label;
			}
			else {
				DataElement.Ctrl label;
				if(!UniqueLabels.ContainsKey(name)) {
					// register new unique label
					// may be caused by a reference instead of the
					// label declaration (-> don't set id here)
					label = new DataElement.Ctrl(CreateScriptValue(name));
					UniqueLabels[name] = label;
				}
				else {
					label = UniqueLabels[name];
				}

				// set id on label declaration only
				if(id != -1) label.Id = id;

				if(link == -1) return label;

				// link the label to itself
				Debug.Assert(link == id, $"Unique label must link to itself: ':{id}:{name} [{link}]'");
				label.LinkedElement = label;
				return label;
			}
		}

		public void LinkLabels(DataElement.Ctrl a, DataElement.Ctrl b) {
			a.LinkedElement = b;
			b.LinkedElement = a;
		}

		public DataElement.Func CreateFunction(string name) {
			if(FuncElements.ContainsKey(name)) return FuncElements[name];
			return FuncElements[name] = new DataElement.Func(CreateScriptValue(name));
		}

		public DataElement.CStr CreateStringConstant(string value) {
			if(CStrElements.ContainsKey(value)) return CStrElements[value];
			return CStrElements[value] = new DataElement.CStr(CreateScriptValue(value));
		}

		public DataElement.CInt CreateIntConstant(int value) {
			if(CIntElements.ContainsKey(value)) return CIntElements[value];
			return CIntElements[value] = new DataElement.CInt(CreateScriptValue(value));
		}

		public DataElement.CInt CreateIntPointer(int pointerId) {
			if(CIntPointers.ContainsKey(pointerId)) return CIntPointers[pointerId];
			return CIntPointers[pointerId] = new DataElement.CInt(CreateScriptValuePointer(pointerId));
		}

		public DataElement CreateVariable(string type, int id) {
			type = type.TrimEnd(':');
			string key = $"{type}:{id}";
			var variable = Variables.ContainsKey(key) ? Variables[key] : null;

			if(variable != null) return variable;

			switch(type) {

				case "$":
					variable = new DataElement.VLoc((uint)id);
					if(id >= MaxLocals) MaxLocals = (uint)id + 1;
					break;

				case YksFormat.Flag:
				case YksFormat.GlobalFlag:
					variable = new DataElement.VInt(CreateScriptValue(type), CreateScriptValue(id));
					break;

				case YksFormat.String:
				case YksFormat.GlobalString:
					variable = new DataElement.VStr(CreateScriptValue(type), CreateScriptValue(id));
					break;

				case YksFormat.TempGlobalString:
				case YksFormat.主人公:
				case YksFormat.汎用文字変数:
					variable = new DataElement.SStr(CreateScriptValue(type));
					break;

				default:
					throw new FormatException($"Unrecognized variable type: '{type}'");
			}

			Variables[key] = variable;
			return variable;
		}

		public DataElement CreateVariablePointer(string type, int id) {
			type = type.TrimEnd(':');
			string key = $"{type}:&{id}";
			var variable = Variables.ContainsKey(key) ? Variables[key] : null;

			if(variable != null) return variable;

			switch(type) {

				case YksFormat.Flag:
				case YksFormat.GlobalFlag:
					variable = new DataElement.VInt(CreateScriptValue(type), CreateScriptValuePointer(id));
					break;

				case YksFormat.String:
				case YksFormat.GlobalString:
					variable = new DataElement.VStr(CreateScriptValue(type), CreateScriptValuePointer(id));
					break;

				default:
					throw new FormatException($"Unrecognized variable pointer type: '{type}'");
			}

			Variables[key] = variable;
			return variable;
		}
	}
}
