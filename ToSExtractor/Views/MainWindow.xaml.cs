using System.Windows;
using ToSExtractor.ViewModels;

namespace ToSExtractor.Views {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded( object sender, RoutedEventArgs e ) {
			( (MainWindowViewModel)this.DataContext ).SettingsLoad.Execute( null );
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
			( (MainWindowViewModel)this.DataContext ).SettingsSave.Execute( null );
		}
	}
}
