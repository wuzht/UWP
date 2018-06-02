using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
using TimeReaper.Classes;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace TimeReaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            timeReaper = TimeReaperManager.getInstance();
            timer = null;
            timerState = 0;
            if(!timeReaper.firstVisit)
            {
                timeReaper.firstVisit = true;
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += OnShareDataRequested;
            }
            UpdateTile();
        }
        //储存结构
        TimeReaperManager timeReaper;
        
        //计时器相关
        DispatcherTimer timer;//计时器本体
        DateTimeOffset beginTime;//表示开始计时的时间
        DateTimeOffset endTime;//现在暂时没有实际作用
        int timerState;//0表示没有工作，1表示正常计时，2表示番茄钟计时
        bool needPress = false;//用户是否需要按下按钮（计时器抵达终点或者用户暂停，按钮等待点击以提交任务）
        bool isBack = false;//是否从外层页面回来。情况：从外层页面回来，如果在OnNavigated中创建计时器会被自动销毁。使用timeReaper进行规避
        //番茄钟相关
        int pomotodoWorkInterval = 1;//默认为25分钟
        int pomotodoShortBreak = 1;//默认为5分钟
        int pomotodoLongBreak = 2;//默认为15分钟
        int pomotodoRestInterval = 3;//短休息数量，3次短休息后一次长休息，当pomotodoPeriod==3时进行一次长休息
        int pomotodoPeriod = 0;//现在已经过了多少 个短休息
        bool work = false;//是否处于休息状态

        /*磁贴更新函数*/
        private void UpdateTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            XmlDocument document = new XmlDocument();
            document.LoadXml(System.IO.File.ReadAllText("Tile.xml"));
            XmlNodeList textElements = document.GetElementsByTagName("text");
            for (int i = 0; i < timeReaper.AllItems.Count; ++i)
            {
                // 小磁贴
                textElements[0].InnerText = timeReaper.AllItems[i].title;
                textElements[1].InnerText = timeReaper.AllItems[i].notes;
                // 中磁贴
                textElements[2].InnerText = timeReaper.AllItems[i].title;
                textElements[3].InnerText = timeReaper.AllItems[i].deadline.ToString();
                textElements[4].InnerText = timeReaper.AllItems[i].notes;
                // 宽磁贴
                textElements[5].InnerText = timeReaper.AllItems[i].title;
                textElements[6].InnerText = timeReaper.AllItems[i].deadline.ToString();
                textElements[7].InnerText = timeReaper.AllItems[i].notes;
                // 大磁贴
                textElements[8].InnerText = timeReaper.AllItems[i].title;
                textElements[9].InnerText = timeReaper.AllItems[i].deadline.ToString();
                textElements[10].InnerText = timeReaper.AllItems[i].notes;
                var tileNotification = new TileNotification(document);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            }
        }

        //常用函数：可以根据beginTime生成按钮上的正常计时信息
        private string getTimerStr()
        {
            int realHour = DateTimeOffset.Now.Hour - beginTime.Hour;
            int realMinute = DateTimeOffset.Now.Minute - beginTime.Minute;
            int realSecond = DateTimeOffset.Now.Second - beginTime.Second;
            if (realSecond< 0)
            {
                realSecond += 60;
                realMinute--;
            }
            if (realHour > 0)
            {
                realMinute += realHour* 60;
            }

            string strMinute = realMinute.ToString();
            string strSecond = realSecond.ToString();
            if (realSecond< 10)
            {
                strSecond = "0" + strSecond;
            }
            return strMinute + ":" + strSecond;
        }

        /*正计时函数*/
        private void Timer_Tick(object sender, object e)
        {
            if (!needPress)
            {
                MainTopStart.Content = getTimerStr();
            }
        }

        //临时函数：根据已有的beginTime生成番茄时间，确保不会超出范围.
        private string getPomotodoTime()
        {
            int realHour = DateTimeOffset.Now.Hour - beginTime.Hour;
            int realMinute = DateTimeOffset.Now.Minute - beginTime.Minute;
            int realSecond = DateTimeOffset.Now.Second - beginTime.Second;
            if (realSecond < 0)
            {
                realSecond += 60;
                realMinute--;
            }
            if (realHour > 0)
            {
                realMinute += realHour * 60;
            }
            int needMinute;
            if (work)
            {
                needMinute = pomotodoWorkInterval;
            }
            else if (!work && pomotodoPeriod == pomotodoRestInterval)
            {
                needMinute = pomotodoLongBreak;
            }
            else
            {
                needMinute = pomotodoShortBreak;
            }
            realMinute = needMinute - realMinute - 1;
            realSecond = 60 - realSecond;
            string strMinute = realMinute.ToString();
            string strSecond = realSecond.ToString();
            if (realSecond < 10)
            {
                strSecond = "0" + strSecond;
            }
            return strMinute + ":" + strSecond;
        }

        /*番茄计时函数*/
        private void Pomotodo_Tick(object sender, object e)
        {
            int realHour = DateTimeOffset.Now.Hour - beginTime.Hour;
            int realMinute = DateTimeOffset.Now.Minute - beginTime.Minute;
            int realSecond = DateTimeOffset.Now.Second - beginTime.Second;
            if (realSecond < 0)
            {
                realSecond += 60;
                realMinute--;
            }
            if (realHour > 0)
            {
                realMinute += realHour * 60;
            }
            //计算倒计时
            int needMinute;
            if (work)
            {
                needMinute = pomotodoWorkInterval;
            }
            else if (!work && pomotodoPeriod == pomotodoRestInterval)
            {
                needMinute = pomotodoLongBreak;
            }
            else
            {
                needMinute = pomotodoShortBreak;
            }

            realMinute = needMinute - realMinute - 1;
            realSecond = 60 - realSecond;
            //结束工作
            if (realMinute == -1 && realSecond == 60)//计时器倒计时达到界限
            {
                needPress = true;
                if (work)
                {
                    MainTopStart.Content = "点击休息";
                }
                else
                {
                    MainTopStart.Content = "点击工作";
                }
            }

            if (needPress)
            {
                return;//等待用户按下按钮，计时器暂停
            }

            string strMinute = realMinute.ToString();
            string strSecond = realSecond.ToString();
            if (realSecond < 10)
            {
                strSecond = "0" + strSecond;
            }
            MainTopStart.Content = strMinute + ":" + strSecond;

        }


        //计时器开始计时
        /*
         用DateTimeOffset.Now记录下开始时间
         用DateTimeOffset.Now记录下结束时间
         使用id拿取ListItem，然后生成TaskItem，存进数据库
         */
        private void MainTopStart_Click(object sender, RoutedEventArgs e)
        {
            if(isBack)
            {
                timer = timeReaper.cacheTimer;
                isBack = false;
                timeReaper.cacheTimer = null;
            }

            if (timer == null)
            {
                Media_SourceSelect(1);
                string timerMode = (MainTopSelect.SelectedValue as ComboBoxItem).Content.ToString();
                if (timerMode.Equals("正常计时"))
                {
                    timerState = 1;
                }
                else if (timerMode.Equals("番茄钟计时"))
                {
                    timerState = 2;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                if (timerState == 1)
                {
                    timer.Tick += Timer_Tick;
                    beginTime = DateTimeOffset.Now;
                    needPress = false;
                    timer.Start();
                }
                else if (timerState == 2)
                {
                    timer.Tick += Pomotodo_Tick;
                    beginTime = DateTimeOffset.Now;
                    work = true;
                    needPress = false;
                    timer.Start();
                }
            }
            else
            {
                //番茄钟工作状态
                if (timerState == 2)
                {
                    if (needPress)
                    {
                        Media_SourceSelect(2);
                        if (pomotodoPeriod == pomotodoRestInterval)//第3次为长休息，休息完以后归0
                        {
                            pomotodoPeriod = 0;
                        }
                        if (work)//每次工作完毕后，休息记录加1
                            pomotodoPeriod++;



                        if (work)//工作结束，将列表中提交的任务进行结束计时装载
                        {
                            foreach (ListItem listitem in timeReaper.AllItems)
                            {
                                if (listitem.isDoing)
                                {
                                    timeReaper.AddTaskItem(listitem.getId(), beginTime, DateTimeOffset.Now);
                                    listitem.isDoing = false;
                                }
                            }
                        }
                        beginTime = DateTimeOffset.Now;
                        work = !work;
                        needPress = false;
                    }
                }
                //正常计时状态
                else if (timerState == 1)
                {
                    if (!needPress)
                    {
                        Media_SourceSelect(1);
                        needPress = true;
                        MainTopStart.Content = "暂停，点击提交";
                    }
                    else
                    {
                        //提交任务，按钮恢复
                        Media_SourceSelect(1);
                        needPress = false;
                        foreach (ListItem listitem in timeReaper.AllItems)
                        {
                            if (listitem.isDoing)
                            {
                                timeReaper.AddTaskItem(listitem.getId(), beginTime, DateTimeOffset.Now);
                                listitem.isDoing = false;
                            }

                        }
                        MainTopStart.Content = "开始计时";
                        timer.Stop();
                        timer = null;
                        timerState = 0;
                    }
                }

            }
            UpdateTile();
        }

        /*取消计时函数
         作用：取消计时器，记录最后时间，(未完成：创建已完成计时任务对象并记录)
             */
        private void MainTopCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isBack)
            {
                timer = timeReaper.cacheTimer;
                isBack = false;
                timeReaper.cacheTimer = null;
            }
            if (timer != null)
            {
                Media_SourceSelect(3);
                timer.Stop();
                endTime = DateTimeOffset.Now;
                timer = null;
                MainTopStart.Content = "开始计时";
                timerState = 0;
            }

        }

        /*
         * 点击元素进入修改界面
         */
        private void MainLeftItemList_ItemClick(object sender, ItemClickEventArgs e)
        {
            timeReaper.SelectedItem = e.ClickedItem as ListItem;
            if(timer!=null)
            {
                timeReaper.cacheBeginTime = beginTime;
                timeReaper.cacheTimerState = timerState;
                timeReaper.cacheNeedPress = needPress;
                timeReaper.cacheWork = work;
            }

            Frame.Navigate(typeof(CreatePage));
        }

        /*创建新的任务，跳转到新的页面*/
        private void CreateNewItem(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timeReaper.cacheBeginTime = beginTime;
                timeReaper.cacheTimerState = timerState;
                timeReaper.cacheNeedPress = needPress;
                timeReaper.cacheWork = work;
            }
            Frame.Navigate(typeof(CreatePage));
        }
        //前往编辑页面（编辑与创造的区别在于timeReaper.SelectedItem是否为空）
        private void MenuFlyEdit_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (sender as FrameworkElement).DataContext;
            var item = MainLeftItemList.ContainerFromItem(datacontext) as ListViewItem;
            timeReaper.SelectedItem = (ListItem)(item.Content);

            if (timer != null)
            {
                timeReaper.cacheBeginTime = beginTime;
                timeReaper.cacheTimerState = timerState;
                timeReaper.cacheNeedPress = needPress;
                timeReaper.cacheWork = work;
            }

            Frame.Navigate(typeof(CreatePage));
        }

        //删除指定文件
        private void MenuFlyDelete_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (sender as FrameworkElement).DataContext;
            var item = MainLeftItemList.ContainerFromItem(datacontext) as ListViewItem;
            timeReaper.SelectedItem = (ListItem)(item.Content);
            timeReaper.RemoveTodoItem(timeReaper.SelectedItem);
            timeReaper.SelectedItem = null;
            UpdateTile();
        }

        /*删除已经完成的任务计时*/
        private void DeleteTaskItem_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (sender as FrameworkElement).DataContext;
            var item = MainRightDoneTask.ContainerFromItem(datacontext) as ListViewItem;
            timeReaper.RemoveTaskitem((TaskItem)item.Content);
            UpdateTile();
        }
        private void DeleteTaskItem_Click2(object sender, RoutedEventArgs e)
        {
            var datacontext = (sender as FrameworkElement).DataContext;
            var item = MainLeftTaskList.ContainerFromItem(datacontext) as ListViewItem;
            timeReaper.RemoveTaskitem((TaskItem)item.Content);
            UpdateTile();
        }

        //前往设置页面
        private void AppBarSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new SettingParameterPassing();

            parameters.pomotodoLongBreak = pomotodoLongBreak;
            parameters.pomotodoShortBreak = pomotodoShortBreak;
            parameters.pomotodoWorkInterval = pomotodoWorkInterval;
            parameters.pomotodoRestInterval = pomotodoRestInterval;

            if(timer!=null)
            {
                timeReaper.cacheBeginTime = beginTime;
                timeReaper.cacheTimerState = timerState;
                timeReaper.cacheNeedPress = needPress;
                timeReaper.cacheWork = work;
            }
            

            Frame.Navigate(typeof(SettingPage), parameters);
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (!e.Parameter.Equals(""))
                {
                    //更新设置
                    var parameters = (SettingParameterPassing)e.Parameter;
                    if (parameters != null)
                    {
                        pomotodoWorkInterval = parameters.pomotodoWorkInterval;
                        pomotodoShortBreak = parameters.pomotodoShortBreak;
                        pomotodoLongBreak = parameters.pomotodoLongBreak;
                        pomotodoRestInterval = parameters.pomotodoRestInterval;

                        /*每次修改的时候更新文件缓存*/
                        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        string format = "";
                        format += pomotodoWorkInterval + " ";
                        format += pomotodoShortBreak + " ";
                        format += pomotodoLongBreak + " ";
                        format += pomotodoRestInterval + " ";
                        StorageFile file = await localFolder.CreateFileAsync("dataFile.txt", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, format);
                    }
                }
            }

            if(timeReaper.cacheTimerState !=-1)
            {
                timerState = timeReaper.cacheTimerState;
                beginTime = timeReaper.cacheBeginTime;
                needPress = timeReaper.cacheNeedPress;
                work = timeReaper.cacheWork;
                isBack = true;
                var timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                if (timerState == 1)
                {
                    timer.Tick += Timer_Tick;
                    needPress = false;
                    timer.Start();
                    //更改一下按钮的值以显得没有重新开始过
                    if(needPress)
                    {
                        MainTopStart.Content = "暂停，点击提交";
                    }
                    else
                    {
                        MainTopStart.Content = getTimerStr();
                    }
                }
                else if (timerState == 2)
                {
                    timer.Tick += Pomotodo_Tick;
                    work = true;
                    needPress = false;
                    timer.Start();
                    //更改一下按钮的值以显得没有重新开始过
                    if(needPress)
                    {
                        if (work)
                        {
                            MainTopStart.Content = "点击休息";
                        }
                        else
                        {
                            MainTopStart.Content = "点击工作";
                        }
                    }
                    else
                    {
                        MainTopStart.Content = getPomotodoTime();
                    }
                }
                timeReaper.cacheTimer = timer;
                timeReaper.cacheTimerState = -1;
            }
            timeReaper.SelectedItem = null;
            UpdateTile();
        }
        //吴成文：分享指定文件
        //分享指定文件
        private void MenuFlyShare_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (sender as FrameworkElement).DataContext;
            var item = MainLeftItemList.ContainerFromItem(datacontext) as ListViewItem;
            timeReaper.SelectedItem = (ListItem)(item.Content);

            DataTransferManager.ShowShareUI();
        }

        public void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            var deferral = args.Request.GetDeferral();

            request.Data.Properties.Title = timeReaper.SelectedItem.title;
            request.Data.SetText(timeReaper.SelectedItem.notes);
            request.Data.Properties.Description = timeReaper.SelectedItem.notes;
            request.Data.SetWebLink(new Uri("http://seattletimes.com/ABPub/2006/01/10/2002732410.jpg"));
            deferral.Complete();
        }

        //吴槟负责的多媒体部分
        /*根据传入的信号，选择不同的播放器播放不同的音频*/
        private void Media_SourceSelect(int module)
        {
            /*点击按钮*/
            if (module == 1)
            {
                _startPlayer.Play();

            }
            /*休息提示*/
            else if (module == 2)
            {
                _restPlayer.Play();
            }
            /*取消任务*/
            else if (module == 3)
            {
                _stopPlayer.Play();
            }
        }
    }
    //传递给设置页的对象
    //考虑加上计时器信息：在传递的时候计时器会被清零，如果加上必要的信息可以伪装成
    public class SettingParameterPassing
    {
        public int pomotodoWorkInterval = 1;//默认为25分钟
        public int pomotodoShortBreak = 1;//默认为5分钟
        public int pomotodoLongBreak = 2;//默认为15分钟
        public int pomotodoRestInterval = 3;//短休息数量，3次短休息后一次长休息，当pomotodoPeriod==3时进行一次长休息
    }
 
}
