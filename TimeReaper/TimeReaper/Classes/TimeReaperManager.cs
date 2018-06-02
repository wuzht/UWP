using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace TimeReaper.Classes
{
    class TimeReaperManager
    {
        private SQLiteConnection conn;
        private ObservableCollection<ListItem> allItems = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> AllItems { get { return this.allItems; } }
        private ObservableCollection<TaskItem> allTasks = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> AllTasks { get { return this.allTasks; } }

        private ListItem selectedItem;
        public ListItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }
        
        private static TimeReaperManager _instance;
        public static TimeReaperManager getInstance()
        {
            if (_instance == null)
            {
                _instance = new TimeReaperManager();
            }

            return _instance;
        }
        //应用间缓存
        public DateTimeOffset cacheBeginTime;
        public int cacheTimerState = -1;
        public DispatcherTimer cacheTimer=null;
        public bool cacheNeedPress;
        public bool cacheWork;
        public bool firstVisit=false;

        string FixedData(string current, int x)
        {
            if (x < 10)
                return "0" + current;
            else
                return current;
        }
        public string getTimeStr(DateTimeOffset date)
        {
            string year = FixedData(date.Year.ToString(), date.Year);
            string month = FixedData(date.Month.ToString(), date.Month);
            string day = FixedData(date.Day.ToString(), date.Day);
            string hour = FixedData(date.Hour.ToString(), date.Hour);
            string minute = FixedData(date.Minute.ToString(), date.Minute);

            return year + "-" + month + "-" + day + " " + hour + ":" + minute + ":00";
        }
        public TimeReaperManager()
        {
            cacheTimerState = -1;
            conn = new SQLiteConnection("timeReaper.db");
            using (var statement = conn.Prepare("CREATE TABLE IF NOT EXISTS todolist (id CHAR(36),title VARCHAR(255),notes VARCHAR(255),deadline DATETIME, PRIMARY KEY (id));"))
            {
                statement.Step();
            }
            using (var statement = conn.Prepare("SELECT * FROM todolist;"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    ListItem item = new ListItem((string)statement[0], (string)statement[1], (string)statement[2], (string)statement[3]);//id title deadline notes
                    this.allItems.Add(item);
                }
            }
            using (var statement = conn.Prepare("CREATE TABLE IF NOT EXISTS tasklist (id CHAR(36),title VARCHAR(255),notes VARCHAR(255),deadline DATETIME,taskId CHAR(36),beginTime DATETIME,endtime DATETIME, PRIMARY KEY (taskId));"))
            {
                statement.Step();
            }
            using (var statement = conn.Prepare("SELECT * FROM tasklist;"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    TaskItem task = createDoingTask((string)statement[0], (string)statement[1], (string)statement[2],(string) statement[3],(string)statement[4],(string)statement[5],(string)statement[6]);
                    this.allTasks.Add(task);
                }
            }
        }

        public void AddTodoItem(string title, string notes, string time)
        {

            this.allItems.Add(new ListItem(title, time, notes));
            ListItem item = allItems[allItems.Count - 1];
            using (var statement = conn.Prepare("INSERT INTO todolist VALUES(?,?,?,?);"))
            {
                statement.Bind(1, item.getId());
                statement.Bind(2, item.title);
                statement.Bind(3, item.notes);
                statement.Bind(4, time);
                statement.Step();
            }
        }
        public void AddTaskItem(string id, DateTimeOffset beginTime, DateTimeOffset endTime)
        {
            TaskItem item = createDoingTask(id, beginTime, endTime);
            allTasks.Add(item);
            using (var statement = conn.Prepare("INSERT INTO tasklist VALUES(?,?,?,?,?,?,?);"))
            {
                statement.Bind(1, item.getId());
                statement.Bind(2, item.title);
                statement.Bind(3, item.notes);
                statement.Bind(4, this.getTimeStr(item.deadline));
                statement.Bind(5, item.getTaskId());
                statement.Bind(6, item.getStrTime(beginTime));
                statement.Bind(7, item.getStrTime(endTime));
                
                statement.Step();
            }
        }

        public void RemoveTodoItem(ListItem item)
        {
            using (var statement = conn.Prepare("DELETE FROM todolist WHERE id = ?;"))
            {
                statement.Bind(1, item.getId());
                statement.Step();
            }
            this.allItems.Remove(item);
            this.selectedItem = null;
        }

        public void RemoveTaskitem(TaskItem item)
        {
            using (var statement = conn.Prepare("DELETE FROM tasklist WHERE taskId = ?;"))
            {
                statement.Bind(1, item.getTaskId());
                statement.Step();
            }
            this.allTasks.Remove(item);
        }

        public void UpdateTodoItem(ListItem item)
        {
            using (var statement = conn.Prepare("UPDATE todolist SET title = ?,notes = ?,deadline = ? WHERE id = ?;"))
            {
                statement.Bind(1, item.title);
                statement.Bind(2, item.notes);

                DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
                dateFormat.ShortDatePattern = "yyyy/MM/dd/hh/mm/ss";
                DateTime nowTime = Convert.ToDateTime(item.deadline, dateFormat);
                statement.Bind(3, nowTime.Year.ToString() + "-" + nowTime.Month.ToString() + "-" + nowTime.Day.ToString() + " " + nowTime.Hour.ToString() + ":" + nowTime.Minute.ToString() + ":00");
                statement.Bind(4, item.getId());

                statement.Step();
            }
            
        }
        public void UpdateTaskItem(TaskItem item)
        {
            using (var statement = conn.Prepare("UPDATE tasklist SET id = ?,title=?,notes=?,deadline=?,beginTime = ?,endTime = ? WHERE taskId = ?;"))
            {
                statement.Bind(1, item.getId());
                statement.Bind(2, item.title);
                statement.Bind(3, item.notes);
                statement.Bind(4, this.getTimeStr(item.deadline));
                statement.Bind(5, item.getStrTime(item.beginTime));
                statement.Bind(6, item.getStrTime(item.endTime));
                statement.Bind(7, item.getTaskId());

                statement.Step();
            }

        }
        public ListItem getListItem(string id)
        {
            foreach(ListItem item in allItems)
            {
                if(item.getId().Equals(id))
                {
                    return item;
                }
            }
            return null;
        }

        public TaskItem createDoingTask(string id, DateTimeOffset beginTime,DateTimeOffset endTime)
        {
            ListItem item = this.getListItem(id);
            return new TaskItem(item, beginTime, endTime);
        }
        public TaskItem createDoingTask(string id, string bTime, string eTime)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy/MM/dd/hh/mm/ss";
            DateTime begintime = Convert.ToDateTime(bTime, dateFormat);
            DateTime endtime = Convert.ToDateTime(eTime, dateFormat);

            DateTimeOffset beginTime = new DateTimeOffset(begintime);
            DateTimeOffset endTime = new DateTimeOffset(endtime);
            ListItem item = this.getListItem(id);
            return new TaskItem(item, beginTime, endTime);
        }
        public TaskItem createDoingTask(string id,string taskId, string bTime, string eTime)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy/MM/dd/hh/mm/ss";
            DateTime begintime = Convert.ToDateTime(bTime, dateFormat);
            DateTime endtime = Convert.ToDateTime(eTime, dateFormat);

            DateTimeOffset beginTime = new DateTimeOffset(begintime);
            DateTimeOffset endTime = new DateTimeOffset(endtime);
            ListItem item = this.getListItem(id);
            return new TaskItem(item, beginTime, endTime,taskId);
        }
        //为数据库开的接口，不确定是否存在这样的id
        public TaskItem createDoingTask(string id,string title,string notes,string deadline,string taskId,string bTime,string eTime)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.ShortDatePattern = "yyyy/MM/dd/hh/mm/ss";
            DateTime begintime = Convert.ToDateTime(bTime, dateFormat);
            DateTime endtime = Convert.ToDateTime(eTime, dateFormat);
            DateTime deadtime = Convert.ToDateTime(deadline, dateFormat);
            DateTimeOffset beginTime = new DateTimeOffset(begintime);
            DateTimeOffset endTime = new DateTimeOffset(endtime);


            return new TaskItem(id, title, notes, deadtime, taskId,beginTime, endTime);

        }
    }
}
