using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Amazon;
using Amazon.S3;
//using Amazon.S3.
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.S3.Transfer;
using PolyCloud.Storage;

namespace PolyCloud.Storage.NetCore
{
    //****************
    //*              *
    //*  AWSStorage  *
    //*              *
    //****************
    // AWSStorage - Storage implementation for Amazon Web Services S3.

    public class AWSStorage : Storage
    {
        private StorageAccount Account = null;
        private String AccessKey { get; set; }             // AWS Access Key
        private String SecretKey { get; set; }              // AWS Secret Key

        private String S3Endpoint = null;                   // If not-null, a use a custom AWS endpoint to talk to S3.
        private BasicAWSCredentials Credentials = null;

        private IAmazonS3 StorageClient = null;
        //private AmazonS3Client StorageClient = null;
        private S3Region Region = null;
        private RegionEndpoint RegionEndpoint = null;

        #region Constructor

        // Constructor. Key and Id for AWS are required.
        public AWSStorage(StorageAccount account, String accessKey, String secretKey, String endpoint) : base()
        {
            this.Account = account;
            this.PlatformName = "AWS";
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.S3Endpoint = endpoint;
        }

        #endregion

        #region Connection

        // Return a connection string suitable for saving / loading

        public override String Connection
        {
            get
            {
                //String value = "AccountId=" + this.AccessKey + ";AccountKey=" + this.SecretKey + ";S3Endpoint=" + this.S3Endpoint;
                String value = "AccountKey=" + this.AccessKey + ";SecretKey=" + this.SecretKey + ";S3Endpoint=" + this.S3Endpoint;
                return value;
            }
        }

        #endregion

        #region Open

        // Open connection.
        // Returns true on success, false on error.

        public override bool Open()
        {
            try
            {
                this.Exception = null;
                if (this.AccessKey != null)
                {
                    this.Credentials = new BasicAWSCredentials(this.AccessKey, this.SecretKey);
                }
                this.Region = S3Region.US;
                AmazonS3Config config = new AmazonS3Config { ServiceURL = this.S3Endpoint };
                String regionName = this.S3Endpoint;

                config.RegionEndpoint = this.RegionEndpoint = RegionEndpoint.GetBySystemName(this.S3Endpoint);

                if (this.AccessKey != null)
                {
                    StorageClient = new AmazonS3Client(this.Credentials, config);   // Explicit authentication from supplied access key & secret key
                }
                else
                {
                    StorageClient = new AmazonS3Client(config);                     // Implicit authentication from EC2 role
                }
                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        #endregion

        #region Close

        // Close connection.

        public override bool Close()
        {
            return true;
        }

        #endregion

        #region ListFolders

        // Return a list of folders (buckets)
        // Returns collection of CloudFolder objects, or null on error.

        public override List<CloudFolder> ListFolders()
        {
            try
            {
                this.Exception = null;
                List<CloudFolder> results = new List<CloudFolder>();

                Task<ListBucketsResponse> task = StorageClient.ListBucketsAsync();
                task.Wait();
                ListBucketsResponse response = task.Result;

                if (response != null && response.Buckets != null)
                {
                    foreach (S3Bucket bucket in response.Buckets)
                    {
                        GetBucketLocationRequest GBLrequest = new GetBucketLocationRequest()
                        {
                            BucketName = bucket.BucketName
                        };
                        Task<GetBucketLocationResponse> task2 = StorageClient.GetBucketLocationAsync(GBLrequest);
                        task2.Wait();
                        GetBucketLocationResponse GBLresponse = task2.Result;
                        if (GBLresponse.Location == this.Region)
                        {
                            results.Add(new CloudFolder(bucket.BucketName, this.Account, bucket));
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return null;
            }
        }

        #endregion

        #region ListFiles

        // List files in a bucket (note: cloud platform may limit response to first 1,000)
        // Returns collection of CloudFile objects, or null on error.

        public override List<CloudFile> ListFiles(CloudFolder bucket)
        {
            try
            {
                this.Exception = null;

                List<CloudFile> results = new List<CloudFile>();

                ListObjectsRequest request = new ListObjectsRequest()
                {
                    BucketName = bucket.Name
                };

                Task<ListObjectsResponse> task = this.StorageClient.ListObjectsAsync(request);
                task.Wait();
                ListObjectsResponse response = task.Result;
                if (response != null && response.S3Objects != null)
                {
                    foreach (S3Object obj in response.S3Objects)
                    {
                        results.Add(new CloudFile(bucket, obj.Key, (ulong)obj.Size, null /* ContentType */, obj.LastModified, null /* ETag */, obj));
                    }
                    bucket.ItemsLoaded = true;
                }
                return results;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return null;
            }
        }

        #endregion

        // Download object to local file
        // Returns true on success, false on error.

        public override bool DownloadFile(CloudFile file, String outputFilePath)
        {
            try
            {
                this.Exception = null;
                int pos = outputFilePath.LastIndexOf("\\");
                if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

                S3Object obj = file.StorageObject as S3Object;
                TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(this.AccessKey, this.SecretKey, this.RegionEndpoint));
                fileTransferUtility.Download(outputFilePath, obj.BucketName, file.Name);

                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        // Download object to local file
        // Returns true on success, false on error.

        public override bool DownloadFile(String folder, String file, String outputFilePath)
        {
            try
            {
                this.Exception = null;
                int pos = outputFilePath.LastIndexOf("\\");
                if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

                TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(this.AccessKey, this.SecretKey, this.RegionEndpoint));
                fileTransferUtility.Download(outputFilePath, folder, file);

                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        // Upload local file to bucket
        // Returns true on success, false on error.

        public override bool UploadFile(CloudFolder bucket, String file)
        {
            try
            {
                this.Exception = null;
                TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(this.AccessKey, this.SecretKey, this.RegionEndpoint));
                fileTransferUtility.Upload(file, bucket.Name);
                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }


        // Upload local file to bucket
        // Returns true on success, false on error.

        public override bool UploadFile(String bucket, String file)
        {
            try
            {
                this.Exception = null;
                TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(this.AccessKey, this.SecretKey, this.RegionEndpoint));
                fileTransferUtility.Upload(file, bucket);
                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        // New folder
        // Returns true on success, false on error.

        public override bool NewFolder(String bucket)
        {
            try
            {
                this.Exception = null;
                this.StorageClient.PutBucketAsync(bucket).Wait();
                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        // Delete folder
        // Returns true on success, false on error.

        public override bool DeleteFolder(String bucket)
        {
            try
            {
                this.Exception = null;

                // First delete any objects in the bucket

                List<String> objects = new List<string>();

                ListObjectsRequest request = new ListObjectsRequest()
                {
                    BucketName = bucket
                };

                Task<ListObjectsResponse> task = this.StorageClient.ListObjectsAsync(request);
                task.Wait();
                ListObjectsResponse response = task.Result;
                if (response != null && response.S3Objects != null)
                {
                    foreach (S3Object obj in response.S3Objects)
                    {
                        objects.Add(obj.Key);
                    }
                }

                foreach (String key in objects)
                {
                    this.StorageClient.DeleteObjectAsync(bucket, key).Wait();
                }

                // Now delete the empty bucket

                this.StorageClient.DeleteBucketAsync(bucket).Wait();

                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        // Delete file
        // Returns true on success, false on error.

        //public override bool DeleteFile(String bucket, String file)
        //{
        //    return DeleteFileAsync(bucket, file).Wait();
        //}

        public override bool DeleteFile(String bucket, String file)
        { 
            try
            {
                this.Exception = null;
                Task<DeleteObjectResponse> task = this.StorageClient.DeleteObjectAsync(bucket, file);
                task.Wait();
                return true;
            }
            catch (Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }
    }
}
