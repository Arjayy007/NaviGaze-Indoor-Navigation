using System.Collections.Generic;

namespace ProfessorDataModel.Models
{

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
