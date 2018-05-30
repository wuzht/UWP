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
using Windows.UI.ViewManagement;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Storage.Pickers;
using Windows.ApplicationModel;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModels.ListItemViewModels ViewModel;
        Models.ListItem ShareItem;

        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.White;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.White;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 420, Height = 480 });
            ViewModel = ViewModels.ListItemViewModels.GetInstance();
            UpdateTile();
        }

        private void UpdateTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            XmlDocument document = new XmlDocument();
            document.LoadXml(System.IO.File.ReadAllText("Tile.xml"));
            XmlNodeList textElements = document.GetElementsByTagName("text");
            for (int i = 0; i < ViewModel.AllItems.Count; ++i)
            {
                string imgUri = ((BitmapImage)ViewModel.AllItems[i].img).UriSource.ToString();
                Debug.WriteLine(imgUri);   
                textElements[0].InnerText = ViewModel.AllItems[i].title;
                textElements[1].InnerText = ViewModel.AllItems[i].description;
                textElements[2].InnerText = ViewModel.AllItems[i].title;
                textElements[3].InnerText = ViewModel.AllItems[i].description;
                textElements[4].InnerText = ViewModel.AllItems[i].title;
                textElements[5].InnerText = ViewModel.AllItems[i].description;
                textElements[6].InnerText = ViewModel.AllItems[i].title;
                textElements[7].InnerText = ViewModel.AllItems[i].description;
                var tileNotification = new TileNotification(document);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            }  
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
                ApplicationData.Current.LocalSettings.Values["MainPage"] = composite;
            }
            //base.OnNavigatedFrom(e); 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateTile();
            ViewModel.SelectedItem = null;
            PanelReset();
            if (e.NavigationMode == NavigationMode.New)
            {
                //If this is a new navigation, this is a fresh launch so we can
                //discard any saved state
                ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
            }
            else
            {
                // Try to restore state if any, in case we were terminated
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MainPage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["MainPage"] as ApplicationDataCompositeValue;
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
                    ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
                }
            }
            /*
            if (e.Parameter.GetType() == typeof(ViewModels.ListItemViewModels))
                ViewModel = (ViewModels.ListItemViewModels)(e.Parameter);*/
        }

        private void PrimaryCmd_turnToNewPage(object sender, RoutedEventArgs e)
        {
            if (this.VisualStateGroup.CurrentState != VisualState800)
                Frame.Navigate(typeof(NewPage));
        }

        private void SecondaryCmd_turnToNewPage(object sender, RoutedEventArgs e)
        {
            if (this.VisualStateGroup.CurrentState != VisualState800)
                Frame.Navigate(typeof(NewPage));          
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
            if (TitleBox.Text == "" || DetailBox.Text == "" || !IsDateValid())
            {
                var errorMsg = "";
                if (TitleBox.Text == "")
                    errorMsg += "Title can't be empty!\n";
                if (DetailBox.Text == "")
                    errorMsg += "Detail can't be empty!\n";
                if (!IsDateValid())
                    errorMsg += "The due date has passed!\n";
                // Create the message dialog and set its content
                var messageDialog = new MessageDialog(errorMsg);
                // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                messageDialog.Commands.Add(new UICommand("关闭"));
                // Show the message dialog
                await messageDialog.ShowAsync();
                return;
            }

            if (ViewModel.SelectedItem == null)
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
                PanelReset();
                //弹出窗口
                var messageDialog = new MessageDialog("Create successfully!");
                messageDialog.Commands.Add(new UICommand("关闭"));
                await messageDialog.ShowAsync();
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
                //重置界面
                PanelReset();

                //刷新（方法是导航到自身界面，然后再返回）
                Frame.Navigate(typeof(MainPage));
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null)
                    rootFrame.GoBack();

                //弹出窗口
                var messageDialog = new MessageDialog("Update successfully!");
                messageDialog.Commands.Add(new UICommand("关闭"));
                await messageDialog.ShowAsync();
            }
            UpdateTile();
        }

        private bool IsDateValid()
        {
            //验证DueDate是否正确（是否大于或等于当前日期）
            if (DueDate.Date.Year < DateTime.Now.Year)
                return false;
            else if (DueDate.Date.Year == DateTime.Now.Year)
            {
                if (DueDate.Date.Month < DateTime.Now.Month)
                    return false;
                else if (DueDate.Date.Month == DateTime.Now.Month)
                {
                    if (DueDate.Date.Day < DateTime.Now.Day)
                        return false;
                }
            }           
            return true;
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
            try
            {
                string myFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + (new Random()).Next() + file.FileType;
                StorageFile fileCopy = await file.CopyAsync(localFolder, myFileName, NameCollisionOption.ReplaceExisting);
                string imgStr = "ms-appdata:///local/" + myFileName;
                Debug.WriteLine(imgStr);
                Img.Source = new BitmapImage(new Uri(imgStr));
            }
            catch (Exception error)
            {
                var box1 = new MessageDialog("Oops! Something Went Wrong!\n\n" + error.ToString()).ShowAsync();
            }
            /*
            if (file != null)
            {
                var inputFile = SharedStorageAccessManager.AddFile(file);
                var destination = await ApplicationData.Current.LocalFolder.CreateFileAsync("Cropped.jpg", CreationCollisionOption.ReplaceExisting);
                var destinationFile = SharedStorageAccessManager.AddFile(destination);
                var options = new LauncherOptions();
                options.TargetApplicationPackageFamilyName = "Microsoft.Windows.Photos_8wekyb3d8bbwe";

                var parameters = new ValueSet();
                parameters.Add("InputToken", inputFile);
                parameters.Add("DestinationToken", destinationFile);
                parameters.Add("ShowCamera", false);
                parameters.Add("CropWidthPixals", 280);
                parameters.Add("CropHeightPixals", 300);

                var result = await Launcher.LaunchUriForResultsAsync(new Uri("microsoft.windows.photos.crop:"), options, parameters);

                if (result.Status == LaunchUriStatus.Success && result.Result != null)
                {
                    try
                    {
                        var stream = await destination.OpenReadAsync();
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);

                        Img.Source = bitmap;
                        Debug.WriteLine(((BitmapImage)Img.Source).UriSource.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message + ex.StackTrace);
                    }
                }
            }*/
        }

        private void ListItem_Click(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (Models.ListItem)e.ClickedItem;
            if (VisualStateGroup.CurrentState != VisualState800)
                Frame.Navigate(typeof(NewPage));
            else
            {
                Create_btn.Content = "Update";
                TitleBox.Text = ViewModel.SelectedItem.title;
                DetailBox.Text = ViewModel.SelectedItem.description;
                DueDate.Date = ViewModel.SelectedItem.date.Date;
                Img.Source = ViewModel.SelectedItem.img;
            }
        }

        private async void PrimaryCmd_delete(object sender, RoutedEventArgs e)
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
                //弹出窗口
                var messageDialog = new MessageDialog("Delete successfully!");
                messageDialog.Commands.Add(new UICommand("关闭"));
                await messageDialog.ShowAsync();
                PanelReset();
                UpdateTile();
            }      
        }

        private void PrimaryCmd_Refresh(object sender, RoutedEventArgs e)
        {
            //刷新（方法是导航到自身界面，然后再返回）
            Frame.Navigate(typeof(MainPage));
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
                rootFrame.GoBack();
            UpdateTile();
        }

        private async void MenuFlyoutItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            string deleteItemId = ((MenuFlyoutItem)e.OriginalSource).DataContext.ToString();
            ViewModel.SelectedItem = ViewModel.GetListItemById(deleteItemId);
            var db = App.conn;
            using (var ListItem = db.Prepare(App.SQL_DELETE))
            {
                ListItem.Bind(1, ViewModel.SelectedItem.idInDatabase);
                ListItem.Step();
            }
            ViewModel.RemoveTodoItem(ViewModel.SelectedItem.id);

            //弹出窗口
            var messageDialog = new MessageDialog("Delete successfully!");
            messageDialog.Commands.Add(new UICommand("关闭"));
            await messageDialog.ShowAsync();
            PanelReset();
            UpdateTile();
        }

        private void MenuFlyoutItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedItem = (Models.ListItem)((MenuFlyoutItem)sender).DataContext;
            if (VisualStateGroup.CurrentState != VisualState800)
                Frame.Navigate(typeof(NewPage));
            else
            {
                Create_btn.Content = "Update";
                TitleBox.Text = ViewModel.SelectedItem.title;
                DetailBox.Text = ViewModel.SelectedItem.description;
                DueDate.Date = ViewModel.SelectedItem.date.Date;
                Img.Source = ViewModel.SelectedItem.img;
            }
        }

        // 切换自带图片
        private uint picNum = 1;

        private void Click_SwitchPicture(object sender, RoutedEventArgs e)
        {
            picNum = (picNum + 1 > 5) ? 1 : (picNum + 1);
            Img.Source = new BitmapImage(new Uri("ms-appx:///Assets/Pics/" + picNum + ".jpg"));
        }

        // 分享按钮
        private void MenuFlyoutItem_Share_Click(object sender, RoutedEventArgs e)
        {
            ShareItem = (Models.ListItem)((MenuFlyoutItem)sender).DataContext;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        // 分享处理函数
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;

            request.Data.Properties.Title = ShareItem.title;
            request.Data.Properties.Description = ShareItem.description;
            request.Data.SetText(ShareItem.description);

            var Deferral = args.Request.GetDeferral();
            // SharePhoto = await Package.Current.InstalledLocation.GetFileAsync("Assets/Pics/2.jpg");
            //request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(SharePhoto);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(((BitmapImage)ShareItem.img).UriSource.ToString())));
            Deferral.Complete();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string tempIdInDataBase = ((CheckBox)e.OriginalSource).DataContext.ToString();
            var db = App.conn;
            using (var ListItem = db.Prepare(App.SQL_UPDATE_COMPLETE))
            {
                ListItem.Bind(1, "true");
                ListItem.Bind(2, tempIdInDataBase);
                ListItem.Step();
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string tempIdInDataBase = ((CheckBox)e.OriginalSource).DataContext.ToString();
            var db = App.conn;
            using (var ListItem = db.Prepare(App.SQL_UPDATE_COMPLETE))
            {
                ListItem.Bind(1, "false");
                ListItem.Bind(2, tempIdInDataBase);
                ListItem.Step();
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            String result = String.Empty;
            StringBuilder DataQuery = new StringBuilder("%%");
            DataQuery.Insert(1, SearchBox.Text);
            var db = App.conn;
            using (var statement = db.Prepare(App.SQL_SEARCH))
            {
                statement.Bind(1, DataQuery.ToString());
                statement.Bind(2, DataQuery.ToString());
                statement.Bind(3, DataQuery.ToString());
                while (SQLitePCL.SQLiteResult.ROW == statement.Step())
                {
                    result += "Title: " + statement[0].ToString() + " ";
                    result += "Description: " + statement[1].ToString() + " ";
                    result += "DueDate: " + statement[2].ToString() + "\n";
                }
            }

            if (result == String.Empty)
            {
                var box1 = new MessageDialog("Not found").ShowAsync();
            }
            else
            {
                var box2 = new MessageDialog(result).ShowAsync();
            }
        }
    }
}
