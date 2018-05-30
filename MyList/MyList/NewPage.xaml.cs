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

using Windows.UI.Popups;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        private ViewModels.ListItemViewModels ViewModel;

        public NewPage()
        {
            this.InitializeComponent();
            ViewModel = ViewModels.ListItemViewModels.GetInstance();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).isSuspend;
            if (suspending)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = TitleBox.Text;
                composite["des"] = DetailBox.Text;
                composite["date"] = DueDate.Date;
                composite["createBtnContent"] = Create_btn.Content;
                composite["imgUri"] = ((BitmapImage)Img.Source).UriSource.ToString();
                for (int i = 0; i < ViewModel.AllItems.Count; ++i)
                {
                    composite["ListItemTitle" + i] = ViewModel.AllItems[i].title;
                    composite["ListItemDes" + i] = ViewModel.AllItems[i].description;
                    composite["ListItemDate" + i] = (DateTimeOffset)ViewModel.AllItems[i].date;
                    composite["ListItemImgUri" + i] = ((BitmapImage)ViewModel.AllItems[i].img).UriSource.ToString();
                    composite["ListItemCompleted" + i] = ViewModel.AllItems[i].completed;
                    composite["ListItemIdInDataBase" + i] = ViewModel.AllItems[i].idInDatabase;
                }
                composite["hasSelectedItem"] = false;
                if (ViewModel.SelectedItem != null)
                {
                    composite["hasSelectedItem"] = true;
                    for (int i = 0; i < ViewModel.AllItems.Count; ++i)
                    {
                        if (ViewModel.SelectedItem.id == ViewModel.AllItems[i].id)
                        {
                            composite["selectedItemPosition"] = i;
                            break;
                        }
                    }
                }
                ApplicationData.Current.LocalSettings.Values["NewPage"] = composite;
            }
            //base.OnNavigatedFrom(e); 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ViewModel = ((ViewModels.ListItemViewModels)e.Parameter);
            if (ViewModel.SelectedItem == null)
            {
                Create_btn.Content = "Create";
                PanelReset();
            }
            else
            {
                Create_btn.Content = "Update";
                TitleBox.Text = ViewModel.SelectedItem.title;
                DetailBox.Text = ViewModel.SelectedItem.description;
                DueDate.Date = ViewModel.SelectedItem.date.Date;
                Img.Source = ViewModel.SelectedItem.img;
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                //If this is a new navigation, this is a fresh launch so we can
                //discard any saved state
                ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            }
            else
            {
                // Try to restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["NewPage"] as ApplicationDataCompositeValue;
                    TitleBox.Text = (string)composite["title"];
                    DetailBox.Text = (string)composite["des"];
                    DueDate.Date = (DateTimeOffset)composite["date"];
                    Create_btn.Content = (string)composite["createBtnContent"];
                    Img.Source = new BitmapImage(new Uri((string)composite["imgUri"]));
                    for (int i = 0; i < ViewModel.AllItems.Count; ++i)
                    {
                        ViewModel.AllItems[i].title = (string)composite["ListItemTitle" + i];
                        ViewModel.AllItems[i].description = (string)composite["ListItemDes" + i];
                        ViewModel.AllItems[i].date = ((DateTimeOffset)composite["ListItemDate" + i]).DateTime;
                        ViewModel.AllItems[i].img = new BitmapImage(new Uri((string)composite["ListItemImgUri" + i]));
                        ViewModel.AllItems[i].completed = (bool)composite["ListItemCompleted" + i];
                        ViewModel.AllItems[i].idInDatabase = (int)composite["ListItemIdInDataBase" + i];
                    }
                    if ((bool)composite["hasSelectedItem"])
                        ViewModel.SelectedItem = ViewModel.AllItems[(int)composite["selectedItemPosition"]];
                    // We're done with it, so remove it
                    ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
                }
            }
        }
        
        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            PanelReset();
        }

        private void PanelReset()
        {
            TitleBox.Text = "";
            DetailBox.Text = "";
            Create_btn.Content = "Create";
            DueDate.Date = DateTime.Now;
            Img.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/Pics/1.jpg"));
        }

        private async void Button_Create(object sender, RoutedEventArgs e)
        {
            bool isDateValid = true;
            var errorMsg = "";

            //验证DueDate是否正确（是否大于或等于当前日期）
            if (DueDate.Date.Year < DateTime.Now.Year)
                isDateValid = false;
            else if (DueDate.Date.Year == DateTime.Now.Year)
            {
                if (DueDate.Date.Month < DateTime.Now.Month)
                    isDateValid = false;
                else if (DueDate.Date.Month == DateTime.Now.Month)
                {
                    if (DueDate.Date.Day < DateTime.Now.Day)
                        isDateValid = false;
                }
            }
                
            if (TitleBox.Text == "" || DetailBox.Text == "" || isDateValid == false)
            {
                if (TitleBox.Text == "")
                    errorMsg += "Title can't be empty!\n";
                if (DetailBox.Text == "")
                    errorMsg += "Detail can't be empty!\n";
                if (isDateValid == false)
                    errorMsg += "The due date has passed!\n";
                // Create the message dialog and set its content
                var messageDialog = new MessageDialog(errorMsg);

                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand("关闭"));

                // Show the message dialog
                await messageDialog.ShowAsync();
                return;
            }

            var Msg = "";
            if (Create_btn.Content as string == "Create")
            {
                Random ran = new Random();
                int idInDB = ran.Next();
                while (!ViewModel.IsIdInDataBaseUnique(idInDB))
                {
                    idInDB = ran.Next();
                }
                ViewModel.AddTodoItem(TitleBox.Text, DetailBox.Text, DueDate.Date.DateTime, Img.Source, idInDB);
                // 向数据库添加Item
                var db = App.conn;
                using (var ListItem = db.Prepare(App.SQL_INSERT))
                {
                    ListItem.Bind(1, TitleBox.Text);
                    ListItem.Bind(2, DetailBox.Text);
                    ListItem.Bind(3, DueDate.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    ListItem.Bind(4, "false");
                    ListItem.Bind(5, ((BitmapImage)Img.Source).UriSource.ToString());
                    ListItem.Bind(6, idInDB);
                    ListItem.Step();
                }
                Msg = "Create successfully!";
            }  
            else
            {  
                var db = App.conn;
                using (var ListItem = db.Prepare(App.SQL_UPDATE))
                {
                    ListItem.Bind(1, TitleBox.Text);
                    ListItem.Bind(2, DetailBox.Text);
                    ListItem.Bind(3, DueDate.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    ListItem.Bind(4, ViewModel.SelectedItem.completed.ToString());
                    ListItem.Bind(5, ((BitmapImage)Img.Source).UriSource.ToString());
                    ListItem.Bind(6, ViewModel.SelectedItem.idInDatabase);
                    ListItem.Step();
                }
                ViewModel.UpdateTodoItem(ViewModel.SelectedItem.id, TitleBox.Text, DetailBox.Text, DueDate.Date.DateTime, Img.Source);
                Msg = "Update successfully!";
            }

            PanelReset();
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
                rootFrame.GoBack();

            //弹出窗口
            var MsgDialog = new MessageDialog(Msg);
            MsgDialog.Commands.Add(new UICommand("关闭"));
            await MsgDialog.ShowAsync();   
        }

        private async void Click_SelectPicture(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (file == null)
                return;
            StorageFile fileCopy = await file.CopyAsync(localFolder, file.Name, NameCollisionOption.ReplaceExisting);

            string imgStr = "ms-appdata:///local/" + file.Name;
            Debug.WriteLine(imgStr);
            Img.Source = new BitmapImage(new Uri(imgStr));
        }

        private async void PrimaryCmd_Delete(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                var db = App.conn;
                using (var ListItem = db.Prepare(App.SQL_DELETE))
                {
                    ListItem.Bind(1, ViewModel.SelectedItem.idInDatabase);
                    ListItem.Step();
                }
                ViewModel.RemoveTodoItem(ViewModel.SelectedItem.id);

                PanelReset();
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null)
                    rootFrame.GoBack();

                //弹出窗口
                var MsgDialog = new MessageDialog("Delete successfully!");
                MsgDialog.Commands.Add(new UICommand("关闭"));
                await MsgDialog.ShowAsync();      
            }
        }

        private void PrimaryCmd_Refresh(object sender, RoutedEventArgs e)
        {
            PanelReset();
        }

        private uint picNum = 1;

        private void Click_SwitchPicture(object sender, RoutedEventArgs e)
        {
            picNum = (picNum + 1 > 5) ? 1 : (picNum + 1);
            Img.Source = new BitmapImage(new Uri("ms-appx:///Assets/Pics/" + picNum + ".jpg"));
        }
    }
}