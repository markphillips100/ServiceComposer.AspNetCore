using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceComposer.AspNetCore.ObjectComposition.Internal
{
    public class ObjectRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public RouteValueDictionary Values { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

    }
}
