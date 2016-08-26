using System;
using System.Windows.Data;

namespace ToSExtractor.Views.Converters {
	class FileSizeConverter :IValueConverter {
		private static readonly string[] Suffix = { "", "K", "M", "G", "T" };
		private const double UNIT = 1024;

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {

			var size = (double)(uint)value;
			var i = 0;
			for(; i < Suffix.Length - 1; i++ ) {
				if( size < UNIT ) {
					break;
				}
				size /= UNIT;
			}

			return $"{size:0} {Suffix[i]}B";
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}

	}
}
