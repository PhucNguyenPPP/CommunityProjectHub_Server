using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class AssignPMToProjectNotification
    {
        public static string SendAssignPMToProjectNotification(string projectName)
        {
            var result = $"Bạn được phân công trở thành quản lý của dự án {projectName}";
            return result;
        }
    }
}
