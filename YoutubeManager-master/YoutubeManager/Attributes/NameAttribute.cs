using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeManager.Attributes
{
    internal class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; }
    }
}
