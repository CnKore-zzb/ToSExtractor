using System;
using System.Drawing;
using System.Windows.Input;

namespace ToSExtractor.ViewModels.Command {
	class SettingsSaveCommand:ICommand {
		private readonly MainWindowViewModel _vm;

		public SettingsSaveCommand( MainWindowViewModel viewmodel ) {
			this._vm = viewmodel;
		}

		public bool CanExecute( object parameter ) {
			return true;
		}

		public event EventHandler CanExecuteChanged {
			add {
				CommandManager.RequerySuggested += value;
			}
			remove {
				CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute( object parameter ) {
			Properties.Settings.Default.DirectoryPath = this._vm.IpfFile.DirectoryPath;
			Properties.Settings.Default.WindowSize = new Size( this._vm.Appearance.WindowWidth, this._vm.Appearance.WindowHeight);
			Properties.Settings.Default.Save();
		}
	}
}
