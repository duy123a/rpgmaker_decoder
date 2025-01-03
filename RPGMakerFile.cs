namespace rpgmaker_decoder
{
    internal class RPGMakerFile
    {
        private static readonly Dictionary<string, string> ExtensionMapping = new()
        {
            { ".rpgmvp", ".png" }, { ".png_", ".png" },
            { ".rpgmvm", ".m4a" }, { ".m4a_", ".m4a" },
            { ".rpgmvo", ".ogg" }, { ".ogg_", ".ogg" }
        };

        private readonly FileInfo _file;

        public RPGMakerFile(string filePath, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required", nameof(filePath));
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required", nameof(sourcePath));

            FilePath = filePath.Trim();
            SourcePath = sourcePath.Trim();
            _file = new FileInfo(FilePath);

            if (!_file.Exists) throw new FileNotFoundException("File not found", FilePath);
            if (_file.Length > int.MaxValue) throw new FileLoadException("File is too large");
        }

        public bool IsExist() => _file.Exists;

        public bool IsImage()
        {
            return _file.Extension.Equals(".rpgmvp", StringComparison.OrdinalIgnoreCase) ||
                   _file.Extension.Equals(".png_", StringComparison.OrdinalIgnoreCase);
        }

        public string CreateNewFilePath()
        {
            if (!ExtensionMapping.TryGetValue(_file.Extension.ToLower(), out var newExtension))
            {
                throw new InvalidOperationException($"Unknown file extension: {_file.Extension}");
            }

            var relativePath = Path.GetRelativePath(SourcePath, FilePath);
            return Path.ChangeExtension(Path.Combine(SourcePath, "Output", relativePath), newExtension);
        }

        public string FilePath { get; }
        public string SourcePath { get; }
    }
}
