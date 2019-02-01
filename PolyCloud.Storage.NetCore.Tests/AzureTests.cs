using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolyCloud.Storage.NetCore;

namespace PolyCloud.Storage.Tests.NetCore
{
    [TestClass]
    public class AzureTests
    {
        [TestCategory("Azure.NetCore")]
        [TestMethod]
        public void CreateFolderAndFile()
        {
            Console.WriteLine("Test: Azure.NetCore | CreateFolderAndFile");

            // Set your storage account credentials here before running tests

            String accountId = "my-blob-storage-account";
            String accountKey = "Y7JTgm7B2g53t8MZ1N+GmuQ8d3aHyf0tGo5uglt7OIF3M2roaFiX6jPPI2mWmIOh/6suLEAMVXF5GPXPFl2nOQ==";

            using (PolyCloud.Storage.NetCore.Storage storage = PolyCloud.Storage.NetCore.Storage.Azure(accountId, accountKey))
            {
                storage.HandleErrors = false;
                storage.Open();

                // Create folder

                String folderName = "test-" + Guid.NewGuid().ToString();
                Console.WriteLine("Creating folder " + folderName);
                storage.NewFolder(folderName);

                // Create file in folder

                String fileName = "test.txt";
                String fileName2 = "test2.txt";

                Console.WriteLine("Creating file " + fileName);

                String content = "This is a test.";

                if (File.Exists(fileName)) File.Delete(fileName);
                File.WriteAllText(fileName, content);

                // Upload file

                Console.WriteLine("Uploading file " + fileName);

                storage.UploadFile(folderName, fileName);

                // Retrieve file

                Console.WriteLine("Downloading file " + fileName + " as " + fileName2);

                if (File.Exists(fileName2)) File.Delete(fileName2);
                storage.DownloadFile(folderName, fileName, fileName2);

                // Delete file

                Console.WriteLine("Deleting test file");
                storage.DeleteFile(folderName, fileName);

                // Delete folder

                Console.WriteLine("Deleting test folder");
                storage.DeleteFolder(folderName);

                Console.WriteLine("Verifying content");

                String content2 = File.ReadAllText(fileName2);
                Assert.AreEqual(content, content2, "Downloaded file content did not match uploaded file content");

                storage.Close();
            }
        }
    }

}
