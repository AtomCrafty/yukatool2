using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Yuka.Graphics;
using Yuka.Gui.Configuration;
using Yuka.Gui.Properties;
using Yuka.Gui.ViewModels.Data;
using Yuka.IO;
using Yuka.IO.Formats;
using Yuka.Script;
using Yuka.Util;

namespace Yuka.Gui.ViewModels {
	public class ShellItemViewModel : ViewModel {
		protected readonly FileSystem FileSystem;
		protected readonly FileSystemViewModel FileSystemViewModel;
		public readonly ShellItemViewModel Parent;

		public ShellItemType Type { get; set; }

		public string FullPath { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public ObservableCollection<ShellItemViewModel> Children { get; set; }

		public bool IsExpanded { get; set; } = true;

		private bool _isSelected;

		public bool IsSelected {
			get => _isSelected;
			set {
				_isSelected = value;
				if(_isSelected) return;

				if(!Config.Current.DeletePreviewOnItemDeselect) return;
				Log.Debug(string.Format(Resources.UI_DeletingPreviewOnDeselect, FullPath), Resources.Tag_UI);
				_previewViewModel = null;
			}
		}

		public ActionCommand DeleteCommand { get; protected set; }
		public ActionCommand ExportCommand { get; protected set; }
		public ActionCommand ExportRawCommand { get; protected set; }

		public string Icon => GetIconName();
		public Format Format => Type == ShellItemType.Directory ? null : Format.GuessFromFileName(Name);

		protected FileViewModel _previewViewModel;
		protected bool _previewLoading;

		public FileViewModel Preview {
			get {
				if(_previewViewModel != null) return _previewViewModel;
				if(_previewLoading) return FileViewModel.Pending;

				// handle special entries
				if(Type != ShellItemType.File) {
					return _previewViewModel = new DummyFileViewModel().WithAttribute("Children", Children.Count);
				}

				// don't even attempt to load a file that doesn't exist
				if(!FileSystem.FileExists(FullPath)) return FileViewModel.Dummy;

				// avoid spawning multiple tasks
				_previewLoading = true;
				var task = Task.Run(() => {
					try {
						// read file content
						object fileContent;
						if(Config.Current.AlwaysUseHexPreview) {
							using(var reader = FileSystem.OpenFile(FullPath).NewReader()) {
								fileContent = reader.ReadToEnd();
							}
						}
						else {
							fileContent = FileReader.DecodeObject(FullPath, FileSystem).Item1;
						}

						// send PropertyChanged update to reload UI
						Preview = GetPreviewViewModel(fileContent);
						_previewLoading = false;
					}
					catch(Exception e) {
						// send PropertyChanged update to reload UI
						Preview = FileViewModel.Error(e);
						_previewLoading = false;
						Log.Fail(string.Format(Resources.UI_PreviewFailed, e.GetType().Name, e.Message), Resources.Tag_UI);
						Log.Fail(e.StackTrace, Resources.Tag_UI);
					}
				});

				// if loading takes longer than 10 ms, return pending viewmodel
				task.Wait(TimeSpan.FromMilliseconds(10));
				return _previewViewModel ?? FileViewModel.Pending;
			}
			protected set => _previewViewModel = value;
		}

		public void DeletePreview() {
			Preview = null;
		}

		protected FileViewModel GetPreviewViewModel(object content) {
			switch(content) {
				case YukaScript script:
					return new ScriptFileViewModel(script);
				case YukaGraphic graphic:
					return new ImageFileViewModel(graphic);
				case string str:
					return new TextFileViewModel(str);
				case byte[] data when !Config.Current.NeverShowHexPreview:
					return new HexFileViewModel(data);
			}

			return FileViewModel.Dummy;
		}

		public ShellItemViewModel(FileSystemViewModel fs, ShellItemViewModel parent, string fullPath, ShellItemType type) {
			Type = type;
			FileSystemViewModel = fs;
			FileSystem = fs.FileSystem;
			Parent = parent;
			FullPath = type == ShellItemType.Root ? "" : fullPath;
			Name = Path.GetFileName(fullPath);
			Size = FileSystem.GetFileSize(fullPath);

			if(Type != ShellItemType.File) Children = new ObservableCollection<ShellItemViewModel>();

			DeleteCommand = new ActionCommand(Delete);
			ExportCommand = new ActionCommand(Export);
			ExportRawCommand = new ActionCommand(ExportRaw);
		}

		public void Delete() => FileSystemViewModel.DeleteNode(this);
		public void Export() => FileSystemViewModel.ExportFileOrFolder(this, true);
		public void ExportRaw() => FileSystemViewModel.ExportFileOrFolder(this, false);

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

		public string DropTargetPath => Type == ShellItemType.File ? Path.GetDirectoryName(FullPath) : Type == ShellItemType.Root ? "" : FullPath;

		public void ImportPaths(string[] paths, bool convert) => FileSystemViewModel.ImportPaths(paths, DropTargetPath, convert);
	}

	public enum ShellItemType {
		Directory,
		File,
		Root
	}
}