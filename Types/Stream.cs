using System;
using System.IO;
using System.Text;

namespace EbbsSoft.ExtensionHelpers.StreamHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Byte Array To Memory Stream
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static System.IO.Stream ToMemoryStream(this byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }
    
        /// <summary>
        /// return a string to a memory stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(this string value, EnumHelpers.Utils.EncodingFormat encodingType)
        {
            switch (encodingType)
            {
                case EnumHelpers.Utils.EncodingFormat.UTF7:
                    return new MemoryStream(Encoding.UTF7.GetBytes(value ?? ""));

                case EnumHelpers.Utils.EncodingFormat.UTF8:
                    return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));

                case EnumHelpers.Utils.EncodingFormat.UT32:
                    return new MemoryStream(Encoding.UTF32.GetBytes(value ?? ""));

                case EnumHelpers.Utils.EncodingFormat.UNICODE:
                    return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));

                case EnumHelpers.Utils.EncodingFormat.ASCII:
                    return new MemoryStream(Encoding.ASCII.GetBytes(value ?? ""));
            }
            return null;
        }
    
        /// <summary>
        /// String to Stream.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static System.IO.Stream ToStream(this string data)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(data);
            sw.Flush();
            ms.Position = 0;
            return ms;
        }
    }
}