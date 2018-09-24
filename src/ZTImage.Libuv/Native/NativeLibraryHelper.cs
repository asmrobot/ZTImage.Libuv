using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ZTImage.Libuv.Native
{
    internal static class NativeLibraryHelper
    {
        public static IntPtr LoadLibrary(string fileName)
        {
            LoadLibraryCallback callback = new LoadLibraryCallback(NativeLibraryHelper.LoadLibraryWin32);
            if (!UVIntrop.IsWindows)
            {
                callback = new LoadLibraryCallback(NativeLibraryHelper.LoadLibraryPosix);
            }
            return callback(fileName);
        }

        private static IntPtr LoadLibraryPosix(string fileName)
        {
            return UnsafeNativeMethodsPosix.dlopen(fileName, 0x102);
        }

        private static IntPtr LoadLibraryWin32(string fileName)
        {
            return UnsafeNativeMethodsWin32.LoadLibrary(fileName);
        }

        private delegate IntPtr LoadLibraryCallback(string fileName);
    }
}
