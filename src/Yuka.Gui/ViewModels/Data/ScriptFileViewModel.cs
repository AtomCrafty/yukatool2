using System.Collections.Generic;
using Yuka.Script;

namespace Yuka.Gui.ViewModels.Data {
	public class ScriptFileViewModel : FileViewModel {

		protected YukaScript _script;

		protected int _instructionCount;

		public ScriptFileViewModel(YukaScript script) {
			_script = script;
			script.EnsureCompiled();
			_instructionCount = script.InstructionList.Count;
		}

		public override Dictionary<string, object> FileInfo => new Dictionary<string, object> {
			{"Instructions", _instructionCount }
		};
	}
}