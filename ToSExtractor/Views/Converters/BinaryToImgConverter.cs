using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ToSExtractor.Models;

namespace ToSExtractor.Views.Converters {
	class BinaryToImgConverter :IValueConverter {
		private static readonly ImageConverter Imgconv = new ImageConverter();
		object IValueConverter.Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			var fd = (IpfFileModel.InnerFile)value;
			if( fd?.InnerFileType != IpfFileModel.InnerFile.FileType.Image ) {
				return null;
			}
			var img = (Bitmap)Imgconv.ConvertFrom( fd.BinaryData );
			if( img != null ) {
				return Imaging.CreateBitmapSourceFromHBitmap( img.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions() );
			}
			return null;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
