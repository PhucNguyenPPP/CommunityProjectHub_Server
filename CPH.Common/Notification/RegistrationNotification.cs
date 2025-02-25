﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.Notification
{
    public static class RegistrationNotification
    {
        public static string SendRegistrationNotification(string senderName, string className, string projectName)
        {
            var result = $"{senderName} đã gửi đơn đăng ký vào lớp {className} của dự án {projectName}";
            return result;
        }
    }
}
