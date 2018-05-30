using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList.Models
{
    class ListItem
    {
        public string id;

        public string title { get; set; }
        
        public string description { get; set; }

        public DateTime date { get; set; }

        public ImageSource img { set; get; }

        public bool completed { get; set; }

        public int idInDatabase { get; set; }

        // 新建ListItem时使用
        public ListItem(string title, string description, DateTime date, ImageSource img, int idInDatabase)
        {
            this.id = Guid.NewGuid().ToString(); //generate id
            this.title = title;
            this.description = description;
            this.date = date;
            this.img = img;
            this.completed = false;
            this.idInDatabase = idInDatabase;
        }

        // 从数据库加载ListItem时使用
        public ListItem(string title, string description, DateTime date, bool completed, string imgPath, int idInDatabase)
        {
            this.id = Guid.NewGuid().ToString(); //generate id
            this.title = title;
            this.description = description;
            this.date = date;
            this.img = new BitmapImage(new Uri(imgPath));
            this.completed = completed;
            this.idInDatabase = idInDatabase;
        }
    }
}
