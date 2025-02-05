using System.Collections.Generic;

namespace SchedulesModel.Models 
{
    [System.Serializable]
    public class ScheduleData
    {
        public string subjectCode;
        public string subjectName;
        public string room;
        public string dayOfTheWeek;
        public string startTime;
        public string endTime;
        public string campus;

        public ScheduleData(string subjectCode, string subjectName, string room, string dayOfTheWeek, string startTime, string endTime, string campus)
        {
            this.subjectCode = subjectCode;
            this.subjectName = subjectName;
            this.room = room;
            this.dayOfTheWeek = dayOfTheWeek;
            this.startTime = startTime;
            this.endTime = endTime;
            this.campus = campus;
        }
    }
}
