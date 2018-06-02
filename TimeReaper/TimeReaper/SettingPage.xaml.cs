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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace TimeReaper
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        SettingParameterPassing parameter;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            parameter = (SettingParameterPassing)e.Parameter;
            SettingWorkIntervalInput.Text = parameter.pomotodoWorkInterval.ToString();
            SettingShortBreakInput.Text = parameter.pomotodoShortBreak.ToString();
            SettingLongBreakInput.Text = parameter.pomotodoLongBreak.ToString();
            SettingLongBreakIntervalInput.Text = parameter.pomotodoRestInterval.ToString();
        }

        private async void SettingChangeButton_Click(object sender, RoutedEventArgs e)
        {
            parameter.pomotodoWorkInterval = Int32.Parse(SettingWorkIntervalInput.Text);
            parameter.pomotodoShortBreak = Int32.Parse(SettingShortBreakInput.Text);
            parameter.pomotodoLongBreak = Int32.Parse(SettingLongBreakInput.Text);
            parameter.pomotodoRestInterval = Int32.Parse(SettingLongBreakIntervalInput.Text);

            bool negative = false;
            if (parameter.pomotodoWorkInterval <= 0)
            {
                parameter.pomotodoWorkInterval = 1;
                negative = true;
            }
            if(parameter.pomotodoShortBreak<=0)
            {
                parameter.pomotodoShortBreak = 1;
                negative = true;
            }
            if(parameter.pomotodoLongBreak<=0)
            {
                parameter.pomotodoLongBreak = 1;
                negative = true;
            }
            if(parameter.pomotodoRestInterval<=0)
            {
                parameter.pomotodoRestInterval = 1;
                negative = true;
            }

            if(negative)
            {
                ContentDialog warningDialog = new ContentDialog()
                {
                    Title= "不能输入非正数",
                    Content= "请检查输入数据，不能输入非正数。已输入的非正数视为1处理",
                    PrimaryButtonText = "OK"
                };
                ContentDialogResult result = await warningDialog.ShowAsync();
                return;
            }

            Frame.Navigate(typeof(MainPage),parameter);
        }
    }
}
