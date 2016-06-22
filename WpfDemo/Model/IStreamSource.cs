using System;
using System.IO;
using System.Threading.Tasks;

namespace WpfDemo.Model
{
    internal interface IStreamSource
    {
        Stream GetStream();
        Task<Stream> GetStreamAsync();
    }
}