using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class UpdateClassNotification
    {
        public static string SendUpdateClassNotification(string roleName,string className, string projectName)
        {
            var result = $"Bạn được phân công trở thành {roleName} tại lớp {className} thuộc dự án {projectName}";
            return result;
        }
    }
}
