using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Yuka.Graphics;
using Yuka.Gui.ViewModels.Data;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script;
using Yuka.Util;

namespace Yuka.Gui.ViewModels {
	public class ShellItemViewModel : ViewModel {

		protected readonly FileSystem FileSystem;

		public ShellItemType Type { get; set; }

		public string FullPath { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public ObservableCollection<ShellItemViewModel> Children { get; } = new ObservableCollection<ShellItemViewModel>();

		public bool IsExpanded { get; set; } = true;

		private bool _isSelected;
		public bool IsSelected {
			get => _isSelected;
			set {
				if(_isSelected == value) return;

				_isSelected = value;
				(_previewContent as IDisposable)?.Dispose();
				_previewContent = null;
			}
		}

		public ActionCommand ExportCommand { get; protected set; }
		public ActionCommand DeleteCommand { get; protected set; }

		public string Icon => GetIconName();
		public Format Format => Type == ShellItemType.Directory ? null : Format.GuessFromFileName(Name);

		protected object _previewContent;
		protected FileViewModel _previewViewModel;
		protected bool _previewLoading;

		public FileViewModel Preview {
			get {
				if(_previewViewModel != null) return _previewViewModel;
				if(_previewLoading) return FileViewModel.Pending;

				// don't even attempt to load a file that doesn't exist
				if(!FileSystem.FileExists(FullPath)) return FileViewModel.Dummy;

				// avoid spawning multiple tasks
				_previewLoading = true;
				var task = Task.Run(() => {
					try {
						if(Options.AlwaysUseHexPreview) {
							using(var reader = FileSystem.OpenFile(FullPath).NewReader()) {
								_previewContent = reader.ReadToEnd();
							}
						}
						else {
							_previewContent = FileReader.DecodeObject(FullPath, FileSystem).Item1;
						}

						// send PropertyChanged update to reload UI
						_previewLoading = false;
						Preview = GetPreviewViewModel(_previewContent);
					}
					catch(Exception e) {
						// send PropertyChanged update to reload UI
						_previewLoading = false;
						Preview = FileViewModel.Error(e);
					}
				});

				// if loading takes longer than 10 ms, return pending viewmodel
				task.Wait(TimeSpan.FromMilliseconds(10));
				return _previewViewModel ?? FileViewModel.Pending;
			}
			protected set => _previewViewModel = value;
		}

		protected FileViewModel GetPreviewViewModel(object content) {
			switch(_previewContent) {
				case YukaScript script:
					return new ScriptFileViewModel(script);
				case YukaGraphic graphic:
					return new ImageFileViewModel(graphic);
				case string str:
					return new TextFileViewModel(str);
				case byte[] data:
					return new HexFileViewModel(data);
			}
			return FileViewModel.Dummy;
		}

		public ShellItemViewModel(FileSystem fs, string fullPath, ShellItemType type) {
			Type = type;
			FileSystem = fs;
			FullPath = fullPath;
			Name = Path.GetFileName(fullPath);
			Size = FileSystem.GetFileSize(fullPath);

			ExportCommand = new ActionCommand(Export);
			DeleteCommand = new ActionCommand(Delete);
		}

		public void Export() {
			Console.WriteLine("Exporting " + FullPath);
		}

		public void Delete() {
			Console.WriteLine("Deleting " + FullPath);
		}

		public string GetIconName() {
			switch(Type) {

				case ShellItemType.Directory:
					return IsExpanded ? "folder-open" : "folder";

				case ShellItemType.File:
					switch(Format) {
						case AniFormat _:
						case CsvFormat _:
						case TxtFormat _:
							return "file-text";

						case YkcFormat _:
							return "folder-archive";

						case BmpFormat _:
						case GnpFormat _:
						case PngFormat _:
						case YkgFormat _:
							return "file-image";

						case YkdFormat _:
						case YkiFormat _:
						case YksFormat _:
							return "file-script";

						case FrmFormat _:
							return "file-binary";

						default:
							return Path.GetExtension(Name).IsOneOf(".mp3", ".ogg") ? "file-audio" : "file";
					}

				case ShellItemType.Root:
					return "folder-archive";

				default:
					throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
			}
		}
	}

	public enum ShellItemType {
		Directory, File, Root
	}
}