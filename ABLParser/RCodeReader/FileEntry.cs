using System;

namespace ABLParser.RCodeReader
{
	/// <summary>
	/// Class representing a file entry in a PL file
	/// </summary>
	public class FileEntry : IComparable<FileEntry>
	{
		private readonly bool valid;
		private readonly string fileName;
		private readonly long modDate, addDate;
		private readonly int offset, size, tocSize;

		/// <summary>
		/// Invalid file entry - Will be skipped in entries list
		/// </summary>
		/// <param name="tocSize"> </param>
		public FileEntry(int tocSize)
		{
			this.tocSize = tocSize;
			valid = false;
			fileName = "";
			modDate = addDate = offset = 0;
			size = 0;
		}

		public FileEntry(string fileName, long modDate, long addDate, int offSet, int size, int tocSize)
		{
			this.valid = true;
			this.fileName = fileName;
			this.modDate = modDate;
			this.addDate = addDate;
			this.offset = offSet;
			this.size = size;
			this.tocSize = tocSize;
		}

		public virtual string FileName => fileName;

		public virtual int Size => size;

		public virtual long ModDate => modDate;

		public virtual long AddDate => addDate;

		public virtual int Offset => offset;

		public virtual int TocSize => tocSize;

		public virtual bool Valid => valid;

		public override string ToString()
		{
			return string.Format("File {0} [{1} bytes] Added {2,date} Modified {3,date} [Offset : {4}]", this.fileName, Convert.ToInt32(size), new DateTime(addDate), new DateTime(modDate), Convert.ToInt64(offset));
		}

		public virtual int CompareTo(FileEntry o)
		{
			return fileName.CompareTo(o.FileName);
		}

		public override int GetHashCode()
		{
			return fileName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() == obj.GetType())
			{
				return ((FileEntry)obj).fileName.Equals(fileName, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
	}
}
