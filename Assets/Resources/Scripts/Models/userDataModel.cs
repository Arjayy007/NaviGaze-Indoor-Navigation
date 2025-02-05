using System.Collections.Generic;

namespace userDataModel.Models
{
    [System.Serializable]
    public class UserData
    {
        public string firstName;
        public string lastName;
        public string email;
        public string password;
        public string department;
        public string program;
        public string yearSection;
        public string role;
        public int userCoins;
        public int exp;
        public Dictionary<string, bool> rewardsClaimed;

        public UserData(string firstName, string lastName, string email, string password, string department, string program, string yearSection, string role)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.password = password;
            this.department = department;
            this.program = program;
            this.yearSection = yearSection;
            this.role = role;

            this.userCoins = 50;
            this.exp = 0;
            this.rewardsClaimed = new Dictionary<string, bool>();

        }
    [System.Serializable]
    public class ProfessorData 
    {
        public string firstName;
        public string lastName;
        public string email;
        public string password;
        public string department;
        public string role;

         public ProfessorData(string firstName, string lastName, string email, string password, string department, string role) 
         {
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.password = password;
            this.department = department;
            this.role = role;
         }
        }
    }
}
