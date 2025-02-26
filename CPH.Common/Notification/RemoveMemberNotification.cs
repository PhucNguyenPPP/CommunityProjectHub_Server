using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class RemoveMemberNotification
    {
        public static string SendRemovedNotification(string className, string projectName)
        {
            var result = $"Bạn không còn là thành viên của lớp {className} dự án {projectName}";
            return result;
        }
    }
}
