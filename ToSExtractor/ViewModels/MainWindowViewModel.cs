using System.ComponentModel;
using System.Windows.Input;
using ToSExtractor.Models;
using ToSExtractor.ViewModels.Command;

namespace ToSExtractor.ViewModels {
	class MainWindowViewModel:INotifyPropertyChanged {


		public event PropertyChangedEventHandler PropertyChanged;

		private ICommand _fileOutput;
		public ICommand FileOutput {
			get {
				return this._fileOutput ?? ( this._fileOutput = new FileOutputCommand( this ) );
			}
		}

		private ICommand _settingsSave;
		public ICommand SettingsSave {
			get {
				return this._settingsSave ?? ( this._settingsSave = new SettingsSaveCommand( this ) );
			}
		}

		private ICommand _settingsLoad;
		public ICommand SettingsLoad {
			get {
				return this._settingsLoad ?? ( this._settingsLoad = new SettingsLoadCommand( this ) );
			}
		}

		#region IpfFileModel 変更通知プロパティ

		private IpfFileModel _ipfFile;
		public IpfFileModel IpfFile {
			get {
				return this._ipfFile;
			}
			set {
				this._ipfFile = value;
				OnPropertyChanged( nameof( this.IpfFile ) );
			}
		}

		private AppearanceModel _appearance;
		public AppearanceModel Appearance{
			get {
				return this._appearance;
			}
			set {
				this._appearance = value;
				OnPropertyChanged( nameof( this.Appearance ) );
			}
		}

		#endregion

		public MainWindowViewModel() {
			this.IpfFile = new IpfFileModel();
			this.Appearance = new AppearanceModel();
		}

		private void OnPropertyChanged( string name ) {
			var handler = this.PropertyChanged;
			handler?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
