using System.Collections.Generic;
using Firebase.Firestore; 

namespace userDataModel.Models
{
    [FirestoreData] 
    public class UserData
    {
        [FirestoreProperty]
        public string firstName { get; set; }

        [FirestoreProperty]
        public string lastName { get; set; }

        [FirestoreProperty]
        public string email { get; set; }

        [FirestoreProperty]
        public string password { get; set; }

        [FirestoreProperty]
        public string department { get; set; }

        [FirestoreProperty]
        public string program { get; set; }

        [FirestoreProperty]
        public string yearSection { get; set; }

        [FirestoreProperty]
        public string role { get; set; }

        [FirestoreProperty]
        public string avatarName { get; set; }

        [FirestoreProperty]
        public int userCoins { get; set; }

        [FirestoreProperty]
        public int exp { get; set; }

        [FirestoreProperty]
        public Dictionary<string, bool> rewardsClaimed { get; set; }

        public UserData() {}

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
            this.avatarName = "Capybara Avatar";
        }
    }
}
