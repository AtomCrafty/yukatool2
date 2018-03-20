using System;
using Yuka.Graphics;

namespace Yuka.Gui.ViewModels {
	public class YukaGraphicViewModel : ViewModel {

		public YukaGraphic Graphic { get; protected set; }

		public YukaGraphicViewModel(YukaGraphic graphic) {
			Graphic = graphic;
		}
	}
}