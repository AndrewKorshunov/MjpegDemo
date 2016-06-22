using System;
using System.IO;
using System.Threading.Tasks;

namespace MjpegLibrary
{
    public interface IStreamSource
    {
        Stream GetStream();
        Task<Stream> GetStreamAsync();
    }
}