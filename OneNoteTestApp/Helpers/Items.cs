using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OneNoteTestApp.Helpers
{
    public class Item : System.ComponentModel.INotifyPropertyChanged
    {


        #region Private Fields
        private string _name = string.Empty;
        private string _id = string.Empty;
        private string _link = string.Empty;
        private ImageSource _image = null;
        
        #endregion

        #region Props
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        public string Id
        {
            get
            {
                return this._id;
            }

            set
            {
                if (this._id != value)
                {
                    this._id = value;
                    this.OnPropertyChanged("Id");
                }
            }
        }
        public string Link
        {
            get
            {
                return this._link;
            }

            set
            {
                if (this._link != value)
                {
                    this._link = value;
                    this.OnPropertyChanged("Link");
                }
            }
        }
        public ImageSource Image
        {
            get
            {
                return this._image;
            }

            set
            {
                if (this._image != value)
                {
                    this._image = value;
                    this.OnPropertyChanged("Image");
                }
            }
        } 
        #endregion

        #region Methods events and overrides

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetImage(String path)
        {
            Image = new BitmapImage(new Uri(path));
        } 
        #endregion

    }

    public class GroupInfoList<T> : List<object>
    {

        public object Key { get; set; }


        public new IEnumerator<object> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
        }
    }

    public class StoreData
    {
        public StoreData()
        {
        }

        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }

        internal List<GroupInfoList<object>> GetGroupsByLetter()
        {
            List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();

            var query = from item in Collection
                        orderby ((Item)item).Name
                        group item by ((Item)item).Name[0] into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList<object> info = new GroupInfoList<object>();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                groups.Add(info);
            }

            return groups;
        }
    }

    public class ItemCollection : IEnumerable<Object>
    {
        private System.Collections.ObjectModel.ObservableCollection<Item> itemCollection = new System.Collections.ObjectModel.ObservableCollection<Item>();

        public IEnumerator<Object> GetEnumerator()
        {
            return itemCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Item item)
        {
            itemCollection.Add(item);
        }
    } 
}
