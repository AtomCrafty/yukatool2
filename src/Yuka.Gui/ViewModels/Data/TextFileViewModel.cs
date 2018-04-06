namespace Yuka.Gui.ViewModels.Data {
	public class TextFileViewModel : FileViewModel {

		public string Value { get; set; }

		public TextFileViewModel(string value) {
			Value = value;
		}
	}
}