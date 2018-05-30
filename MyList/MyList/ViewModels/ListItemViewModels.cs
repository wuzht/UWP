using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace MyList.ViewModels
{
    class ListItemViewModels
    {
        private static ListItemViewModels myInstance = null;
        public static ListItemViewModels GetInstance()
        {
            if (myInstance == null)
                myInstance = new ListItemViewModels();
            return myInstance;
        }
        private ObservableCollection<Models.ListItem> allItems = new ObservableCollection<Models.ListItem>();
        public System.Collections.ObjectModel.ObservableCollection<Models.ListItem> AllItems { get { return this.allItems; } }

        private Models.ListItem selectedItem = null;
        public Models.ListItem SelectedItem { get { return selectedItem; } set { selectedItem = value; } }

        private ListItemViewModels()
        {
            selectedItem = null;
            var db = App.conn;   
            using (var statement = db.Prepare(App.SQL_QUERY_VALUE))
            {
                while (SQLitePCL.SQLiteResult.ROW == statement.Step())
                {
                    this.allItems.Add(new Models.ListItem(statement[0].ToString(), statement[1].ToString(),
                        Convert.ToDateTime(statement[2].ToString()), Convert.ToBoolean(statement[3].ToString()),
                        statement[4].ToString(), Convert.ToInt32(statement[5])));
                }
            }
            /*
            ImageSource imgSource1 = new BitmapImage(new Uri("ms-appx:///Assets/Pics/1.jpg"));
            ImageSource imgSource2 = new BitmapImage(new Uri("ms-appx:///Assets/Pics/2.jpg"));
            ImageSource imgSource3 = new BitmapImage(new Uri("ms-appx:///Assets/Pics/3.jpg"));
            ImageSource imgSource4 = new BitmapImage(new Uri("ms-appx:///Assets/Pics/4.jpg"));
            Models.ListItem item1 = new Models.ListItem("数据结构", "Data Structure", DateTime.Now, imgSource1);
            item1.completed = true;
            Models.ListItem item2 = new Models.ListItem("现代操作系统应用开发", "MOSAD", DateTime.Now, imgSource2);
            allItems.Add(item1);
            allItems.Add(item2);
            allItems.Add(new Models.ListItem("操作系统（OS）", "Opreation System", DateTime.Now, imgSource3));
            allItems.Add(new Models.ListItem("计算机组成原理与接口技术", "Computer Organization and Design", DateTime.Now, imgSource4));
            */
        }
        
        public bool IsIdInDataBaseUnique(int idInDB)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].idInDatabase == idInDB)
                    return false;
            }
            return true;
        }

        public Models.ListItem GetListItemById(string id)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].id == id)
                {
                    return allItems[i];
                }
            }
            return null;
        }
        
        public void AddTodoItem(string title, string description, DateTime date, ImageSource img, int idInDataBase)
        {
            this.allItems.Add(new Models.ListItem(title, description, date, img, idInDataBase));
        }

        public void RemoveTodoItem(string id)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].id == id)
                {
                    this.allItems.RemoveAt(i);
                }
            }
            // set selectedItem to null after remove
            this.selectedItem = null;
        }

        public void UpdateTodoItem(string id, string title, string description, DateTime date, ImageSource img)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].id == id)
                {
                    allItems[i].title = title;
                    allItems[i].description = description;
                    allItems[i].date = date;
                    allItems[i].img = img;
                    break;
                }
            }
            // set selectedItem to null after update
            this.selectedItem = null;
        }
    }
}
