using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class RemovePMFromProjectNotification
    {
        public static string SendRemovePMFromProjectNotification(string projectName)
        {
            var result = $"Bạn không còn là quản lý của dự án {projectName}";
            return result;
        }
    }
}
