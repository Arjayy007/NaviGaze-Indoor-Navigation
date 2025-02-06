using System.Collections.Generic;
using UnityEditor;

namespace NotificationModel.Models 
{
    [System.Serializable]
    public class NotificationData
    {
        public string message;
        public string timestamp;
        public bool isRead = false;
        public bool isNotificationSent = true;

        public int notificationWeek;

        public NotificationData(string message, string timestamp, bool isRead, bool notificationSent, int notificationWeek)
        {
            this.message = message;
            this.timestamp = timestamp;
            this.isRead = isRead;
            this.isNotificationSent = notificationSent;
            this.notificationWeek = notificationWeek;
        }
       
    
}
}