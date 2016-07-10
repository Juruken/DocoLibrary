using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocoLibrary
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => Main();
        }

        public object Main()
        {
            return View["index"];
        }
    }
}