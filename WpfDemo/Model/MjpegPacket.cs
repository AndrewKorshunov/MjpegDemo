using System;
using System.IO;
using System.Drawing;

namespace WpfDemo.Model
{
    static public class MjpegPacket
    {
        //public List<string> Header { get; private set; }
        //public Image Picture { get; private set; }

        static private byte[] jpegStart = new byte[] { 255, 216 };

        static public Image GetImageFromPacket(byte[] packetBytes)
        {
            int startOfJpeg = FindSubArray(packetBytes, jpegStart, 0, packetBytes.Length);
            var imageBytes = new byte[packetBytes.Length - startOfJpeg];
            Array.Copy(packetBytes, startOfJpeg, imageBytes, 0, imageBytes.Length);
            var ms = new MemoryStream(imageBytes);
            var image = Image.FromStream(ms);            
            return image;
        }

        static private int FindSubArray(byte[] array, byte[] needle, int startIndex, int sourceLength)
        {
            int needleLen = needle.Length;
            int index;

            while (sourceLength >= needleLen)
            {
                // find needle's starting element
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);
                // if we did not find even the first element of the needls, then the search is failed
                if (index == -1)
                    return -1;
                int i, p;
                // check for needle
                for (i = 0, p = index; i < needleLen; i++, p++)
                {
                    if (array[p] != needle[i])
                    {
                        break;
                    }
                }
                if (i == needleLen)
                {
                    // needle was found
                    return index;
                }
                // continue to search for needle
                sourceLength -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
    }
}
