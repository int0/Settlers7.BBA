using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Threading;

using zLib.Checksums;
using Compressor;
using IconHelper;
using System.IO.Compression;


namespace Settlers6.BBA
{
	public partial class Form1 : Form
    {
        #region Fields

        #region Constants
        string[] ExtCmp = { ".fx", ".anm", ".dff", ".lua", ".xml", ".spt", ".dds", ".fxa", ".ttf", ".bmp" };
		byte[] c_bbaHead =
        {
            0x42, 0x41, 0x46, 0x06, 0x07, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x89, 0x83, 0x1A, 0x6D
        };
 
        uint s7_bba_head_sig       = 0x06464142;
        uint s7_bba_head_magic     = 0x6d1a8389;
        uint s7_bba_header_magic   = 0x7d7f7696;
        uint s7_bba_header_magic2  = 0xe225d297;
        
        uint s7_bba_magic1         = 0xB57D7B42;
        
		#endregion

		#region Enums
        [Flags]
        enum BBAFlags
        {
            Plain = 0,
            EncryptedHeader = 0x1,
            EncryptedEntireContent = 0x2,
            Crypted = 0x7,
            Gziped = 0x10,
            Directory = 0x0100,
        }
		#endregion

		#region Struct
		struct BBAHead
		{
			public string Header;
			public int Version;
			public int HeaderSize;
			public int Magic;

		}

		struct BBAHeader
		{
			public long IndexTableOffset;
			public int IndexTableSize;
			public int IndexTableCRC32;
			public uint Magic;
            public uint Magic2;
		}
		struct BBA
		{
			public long Time;
			public int UnpackedSize;
			public int UnpackedCRC32;
			public int Flags;
			public int Unknown1;
			public long Offset;
			public int PackedSize;
			public int PackedCRC32;
			public int Unknown2;
			public int Unknown3;
			public int PathSize;
			public int SizeOfPathToRootDir;
			public int IsFile; // 0 / -1 - False/True
			public int OffsetToNextFileName;
			public string FileName;
			//Internal
			public long BBAEntryPositionInTable;
		}
		struct BBAIndexTableHeader
		{
			public int OffsetToBBAHeadInTable;
			public int IndexTableOffset;
			public int HashTableOffset;
		}
		#endregion

		#region Members
		ArrayList m_pakList;
		FileStream m_fs = null;
		BBAHead m_head;
		BBAHeader m_header;
		BBAIndexTableHeader m_tablehead;
		byte[] b_head;
		byte[] b_header;
		MemoryStream m_IndexTable;
		#endregion

		#endregion

		#region Constuctor
		public Form1()
		{
			InitializeComponent();
			IconHelperInit();
            comboBox1.SelectedIndex = 0;
		}
		#endregion

		#region Icon Helper members
		private ImageList _smallImageList = new ImageList();
		private IconHelper.IconListManager _iconListManager;
		#endregion

		#region Icon Helper methods
		private void IconHelperInit()
		{
			_smallImageList.ColorDepth = ColorDepth.Depth32Bit;
			_smallImageList.ImageSize = new System.Drawing.Size( 16, 16 );
			_iconListManager = new IconListManager( _smallImageList, IconReader.IconSize.Small );
			treeView1.ImageList = _smallImageList;
			treeView1.ImageList.Images.Add( IconReader.GetFolderIcon( IconReader.IconSize.Small, IconReader.FolderType.Open ) );
			treeView1.ImageList.Images.Add( IconReader.GetFolderIcon( IconReader.IconSize.Small, IconReader.FolderType.Closed ) );
		}
		#endregion

		#region Form Event Handlers
		/// <summary>
		/// Open PAK
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOpen_Click( object sender, EventArgs e )
		{
			if( openFileDialog1.ShowDialog() == DialogResult.OK )
			{
				string path = openFileDialog1.FileName;

				InitMembers();

				m_fs = OpenBBAFile( path );

				if( m_fs != null )
				{
					ReadFAT( m_fs );

					TreeNode root = new TreeNode( path );
					root.ImageIndex = 1;
					root.SelectedImageIndex = 0;
					treeView1.Nodes.Add( root );

					int idx = listBox1.Items.Add( "Filling tree..." );
					listBox1.Refresh();
					FillTree( root );
					listBox1.Items[ idx ] += "OK";
				}
			}
		}

		/// <summary>
		/// Close PAK
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClose_Click( object sender, EventArgs e )
		{
			InitMembers();
		}

		/// <summary>
		/// Save All 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button4_Click( object sender, EventArgs e )
		{
			if( m_fs != null )
			{
				if( folderBrowserDialog1.ShowDialog() == DialogResult.OK )
				{
					string path = folderBrowserDialog1.SelectedPath;
					SaveAllItems(path);
					//ThreadPool.QueueUserWorkItem( new WaitCallback( SaveAllItems ), path );
				}
			}
		}

		/// <summary>
		/// Make
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button3_Click( object sender, EventArgs e )
		{
			folderBrowserDialog1.SelectedPath = Directory.GetCurrentDirectory();
			if( folderBrowserDialog1.ShowDialog() == DialogResult.OK )
			{
				if( saveFileDialog1.ShowDialog() == DialogResult.OK )
				{
					InitMembers();
					BuildBBA(null);
					
					//ThreadPool.QueueUserWorkItem( new WaitCallback( BuildBBA ) );
				}
			}
		}
		#endregion

		#region Helper Methods
		private void InitMembers()
		{
			m_head = new BBAHead();
			m_header = new BBAHeader();
			m_tablehead = new BBAIndexTableHeader();
			
			if( m_fs != null )
			{
				m_fs.Close();
				m_fs = null;
			}

			if( m_pakList != null )
			{
				if( m_pakList.Count > 0 )
					m_pakList.Clear();
			}
			else
			{
				m_pakList = new ArrayList();
			}

			if( listBox1.Items.Count > 0 )
				listBox1.Items.Clear();

			if( treeView1.Nodes.Count > 0 )
				treeView1.Nodes.Clear();

			progressBar1.Value = 0;

			if( m_IndexTable != null )
			{
				m_IndexTable.Dispose();
				m_IndexTable.Close();
				m_IndexTable = null;
			}
			
			b_head = new byte[ 0x10 ];
			b_header = new byte[ 0x40 ];

		}
		private FileStream OpenBBAFile( string path )
		{
			FileStream f = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
			f.Position = 0;
			f.Read( b_head, 0, b_head.Length );

			//Read header.
			int h = BitConverter.ToInt32( b_head, 0 );

            if ( h == s7_bba_head_sig )
            {
                BinaryReader reader = new BinaryReader( f );

                //Add: header reading code.
                //Fill: PAKHead....

                m_head.Header = Encoding.Default.GetString( b_head, 0, 4 );
                m_head.Version = BitConverter.ToInt32( b_head, 4 );
                m_head.HeaderSize = BitConverter.ToInt32( b_head, 8 );
                m_head.Magic = BitConverter.ToInt32( b_head, 12 );

                if ( m_head.Magic == s7_bba_head_magic )
                {
                    listBox1.Items.Add( "BBA container opened." );

                    f.Read( b_header, 0, b_header.Length );

                    BBACrypt.DecryptHeader( b_header, b_header.Length, b_head, b_head.Length );

                    m_header.IndexTableOffset = BitConverter.ToInt64( b_header, 0 );
                    m_header.IndexTableSize = BitConverter.ToInt32( b_header, 8 );
                    m_header.IndexTableCRC32 = BitConverter.ToInt32( b_header, 12 );
                    m_header.Magic = BitConverter.ToUInt32( b_header, 16 );
                    m_header.Magic2 = BitConverter.ToUInt32( b_header, 20 );

                    if( m_header.Magic == s7_bba_header_magic &&
                        m_header.Magic2 == s7_bba_header_magic2 )
                    {
                        return f;
                    }
                    else
                    {
                        listBox1.Items.Add( String.Format( "BBA container contains bad magic value: {0:X} {1:X}", m_header.Magic, m_header.Magic2 ) );
                    }
                }
                else
                {
                    listBox1.Items.Add( String.Format( "BBA container contains bad magic value: {0:X}", m_head.Magic ) );
                }
            }
            else
            {
                listBox1.Items.Add( "Not Settlers 7 .BBA file" );
            }

			f.Close();
			f.Dispose();
			return null;
		}
		private int GetStringLength( byte[] str, int startIndex )
		{
			if( startIndex > str.Length )
				return 0;

			for( int i = startIndex; i < str.Length; i++ )
			{
				if( str[ i ] == 0 )
					return i - startIndex;
			}
			return 0;
		}
		private void ReadFAT( FileStream f )
		{

			//Prepare FAT reader.
			f.Position = m_header.IndexTableOffset;
			byte[] table = new byte[ m_header.IndexTableSize ];
			f.Read( table, 0, table.Length );
			BBACrypt.DecryptIndexTable( table, table.Length, null, 0 );

			//MemoryStream input = new MemoryStream( table );
            m_IndexTable = new MemoryStream(table);
			//Compression.Decompress( input, 8, m_IndexTable, true );

			//input.Dispose();
			//input.Close();

			//table = m_IndexTable.GetBuffer();

            FileStream fs = File.OpenWrite( openFileDialog1.FileName + ".idx" );

            fs.Write(table, 0, table.Length);
            fs.Close();

			m_tablehead.OffsetToBBAHeadInTable = BitConverter.ToInt32( table, 0 );
			m_tablehead.IndexTableOffset = BitConverter.ToInt32( table, 4 );
			m_tablehead.HashTableOffset = BitConverter.ToInt32( table, 8 );

			m_IndexTable.Position = m_tablehead.IndexTableOffset + 4;
			BinaryReader reader = new BinaryReader( m_IndexTable, Encoding.Default );

			int count = BitConverter.ToInt32( table, m_tablehead.IndexTableOffset );
            long TableBegin = m_IndexTable.Position;

			for( int i = 0; i < count; i++ )
			{
				BBA bba = new BBA();
				bba.BBAEntryPositionInTable = m_IndexTable.Position;
				bba.Time = reader.ReadInt64();
				bba.UnpackedSize = reader.ReadInt32();
				bba.UnpackedCRC32 = reader.ReadInt32();
				bba.Flags = reader.ReadInt32();
				bba.Unknown1 = reader.ReadInt32();
				bba.Offset = reader.ReadInt64();
				bba.PackedSize = reader.ReadInt32();
				bba.PackedCRC32 = reader.ReadInt32();
				bba.Unknown2 = reader.ReadInt32();
				bba.Unknown3 = reader.ReadInt32();
				bba.PathSize = reader.ReadInt32();
				bba.SizeOfPathToRootDir = reader.ReadInt32();
				bba.IsFile = reader.ReadInt32();
				bba.OffsetToNextFileName = reader.ReadInt32();
				byte[] filename = reader.ReadBytes( bba.PathSize );
				bba.FileName = Encoding.Default.GetString( filename );

				//if( ( bba.Unknown1 + bba.Unknown2 + bba.Unknown3 ) != 0 )
				//{
				//    listBox1.Items.Add("u1: " + bba.Unknown1.ToString() + "u2: " + bba.Unknown2.ToString() + "u3: " + bba.Unknown3.ToString());                
				//}

				if( bba.IsFile == -1 )
				{
					m_pakList.Add( bba );
				}
				//else
				//{
				//    m_dirList.Add(bba);
				//}

                int pos = (int)m_IndexTable.Position + 1;
                int x = (pos % 4);
                pos = (x == 0) ? 0 : 4 - x;
                m_IndexTable.Position += pos + 1;

                //if (bba.OffsetToNextFileName == -1)
                //    break;

                //m_IndexTable.Position = pos;//TableBegin + bba.OffsetToNextFileName;
			}
			listBox1.Items.Add( "Files found: " + m_pakList.Count.ToString() );
			m_IndexTable.Position = 0;
		}
		private void FillTree( TreeNode root )
		{
			int count = m_pakList.Count;

			progressBar1.Maximum = count;
			progressBar1.Step = 1;
			progressBar1.Value = 0;

			for( int i = 0; i < count; i++ )
			{
				BBA bba = ( BBA )m_pakList[ i ];

				//if (bba.IsFile == -1)
				AddFileToTree( root, bba.FileName, bba );

				progressBar1.PerformStep();
			}
		}
		private void AddFileToTree( TreeNode rootNode, string path, BBA bba )
		{
			int i = path.IndexOf( "\\" );

			if( i != -1 )
			{

				string item = path.Remove( i );
				int idx = rootNode.Nodes.IndexOfKey( item );
				string nextItem = path.Substring( item.Length + 1 );

				if( idx != -1 )
				{
					AddFileToTree( rootNode.Nodes[ idx ], nextItem, bba );
				}
				else
				{
					TreeNode node = rootNode.Nodes.Add( item );
					node.Name = item;
					node.SelectedImageIndex = 0;
					node.ImageIndex = 1;
					AddFileToTree( node, nextItem, bba );
				}
			}
			else
			{
				TreeNode node = rootNode.Nodes.Add( path );
				int iconIndex = _iconListManager.AddFileIcon( path );

				node.ImageIndex = iconIndex;
				node.SelectedImageIndex = iconIndex;
				node.Name = path;

				string time = DateTime.FromFileTime( bba.Time ).ToString();
				string unk1 = string.Format( "{0:X}", bba.Unknown1 );
				string unk2 = string.Format( "{0:X}", bba.Unknown2 );
				string unk3 = string.Format( "{0:X}", bba.Unknown3 );
				string pcrc = string.Format( "{0:X}", bba.PackedCRC32 );
				string ucrc = string.Format( "{0:X}", bba.UnpackedCRC32 );
                string tip = "File name: " + bba.FileName +
					"\nOffset: " + bba.Offset.ToString() +
					"\nPacked size: " + bba.PackedSize.ToString() +
					"\nUnpacked size: " + bba.UnpackedSize.ToString() +
					"\nFlags: " + bba.Flags.ToString() +
					"\nPackedCRC32: " + pcrc +
					"\nUnpackedCRC32: " + ucrc +
					"\nisFile: " + bba.IsFile.ToString() +					
					"\nunk1: " + unk1 +
					"\nunk2: " + unk2 +
					"\nunk3: " + unk3 +
					"\nFile time: " + time + 
                    "\nOffsetToNextFile:" + String.Format( "{0:X}", bba.OffsetToNextFileName ) +
                    "\nSizeOfPathToRoot: " + String.Format( "{0:X}", bba.SizeOfPathToRootDir );
                    

				node.ToolTipText = tip;
			}
		}
		private FileStream CreateFile( string path, string file )
		{
			int idx = file.LastIndexOf( "\\" );
			if( idx == -1 )
			{
				return File.Open( path + @"\" + file, FileMode.Create, FileAccess.ReadWrite );
			}
			string dir = file.Substring( 0, idx );
			string filename = file.Substring( idx + 1 );
			string fileDir = path + @"\" + dir + @"\";


			if( !( Directory.Exists( fileDir ) ) )
				Directory.CreateDirectory( fileDir );

			FileStream f = File.Open( fileDir + filename, FileMode.Create, FileAccess.ReadWrite );

			return f;
		}
        private bool SaveFile(BBA bba, string outDir)
        {
            FileStream f = null;
            MemoryStream fStream = null;
            string FileName = bba.FileName;
            FileStream pakFile = m_fs;
            
            pakFile.Position = bba.Offset;

            int BytesToWrite = bba.UnpackedSize;
            int BytesToRead = (bba.PackedSize % 4);
            BytesToRead = (BytesToRead == 0) ? bba.PackedSize : (4 - BytesToRead) + bba.PackedSize;
            int BytesRead = 0;

            try
            {             
                f = CreateFile(outDir, FileName);
                if (f == null) return false;

                if (bba.UnpackedSize == 0 )
                { f.Close(); return true; }

                fStream = new MemoryStream(BytesToRead);
                
                
                if (fStream == null)
                { f.Close(); return false; }

                byte[] buff = fStream.GetBuffer();


                BBAFlags Flags = (BBAFlags)bba.Flags;

                bool IsFileWriten = false;
                bool Decrypted = false;
                bool Decompressed = false;

                BytesRead = pakFile.Read( buff, 0, BytesToRead );

                if ( BytesRead == 0 )
                { f.Close(); return false; }

                if ( ( Flags & BBAFlags.EncryptedHeader ) == BBAFlags.EncryptedHeader )
                {
                    int count = bba.PackedSize / bba.Unknown2 + 1;
                    byte[] b = new byte[ 0x10 ];
                    for ( int i = 0; i < count; i++ )
                    {
                        Array.Copy( buff, i * bba.Unknown2, b, 0, 0x10 );

                        using ( MemoryStream s = new MemoryStream() )
                        {
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            byte[] TmpKey = s.GetBuffer();
                            BBACrypt.DecryptFileHeader( b, b.Length, TmpKey, 0x10, false );
                        }

                        Array.Copy( b, 0, buff, i * bba.Unknown2, 0x10 );
                    }
                }

                if ( ( Flags & BBAFlags.EncryptedEntireContent ) == BBAFlags.EncryptedEntireContent )
                {
                    IsFileWriten = true;
                    int count = BytesToRead / bba.Unknown2 + 1;
                    byte[] b = new byte[ 0x8000 ];

                    for ( int i = 0; i < count; i++ )
                    {
                        int ToDecrypt = ( ( BytesToRead - ( i * bba.Unknown2 + b.Length ) ) >= 0 ) ? b.Length : BytesToRead - i * bba.Unknown2;

                        Array.Copy( buff, i * bba.Unknown2, b, 0, ToDecrypt );

                        using ( MemoryStream s = new MemoryStream() )
                        {
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            s.Write( BitConverter.GetBytes( bba.PathSize ), 0, 4 );
                            byte[] TmpKey = s.GetBuffer();
                            bool bRes = BBACrypt.DecryptIndexTable( b, ToDecrypt, TmpKey, 0x10 );
                            if ( !bRes )
                            {
                                this.listBox1.Items.Add( "Failed to decrypt: " + bba.FileName );
                            }

                            int ToWrite = ( ToDecrypt == 0x8000 ) ? 0x8000 : BytesToWrite - i * bba.Unknown2;
                            f.Write( b, 0, ToWrite );
                        }
                    }
                }


                if ( ( Flags & BBAFlags.Gziped ) == BBAFlags.Gziped &&
                    !Decompressed )
                {
                    IsFileWriten = true;
                    using ( MemoryStream m = new MemoryStream( buff ) )
                    {
                        using ( GZipStream Decompress = new GZipStream( m, CompressionMode.Decompress ) )
                        {
                            MemoryStream DecompressionStream = new MemoryStream( 0x10000 );
                            byte[] DecompressionBuffer = DecompressionStream.GetBuffer();
                            int numRead;
                            while ( ( numRead = Decompress.Read( DecompressionBuffer, 0, DecompressionBuffer.Length ) ) != 0 )
                            {
                                f.Write( DecompressionBuffer, 0, numRead );
                                BytesToWrite -= numRead;
                            }
                            DecompressionStream.Dispose();
                            DecompressionStream.Close();
                            Decompressed = true;
                        }
                    }
                }


                if ( !IsFileWriten )
                {
                    f.Write( buff, 0, bba.PackedSize );
                }


                // 						   MemoryStream s = new MemoryStream();                            
                //                             
                //                             s.Write(BitConverter.GetBytes(bba.PathSize), 0, 4);
                //                             s.Write(BitConverter.GetBytes(bba.PathSize), 0, 4);
                //                             s.Write(BitConverter.GetBytes(bba.PathSize), 0, 4);
                //                             s.Write(BitConverter.GetBytes(bba.PathSize), 0, 4);
                // 
                //                             byte[] TmpKey = s.GetBuffer();
                //                             buff = new byte[bba.UnpackedSize];
                //                             pakFile.Read(buff, 0, buff.Length);
                //                             
                //                             bool bRes = BBACrypt.DecryptIndexTable(buff, buff.Length, TmpKey, 0x10);
                // 
                //                             if (bRes == false)
                //                             {
                //                                 this.listBox1.SelectedItem = listBox1.Items.Add("Failed to decrypt: " + bba.FileName );
                //                             }
                //                             else
                //                             {
                //                                 
                //                             }
                // 						}

            }
            catch (System.Exception ex)
            {
                if (FileName != null)
                {
                    this.listBox1.Items.Add(FileName);
                }

                this.listBox1.Items.Add(ex.Message);
            }

            f.Position = 0;
            
            int crc32 = Compression.GetStreamCRC32(f);

            if( bba.UnpackedCRC32 != crc32 )
            {
                listBox1.Items.Add("CRC mismatch: " + bba.FileName);
            }
            
            if( f != null )
            {
                f.Close();
            }

            if( fStream != null )
            {
                fStream.Dispose();
                fStream.Close();
            }

            

            DateTime time = DateTime.FromFileTime(bba.Time);
            File.SetCreationTime(outDir + @"\" + FileName, time);
            File.SetLastWriteTime(outDir + @"\" + FileName, time);
            File.SetLastAccessTime(outDir + @"\" + FileName, time);

            return true;
        }
		private void SaveAllItems( object dir )
		{
			DisableControls();

			string outDir = dir as string;
			
			int count = m_pakList.Count;

			if( count > 0 )
			{
				int refresh = 0;
				int counter = 0;
				this.progressBar1.Maximum = count;
				this.progressBar1.Value = 0;
				this.progressBar1.Step = 1;
                BBA bba;                

				for( int i = 0; i < count; i++ )
				{
                    bba = (BBA)m_pakList[i];

                    SaveFile(bba, outDir);

					this.progressBar1.PerformStep();

					if( refresh == 100 )
					{
						this.progressBar1.Refresh();
						this.listBox1.Refresh();
						refresh = 0;
					}

					refresh++;
					counter++;
				}
				this.listBox1.SelectedItem = listBox1.Items.Add( "Saved: " + counter + " files." );

			}

			EnableControls();
		}
		private void DisableControls()
		{
			btnClose.Enabled = false;
			btnOpen.Enabled = false;
			btnSaveAll.Enabled = false;
			btnMake.Enabled = false;
			checkBox1.Enabled = false;
		}
		private void EnableControls()
		{
			btnClose.Enabled = true;
			btnOpen.Enabled = true;
			btnSaveAll.Enabled = true;
			btnMake.Enabled = true;
			checkBox1.Enabled = true;
		}
		private int GetCRC32( string str )
		{
			return GetCRC32( Encoding.Default.GetBytes( str ) );
		}
        private int GetCRC32( byte[] data )
        {
            IChecksum crc = new Crc32();
            crc.Reset();
            crc.Update( data );
            return (int)crc.Value;
        }
		#endregion

		#region Hash Table Methods
        private byte[] BuildHashTable()
        {
            int tableSize = CalulateHashTableSize( m_pakList.Count );
            byte[] table = new byte[ ( tableSize * 8 ) + 4 ];

            BinaryWriter bw = new BinaryWriter( new MemoryStream( table ) );

            bw.Write( tableSize );

            int count = m_pakList.Count;
            for ( int i = 0; i < count; i++ )
            {
                BBA bba = (BBA)m_pakList[ i ];
                int crc = GetCRC32( bba.FileName );
                int pointer = ( ( tableSize - 1 ) & crc );
                int pos = pointer * 8 + 4;

                while ( BitConverter.ToInt64( table, pos ) != 0 )
                {
                    pointer++;
                    pos = pointer * 8 + 4;
                }
                bw.BaseStream.Position = (long)pos;
                bw.Write( crc );
                bw.Write( (int)bba.BBAEntryPositionInTable - 0x114 );
            }

            return table;
        }
		private int CalulateHashTableSize( int NumberOfEntries )
		{
			int c = 0;
			int a = 1;
			int x = ( NumberOfEntries + 1 ) >> 1;
			if( x != 0 )
			{
				while( x != 0 )
				{
					x = x >> 1;
					c++;
				}
			}

			c++;

			a = a << c;
			c = NumberOfEntries >> 1;
			c += NumberOfEntries;
			if( c > a )
			{
				a += a;
			}

			a += a;

			return a;
		}
		#endregion

		#region BBA Building
		private void ClearMembers()
		{

			if( m_pakList.Count > 0 )
				m_pakList.Clear();

			if( treeView1.Nodes.Count > 0 )
				treeView1.Nodes.Clear();

			progressBar1.Value = 0;

			if( m_IndexTable != null )
			{
				m_IndexTable.Dispose();
				m_IndexTable.Close();
				m_IndexTable = null;
			}
		}
		private void GetFilesInfo( DirectoryInfo d, ref int ptrToFirstElement )
		{
			BBA bba;
			DirectoryInfo[] dirs = d.GetDirectories( "*" );
			int pathlen = folderBrowserDialog1.SelectedPath.Length;

			int count = dirs.Length;

			for( int i = 0; i < count; i++ )
			{

				bba = new BBA();
				bba.FileName = dirs[ i ].FullName.Substring( pathlen + 1 );
				bba.PathSize = bba.FileName.Length;
				bba.SizeOfPathToRootDir = bba.PathSize - dirs[ i ].Name.Length;
                bba.Flags = (int)BBAFlags.Directory; 

				ptrToFirstElement += 0x41;
				ptrToFirstElement += bba.PathSize;
				int len = ptrToFirstElement % 4;
				len = ( len == 0 ) ? 0 : 4 - len;
				ptrToFirstElement += len;
				bba.IsFile = 0;

				int idx = m_pakList.Add( bba );

				GetFilesInfo( dirs[ i ], ref ptrToFirstElement );

				if( i == count - 1 )
				{
					bba.OffsetToNextFileName = -1;
				}
				else
				{
					bba.OffsetToNextFileName = ptrToFirstElement;
				}

				m_pakList[ idx ] = bba;

			}

			FileInfo[] files;
			files = d.GetFiles( "*" );
			count = files.Length;

			for( int i = 0; i < count; i++ )
			{
				bba = new BBA();
				bba.FileName = files[ i ].FullName.Substring( pathlen + 1 );
				bba.IsFile = -1;
				bba.PathSize = bba.FileName.Length;
				bba.SizeOfPathToRootDir = bba.PathSize - files[ i ].Name.Length;
				bba.Time = files[ i ].CreationTime.ToFileTime();
				bba.UnpackedSize = ( int )files[ i ].Length;
                bba.Unknown2 = 0x100000;
				if( i == count - 1 )
					bba.OffsetToNextFileName = -1;

				m_pakList.Add( bba );

				ptrToFirstElement += 0x41;
				ptrToFirstElement += bba.PathSize;
				int len = ptrToFirstElement % 4;
				len = ( len == 0 ) ? 0 : 4 - len;
				ptrToFirstElement += len;

			}
		}
		private bool IsNeedToCompress( FileInfo finfo )
		{
			string ext = finfo.Extension;
			ext = ext.ToLower();

			foreach( string str in ExtCmp )
			{
				if( ext == str )
					return true;
			}

			return false;
		}
		private int WriteFile( Stream destination, Stream source, FileInfo sourceInfo )
		{
// 			int size = 0;
// 
// 			if( checkBox1.Checked || IsNeedToCompress( sourceInfo ) )
// 			{
// 				long position = destination.Position;
// 				BinaryWriter bw = new BinaryWriter( destination, Encoding.Default );
// 				bw.Write( size );
// 				bw.Write( ( int )sourceInfo.Length );
// 				if( sourceInfo.Length < 100000 )
// 				{
// 					size = Compression.Compress( source, destination, 9, true );
// 				}
// 				else if( sourceInfo.Length < 1000000 )
// 				{
// 					size = Compression.Compress( source, destination, 6, true );
// 				}
// 				else
// 				{
// 					size = Compression.Compress( source, destination, 3, true );
// 				}
// 				destination.Position = position;
// 				bw.Write( size );
// 				destination.Position += size + 4;
// 				size += 8;
// 			}
// 			else
// 			{
// 				size = Compression.CopyStream( source, destination );
// 			}

            return Compression.CopyStream( source, destination ); ;
		}
		private void BuildBBA( object obj )
		{
			DisableControls();
			string saveTo = saveFileDialog1.FileName;
			string openDir = folderBrowserDialog1.SelectedPath;

			try
			{
				m_fs = new FileStream( saveTo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite );
				m_fs.Position = 0;
				m_fs.Write( new byte[ 0x50 ], 0, 0x50 );

				int idx = listBox1.Items.Add( "Getting Files Info..." );
				ClearMembers();
				int aproxsize = 0;
				GetFilesInfo( new DirectoryInfo( openDir ), ref aproxsize );
				listBox1.Items[ idx ] += "OK";

				int count = m_pakList.Count;

				int refresh = 0;
				int counter = 0;
				this.progressBar1.Maximum = count;
				this.progressBar1.Value = 0;
				this.progressBar1.Step = 1;

				idx = listBox1.Items.Add( "writing data..." );

				for( int i = 0; i < count; i++ )
				{
					BBA bba = ( BBA )m_pakList[ i ];

					if( bba.IsFile == -1 )
					{
						string file = openDir + "\\" + bba.FileName;
						FileStream f1 = File.Open( file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

						f1.Position = 0;
						bba.UnpackedCRC32 = Compression.GetStreamCRC32( f1 );
						f1.Position = 0;

						bba.Offset = m_fs.Position;
						bba.PackedSize = WriteFile( m_fs, f1, new FileInfo( file ) );

						if( bba.PackedSize != bba.UnpackedSize )
							bba.Flags = 1;

						m_fs.Flush();
						f1.Close();

						m_pakList[ i ] = bba;
					}

					this.progressBar1.PerformStep();

					if( refresh == 100 )
					{
						this.progressBar1.Refresh();
						this.listBox1.Refresh();
						refresh = 0;
					}

					refresh++;
					counter++;

				}
				listBox1.Items[ idx ] += "OK";

				m_header.IndexTableOffset = m_fs.Position;



				refresh = 0;
				counter = 0;
				this.progressBar1.Maximum = count;
				this.progressBar1.Value = 0;
				this.progressBar1.Step = 1;


				idx = listBox1.Items.Add( "Generating Index Table and Encrypting Headers..." );

				m_IndexTable = new MemoryStream();

				m_IndexTable.Write( new byte[ 0x110 ], 0, 0x110 );
				m_IndexTable.Write( BitConverter.GetBytes( count ), 0, 4 );

				int OffsetToNextEntry = 0;

				for( int i = 0; i < count; i++ )
				{
					BBA bba = ( BBA )m_pakList[ i ];

					byte[] entry = BuildEntry( bba );

					if( bba.Flags == 1 )
					{
						byte[] dataheader = new byte[ 0x10 ];
						m_fs.Position = bba.Offset;
						m_fs.Read( dataheader, 0, dataheader.Length );

						BBACrypt.EncryptFileHeader( dataheader, dataheader.Length, entry, entry.Length );

						m_fs.Position = bba.Offset;
						m_fs.Write( dataheader, 0, dataheader.Length );
						m_fs.Position = bba.Offset;
						bba.PackedCRC32 = Compression.GetStreamCRC32( m_fs, bba.PackedSize );
					}
					else
					{
						bba.PackedSize = bba.UnpackedSize;
						bba.PackedCRC32 = bba.UnpackedCRC32;
					}
					OffsetToNextEntry += entry.Length;
					OffsetToNextEntry += bba.PathSize + 1;

					int pos = ( int )m_IndexTable.Position;
					pos += bba.PathSize + 0x40 + 1;
					int x = ( pos % 4 );
					pos = ( x == 0 ) ? 0 : 4 - x;
					OffsetToNextEntry += pos;

					bba.BBAEntryPositionInTable = m_IndexTable.Position;

					if( bba.IsFile == -1 )
					{
						if( bba.OffsetToNextFileName != -1 )
						{
							bba.OffsetToNextFileName = OffsetToNextEntry;
						}
					}

					m_pakList[ i ] = bba;
					entry = BuildEntry( bba );

					m_IndexTable.Write( entry, 0, entry.Length );
					m_IndexTable.Write( Encoding.Default.GetBytes( bba.FileName ), 0, bba.PathSize );
					m_IndexTable.Write( new byte[ pos + 1 ], 0, pos + 1 );

					this.progressBar1.PerformStep();

					if( refresh == 100 )
					{
						this.progressBar1.Refresh();
						this.listBox1.Refresh();
						refresh = 0;
					}

					refresh++;
					counter++;
				}

				int HashTablePosition = ( int )m_IndexTable.Position;
				byte[] hashtable = BuildHashTable();
				m_IndexTable.Write( hashtable, 0, hashtable.Length );

				BinaryWriter bw = new BinaryWriter( m_IndexTable );
				m_IndexTable.Position = 0;
				bw.Write( ( int )0x40 );
				bw.Write( ( int )0x110 );
				bw.Write( HashTablePosition );
				m_IndexTable.Position = 0x40;
				bw.Write( c_bbaHead, 0, c_bbaHead.Length );
				m_IndexTable.Position = 0x60;
                bw.Write( s7_bba_header_magic );
                bw.Write( s7_bba_header_magic2 );
                m_IndexTable.Position = 0x94;
                bw.Write( s7_bba_magic1 );
                bw.Write( s7_bba_header_magic );
				m_IndexTable.Position = 0;

                int IndexTableAlignedSize = (int)m_IndexTable.Length;

                int len = (int)m_IndexTable.Length;
                len = ( len % 4 );
                len = ( len == 0 ) ? 0 : 4 - len;
                if ( len != 0 )
                {
                    IndexTableAlignedSize += len;
                }
                
//                 bw = new BinaryWriter( m_IndexTable );
// 				bw.Write( ( int )m_IndexTable.Length );

                m_header.IndexTableSize = IndexTableAlignedSize;
                m_IndexTable.Capacity = IndexTableAlignedSize;

                byte[] idxtable = m_IndexTable.GetBuffer();
				bool res = BBACrypt.EncryptIndexTable( idxtable, m_header.IndexTableSize, null, 0 );

				m_header.IndexTableCRC32 = GetCRC32( idxtable );
                m_header.Magic  = s7_bba_header_magic;
                m_header.Magic2 = s7_bba_header_magic2;
				m_fs.Position = m_header.IndexTableOffset;
				m_fs.Write( idxtable, 0, idxtable.Length );
				m_fs.Position = 0;
				m_fs.Write( c_bbaHead, 0, c_bbaHead.Length );
				byte[] header = BuildHeader();
				BBACrypt.EncryptHeader( header, header.Length, c_bbaHead, c_bbaHead.Length );
				m_fs.Write( header, 0, header.Length );
				listBox1.Items[ idx ] += "OK";
				listBox1.Items.Add( "BBA file successfully created." );

			}
			catch( Exception e )
			{
				listBox1.Items.Add( e.Message );
			}

			m_fs.Close();
			m_fs = null;
			EnableControls();
			GC.Collect();
		}
		private byte[] BuildHeader()
		{
			MemoryStream header = new MemoryStream( 0x40 );
			BinaryWriter bw = new BinaryWriter( header );

			bw.Write( m_header.IndexTableOffset );
			bw.Write( m_header.IndexTableSize );
			bw.Write( m_header.IndexTableCRC32 );
			bw.Write( m_header.Magic );
            bw.Write( m_header.Magic2 );

			byte[] buff = header.GetBuffer();

			return buff;
		}
		private byte[] BuildEntry( BBA bba )
		{
			MemoryStream entry = new MemoryStream( 0x40 );
			BinaryWriter bw = new BinaryWriter( entry );

			bw.Write( bba.Time );
			bw.Write( bba.UnpackedSize );
			bw.Write( bba.UnpackedCRC32 );
			bw.Write( bba.Flags );
			bw.Write( bba.Unknown1 );
			bw.Write( bba.Offset );
			bw.Write( bba.PackedSize );
			bw.Write( bba.PackedCRC32 );
			bw.Write( bba.Unknown2 );
			bw.Write( bba.Unknown3 );
			bw.Write( bba.PathSize );
			bw.Write( bba.SizeOfPathToRootDir );
			bw.Write( bba.IsFile );
			bw.Write( bba.OffsetToNextFileName );

			byte[] buff = entry.GetBuffer();

			return buff;
		}
		#endregion

        private void comboBox1_SelectedIndexChanged( object sender, EventArgs e )
        {
            BBACrypt.SetMode( comboBox1.SelectedIndex );
        }

	}
}