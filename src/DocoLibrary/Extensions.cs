using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocoLibrary
{
    public static class Extensions
    {
        public static string GetExtension(this HttpFile file)
        {
            var split = file.Name.Split(new[] { '.' }, 2);
            var fExt = string.Empty;
            if (split.Count() > 1)
                fExt = "." + split[1];
            return fExt;
        }
    }
}