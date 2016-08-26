using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToSExtractor.Models {
	/// <summary>
	/// ipfファイルクラス
	/// </summary>
	class IpfFileModel :INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;


		#region Taskたち

		/// <summary>
		/// ファイルリスト作成タスク
		/// </summary>
		private Task _createFileListTask;

		/// <summary>
		/// 内部ファイルリスト作成タスク
		/// </summary>
		private Task _createInnerFileListTask;

		/// <summary>
		/// 内部ファイルバイナリデータ作成タスク
		/// </summary>
		private Task _createInnerFileData;

		#endregion

		private string _selectedFileName;
		/// <summary>
		/// 選択中のファイル名
		/// </summary>
		public string SelectedFileName {
			private get {
				return this._selectedFileName;
			}
			set {
				this._selectedFileName = value;
				CreateInnerFileList();
			}
		}

		private int _selectedInnerFileIndex = -1;
		/// <summary>
		/// 選択中の内部ファイルインデックス
		/// </summary>
		public int SelectedInnerFileIndex {
			private get {
				return this._selectedInnerFileIndex;
			}
			set {
				this._selectedInnerFileIndex = value;
				CreateInnerFileData();
			}
		}



		#region DirectoryPath 変更通知プロパティ
		private string _directoryPath;
		public string DirectoryPath {
			get {
				return this._directoryPath;
			}
			set {
				this._directoryPath = value;
				CreateFileList();
				OnPropertyChanged( nameof( this.DirectoryPath ) );
			}
		}
		#endregion
		#region FileList 変更通知プロパティ

		private string[] _fileList = Array.Empty<string>();
		public string[] FileList {
			get {
				return this._fileList;
			}
			private set {
				this._fileList = value;
				OnPropertyChanged( nameof( this.FileList ) );
			}
		}
		#endregion

		#region InnerFileList 変更通知プロパティ

		private InnerFileInfo[] _innerFileList = Array.Empty<InnerFileInfo>();
		public InnerFileInfo[] InnerFileList {
			get {
				return this._innerFileList;
			}
			private set {
				this._innerFileList = value;
				OnPropertyChanged( nameof( this.InnerFileList ) );
			}
		}

		#endregion

		#region InnerFileData 変更通知プロパティ

		private InnerFile _innerFileData = new InnerFile(InnerFile.FileType.None,null);

		public InnerFile InnerFileData {
			get {
				return this._innerFileData;
			}
			private set {
				this._innerFileData = value;
				OnPropertyChanged( nameof( this.InnerFileData ) );
			}
		}

		#endregion

		/// <summary>
		/// ディレクトリ内ファイルリストを作成し、FileListに格納する。
		/// </summary>
		private async void CreateFileList() {
			this._createFileListTask = Task.Run( () => {
				lock( this.FileList ) {
					if( !Directory.Exists( this.DirectoryPath) ) {
						//ディレクトリが存在しない場合、リストを空にする。
						this.FileList = Array.Empty<string>();
						return;
					}
					this.FileList = Directory.GetFiles( this.DirectoryPath ).Where( x => Regex.IsMatch( x, @"\.ipf$" ) ).Select( Path.GetFileName ).ToArray();
				}
			} );

			await this._createFileListTask;
		}


		/// <summary>
		/// ファイル内の内部ファイル情報一覧を作成し、InnerFileListに格納する。
		/// </summary>
		private async void CreateInnerFileList() {
			this._createInnerFileListTask = Task.Run( () => {
				lock( this.InnerFileList ) {

					if( this.SelectedFileName == null ) {
						//何も選択されていない場合、リストを空にする。
						this.InnerFileList = Array.Empty<InnerFileInfo>();
						return;
					}
					try {
						using( var fs = File.OpenRead( Path.Combine( this.DirectoryPath, this.SelectedFileName ) ) )
						using( var br = new BinaryReader( fs ) ) {
							fs.Seek( -24, SeekOrigin.End );
							var filenumb = br.ReadUInt16();
							var tableOffset = br.ReadInt32();
							br.ReadUInt16();

							fs.Seek( tableOffset, SeekOrigin.Begin );

							var tmpIpfFiles = new List<InnerFileInfo>();
							for( var i = 0; i < filenumb; i++ ) {
								var nsize = br.ReadUInt16();
								var crc = br.ReadUInt32();
								var zsize = br.ReadUInt32();
								var size = br.ReadUInt32();
								var offset = br.ReadUInt32();
								var csize = br.ReadUInt16();
								var comment = Encoding.UTF8.GetString( br.ReadBytes( csize ) );
								var name = Encoding.UTF8.GetString( br.ReadBytes( nsize ) );
								var innerFile = new InnerFileInfo( nsize, crc, zsize, size, offset, csize, comment, name );
								tmpIpfFiles.Add( innerFile );
							}
							this.InnerFileList = tmpIpfFiles.ToArray();
						}
					} catch( IOException ) {
						//ファイルロック中
						this.InnerFileList = Array.Empty<InnerFileInfo>();
						return;
					}
				}
			} );
			await this._createInnerFileListTask;
		}

		/// <summary>
		/// ファイル内の内部ファイルからバイナリデータを作成し、InnerFileDataに格納する。
		/// </summary>
		private async void CreateInnerFileData() {
			this._createInnerFileData = Task.Run( () => {
				lock( this.InnerFileData ) {
					if( this.SelectedInnerFileIndex < 0 ) {
						//何も選択されていない場合、ファイルデータを空にする。
						this.InnerFileData = new InnerFile( InnerFile.FileType.None, null );
						return;
					}
					var fileInfo = this.InnerFileList[this.SelectedInnerFileIndex];
					byte[] buff;

					//InnerFileData作成
					try {
						using( var fs = File.OpenRead( Path.Combine( this.DirectoryPath, this.SelectedFileName ) ) )
						using( var br = new BinaryReader( fs ) ) {
							fs.Seek( fileInfo.Offset, SeekOrigin.Begin );
							buff = br.ReadBytes( (int)fileInfo.Zsize );
							if( fileInfo.Zsize != fileInfo.Size ) {
								var ipfd = new IpfDecrypter();
								buff = ipfd.Decrypt( buff );
								buff = new Func<byte[], byte[]>( source => {
									var result = new byte[fileInfo.Size];
									using( var ms = new MemoryStream( source ) )
									using( var ds = new DeflateStream( ms, CompressionMode.Decompress ) ) {
										ds.Read( result, 0, (int)fileInfo.Size );
										ms.Close();
										ds.Close();
									}
									return result;
								} )( buff );
							}
						}
					} catch( IOException ) {
						//ファイルロック中
						this.InnerFileData = new InnerFile( InnerFile.FileType.None, null );
						return;
					}
					//FileType作成
					InnerFile.FileType fileType;
					if( Regex.IsMatch( fileInfo.Name, @"\.(jpg|png|bmp|gif)$" ) ) {
						fileType = InnerFile.FileType.Image;
					} else if( Regex.IsMatch( fileInfo.Name, @"\.(xml|lun|skn|effect|lua)$" ) ) {
						fileType = InnerFile.FileType.Text;
					} else if( Regex.IsMatch( fileInfo.Name, @"\.(ttf)$" ) ) {
						fileType = InnerFile.FileType.Font;
					} else {
						fileType = InnerFile.FileType.Other;
					}

					this.InnerFileData = new InnerFile( fileType, buff );
				}
			} );

			await this._createInnerFileData;
		}

		/// <summary>
		/// 選択中の内部ファイルを出力する
		/// </summary>
		/// <param name="dir">出力先ディレクトリ</param>
		public async void SelectedInnerFileOutput(string dir) {
			var filePath = Path.Combine( dir, this.InnerFileList[this.SelectedInnerFileIndex].Name );
			var dirPath = Path.GetDirectoryName( filePath );
			if( string.IsNullOrEmpty( dirPath ) ) {
				return;
			}
			if( !Directory.Exists( dirPath ) ) {
				Directory.CreateDirectory( dirPath );
			}
			using( var fs = File.OpenWrite( filePath ) ) {
				await fs.WriteAsync( this.InnerFileData.BinaryData, 0, this.InnerFileData.BinaryData.Length );
			}
		}


		private void OnPropertyChanged( string name ) {
			var handler = this.PropertyChanged;
			handler?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		/// <summary>
		/// 内部ファイル復号化
		/// </summary>
		private class IpfDecrypter {

			private readonly long[] _crctable = new long[256];

			private long _key0 = 305419896;
			private long _key1 = 591751049;
			private long _key2 = 878082192;

			public IpfDecrypter() {
				for( var i = 0; i < 256; i++ ) {
					long crc = i;
					for( var j = 0; j < 8; j++ ) {
						if( ( crc & 0x01 ) == 0x01 ) {
							crc = ( ( crc >> 1 ) & 0x7fffffff ) ^ 0xedb88320;
						} else {
							crc = ( ( crc >> 1 ) & 0x7fffffff );
						}
					}
					this._crctable[i] = crc;
				}
				foreach( var c in "ofO1a0ueXA? [\xFFs h %?" ) {
					UpdateKeys( c );
				}
			}

			//キー更新関数定義
			private void UpdateKeys( char ch ) {
				//crc計算関数定義
				Func<char, long, long> crc32 = ( c, crc ) => ( ( crc >> 8 ) & 0xffffff ) ^ this._crctable[( ( crc ^ c ) & 0xff )];
				this._key0 = crc32( ch, this._key0 );
				this._key1 = ( this._key1 + ( this._key0 & 0xFF ) ) & 0xFFFFFFFF;
				this._key1 = ( this._key1 * 134775813 + 1 ) & 0xFFFFFFFF;
				this._key2 = crc32( (char)( ( ( this._key1 >> 24 ) % 256 ) & 0xff ), this._key2 );
			}

			//復号化
			public byte[] Decrypt (IEnumerable<byte> buff) {
				return buff.Select( ( t, i ) => i % 2 != 0 ? t : new Func<byte, byte>( c => {
					var k = this._key2 | 2;
					var x = c ^ ( ( ( k * ( k ^ 1 ) ) >> 8 ) & 0xff );
					c = (byte)x;
					UpdateKeys( (char)c );
					return c;
				} )( t ) ).ToArray();
			}
		}

		/// <summary>
		/// 内部ファイルバイナリデータ
		/// </summary>
		public class InnerFile {

			/// <summary>
			/// ファイルタイプ
			/// </summary>
			public enum FileType {
				/// <summary>
				/// なし
				/// </summary>
				None,
				/// <summary>
				/// 画像
				/// </summary>
				Image,
				/// <summary>
				/// テキスト
				/// </summary>
				Text,
				/// <summary>
				/// フォント
				/// </summary>
				Font,
				/// <summary>
				/// その他
				/// </summary>
				Other
			}

			public FileType InnerFileType {
				get;
			}

			public byte[] BinaryData {
				get;
			}

			public InnerFile( FileType fileType, byte[] fileData ) {
				this.InnerFileType = fileType;
				this.BinaryData = fileData;
			}

		}

		/// <summary>
		/// .ipfファイル内部ファイル
		/// </summary>
		public class InnerFileInfo {

			/// <summary>
			/// Nameサイズ
			/// </summary>
			public ushort Nsize {
				get;
			}

			/// <summary>
			/// CRC
			/// </summary>
			public uint Crc {
				get;
			}

			/// <summary>
			/// 圧縮後サイズ
			/// </summary>
			public uint Zsize {
				get;
			}

			/// <summary>
			/// 圧縮前サイズ
			/// </summary>
			public uint Size {
				get;
			}

			/// <summary>
			/// データ位置
			/// </summary>
			public uint Offset {
				get;
			}

			/// <summary>
			/// Commentサイズ
			/// </summary>
			public ushort Csize {
				get;
			}

			/// <summary>
			/// コメント(内部ファイル名?)
			/// </summary>
			public string Comment {
				get;
			}

			/// <summary>
			/// ファイル名
			/// </summary>
			public string Name {
				get;
			}

			public InnerFileInfo( ushort nsize, uint crc, uint zsize, uint size, uint offset, ushort csize, string comment, string name ) {
				this.Nsize = nsize;
				this.Crc = crc;
				this.Zsize = zsize;
				this.Size = size;
				this.Offset = offset;
				this.Csize = csize;
				this.Comment = comment;
				this.Name = name;
			}
		}

	}
}