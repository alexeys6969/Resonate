using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Model
{
    public class Employees
    {
        public int Id { get; set; }
        public string Full_Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Position { get; set; }
        public string GetShortName(string Full_Name)
        {
            if (string.IsNullOrWhiteSpace(Full_Name))
                return "";

            string[] fullname = Full_Name.Split(' ');

            if (fullname.Length == 1)
                return fullname[0];

            if (fullname.Length >= 2)
            {
                string lastName = fullname[0];
                string firstName = fullname[1];

                if (fullname.Length >= 3)
                {
                    string middleName = fullname[2];
                    return $"{lastName} {firstName[0]}.{middleName[0]}.";
                }
                else
                {
                    return $"{lastName} {firstName[0]}.";
                }
            }

            return Full_Name;
        }
    }
}
