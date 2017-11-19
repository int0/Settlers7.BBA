using System;
using System.IO;


using zLib.Compression;
using zLib.Compression.Streams;

namespace Compressor
{
	class Compression
	{
		/// <summary>
		/// Decompresses input stream.
		/// </summary>
		/// <param name="in_stream">Input stream to decompress.</param>
		/// <param name="outStream">Output stream.</param>
		/// <param name="zlib">zlib header.</param>
		public static void Decompress( Stream in_stream, int position, Stream outStream, bool zlib )
		{
			in_stream.Position = position;

			Inflater inflater = new Inflater( !zlib );
			InflaterInputStream inStream = new InflaterInputStream( in_stream, inflater );

			byte[] buf2 = new byte[ 0x100 ];

			int count = buf2.Length;

			try
			{
				while( true )
				{
					int numRead = inStream.Read( buf2, 0, count );
					if( numRead <= 0 )
					{
						break;
					}
					outStream.Write( buf2, 0, numRead );
					outStream.Flush();
				}
			}
			catch( Exception ex )
			{
			}
		}
		/// <summary>
		/// Compresses input byte array.
		/// </summary>
		/// <param name="data">Byte array for compression.</param>
		/// <param name="level">Compression level.</param>
		/// <param name="zlib">zlib header.</param>
		/// <returns>Compressed byte array.</returns>
		public static byte[] Compress( byte[] in_data, int level, bool zlib )
		{
			MemoryStream memoryStream = new MemoryStream();

			Deflater deflater = new Deflater( level, !zlib );

			using( DeflaterOutputStream outStream = new DeflaterOutputStream( memoryStream, deflater ) )
			{
				outStream.IsStreamOwner = false;
				outStream.Write( in_data, 0, in_data.Length );
				outStream.Flush();
				outStream.Finish();
			}

			memoryStream.Capacity = ( int )memoryStream.Length;
			byte[] outbuff = memoryStream.GetBuffer();

			return outbuff;
		}
		/// <summary>
		/// Compress input stream.
		/// </summary>
		/// <param name="in_stream">Input Stream which will be compressed.</param>
		/// <param name="out_stream">Output stream where compressed data will be written.</param>
		/// <param name="level">Compression level.</param>
		/// <param name="zlib">Zlib header usage.</param>
		/// <returns></returns>
		public static int Compress( Stream in_stream, Stream out_stream, int level, bool zlib )
		{
			Deflater deflater = new Deflater( level, !zlib );

			int compressedsize = 0;

			using( DeflaterOutputStream outStream = new DeflaterOutputStream( out_stream, deflater ) )
			{
				compressedsize = ( int )out_stream.Length;
				outStream.IsStreamOwner = false;
				CopyStream( in_stream, outStream );
				outStream.Flush();
				outStream.Finish();
				compressedsize = ( int )out_stream.Length - compressedsize;
			}

			return compressedsize;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public static int CopyStream( System.IO.Stream input, System.IO.Stream output )
		{
			int len;
			int size = 0;
			byte[] buffer = new byte[ 512 ];

			while( ( len = input.Read( buffer, 0, 512 ) ) > 0 )
			{
				output.Write( buffer, 0, len );
				size += len;
			}

			output.Flush();
			return size;
		}
		public static int GetStreamCRC32( Stream stream )
		{
			zLib.Checksums.IChecksum crc = new zLib.Checksums.Crc32();
			crc.Reset();

			int len;
			byte[] buffer = new byte[ 512 ];

			while( ( len = stream.Read( buffer, 0, 512 ) ) > 0 )
			{
				crc.Update( buffer, 0, len );
			}

			return ( int )crc.Value;
		}
		public static int GetStreamCRC32( Stream stream, int count )
		{
			zLib.Checksums.IChecksum crc = new zLib.Checksums.Crc32();
			crc.Reset();
			byte[] b = new byte[ 1 ];

			for( int i = 0; i < count; i++ )
			{
				int len = stream.Read( b, 0, b.Length );
				if( len == 0 )
					return 0;

				crc.Update( b );
			}

			return ( int )crc.Value;
		}
	}
}
