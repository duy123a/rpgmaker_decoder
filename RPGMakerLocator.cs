namespace rpgmaker_decoder
{
    internal class RPGMakerLocator
    {
        private readonly HashSet<string> _fakeExtList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".rpgmvp", ".rpgmvm", ".rpgmvo", ".png_", ".m4a_", ".ogg_"
        };

        public RPGMakerLocator(string sourcePath)
        {
            SourcePath = sourcePath.Trim();
            if (!Directory.Exists(SourcePath))
            {
                throw new DirectoryNotFoundException($"The source path '{SourcePath}' does not exist.");
            }
        }

        public IEnumerable<string> GetFilesWithFakeExtensions()
        {
            // Get all files in SourcePath and subdirectories
            var allFiles = Directory.EnumerateFiles(SourcePath, "*", SearchOption.AllDirectories);

            // Filter files where the last extension matches _fakeExtList
            var filteredFiles = allFiles
                .Where(file => _fakeExtList.Contains(Path.GetExtension(file)));

            return filteredFiles;
        }

        public string SourcePath { get; }
    }
}
