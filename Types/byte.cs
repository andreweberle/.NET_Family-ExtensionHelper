using System;
using System.IO;

namespace EbbsSoft.ExtensionHelpers.ByteHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Convert Stream To Byte Array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToByteArray(this Stream stream)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer,0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}