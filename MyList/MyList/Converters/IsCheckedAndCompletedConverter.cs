using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyList.Converters
{
    class IsCheckedAndCompletedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? isChecked = value as bool?;
            if (isChecked == null || isChecked == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool? isChecked = value as bool?;
            if (isChecked == null || isChecked == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
