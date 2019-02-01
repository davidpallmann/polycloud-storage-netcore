using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolyCloud.Storage;

namespace PolyCloud.Storage.NetCore
{
    //*************
    //*           *
    //*  Storage  *
    //*           *
    //*************
    // Abstract class - represents a cloud storage client, with a common set of properties and methods.
    // Specific implementations derive from this class with platform-specific code. 

    public abstract class Storage : IDisposable
    {
        public String PlatformName { get; set; }    // Platform name - GCP, Azure, AWS, Windows

        private bool _handleErrors = true;
        public bool HandleErrors                    // If true, handle errors and do not throw exceptions
        {     
            get
            {
                return _handleErrors;
            }
            set
            {
                _handleErrors = value;
            }     
        } 

        public Exception Exception { get; set; }    // Last exception
        protected bool disposed { get; set; }       // If true, class has been disposed

        #region Constructors

        // Constructor. Connection parameter details are platform-specific.

        public Storage()
        { }

        // Instantiate an AWS storage account

        public static Storage AWS(String accessKey, String secretKey, String endpoint)
        {
            return StorageAccount.AWS(accessKey, secretKey, endpoint).Storage;
        }


        public static Storage AWS(String endpoint)
        {
            return StorageAccount.AWS(endpoint).Storage;
        }

        // Instantiate an Azure storage account

        public static Storage Azure(String accountId, String accountKey)
        {
            return StorageAccount.Azure(accountId, accountKey).Storage;
        }

        // Instantiate a GCP storage account

        public static Storage GCP(String jsonFile, String projectId)
        {
            return StorageAccount.GCP(jsonFile, projectId).Storage;
        }

        #endregion

        public virtual String Connection
        {
            get
            {
                String value = "";
                return value;
            }
        }

        // Open (access) platform.

        public virtual bool Open()
        {
            return false;
        }

        // Close platform.

        public virtual bool Close()
        {
            return false;
        }

        // List folders

        public virtual List<CloudFolder> ListFolders()
        {
            return new List<CloudFolder>();
        }

        // List files in a folder

        public virtual List<CloudFile> ListFiles(CloudFolder folder)
        {
            return new List<CloudFile>();
        }

        // Download file

        public virtual bool DownloadFile(CloudFile file, String outputFilePath)
        {
            return false;
        }

        public virtual bool DownloadFile(String folder, String file, String outputFilePath)
        {
            return false;
        }

        // Upload file

        public virtual bool UploadFile(CloudFolder folder, String file)
        {
            return false;
        }

        // Upload file

        public virtual bool UploadFile(String folder, String file)
        {
            return false;
        }

        // New folder

        public virtual bool NewFolder(String folder)
        {
            return false;
        }

        // Delete folder

        public virtual bool DeleteFolder(String folder)
        {
            return false;
        }

        // Delete file

        public virtual bool DeleteFile(String folder, String file)
        {
            return false;
        }

        // Dispose - free unmanaged resources

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any unmanaged objects here.
                // Free any managed objects here.
           }

            disposed = true;
        }
    }
}
