using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyCloud.Storage.NetCore
{
    //********************
    //*                  *
    //*  StorageAccount  *
    //*                  *
    //********************
    // StorageAccount : Represents a cloud storage account. The Storage property is the storage client for the account.

    public class StorageAccount
    {
        #region Properties

        public string Name { get; set; }                                // Account reference name (optional)
        public string Timestamp { get; set; }                           // Timestamp - when last accessed
        public Storage Storage { get; set; }                            // Azure, AWS, GCP storage client
        public String Icon { get; set; }                                // Platform icon
        public ObservableCollection<CloudFolder> Items { get; set; }    // Collection of folders
        public bool ItemsLoaded { get; set; }                           // True if account items have been loaded

        #endregion

        #region Constructors

        public StorageAccount()
        {
            this.Items = new ObservableCollection<CloudFolder>();
        }

        // Instantiate an AWS storage account

        public static StorageAccount AWS(String accessKey, String secretKey, String endpoint)
        {
            return new StorageAccount("AWS1", "AWS", accessKey, secretKey, endpoint);
        }

        public static StorageAccount AWS(String endpoint)
        {
            return new StorageAccount("AWS1", "AWS", null, null, endpoint);
        }

        // Instantiate an Azure storage account

        public static StorageAccount Azure(String accountId, String accountKey)
        {
            return new StorageAccount("Azure1", "Azure", accountId, accountKey);
        }

        // Instantiate a GCP storage account

        public static StorageAccount GCP(String jsonFile, String projectId)
        {
            return new StorageAccount("GCP1", "GCP", jsonFile, projectId);
        }

        public StorageAccount(String name, String platformName, String key, String projectId, String endpoint = null)
        {
            this.Name = name;
            switch (platformName)
            {
                case "select":
                    this.Storage = null;
                    break;
                case "AWS":
                    this.Storage = new AWSStorage(this, key, projectId, endpoint);   // details : Id, Key
                    break;
                case "Azure":
                    if (String.IsNullOrEmpty(projectId))
                    {
                        this.Storage = new AzureStorage(this, name, key);   // details : connection string
                    }
                    else
                    {
                        this.Storage = new AzureStorage(this, key, projectId);   // details : name, key
                    }
                    break;
                case "GCP":
                    this.Storage = new GCPStorage(this, key, projectId);   // details : JsonPath, ProjectId
                    break;
                default:
                    break;
            }
            this.Items = new ObservableCollection<CloudFolder>();
        }
    }

    #endregion
}


