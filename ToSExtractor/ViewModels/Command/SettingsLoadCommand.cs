using System;
using System.Windows.Input;

namespace ToSExtractor.ViewModels.Command {
	class SettingsLoadCommand:ICommand {
		private readonly MainWindowViewModel _vm;

		public SettingsLoadCommand( MainWindowViewModel viewmodel ) {
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
			this._vm.IpfFile.DirectoryPath = Properties.Settings.Default.DirectoryPath;
			this._vm.Appearance.WindowHeight = Properties.Settings.Default.WindowSize.Height;
			this._vm.Appearance.WindowWidth = Properties.Settings.Default.WindowSize.Width;
		}
	}
}
