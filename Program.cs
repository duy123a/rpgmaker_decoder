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
            var decryptor = new RPGMakerDecryptor();

            var encryptedFileList = locator.GetFilesWithFakeExtensions()?.ToList();

            if (encryptedFileList == null || !encryptedFileList.Any())
            {
                Console.WriteLine("No files need to be decrypted");
                return;
            }

            var firstImage = encryptedFileList.FirstOrDefault(x =>
                (Path.GetExtension(x)?.Equals(".rpgmvp", StringComparison.OrdinalIgnoreCase) ?? false)
                || (Path.GetExtension(x)?.Equals(".png_", StringComparison.OrdinalIgnoreCase) ?? false));

            if (string.IsNullOrEmpty(firstImage))
            {
                Console.WriteLine("No encrypted image file, so we can't get a decryption key");
                return;
            }

            try
            {
                decryptor.ExtractEncryptionKey(new RPGMakerFile(firstImage, baseDirectory));
                Console.WriteLine($"Decryption key extracted: {decryptor.DecryptKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract decryption key: {ex.Message}");
                return;
            }

            Console.WriteLine($"Starting decryption of {encryptedFileList.Count} files...");

            int processedCount = 0;

            foreach (var encryptedFile in encryptedFileList)
            {
                try
                {
                    decryptor.DecryptFile(new RPGMakerFile(encryptedFile, baseDirectory));
                    processedCount++;

                    Console.WriteLine($"Decrypted successfully: {Path.GetFileName(encryptedFile)}");

                    // Calculate and display progress
                    int progressPercentage = (processedCount * 100) / encryptedFileList.Count;
                    Console.Write($"\rProgress: {progressPercentage}%\t");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError processing file '{encryptedFile}': {ex.Message}");
                }
            }

            Console.WriteLine("\nDecryption completed!");

            // Stop timing and print execution time
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadKey();
        }
    }
}
