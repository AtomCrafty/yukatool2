﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Yuka.IO;

namespace Yuka.Gui.Files {
	/// <summary>
	/// Interaction logic for ArchiveTab.xaml
	/// </summary>
	public partial class FilesTab {

		public FilesTab() {
			InitializeComponent();
		}

		private void BtnOpenArchive_OnClick(object sender, RoutedEventArgs e) {
			//FileView.LoadArchive(@"S:\Games\Visual Novels\AkaSaka\data01.ykc");
		}
	}
}