using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLayer
{
    public class Directory
    {
        public string Path { get; set; }
        public string MoveTo { get; set; }
        public bool ForceDelete { get; set; }
        public List<string> Patterns { get; set; }
        public int KeepDaysOfLog { get; set; }
    }
}
