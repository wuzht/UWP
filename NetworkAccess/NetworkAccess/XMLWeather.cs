using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetworkAccessXML
{
    class XMLWeather
    {
        public async static Task<WeatherResult> GetWeather(string cityName)
        {
            string url = "http://api.avatardata.cn/Weather/Query?key=89c901d5ff454905a875c1340a4d62d6&dtype=XML&cityname="
                + cityName;
            var http = new HttpClient();
            var response = await http.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(WeatherResult));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (WeatherResult)serializer.Deserialize(ms);

            return data;
        }
    }

    [XmlRoot(ElementName = "wind")]
    public class Wind
    {
        [XmlElement(ElementName = "windspeed")]
        public string Windspeed { get; set; }
        [XmlElement(ElementName = "direct")]
        public string Direct { get; set; }
        [XmlElement(ElementName = "power")]
        public string Power { get; set; }
        [XmlElement(ElementName = "offset")]
        public string Offset { get; set; }
    }

    [XmlRoot(ElementName = "weather")]
    public class Weather
    {
        [XmlElement(ElementName = "humidity")]
        public string Humidity { get; set; }
        [XmlElement(ElementName = "img")]
        public string Img { get; set; }
        [XmlElement(ElementName = "info")]
        public string Info { get; set; }
        [XmlElement(ElementName = "temperature")]
        public string Temperature { get; set; }
        [XmlElement(ElementName = "WeatherDetailObj")]
        public List<WeatherDetailObj> WeatherDetailObj { get; set; }
    }

    [XmlRoot(ElementName = "realtime")]
    public class Realtime
    {
        [XmlElement(ElementName = "wind")]
        public Wind Wind { get; set; }
        [XmlElement(ElementName = "time")]
        public string Time { get; set; }
        [XmlElement(ElementName = "weather")]
        public Weather Weather { get; set; }
        [XmlElement(ElementName = "dataUptime")]
        public string DataUptime { get; set; }
        [XmlElement(ElementName = "date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "city_code")]
        public string City_code { get; set; }
        [XmlElement(ElementName = "city_name")]
        public string City_name { get; set; }
        [XmlElement(ElementName = "week")]
        public string Week { get; set; }
        [XmlElement(ElementName = "moon")]
        public string Moon { get; set; }
    }

    [XmlRoot(ElementName = "kongtiao")]
    public class Kongtiao
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "yundong")]
    public class Yundong
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "ziwaixian")]
    public class Ziwaixian
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "ganmao")]
    public class Ganmao
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "xiche")]
    public class Xiche
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "chuanyi")]
    public class Chuanyi
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "info")]
    public class Info
    {
        [XmlElement(ElementName = "kongtiao")]
        public Kongtiao Kongtiao { get; set; }
        [XmlElement(ElementName = "yundong")]
        public Yundong Yundong { get; set; }
        [XmlElement(ElementName = "ziwaixian")]
        public Ziwaixian Ziwaixian { get; set; }
        [XmlElement(ElementName = "ganmao")]
        public Ganmao Ganmao { get; set; }
        [XmlElement(ElementName = "xiche")]
        public Xiche Xiche { get; set; }
        [XmlElement(ElementName = "chuanyi")]
        public Chuanyi Chuanyi { get; set; }
        [XmlElement(ElementName = "dawn")]
        public Dawn Dawn { get; set; }
        [XmlElement(ElementName = "day")]
        public Day Day { get; set; }
        [XmlElement(ElementName = "night")]
        public Night Night { get; set; }
    }

    [XmlRoot(ElementName = "life")]
    public class Life
    {
        [XmlElement(ElementName = "date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "info")]
        public Info Info { get; set; }
    }

    [XmlRoot(ElementName = "dawn")]
    public class Dawn
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "day")]
    public class Day
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "night")]
    public class Night
    {
        [XmlElement(ElementName = "string")]
        public List<string> String { get; set; }
    }

    [XmlRoot(ElementName = "WeatherDetailObj")]
    public class WeatherDetailObj
    {
        [XmlElement(ElementName = "date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "week")]
        public string Week { get; set; }
        [XmlElement(ElementName = "nongli")]
        public string Nongli { get; set; }
        [XmlElement(ElementName = "info")]
        public Info Info { get; set; }
    }

    [XmlRoot(ElementName = "pm25")]
    public class Pm25
    {
        [XmlElement(ElementName = "curPm")]
        public string CurPm { get; set; }
        /*
        [XmlElement(ElementName = "pm25")]
        public string Pm25_not { get; set; }*/
        [XmlElement(ElementName = "pm10")]
        public string Pm10 { get; set; }
        [XmlElement(ElementName = "level")]
        public string Level { get; set; }
        [XmlElement(ElementName = "quality")]
        public string Quality { get; set; }
        [XmlElement(ElementName = "des")]
        public string Des { get; set; }
    }

    [XmlRoot(ElementName = "result")]
    public class Result
    {
        [XmlElement(ElementName = "realtime")]
        public Realtime Realtime { get; set; }
        [XmlElement(ElementName = "life")]
        public Life Life { get; set; }
        [XmlElement(ElementName = "weather")]
        public Weather Weather { get; set; }
        [XmlElement(ElementName = "pm25")]
        public Pm25 Pm25 { get; set; }
        [XmlElement(ElementName = "isForeign")]
        public string IsForeign { get; set; }
    }

    [XmlRoot(ElementName = "WeatherResult")]
    public class WeatherResult
    {
        [XmlElement(ElementName = "error_code")]
        public string Error_code { get; set; }
        [XmlElement(ElementName = "reason")]
        public string Reason { get; set; }
        [XmlElement(ElementName = "result")]
        public Result Result { get; set; }
    }

}
