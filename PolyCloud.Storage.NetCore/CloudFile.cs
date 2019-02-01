using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolyCloud.Storage;

namespace PolyCloud.Storage.NetCore
{
    //***************
    //*             *
    //*  CloudFile  *
    //*             *
    //***************
    // Represents a cloud file (Azure blob | AWS/GCP object).

    public class CloudFile
    {
        public string Name { get; set; }            // Filename (may include a prefix/name)
        public String Icon { get; set; }            // File icon
        public ulong Size { get; set; }             // Size in bytes
        public String ContentType { get; set; }     // Content type
        public DateTime? Created { get; set; }      // Created date/time
        public String ETag { get; set; }            // ETag
        public Object StorageObject { get; set; }   // Storage object in cloud platform SDK
        public CloudFolder Folder { get; set; }     // Cloud folder (bucket/container that contains this file)

        #region Constructors

        public CloudFile() { }

        public CloudFile(String name)
        {
            this.Name = name;
        }

        public CloudFile(CloudFolder folder, String name, ulong size, String contentType, DateTime created, String ETag, Object platformObject = null)
        {
            this.Folder = folder;
            this.Name = name;
            this.Size = size;
            this.ContentType = contentType;
            this.Created = created;
            this.ETag = ETag;
            this.StorageObject = platformObject;
        }

        #endregion
    }
}


