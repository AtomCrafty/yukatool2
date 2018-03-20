using Yuka.Script;

namespace Yuka.Gui.ViewModels {
	public class YukaScriptViewModel : ViewModel {

		protected YukaScript _script;

		public YukaScriptViewModel(YukaScript script) {
			_script = script;
		}
	}
}