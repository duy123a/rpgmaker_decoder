using System;
using System.IO;
using System.Linq;

namespace rpgmaker_decoder
{
	public class RPGMakerDecryptor
	{
		private const int DEFAULT_HEADER_LENGTH = 16;
		private const string DEFAULT_SIGNATURE = "5250474d56000000";
		private const string DEFAULT_VERSION = "000301";
		private const string DEFAULT_REMAIN = "0000000000";
		private const string PNG_HEADER = "89504e470d0a1a0a0000000d49484452";

		public RPGMakerDecryptor()
		{
			this.HeaderLength = DEFAULT_HEADER_LENGTH;
			this.Signature = DEFAULT_SIGNATURE;
			this.Version = DEFAULT_VERSION;
			this.Remain = DEFAULT_REMAIN;
			this.IgnoreFakeHeader = false;
		}

		public RPGMakerDecryptor(string decryptCode) : this()
		{
			this.DecryptCode = decryptCode;
		}

		public void DecryptFile(RPGMakerFile file, bool isRestorePngImage)
		{
			if (file.CheckExist() == false)
			{
				throw new FileNotFoundException("File is not existed", nameof(file.Name));
			}

			var content = file.Content;

			// Check fake header
			if (CheckFakeHeader(content) == false)
			{
				throw new Exception("Header is Invalid!");
			}

			// Remove fake header
			content = GetByteArray(content, GetHeaderLength());

			var pngHeaderByteArray = GetPngHeaderByteArray();
			var decryptionCodeByteArray = GetDecryptionCodeByteArray();

			if (content.Length > 0)
			{
				for (var i = 0; i < this.GetHeaderLength(); i++)
				{
					if (isRestorePngImage)
					{
						// Restore pictures by replacing content with PNG header bytes
						content[i] = pngHeaderByteArray[i];
					}
					else
					{
						// Decrypt by using decryption code
						content[i] = (byte)(content[i] ^ decryptionCodeByteArray[i]);
					}
				}
			}

			var newFilePath = file.CreateNewFilePath();
			File.WriteAllBytes(newFilePath, content);
		}

		public void DetectEncryptionKeyFromImage(RPGMakerFile file)
		{
			if (file.CheckExist() == false)
			{
				throw new FileNotFoundException("File is not existed", nameof(file.Name));
			}

			var content = file.Content;

			var keyBytes = new byte[GetHeaderLength()];

			// Check fake header
			if (CheckFakeHeader(content) == false)
			{
				throw new Exception("Header is Invalid!");
			}

			// Remove fake header
			content = GetByteArray(content, GetHeaderLength());

			var pngHeaderByteArray = GetPngHeaderByteArray();

			if (content.Length > 0)
			{
				for (var i = 0; i < GetHeaderLength(); i++)
				{
					keyBytes[i] = (byte)(content[i] ^ pngHeaderByteArray[i]);
				}
			}

			this.DecryptCode = BytesToHex(keyBytes);
		}

		private byte[] GetDecryptionCodeByteArray()
		{
			if (this.DecryptCode == null)
			{
				throw new ArgumentNullException(nameof(this.DecryptCode));
			}
			return HexToBytes(this.DecryptCode);
		}

		private bool CheckFakeHeader(byte[] content)
		{
			if (this.IgnoreFakeHeader == false) return true;

			var header = GetByteArray(content, 0, GetHeaderLength());
			var refBytes = GetRpgHeaderBytes();

			// Verify header (Check if its an encrypted file)
			for (var i = 0; i < this.GetHeaderLength(); i++)
			{
				if (refBytes[i] != header[i])
					return false;
			}

			return true;
		}

		private void GenerateRpgHeaderBytes()
		{
			var headerHex = this.Signature + this.Version + this.Remain;
			var headerBytes = HexToBytes(headerHex);
			this.RpgHeaderBytes = headerBytes;
		}

		private byte[] GetPngHeaderByteArray()
		{
			return HexToBytes(PNG_HEADER);
		}

		private byte[] HexToBytes(string hex)
		{
			return Enumerable.Range(0, hex.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				.ToArray();
		}

		private string BytesToHex(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

		private static byte[] GetByteArray(byte[] byteArray, int startPos, int? length = null)
		{
			// Don't allow start-values below 0
			if (startPos < 0)
				startPos = 0;

			// Check if length is below 0 (to end of array)
			if (length < 0 || length == null)
				length = byteArray.Length - startPos;

			var newByteArray = new byte[(int)length];
			var n = 0;

			for (var i = startPos; i < (startPos + length); i++)
			{
				// Check if byte array is at the last pos and return shorter byte array if necessary
				if (byteArray.Length <= i)
					return GetByteArray(newByteArray, 0, n);

				newByteArray[n] = byteArray[i];
				n++;
			}

			return newByteArray;
		}

		/// <summary>
		/// Update source path
		/// </summary>
		/// <exception cref="ArgumentException">srcPath is invalid</exception>
		public void UpdateSrcPath()
		{
			var srcPath = this.SourcePath;
			var srcInfo = new DirectoryInfo(srcPath);

			if (Directory.Exists(Path.Combine(srcPath, "img")))
			{
				Console.WriteLine("Found 'img' in source path, using parent directory name");
				if (srcInfo.Parent == null || srcInfo.Parent.Parent == null)
				{
					throw new ArgumentException("Source path is invalid (unable to move two directories up)", nameof(srcPath));
				}

				srcPath = srcInfo.Parent.Parent.FullName; // Move two directories up
			}
			else if (Directory.Exists(Path.Combine(srcPath, "www")))
			{
				Console.WriteLine("Found 'www' in source path, using parent directory name");
				if (srcInfo.Parent == null)
				{
					throw new ArgumentException("Source path is invalid (unable to move one directory up)", nameof(srcPath));
				}

				srcPath = srcInfo.Parent.FullName; // Move one directory up
			}
			else
			{
				throw new ArgumentException("Source path is invalid (not contain RPG Maker project)", nameof(srcPath));
			}

			this.SourcePath = srcPath;
		}

		public int GetHeaderLength()
		{
			return this.HeaderLength;
		}

		public string GetSignature()
		{
			return this.Signature;
		}

		public string GetVersion()
		{
			return this.Version;
		}

		public string GetRemain()
		{
			return this.Remain;
		}

		public string GetSourcePath()
		{
			return this.SourcePath;
		}

		public byte[] GetRpgHeaderBytes()
		{
			if (this.RpgHeaderBytes == null)
			{
				GenerateRpgHeaderBytes();
			}
			return this.RpgHeaderBytes;
		}

		private int HeaderLength { get; set; }
		private string Signature { get; set; }
		private string Version { get; set; }
		private string Remain { get; set; }
		private byte[] RpgHeaderBytes { get; set; }
		private bool IgnoreFakeHeader { get; set; }
		private string DecryptCode { get; set; }
		private string SourcePath { get; set; }
	}
}
