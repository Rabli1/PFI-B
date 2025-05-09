using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public static class DateExt
    {
        public static string ToFrenchDateString(this DateTime date)
        {
            return date.ToString("dd/MMMM/yyyy", System.Globalization.CultureInfo.GetCultureInfo("fr-CA")).Replace('-', ' ');
        }
    }
}