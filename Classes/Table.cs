using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate_course_project.Classes
{
    public class Table
    {
        public string name { get ; set; }
        public string image { get; set; }
        public Table(string name, string image)
        {
            this.name = name;
            this.image = image;
        }
    }
}
