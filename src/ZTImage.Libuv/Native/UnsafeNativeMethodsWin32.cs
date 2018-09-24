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
    internal static class UnsafeNativeMethodsWin32
    {
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string fileName);
    }
}
