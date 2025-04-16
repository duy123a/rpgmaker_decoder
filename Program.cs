using System.Diagnostics;

namespace rpgmaker_decoder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Start timing
            var stopwatch = Stopwatch.StartNew();

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var locator = new RPGMakerLocator(baseDirectory);
            var decryptKey = string.Empty;

            var encryptedFileList = locator.GetFilesWithFakeExtensions()?.ToList();

            if (encryptedFileList == null || !encryptedFileList.Any())
            {
                Console.WriteLine("No files need to be decrypted");
                Console.ReadLine();
                return;
            }

            var firstImage = encryptedFileList.FirstOrDefault(x =>
                (Path.GetExtension(x)?.Equals(".rpgmvp", StringComparison.OrdinalIgnoreCase) ?? false)
                || (Path.GetExtension(x)?.Equals(".png_", StringComparison.OrdinalIgnoreCase) ?? false));

            if (string.IsNullOrEmpty(firstImage))
            {
                Console.WriteLine("No encrypted image file, so we can't get a decryption key");
                Console.ReadLine();
                return;
            }

            try
            {
                decryptKey = RPGMakerDecryptor.ExtractEncryptionKey(new RPGMakerFile(firstImage, baseDirectory));
                Console.WriteLine($"Decryption key extracted: {decryptKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract decryption key: {ex.Message}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Starting decryption of {encryptedFileList.Count} files...");

            int processedCount = 0;

            Parallel.ForEach(encryptedFileList, encryptedFile =>
            {
                try
                {
                    RPGMakerDecryptor.DecryptWithKey(new RPGMakerFile(encryptedFile, baseDirectory), decryptKey);
                    var currentCount = Interlocked.Increment(ref processedCount);

                    var progress = (currentCount * 100) / encryptedFileList.Count;
                    Console.WriteLine($"[{progress}%] Decrypted: {Path.GetFileName(encryptedFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError on {encryptedFile}: {ex.Message}");
                }
            });

            Console.WriteLine("\nDecryption completed!");

            // Stop timing and print execution time
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
        }
    }
}
