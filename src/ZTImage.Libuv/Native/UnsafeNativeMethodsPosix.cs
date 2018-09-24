using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv.Native
{
    [SuppressUnmanagedCodeSecurity]
    internal class UnsafeNativeMethodsPosix
    {
        internal const int RTLD_DEFAULT = 0x102;
        internal const int RTLD_GLOBAL = 0x100;
        internal const int RTLD_LAZY = 1;
        internal const int RTLD_LOCAL = 0;
        internal const int RTLD_NOW = 2;

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr dlopen(string fileName, int mode);
    }
}
