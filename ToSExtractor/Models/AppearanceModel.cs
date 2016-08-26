using System.ComponentModel;
using System.Drawing;

namespace ToSExtractor.Models {
	class AppearanceModel :INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		private int _windowHeight;
		public int WindowHeight {
			get {
				return this._windowHeight;
			}
			set {
				this._windowHeight = value;
				OnPropertyChanged( nameof( this.WindowHeight ) );
			}
		}
		private int _windowWidth;
		public int WindowWidth {
			get {
				return this._windowWidth;
			}
			set {
				this._windowWidth = value;
				OnPropertyChanged( nameof( this.WindowWidth ) );
			}
		}

		private void OnPropertyChanged( string name ) {
			var handler = this.PropertyChanged;
			handler?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}
	}
}
