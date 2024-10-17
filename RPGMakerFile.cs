using System;
using System.IO;

namespace rpgmaker_decoder
{
	/// <summary>
	/// Represents a file for RPG Maker with its properties and content.
	/// </summary>
	public class RPGMakerFile
	{
		private readonly static string OUTPUT_FOLDER = "Output";

		private readonly FileInfo _file;
		private readonly string _name;
		private readonly string _extension;
		private readonly byte[] _content;
		private readonly string _filePath;
		private string _sourcePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="RPGMakerFile"/> class.
		/// </summary>
		/// <param name="filePath">The path of the file.</param>
		/// <param name="sourcePath">The path of the source project</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
		/// <exception cref="FileLoadException">Thrown when the file is too large to load.</exception>
		public RPGMakerFile(string filePath, string sourcePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
			}

			_file = new FileInfo(filePath);
			if (!_file.Exists)
			{
				throw new FileNotFoundException("File does not exist.", filePath);
			}

			if (_file.Length > int.MaxValue)
			{
				throw new FileLoadException("File is too big.");
			}

			_name = _file.Name;
			_extension = _file.Extension;
			_filePath = filePath;
			_sourcePath = sourcePath;
			_content = File.ReadAllBytes(filePath);
		}

		/// <summary>
		/// Check exist
		/// </summary>
		/// <returns>True if the file exists</returns>
		public bool CheckExist()
		{
			return _file.Exists;
		}

		/// <summary>
		/// Create new file path
		/// </summary>
		/// <returns>New file path</returns>
		/// <exception cref="ArgumentException">File path is not inside the source path</exception>
		public string CreateNewFilePath()
		{
			var outputRootPath = Path.Combine(_sourcePath, OUTPUT_FOLDER);

			// Ensure filePath is inside sourcePath
			if (!_filePath.StartsWith(_sourcePath, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("File path is not inside the source path");
			}

			// Get the relative path from the sourcePath to the filePath (excluding the file name)
			string relativePath = _filePath.Substring(_sourcePath.Length);

			// Combine the relative path with the output directory
			string outputFilePath = Path.Combine(outputRootPath, relativePath);

			// Change the extension of the file using RealExtByFakeExt
			string newFilePath = Path.ChangeExtension(outputFilePath, RealExtByFakeExt());

			return newFilePath;
		}

		/// <summary>
		/// Returns the real extension of the current fake extension
		/// </summary>
		/// <returns>Real file extension</returns>
		public string RealExtByFakeExt()
		{
			switch (_extension)
			{
				case "rpgmvp":
				case "png_":
					return "png";

				case "rpgmvm":
				case "m4a_":
					return "m4a";

				case "rpgmvo":
				case "ogg_":
					return "ogg";

				default:
					throw new Exception("Unknown extension");
			}
		}

		// Properties to expose relevant file information if needed
		public string Name => _name;
		public string Extension => _extension;
		public byte[] Content => _content;
		public string FilePath => _filePath;
		public string NewFilePath
		{
			get => _sourcePath;
			set => _sourcePath = value;
		}
	}
}
