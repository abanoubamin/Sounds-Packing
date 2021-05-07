using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sound_packing
{
    public class Node
    {
        public Node parent;
        public Node right;
        public Node left;
        public double total_remaining_time;
        public string name;
        public List<string> list_names;
        public List<double> list_durations;
        public Node(double total_remaining_time, string name)
        {
            this.total_remaining_time = total_remaining_time;
            this.name = name;
            list_names = new List<string>();
            list_durations = new List<double>();
        }
    }
}
