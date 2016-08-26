using System;
using System.Windows;
using System.Windows.Data;
using ToSExtractor.Models;

namespace ToSExtractor.Views.Converters {
	class FileTypeToImageVisible :IValueConverter {
		object IValueConverter.Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			var innerFile = (IpfFileModel.InnerFile)value;
			if( innerFile?.InnerFileType == IpfFileModel.InnerFile.FileType.Image ) {
				return Visibility.Visible;
			} else {
				return Visibility.Hidden;
			}
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}

	class FileTypeToTextVisible :IValueConverter {
		object IValueConverter.Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			var innerFile = (IpfFileModel.InnerFile)value;
			if( innerFile?.InnerFileType == IpfFileModel.InnerFile.FileType.Text ) {
				return Visibility.Visible;
			} else {
				return Visibility.Hidden;
			}
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
