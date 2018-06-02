using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeReaper.Classes
{
    //已经完成计时的部分
    class TaskItem
    {
        private DateTimeOffset btime;
        private DateTimeOffset etime;
        public string formatBeginTime;
        public string formatEndTime;

        public DateTimeOffset beginTime
        {
            get
            {
                return btime;
            }
            set
            {
                btime = value;
                formatBeginTime = btime.Year + "-" + btime.Month + "-" + btime.Day + " " + btime.Hour + ":" + btime.Minute + ":" + btime.Second;
            }
        }
        public DateTimeOffset endTime
        {
            get
            {
                return etime;
            }
            set
            {
                etime = value;
                formatEndTime = etime.Year + "-" + etime.Month + "-" + etime.Day + " " + etime.Hour + ":" + etime.Minute + ":" + etime.Second;
            }
        }
        /*
         taskitem中保存一份listitem的快照
             */
        private string itemId;
        public string title;
        public string notes;
        public DateTime deadline;
        string taskId;
        public TaskItem(ListItem item,DateTimeOffset begin,DateTimeOffset end)
        {
            itemId = item.getId();
            beginTime = begin;
            endTime = end;
            title = item.title;
            notes = item.notes;
            deadline = item.deadline;
            this.taskId = Guid.NewGuid().ToString();
        }
        public TaskItem(ListItem item, DateTimeOffset begin, DateTimeOffset end,string taskId)
        {
            itemId = item.getId();
            beginTime = begin;
            endTime = end;
            title = item.title;
            notes = item.notes;
            deadline = item.deadline;
            this.taskId = Guid.NewGuid().ToString();
        }
        //为数据库留的接口，接收一个ListItem的快照，防止ListItem被删除的情况
        public TaskItem(string id,string title,string notes,DateTime deadline, string taskId,DateTimeOffset begin,DateTimeOffset end)
        {
            itemId = id;
            this.title = title;
            this.notes = notes;
            this.deadline = deadline;
            beginTime = begin;
            endTime = end;
            this.taskId = taskId;
        }


        public string getId()
        {
            return itemId;
        }
        string zeroExtend(int i)
        {
            string t = i.ToString();
            if(i<10)
            {
                t = "0" + t;
            }
            return t;
        }

        public string getStrTime(DateTimeOffset date)
        {
            string ans;
            string year = date.Year.ToString();
            if(date.Year < 10)
            {
                year = "000" + year;
            }
            else if(date.Year < 100)
            {
                year = "00" + year;
            }
            else if(date.Year < 1000)
            {
                year = "0" + year;
            }

            string month = zeroExtend(date.Month);
            string day = zeroExtend(date.Day);
            string hour = zeroExtend(date.Hour);
            string minute = zeroExtend(date.Minute);
            string second = zeroExtend(date.Second);

            ans = year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + second;
            return ans;
        }

        public string getTaskId()
        {
            return taskId;
        }
        
    }
}
