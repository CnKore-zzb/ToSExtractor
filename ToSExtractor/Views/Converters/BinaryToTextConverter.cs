using System;
using System.Windows.Data;
using System.Windows.Documents;
using ToSExtractor.Models;

namespace ToSExtractor.Views.Converters {
	class BinaryToTextConverter :IValueConverter {
		object IValueConverter.Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			var fd = (IpfFileModel.InnerFile)value;
			if( fd?.InnerFileType != IpfFileModel.InnerFile.FileType.Text ) {
				return null;
			}
			var b = fd.BinaryData ?? Array.Empty<byte>();

			var result = new FlowDocument();
			result.Blocks.Add( new Paragraph( new Run( System.Text.Encoding.UTF8.GetString( b ) )));
			return result;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
