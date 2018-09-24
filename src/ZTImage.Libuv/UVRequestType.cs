using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    public enum UVRequestType
    {
        Unknown = 0,
        REQ,
        CONNECT,
        WRITE,
        SHUTDOWN,
        UDP_SEND,
        FS,
        WORK,
        GETADDRINFO,
        GETNAMEINFO,
    }
}
