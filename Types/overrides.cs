using System;
using System.IO;
using System.Linq;
using System.Text;
using EbbsSoft.ExtensionHelpers.StringHelpers;

namespace EbbsSoft.ExtensionHelpers.Overrides
{
    public static class Utils
    {
        /// <summary>
        /// UTF-8 String Writer
        /// </summary>
        public class Utf8StringWriter : StringWriter
        {
            /// <summary>
            /// Override Encoding Type To UTF-8
            /// </summary>
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}