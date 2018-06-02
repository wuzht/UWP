using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TimeReaper.Classes
{
    class ListItem
    {
        private string id;
        private bool isCompleted;

        private bool done;
        public bool isDoing
        {
            get
            {
                return done;
            }
            set
            {
                done = value;
            }
        }

        public string getId() { return id; }
        public void setID(string _id)
        {
            this.id = _id;
        }
        public string title { get; set; }//标题
        public string notes { get; set; }//备注
        public DateTime deadline { get; set; }//ddl

        public bool completed
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

        public void setComplete(int i)
        {
            if (i == 1)
                isCompleted = true;
            else
                isCompleted = false;
        }

        public void SetTime(string deadline)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy/MM/dd/hh/mm/ss";
            DateTime nowTime = Convert.ToDateTime(deadline, dateFormat);
            this.deadline = nowTime;
            //this.deadline = DateTime.Now;
        }

        public ListItem(string title, string deadline, string notes = "")
        {
            this.id = Guid.NewGuid().ToString(); //生成id
            this.title = title;
            SetTime(deadline);
            this.isCompleted = false; //默认为未完成
            this.notes = notes;

            isDoing = false;
        }
        public ListItem(string id, string title, string notes, string deadline)
        {
            this.id = id; //生成id
            this.title = title;
            SetTime(deadline);
            this.isCompleted = false; //默认为未完成
            this.notes = notes;

            isDoing = false;
        }


    }
}
