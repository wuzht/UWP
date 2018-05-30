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



// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace NetworkAccess
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_ByJson(object sender, RoutedEventArgs e)
        {
            JsonTextBlock.Text = "";
            JsonRing.Visibility = Visibility.Visible;
            try
            {
                RootObject myWeather = await OpenWeatherMapProxy.GetWeather(JsonTextBox.Text);
                string presentStr = "城市：" + myWeather.result.realtime.city_name + "\n" +
                                 "温度：" + myWeather.result.realtime.weather.temperature + "℃\n" +
                                 "湿度：" + myWeather.result.realtime.weather.humidity + "%\n" +
                                 myWeather.result.realtime.weather.info + "\n";
                JsonTextBlock.Text = presentStr;        
            }
            catch
            {
                //var box = new Windows.UI.Popups.MessageDialog("Oops！您的输入有问题！\n\n" + error.ToString()).ShowAsync();
                JsonTextBlock.Text = "您的输入有误！";
            }
            JsonRing.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click_ByXML(object sender, RoutedEventArgs e)
        {
            XMLTextBlock.Text = "";
            XMLRing.Visibility = Visibility.Visible;
            try
            {
                NetworkAccessXML.WeatherResult myWeather = await NetworkAccessXML.XMLWeather.GetWeather(XMLTextBox.Text);
            
                string presentStr = "城市：" + myWeather.Result.Realtime.City_name + "\n" +
                                 "温度：" + myWeather.Result.Realtime.Weather.Temperature + "℃\n" +
                                 "湿度：" + myWeather.Result.Realtime.Weather.Humidity + "%\n" +
                                 myWeather.Result.Realtime.Weather.Info + "\n";
                XMLTextBlock.Text = presentStr;
                
            }
            catch
            {
                XMLTextBlock.Text = "您的输入有误！";
            }
            XMLRing.Visibility = Visibility.Collapsed;
        }
    }
}
