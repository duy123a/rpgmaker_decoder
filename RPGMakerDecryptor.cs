﻿namespace rpgmaker_decoder
{
    internal class RPGMakerDecryptor
    {
        private const int DefaultHeaderLength = 16;
        private const string DefaultSignature = "5250474d56000000";
        private const string DefaultVersion = "000301";
        private const string DefaultRemain = "0000000000";
        private const string PngHeaderHex = "89504e470d0a1a0a0000000d49484452";

        private readonly byte[] _rpgHeaderBytes;
        private readonly byte[] _pngHeaderBytes;
        private readonly bool _ignoreFakeHeader;

        public string? DecryptKey { get; private set; }

        public RPGMakerDecryptor(bool ignoreFakeHeader = false)
        {
            _rpgHeaderBytes = HexToBytes(DefaultSignature + DefaultVersion + DefaultRemain);
            _pngHeaderBytes = HexToBytes(PngHeaderHex);
            _ignoreFakeHeader = ignoreFakeHeader;
        }

        public void DecryptFile(RPGMakerFile file, bool restorePngImage = false)
        {
            if (!file.IsExist()) throw new FileNotFoundException("File not found", file.FilePath);

            var content = File.ReadAllBytes(file.FilePath);

            if (!IsValidFakeHeader(content))
                throw new Exception("Invalid header!");

            var contentWithoutHeader = RemoveHeader(content);

            for (var i = 0; i < DefaultHeaderLength; i++)
            {
                contentWithoutHeader[i] = restorePngImage
                    ? _pngHeaderBytes[i]
                    : (byte)(contentWithoutHeader[i] ^ HexToBytes(DecryptKey!)[i]);
            }

            var outputPath = file.CreateNewFilePath();
            EnsureDirectoryExists(outputPath);

            File.WriteAllBytes(outputPath, contentWithoutHeader);
        }

        public void ExtractEncryptionKey(RPGMakerFile file)
        {
            if (!file.IsExist() || !file.IsImage())
                throw new FileNotFoundException("Invalid image file", file.FilePath);

            var content = File.ReadAllBytes(file.FilePath);

            if (!IsValidFakeHeader(content))
                throw new Exception("Invalid header!");

            var contentWithoutHeader = RemoveHeader(content);

            var keyBytes = new byte[DefaultHeaderLength];
            for (var i = 0; i < DefaultHeaderLength; i++)
            {
                keyBytes[i] = (byte)(contentWithoutHeader[i] ^ _pngHeaderBytes[i]);
            }

            DecryptKey = BytesToHex(keyBytes);
        }

        private bool IsValidFakeHeader(byte[] content) =>
            _ignoreFakeHeader || _rpgHeaderBytes.SequenceEqual(GetHeader(content));

        private static byte[] RemoveHeader(byte[] content) =>
            content.Skip(DefaultHeaderLength).ToArray();

        private static byte[] GetHeader(byte[] content) =>
            content.Take(DefaultHeaderLength).ToArray();

        private static byte[] HexToBytes(string hex) =>
            Enumerable.Range(0, hex.Length / 2)
                      .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                      .ToArray();

        private static string BytesToHex(byte[] bytes) =>
            BitConverter.ToString(bytes).Replace("-", "").ToLower();

        private static void EnsureDirectoryExists(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
