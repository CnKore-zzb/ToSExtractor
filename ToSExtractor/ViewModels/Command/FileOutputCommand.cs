using System;
using System.Windows.Input;

namespace ToSExtractor.ViewModels.Command {
	class FileOutputCommand:ICommand {
		private readonly MainWindowViewModel _vm;

		public FileOutputCommand( MainWindowViewModel viewmodel ) {
			this._vm = viewmodel;
		}

		public bool CanExecute( object parameter ) {
			return this._vm.IpfFile.InnerFileData.BinaryData != null;
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
			this._vm.IpfFile.SelectedInnerFileOutput( Environment.CurrentDirectory );
		}
	}
}
