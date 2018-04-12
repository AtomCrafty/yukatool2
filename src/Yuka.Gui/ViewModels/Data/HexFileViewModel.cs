using System;
using System.Text;
using Yuka.Gui.Properties;

namespace Yuka.Gui.ViewModels.Data {
	public class HexFileViewModel : FileViewModel {

		public byte[] Data { get; protected set; }
		public string HexNumbers { get; protected set; }
		public string HexText { get; protected set; }

		public HexFileViewModel(byte[] data) {
			Data = data;
			Update();
		}

		public void Update() {
			UpdateHexNumbers();
			UpdateHexText();
		}

		public void UpdateHexNumbers() {
			if(Data.LongLength > Options.HexPreviewMaxFileSize) {
				HexNumbers = Resources.UI_FileTooLongForHexPreview;
				return;
			}
			var sb = new StringBuilder();

			for(int i = 0; i < Data.Length; i++) {
				sb.Append(Data[i].ToString("X2"));

				if((i & 15) == 15) sb.Append(Environment.NewLine);
				else if((i & 7) == 7) sb.Append("  ");
				else sb.Append(' ');
			}

			HexNumbers = sb.ToString();
		}

		public void UpdateHexText() {
			if(Data.LongLength > Options.HexPreviewMaxFileSize) {
				HexText = "";
				return;
			}
			var sb = new StringBuilder();

			for(int i = 0; i < Data.Length; i++) {

				char ch = (char)Data[i];
				sb.Append(char.IsControl(ch) ? '.' : ch);

				if((i & 15) == 15) sb.Append(Environment.NewLine);
				//else if((i & 7) == 7) sb.Append(' ');
			}

			HexText = sb.ToString();
		}
	}
}
