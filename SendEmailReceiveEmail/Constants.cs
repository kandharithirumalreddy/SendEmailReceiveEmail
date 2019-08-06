using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendEmailReceiveEmail
{
    class Constants
    {
        public const string template1Subject = "this is test mail";
        public static string getTemplate1(string bookingid)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<img src='https://google.co.in/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png'/>");
            sb.AppendLine("<br/>");
            sb.AppendLine($"this is email template line1 {bookingid}");
            sb.AppendLine("<br/>");
            sb.AppendLine("this is email template line2");
            sb.AppendLine("<br/>");
            sb.AppendLine("this is email template line3");
            sb.AppendLine("<br/>");
            sb.AppendLine("this is email template line4");
            sb.AppendLine("<br/>");
            sb.AppendLine("this is email template line5");
            sb.AppendLine("<br/>");
            return sb.ToString();
        }
    }
}
