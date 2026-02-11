using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate_course_project.Models
{
    public class Employee
    {
        public int id {  get; set; }
        public string full_name { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string position {  get; set; }
        public Employee(int id, string full_name, string login, string password, string position) 
        {
            this.id = id;
            this.full_name = full_name;
            this.login = login;
            this.password = password;
            this.position = position;
        }
        public Employee(string full_name, string login, string password, string position)
        {
            this.full_name = full_name;
            this.login = login;
            this.password = password;
            this.position = position;
        }
    }
}
