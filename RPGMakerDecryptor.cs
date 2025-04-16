namespace rpgmaker_decoder
{
    internal static class RPGMakerDecryptor
    {
        private const int HeaderLength = 16;
        private const string DefaultHeaderHex = "5250474d560000000003010000000000";
        private const string PngHeaderHex = "89504e470d0a1a0a0000000d49484452";

        public static string ExtractEncryptionKey(RPGMakerFile file, bool ignoreFakeHeader = false)
        {
            if (!file.IsExist() || !file.IsImage())
                throw new FileNotFoundException("Invalid image file", file.FilePath);

            var content = File.ReadAllBytes(file.FilePath);
            var header = content.Take(HeaderLength).ToArray();

            var expectedHeader = HexToBytes(DefaultHeaderHex);
            if (!expectedHeader.SequenceEqual(header) && !ignoreFakeHeader)
                throw new Exception("Invalid header!");

            var contentWithoutHeader = content.Skip(HeaderLength).ToArray();
            var pngHeaderBytes = HexToBytes(PngHeaderHex);

            var keyBytes = new byte[HeaderLength];
            for (int i = 0; i < HeaderLength; i++)
            {
                keyBytes[i] = (byte)(contentWithoutHeader[i] ^ pngHeaderBytes[i]);
            }

            return BytesToHex(keyBytes);
        }

        public static void DecryptWithKey(RPGMakerFile file, string key, bool restorePngImage = false)
        {
            if (!file.IsExist()) throw new FileNotFoundException("File not found", file.FilePath);

            var content = File.ReadAllBytes(file.FilePath);
            var header = content.Take(HeaderLength).ToArray();

            var expectedHeader = HexToBytes(DefaultHeaderHex);
            if (!expectedHeader.SequenceEqual(header))
                throw new Exception("Invalid header!");

            var contentWithoutHeader = content.Skip(HeaderLength).ToArray();
            var keyBytes = HexToBytes(key);
            var pngHeaderBytes = HexToBytes(PngHeaderHex);

            for (int i = 0; i < HeaderLength; i++)
            {
                contentWithoutHeader[i] = restorePngImage
                    ? pngHeaderBytes[i]
                    : (byte)(contentWithoutHeader[i] ^ keyBytes[i]);
            }

            var outputPath = file.CreateNewFilePath();
            EnsureDirectoryExists(outputPath);

            File.WriteAllBytes(outputPath, contentWithoutHeader);
        }

        private static byte[] HexToBytes(string hex) =>
            Enumerable.Range(0, hex.Length / 2)
                      .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                      .ToArray();

        private static string BytesToHex(byte[] bytes) =>
            BitConverter.ToString(bytes).Replace("-", "").ToLower();

        private static void EnsureDirectoryExists(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
