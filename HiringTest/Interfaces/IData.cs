using System.IO;

namespace HiringTest
{
    public interface IData<T> : IVersion where T : IData<T>, new()
    {
        T FromStream(Stream s);

        void ToStream(Stream s);
    }
}