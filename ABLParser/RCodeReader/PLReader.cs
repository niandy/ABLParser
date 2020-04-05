using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Class for reading and extracting contents of a Progress Library file.
	/// 
	/// @author <a href="mailto:g.querret+PCT@gmail.com">Gilles QUERRET</a>
	/// </summary>
	public class PLReader
	{
		private const int MAGIC_V11 = 0xd70b;
		private const int ENCODING_OFFSET = 0x02;
		private const int ENCODING_SIZE = 20;
		private const int FILE_LIST_OFFSET_V11 = 0x22;

		private readonly FileInfo pl;
		private IList<FileEntry> files = null;

		public PLReader(FileInfo file)
		{
			this.pl = file;
		}

		/// <summary>
		/// Returns entries contained in this procedure library
		/// </summary>
		/// <exception cref="RuntimeException"> If file is not a valid procedure library </exception>
		public virtual IList<FileEntry> FileList
		{
			get
			{
				if (this.files == null)
				{
					ReadFileList();
				}
				return files;
			}
		}

		public virtual FileEntry GetEntry(string name)
		{
			foreach (FileEntry entry in FileList)
			{
				if (entry.FileName.Equals(name))
				{
					return entry;
				}
			}
			return null;
		}

		private void ReadFileList()
		{
			try
			{
				FileStream fs = pl.OpenRead();
				BinaryReader br = new BinaryReader(fs);
				ushort magic;
				magic = ByteBuffer.Wrap(br.ReadBytes(2), 0, 2).Order(false).GetUnsignedShort();

				if (magic != MAGIC_V11)
				{
					throw new Exception("Not a valid PL file");
				}

				Encoding charset = GetCharset(br);
				int offset = GetTOCOffset(br);
				files = new List<FileEntry>();
				FileEntry fe = null;
				while ((fe = ReadEntry(br, offset, charset)) != null)
				{
					if (fe.Valid)
					{
						files.Add(fe);
					}
					offset += fe.TocSize;
				}
			}
			catch (IOException caught)
			{
				throw new Exception("Error while reading PL file", caught);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public InputStream getInputStream(FileEntry fe) throws IOException
		public virtual Stream GetInputStream(FileEntry fe)
		{
			byte[] bb;
			using (FileStream fs = pl.OpenRead())
			{
				BinaryReader br = new BinaryReader(fs);
				fs.Seek(fe.Offset, SeekOrigin.Begin);
				bb = br.ReadBytes(fe.Size);				
			}
			return new MemoryStream(bb);
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private Charset getCharset(FileChannel fc) throws IOException
		private Encoding GetCharset(BinaryReader br)
		{
			br.BaseStream.Seek(ENCODING_OFFSET, SeekOrigin.Begin);			
			byte[] bEncoding = br.ReadBytes(ENCODING_SIZE);
			if (bEncoding.Length != ENCODING_SIZE)
			{
				throw new Exception("Invalid PL file");
			}			
			
			StringBuilder sbEncoding = new StringBuilder();
			int zz = 0;
			while ((zz < 20) && (bEncoding[zz] != 0))
			{
				sbEncoding.Append((char)bEncoding[zz++]);
			}
			try
			{
				return Encoding.GetEncoding(sbEncoding.ToString());
			}
			catch (System.ArgumentException)
			{
				return Encoding.GetEncoding("US-ASCII");
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private int getTOCOffset(FileChannel fc) throws IOException
		private int GetTOCOffset(BinaryReader br)
		{
			br.BaseStream.Seek(FILE_LIST_OFFSET_V11, SeekOrigin.Begin);
			byte[] bTOC = br.ReadBytes(4);
			if (bTOC.Length != 4)
			{
				throw new Exception("Invalid PL file");
			}
			return ByteBuffer.Wrap(bTOC, 0, 4).Order(false).GetInt();
		}
		
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private FileEntry readEntry(FileChannel fc, int offset, Charset charset) throws IOException
		private FileEntry ReadEntry(BinaryReader br, int offset, Encoding charset)
		{
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			byte b1 = br.ReadByte();

			if (b1 == (byte)0xFE)
			{
				bool stop = false;
				int zz = 0;
				while (br.BaseStream.Position < br.BaseStream.Length && !stop)
				{
					br.BaseStream.Seek(offset + ++zz, SeekOrigin.Begin);
					b1 = br.ReadByte();
					stop = (b1 == (byte)0xFF);
				}

				return new FileEntry(zz);
			}
			else if (b1 == (byte)0xFF)
			{
				b1 = br.ReadByte();				
				int fNameSize = (int)b1;
				if (fNameSize == 0)
				{
					return new FileEntry(29);
				}
				byte[] b2 = br.ReadBytes(fNameSize);
				string fName = charset.GetString(b2);
				byte[] b3 = br.ReadBytes(48); // Ou 47				
				int fileOffset = ByteBuffer.Wrap(b3, 6, 4).Order(false).GetInt(); // 7
				int fileSize = ByteBuffer.Wrap(b3, 11, 4).Order(false).GetInt(); // 12
				long added = ByteBuffer.Wrap(b3, 15, 4).Order(false).GetInt() * 1000L; // 16
				long modified = ByteBuffer.Wrap(b3, 19, 4).Order(false).GetInt() * 1000L; // 20

				int tocSize = (b3[47] == 0 ? 50 : 49) + fNameSize;
				return new FileEntry(fName, modified, added, fileOffset, fileSize, tocSize);
			}
			else
			{
				return null;
			}

		}
	}
}
