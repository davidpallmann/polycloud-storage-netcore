using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.StaticFiles;
using PolyCloud;

namespace PolyCloud.Storage.NetCore
{
    //****************
    //*              *
    //*  GCPStorage  *
    //*              *
    //****************
    // GCPStorage - Storage implementation for Google Cloud Platform Storage.

    public class GCPStorage : Storage
    {
        private StorageAccount Account { get; set; }
        private String JsonPath { get; set; }
        private StorageClient StorageClient { get; set; }
        private String ProjectId { get; set; }

        // Constructor. Path to a GCP credentials JSON file is required.
        public GCPStorage(StorageAccount account, String jsonPath, String projectId) : base()
        {
            this.PlatformName = "GCP";
            this.Account = account;
            this.JsonPath = jsonPath;
            this.ProjectId = projectId;
        }

        // Return a connection string suitable for saving / loading

        public override String Connection
        {
            get
            {
                String value = "JsonPath=" + this.JsonPath + ";ProjectId=" + this.ProjectId;
                return value;
            }
        }

        #region Open

        // Open connection.

        public override bool Open()                            // Open (access) platform.
        {
            try
            {
                this.Exception = null;
                //StorageClient = StorageClient.Create(Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(@"C:\dev\gcp\gcp-storage\helloworld-228219-ba4bb62a59c8.json"));
                StorageClient = StorageClient.Create(Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(JsonPath));

                return true;
            }
            catch(Exception ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        #endregion

        #region Close

        // Close connection.

        public override bool Close()                           // Close platform.
        {
            return true;
        }

        #endregion

        #region ListFolders

        // Return a list of folders (buckets)

        public override List<CloudFolder> ListFolders()
        {
            try
            {
                this.Exception = null;
                List<CloudFolder> results = new List<CloudFolder>();

                var folderList = StorageClient.ListBuckets(ProjectId);

                if (folderList != null)
                {
                    foreach (Google.Apis.Storage.v1.Data.Bucket folder in folderList)
                    {
                        results.Add(new CloudFolder(folder.Name, this.Account, folder));
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

        // List files in a folder

        public override List<CloudFile> ListFiles(CloudFolder folder)
        {
            try
            {
                this.Exception = null;

                List<CloudFile> results = new List<CloudFile>();

                var fileList = StorageClient.ListObjects(folder.Name);

                if (fileList != null)
                {
                    foreach (Google.Apis.Storage.v1.Data.Object file in fileList)
                    {
                        results.Add(new CloudFile(folder, file.Name, file.Size.HasValue ? file.Size.Value : 0, file.ContentType, file.TimeCreated.HasValue ? file.TimeCreated.Value : DateTime.MinValue, file.ETag, file));
                    }
                }

                folder.ItemsLoaded = true;

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

        #region DownloadFile

        // Download object to local file.
        // Returns true on success, false on error.

        public override bool DownloadFile(CloudFile file, String outputFilePath)
        {
            try
            {
                this.Exception = null;
                int pos = outputFilePath.LastIndexOf("\\");
                if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

                Google.Apis.Storage.v1.Data.Object obj = file.StorageObject as Google.Apis.Storage.v1.Data.Object;

                using (var outputFile = System.IO.File.OpenWrite(outputFilePath))
                {
                    this.StorageClient.DownloadObject(obj.Bucket, obj.Name, outputFile);
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

        public override bool DownloadFile(String folder, String file, String outputFilePath)
        {
            try
            {
                this.Exception = null;
                int pos = outputFilePath.LastIndexOf("\\");
                if (pos != -1) outputFilePath = outputFilePath.Substring(0, pos);

                //Google.Apis.Storage.v1.Data.Object obj = file.StorageObject as Google.Apis.Storage.v1.Data.Object;

                using (var outputFile = System.IO.File.OpenWrite(outputFilePath))
                {
                    this.StorageClient.DownloadObject(folder, file, outputFile);
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

        #region UploadFile

        // Upload local file to folder (bucket).
        // Returns true on success, false on error.

        public override bool UploadFile(CloudFolder folder, String file)
        {
            return UploadFile(folder.Name, file);
        }

        public override bool UploadFile(String folder, String file)
        {
            try
            {
                this.Exception = null;

                String filename = file;
                int pos = filename.LastIndexOf("\\");
                if (pos != -1) filename = filename.Substring(pos + 1);

                //String contentType = System.Web.MimeMapping.GetMimeMapping(filename);

                var provider = new FileExtensionContentTypeProvider();
                String contentType;
                if (!provider.TryGetContentType(filename, out contentType))
                {
                    contentType = "application/octet-stream";
                }

                using (var inputFile = System.IO.File.OpenRead(file))
                {
                    this.StorageClient.UploadObject(folder, filename, contentType, inputFile);
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

        #region NewFolder

        // New folder.
        // Returns true on success, false on error.

        public override bool NewFolder(String folder)
        {
            try
            {
                this.Exception = null;
                this.StorageClient.CreateBucket(this.ProjectId, folder, null);
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        #endregion

        #region DeleteFolder

        // Delete folder.
        // Returns true on success, false on error.

        public override bool DeleteFolder(String folder)
        {
            try
            {
                this.Exception = null;
                this.StorageClient.DeleteBucket(folder, null);
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        #endregion

        #region DeleteFile

        // Delete file.
        // Returns true on success, false on error.

        public override bool DeleteFile(String folder, String file)
        {
            try
            {
                this.Exception = null;
                this.StorageClient.DeleteObject(folder, file, null);
                return true;
            }
            catch (Google.GoogleApiException ex)
            {
                this.Exception = ex;
                if (!this.HandleErrors) throw ex;
                return false;
            }
        }

        #endregion
    }
}
