using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sound_packing
{
    public class folder
    {
        public string folder_name;
        public double totalremainduration;
        public List<string> files_names = new List<string>();
        public List<double> files_Durations = new List<double>();
        public int number_files;

        public folder()
        {
            number_files = 0;
        }
    }
}
