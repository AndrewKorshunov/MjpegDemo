using System;
using System.IO;

namespace MjpegLibrary
{
    public static class MjpegPacket
    {
        //public List<string> Header { get; private set; }
        //public Image Picture { get; private set; }

        private static byte[] jpegStart = new byte[] { 255, 216 };

        public static byte[] GetImageBytes(byte[] packetBytes)
        {
            int startOfJpeg = FindStartOfSubArray(packetBytes, jpegStart);
            var imageBytes = new byte[packetBytes.Length - startOfJpeg];
            Array.Copy(packetBytes, startOfJpeg, imageBytes, 0, imageBytes.Length);
            return imageBytes;
        }

        // Duplication from parser, do I need to add, like, Utils class? really?
        private static int FindStartOfSubArray(byte[] array, byte[] subArray)
        {
            int leftToCheck = array.Length; // indexes of array left to check
            int searchStartIndex = 0; // searching in array from this index
            while (leftToCheck >= subArray.Length)
            {
                // first element of subarray found in array at
                int startOfSubArray = Array.IndexOf(array, subArray[0], searchStartIndex, leftToCheck - subArray.Length + 1);
                if (startOfSubArray == -1)
                    return -1;

                int i = 0;
                for (; i < subArray.Length; i++) // I assume that subarray.Length < array.Length
                {
                    if (array[startOfSubArray + i] != subArray[i])
                        break;
                }
                if (i == subArray.Length)
                {
                    return startOfSubArray;
                }
                searchStartIndex = startOfSubArray + 1;
                leftToCheck = array.Length - searchStartIndex;
            }
            return -1;
        }
    }
}
