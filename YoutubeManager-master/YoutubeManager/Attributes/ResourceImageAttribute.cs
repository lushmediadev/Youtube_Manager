using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeManager.Attributes
{
    internal class ResourceImageAttribute : Attribute
    {
        public ResourceImageAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) throw new ArgumentNullException(nameof(resourceName));
            this._resourceName = resourceName;
        }
        readonly string _resourceName;

        public Bitmap Bitmap
        {
            get
            {
                object obj = Resource.ResourceManager.GetObject(_resourceName);
                return (Bitmap)obj;
            }
        }
    }
}
