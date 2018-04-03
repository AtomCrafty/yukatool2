﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Yuka.Graphics;
using Yuka.Gui.Converters;
using Yuka.Gui.ViewModels.Data;
using Yuka.IO;
using Yuka.Script;

namespace Yuka.Gui.ViewModels {
	public class FileSystemEntryViewModel : ViewModel {

		protected readonly FileSystem FileSystem;

		public FileSystemEntryType Type { get; set; }

		public string FullPath { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public ObservableCollection<FileSystemEntryViewModel> Children { get; } = new ObservableCollection<FileSystemEntryViewModel>();

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

		public string Icon => FileSystemEntryToImageConverter.GetIconNameForFileSystemEntry(Type, Name, IsExpanded);
		public Format Format => Type == FileSystemEntryType.Directory ? null : Format.GuessFromFileName(Name);

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
					_previewContent = _previewContent ?? FileReader.DecodeObject(FullPath, FileSystem).Item1;
					// send PropertyChanged update to reload UI
					Preview = _previewViewModel ?? GetPreviewViewModel(_previewContent);
					_previewLoading = false;
				});

				// if loading takes longer than 10 ms, return pending viewmodel
				task.Wait(TimeSpan.FromMilliseconds(10));
				if(_previewContent == null) return FileViewModel.Pending;
				return _previewViewModel;
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
			}
			return FileViewModel.Dummy;
		}

		public FileSystemEntryViewModel(FileSystem fs, string fullPath, FileSystemEntryType type) {
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
	}

	public enum FileSystemEntryType {
		Directory, File, Root
	}
}