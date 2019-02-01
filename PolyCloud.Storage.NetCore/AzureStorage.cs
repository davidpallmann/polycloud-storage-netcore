using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PolyCloud.Storage;

namespace PolyCloud.Storage.NetCore
{
    //******************
    //*                *
    //*  AzureStorage  *
    //*                *
    //******************
    // AzureStorage - Storage implementation for Microsoft Azure Blob Storage.

    public class AzureStorage : Storage
    {
        private StorageAccount Account { get; set; }
        private CloudStorageAccount StorageAccount { get; set; }
        private CloudBlobClient StorageClient { get; set; }
        private String ConnStr { get; set; }
        private String AccountName { get; set; }
        private String AccountKey { get; set; }

        // Constructor. Path to am Azure connection string is required.
        //public AzureStorage(Account account, String connectionString) : base()
        //{
        //    this.PlatformName = "Azure";
        //    //this.Account = account;
        //    this.ConnStr = connectionString;
        //}

        public AzureStorage(StorageAccount account, String accountName, String accountKey) : base()
        {
            this.PlatformName = "Azure";
            this.Account = account;
            this.AccountName = accountName;
            this.AccountKey = accountKey;
        }

        // Return a connection string suitable for saving / loading

        public override String Connection
        {
            get
            {
                String value = "accountName=" + this.AccountName + ";AccountKey=" + this.AccountKey;
                return value;
            }
        }

        public override bool Open()                            // Open (access) platform.
        {
            CloudStorageAccount storageAccount = null;

            if (!String.IsNullOrEmpty(this.ConnStr))
            {
                if (CloudStorageAccount.TryParse(this.ConnStr, out storageAccount))
                {
                    this.StorageAccount = storageAccount;
                    this.StorageClient = StorageAccount.CreateCloudBlobClient();
                    return true;
                }
            }
            else
            {
                this.StorageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(this.AccountName, this.AccountKey), false /* Use HTTPS */);
                this.StorageClient = StorageAccount.CreateCloudBlobClient();
                return true;
            }

            return false;
        }
        public override bool Close()                           // Close platform.
        {
            return true;
        }

        // Return a list of folders (containers)

        public override List<CloudFolder> ListFolders()
        {
            List<CloudFolder> results = new List<CloudFolder>();

            IEnumerable<CloudBlobContainer> containers = this.StorageClient.ListContainers();
            if (containers != null)
            {
                foreach (CloudBlobContainer container in containers)
                {
                    results.Add(new CloudFolder(container.Name, this.Account, container));
                }
            }

            return results;
        }

        // List files in a bucket

        public override List<CloudFile> ListFiles(CloudFolder bucket)
        {
            List<CloudFile> results = new List<CloudFile>();

            CloudBlobContainer container = bucket.PlatformObject as CloudBlobContainer; // new CloudBlobContainer(new Uri(bucket.Name));

            IEnumerable<IListBlobItem> blobs = container.ListBlobs(); //this.StorageClient.ListBlobs(bucket.Name, true, BlobListingDetails.Metadata);

            CloudBlob b = null;
            if (blobs != null)
            {
                foreach (var blob in blobs)
                {
                    if (blob is CloudBlob)
                    {
                        b = blob as CloudBlob;
                        b.FetchAttributes();
                        results.Add(new CloudFile(bucket, b.Name, (ulong)b.Properties.Length, b.Properties.ContentType, b.Properties.LastModified.HasValue ? b.Properties.LastModified.Value.DateTime : DateTime.MinValue, b.Properties.ETag, b));
                    }
                }
            }

            bucket.ItemsLoaded = true;

            return results;
        }

        // Download file

        public override bool DownloadFile(CloudFile file, String outputFilePath)
        {
            int pos = outputFilePath.LastIndexOf("\\");
            if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

            CloudBlob blob = file.StorageObject as CloudBlob;

            blob.DownloadToFile(outputFilePath, FileMode.CreateNew);

            return true;
        }

        public override bool DownloadFile(String folder, String file, String outputFilePath)
        {
            try
            {
                this.Exception = null;
                int pos = outputFilePath.LastIndexOf("\\");
                if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

                CloudBlob blob = this.StorageClient.GetContainerReference(folder).GetBlobReference(file);

                blob.DownloadToFile(outputFilePath, FileMode.CreateNew);

                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

    // Upload file

    public override bool UploadFile(CloudFolder bucket, String file)
        {
            return this.UploadFile(bucket.Name, file);
        }

        public override bool UploadFile(String bucket, String file)
        {
            String filename = file;
            int pos = filename.LastIndexOf("\\");
            if (pos != -1) filename = filename.Substring(pos + 1);
            CloudBlockBlob blob = this.StorageClient.GetContainerReference(bucket).GetBlockBlobReference(filename);

            blob.UploadFromFile(file);

            return true;
        }

        // New folder

        public override bool NewFolder(String bucket)
        {
            CloudBlobContainer container = this.StorageClient.GetContainerReference(bucket);
            container.CreateIfNotExists();
            return true;
        }

        // Delete folder

        public override bool DeleteFolder(String bucket)
        {
            CloudBlobContainer container = this.StorageClient.GetContainerReference(bucket);
            container.Delete();

            return true;
        }

        // Delete file

        public override bool DeleteFile(String bucket, String file)
        {
            CloudBlobContainer container = this.StorageClient.GetContainerReference(bucket);
            CloudBlob blob = container.GetBlobReference(file);
            blob.Delete();

            return true;
        }

    }
}
