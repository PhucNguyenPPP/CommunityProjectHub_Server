using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class RemoveTraineeNotification
    {
        public static string SendRemovedNotification(string className, string projectName)
        {
            var result = $"Bạn không còn là học viên của lớp {className} của dự án {projectName}";
            return result;
        }
    }
}