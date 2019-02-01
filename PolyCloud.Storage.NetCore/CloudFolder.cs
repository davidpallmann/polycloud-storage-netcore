using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PolyCloud.Storage.NetCore
{
    //*****************
    //*               *
    //*  CloudFolder  *
    //*               *
    //*****************
    // Represents a cloud folder (GCP bucket | AWS bucket | Azure container)

    public class CloudFolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name { get; set; }

        public bool ItemsLoaded { get; set; }

        public StorageAccount Account { get; set; }

        private ObservableCollection<CloudFile> _items = new ObservableCollection<CloudFile>();
        public ObservableCollection<CloudFile> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                NotifyPropertyChanged();
            }
        }

        public Object PlatformObject { get; set; }

        public String ItemCount
        {
            get
            {
                if (this.ItemsLoaded)
                {
                    if (this.Items == null)
                    {
                        return "(0)";
                    }
                    else if (this.Items.Count >= 1000)
                    {
                        return "(1000+)";
                    }
                    else
                    {
                        return "(" + this.Items.Count.ToString() + ")";
                    }
                }
                else
                {
                    return ""; // "?";
                }
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public bool IsExpanded { get; set; }

        public CloudFolder()
        {
            this.Items = new ObservableCollection<CloudFile>();
        }

        public CloudFolder(String name)
        {
            this.Name = name;
            this.Items = new ObservableCollection<CloudFile>();
        }

        public CloudFolder(String name, StorageAccount account, Object platformObject)
        {
            this.Name = name;
            this.Account = account;
            this.Items = new ObservableCollection<CloudFile>();
            this.PlatformObject = platformObject;
        }

    }

}


