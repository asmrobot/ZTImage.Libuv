using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    public enum UVHandleType
    {
        Unknown = 0,
        ASYNC,
        CHECK,
        FS_EVENT,
        FS_POLL,
        HANDLE,
        IDLE,
        NAMED_PIPE,
        POLL,
        PREPARE,
        PROCESS,
        STREAM,
        TCP,
        TIMER,
        TTY,
        UDP,
        SIGNAL,
    }
}
