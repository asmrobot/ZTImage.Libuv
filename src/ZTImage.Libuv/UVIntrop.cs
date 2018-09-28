using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZTImage.Libuv.Collections;
using ZTImage.Libuv.Native;

/*********************
 * reference from kestrel
 * *************************/
namespace ZTImage.Libuv
{
    /// <summary>
    /// libuv互操作
    /// </summary>
    public class UVIntrop
    {
        public static readonly bool IsWindows;
        
        static UVIntrop()
        {
            IsWindows = System.Environment.OSVersion.Platform == PlatformID.Win32NT;
            Initialize();
        }

        public const Int32 UV_EOF = -4095;

        private static void Initialize()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string filename = string.Empty;
#if NET45
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (IntPtr.Size == 4)
                {
                    filename = Path.Combine(dir, "win-x86", "native", "libuv.dll");
                }
                else
                {
                    filename = Path.Combine(dir, "win-x64", "native", "libuv.dll");
                }
            }
            else if (System.Environment.OSVersion.Platform == PlatformID.Unix)
            {
                filename = Path.Combine(dir, "linux-x64", "native", "libuv.so");
            }
            else
            {
                filename = Path.Combine(dir, "osx", "native", "libuv.dylib");
            }

            NativeLibraryHelper.LoadLibrary(filename);
#endif
            //#else
            //            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //            {
            //                if (RuntimeInformation.OSArchitecture == Architecture.X86)
            //                {
            //                    filename = Path.Combine(dir, "win-x86", "native", "libuv.dll");
            //                }
            //                else if (RuntimeInformation.OSArchitecture == Architecture.X64)
            //                {
            //                    filename = Path.Combine(dir, "win-x64",  "native","libuv.dll");
            //                }
            //                else if (RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64)
            //                {
            //                    filename = Path.Combine(dir, "win-arm", "native", "libuv.dll");
            //                }
            //            }
            //            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //            {
            //                if (RuntimeInformation.OSArchitecture == Architecture.Arm)
            //                {
            //                    filename = Path.Combine(dir, "linux-arm", "native", "libuv.so");
            //                }
            //                else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            //                {
            //                    filename = Path.Combine(dir, "linux-arm64",  "native","libuv.so");
            //                }
            //                else if (RuntimeInformation.OSArchitecture == Architecture.X64)
            //                {
            //                    filename = Path.Combine(dir, "linux-x64",  "native","libuv.so");
            //                }
            //            }
            //            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //            {
            //                filename = Path.Combine(dir, "osx",  "native","libuv.dylib");
            //            }
            //            else
            //            {
            //                throw new Exception("unknow operation");
            //            }
            //            if (string.IsNullOrEmpty(filename))
            //            {
            //                throw new Exception("unknow process arch");
            //            }
            //            NativeLibraryHelper.LoadLibrary(filename);
            //#endif


        }

        #region tools
        public static void ThrowIfErrored(int statusCode)
        {
            // Note: method is explicitly small so the success case is easily inlined
            if (statusCode < 0)
            {
                ThrowError(statusCode);
            }
        }

        private static void ThrowError(int statusCode)
        {
            // Note: only has one throw block so it will marked as "Does not return" by the jit
            // and not inlined into previous function, while also marking as a function
            // that does not need cpu register prep to call (see: https://github.com/dotnet/coreclr/pull/6103)
            throw GetError(statusCode);
        }

        public static void Check(int statusCode, out UVException error)
        {
            // Note: method is explicitly small so the success case is easily inlined
            error = statusCode < 0 ? GetError(statusCode) : null;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static UVException GetError(int statusCode)
        {
            // Note: method marked as NoInlining so it doesn't bloat either of the two preceeding functions
            // Check and ThrowError and alter their jit heuristics.
            var errorName =err_name(statusCode);
            var errorDescription =strerror(statusCode);
            return new UVException("Error " + statusCode + " " + errorName + " " + errorDescription, statusCode);
        }
#endregion
        
#region Struct
        [StructLayout(LayoutKind.Sequential)]
        public struct uv_req_t
        {
            public IntPtr data;
            public UVRequestType type;
        }
        
        public enum UV_RUN_MODE:Int32
        {
            UV_RUN_DEFAULT = 0,
            UV_RUN_ONCE,
            UV_RUN_NOWAIT
        }

        public struct uv_buf_t
        {
            // this type represents a WSABUF struct on Windows
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms741542(v=vs.85).aspx
            // and an iovec struct on *nix
            // http://man7.org/linux/man-pages/man2/readv.2.html
            // because the order of the fields in these structs is different, the field
            // names in this type don't have meaningful symbolic names. instead, they are
            // assigned in the correct order by the constructor at runtime

            public readonly IntPtr _field0;
            public readonly IntPtr _field1;

            public uv_buf_t(IntPtr memory, int len, bool IsWindows)
            {
                if (IsWindows)
                {
                    _field0 = (IntPtr)len;
                    _field1 = memory;
                }
                else
                {
                    _field0 = memory;
                    _field1 = (IntPtr)len;
                }
            }
            
        }

#endregion


#region Functions

        public static void loop_init(UVLoopHandle handle)
        {
            ThrowIfErrored(uv_loop_init(handle));
        }

        public static void loop_close(UVLoopHandle handle)
        {
            handle.Validate(closed: true);
            ThrowIfErrored(uv_loop_close(handle.InternalGetHandle()));
        }

        public static void run(UVLoopHandle handle, int mode)
        {
            handle.Validate();
            ThrowIfErrored(uv_run(handle, mode));
        }

        public static void stop(UVLoopHandle handle)
        {
            handle.Validate();
            uv_stop(handle);
        }

        public static void @ref(UVHandle handle)
        {
            handle.Validate();
            uv_ref(handle);
        }

        public static void unref(UVHandle handle)
        {
            handle.Validate();
            uv_unref(handle);
        }

        public static void fileno(UVHandle handle, ref IntPtr socket)
        {
            handle.Validate();
            ThrowIfErrored(uv_fileno(handle, ref socket));
        }
        
        public static void close(UVHandle handle, uv_close_cb close_cb)
        {
            handle.Validate(closed: true);
            uv_close(handle.InternalGetHandle(), close_cb);
        }

        public static void close(IntPtr handle, uv_close_cb close_cb)
        {
            uv_close(handle, close_cb);
        }

        public static void idle_init(UVLoopHandle loop, UVIdleHandle handle)
        {
            loop.Validate();
            handle.Validate();
            ThrowIfErrored(uv_idle_init(loop, handle));
        }

        public static void idle_start( UVIdleHandle handle,uv_idle_cb cb)
        {
            handle.Validate();
            ThrowIfErrored(uv_idle_start(handle, cb));
        }

        public static void idle_stop(UVIdleHandle handle)
        {
            handle.Validate();
            ThrowIfErrored(uv_idle_stop(handle));
        }


        public static void unsafe_async_send(IntPtr handle)
        {
            ThrowIfErrored(uv_unsafe_async_send(handle));
        }

        public static void tcp_init(UVLoopHandle loop, UVTCPHandle handle)
        {
            loop.Validate();
            handle.Validate();
            ThrowIfErrored(uv_tcp_init(loop, handle));
        }

        public static void tcp_bind(UVTCPHandle handle, ref SockAddr addr, int flags)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_bind(handle, ref addr, flags));
        }

        public static void tcp_open(UVTCPHandle handle, IntPtr hSocket)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_open(handle, hSocket));
        }

        public static void tcp_connect(UVConnectRquest handle, UVTCPHandle socket, ref SockAddr addr, uv_connect_cb cb)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_connect(handle, socket, ref addr, cb));
        }

        public static void tcp_nodelay(UVTCPHandle handle, bool enable)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_nodelay(handle, enable ? 1 : 0));
        }
        
        public static void listen(UVStreamHandle handle, int backlog, uv_connection_cb cb)
        {
            handle.Validate();
            ThrowIfErrored(uv_listen(handle, backlog, cb));
        }

        public static void accept(UVStreamHandle server, UVStreamHandle client)
        {
            server.Validate();
            client.Validate();
            ThrowIfErrored(uv_accept(server, client));
        }
        

        public static void read_start(UVStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb)
        {
            handle.Validate();
            ThrowIfErrored(uv_read_start(handle, alloc_cb, read_cb));
        }

        public static void read_stop(UVStreamHandle handle)
        {
            handle.Validate();
            ThrowIfErrored(uv_read_stop(handle));
        }

        public static int try_write(UVStreamHandle handle, uv_buf_t[] bufs, int nbufs)
        {
            handle.Validate();
            var count = uv_try_write(handle, bufs, nbufs);
            ThrowIfErrored(count);
            return count;
        }

        public unsafe static void write(UVWriteRequest req, UVStreamHandle handle, uv_buf_t* bufs, int nbufs, uv_write_cb cb)
        {
            req.Validate();
            handle.Validate();
            ThrowIfErrored(uv_write(req, handle, bufs, nbufs, cb));
        }

        public unsafe static void write2(UVWriteRequest req, UVStreamHandle handle, uv_buf_t* bufs, int nbufs, UVStreamHandle sendHandle, uv_write_cb cb)
        {
            req.Validate();
            handle.Validate();
            ThrowIfErrored(uv_write2(req, handle, bufs, nbufs, sendHandle, cb));
        }

        public static string err_name(int err)
        {
            IntPtr ptr = uv_err_name(err);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        public static string strerror(int err)
        {
            IntPtr ptr = uv_strerror(err);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        public static int loop_size()
        {
            return uv_loop_size();
        }

        public static int handle_size(UVHandleType handleType)
        {
            return uv_handle_size(handleType);
        }

        public static int req_size(UVRequestType reqType)
        {
            return uv_req_size(reqType);
        }

        public static void ip4_addr(string ip, int port, out SockAddr addr, out UVException error)
        {
            Check(uv_ip4_addr(ip, port, out addr), out error);
        }

        public static void ip6_addr(string ip, int port, out SockAddr addr, out UVException error)
        {
            Check(uv_ip6_addr(ip, port, out addr), out error);
        }

        public static void walk(UVLoopHandle loop, uv_walk_cb walk_cb, IntPtr arg)
        {
            loop.Validate();
            uv_walk(loop, walk_cb, arg);
        }

        public unsafe static long now(UVLoopHandle loop)
        {
            loop.Validate();
            return uv_now(loop);
        }

        
        public static void tcp_getsockname(UVTCPHandle handle, out SockAddr addr, ref int namelen)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_getsockname(handle, out addr, ref namelen));
        }

        
        public static void tcp_getpeername(UVTCPHandle handle, out SockAddr addr, ref int namelen)
        {
            handle.Validate();
            ThrowIfErrored(uv_tcp_getpeername(handle, out addr, ref namelen));
        }

        public static uv_buf_t buf_init(IntPtr memory, int len)
        {
            return new uv_buf_t(memory, len, IsWindows);
        }


        /// <summary>
        /// 内存数据移动
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
        /// <param name="isWindows"></param>
        public static void memorymove( IntPtr src, IntPtr dest, Int32 size, bool isWindows)
        {
            if (isWindows)
            {
                MoveMemory(dest, src, (uint)size);
            }
            else
            {
                memmove(dest, src, (uint)size);
            }
        }

#endregion
        
#region Unmanaged Functions
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_connect_cb(IntPtr req, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_connection_cb(IntPtr server, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_close_cb(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_async_cb(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_alloc_cb(IntPtr server, int suggested_size, out uv_buf_t buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_read_cb(IntPtr server, int nread, ref uv_buf_t buf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_write_cb(IntPtr req, int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_walk_cb(IntPtr handle, IntPtr arg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_timer_cb(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_idle_cb(IntPtr server);
#endregion

#region P/Invoke
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_init(UVLoopHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_close(IntPtr a0);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_run(UVLoopHandle handle, int mode);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_stop(UVLoopHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_ref(UVHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_unref(UVHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_fileno(UVHandle handle, ref IntPtr socket);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void uv_close(IntPtr handle, uv_close_cb close_cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_idle_init(UVLoopHandle loop, UVIdleHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_idle_start(UVIdleHandle handle, uv_idle_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_idle_stop(UVIdleHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl, EntryPoint = "uv_async_send")]
        public extern static int uv_unsafe_async_send(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_init(UVLoopHandle loop, UVTCPHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_bind(UVTCPHandle handle, ref SockAddr addr, int flags);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_open(UVTCPHandle handle, IntPtr hSocket);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_nodelay(UVTCPHandle handle, int enable);

        

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_listen(UVStreamHandle handle, int backlog, uv_connection_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_accept(UVStreamHandle server, UVStreamHandle client);
        

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public extern static int uv_read_start(UVStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_read_stop(UVStreamHandle handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_try_write(UVStreamHandle handle, uv_buf_t[] bufs, int nbufs);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern int uv_write(UVWriteRequest req, UVStreamHandle handle, uv_buf_t* bufs, int nbufs, uv_write_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern int uv_write2(UVWriteRequest req, UVStreamHandle handle, uv_buf_t* bufs, int nbufs, UVStreamHandle sendHandle, uv_write_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr uv_err_name(int err);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr uv_strerror(int err);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_loop_size();

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_handle_size(UVHandleType handleType);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_req_size(UVRequestType reqType);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_ip4_addr(string ip, int port, out SockAddr addr);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_ip6_addr(string ip, int port, out SockAddr addr);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_connect(UVConnectRquest connect, UVTCPHandle socket,ref SockAddr addr, uv_connect_cb cb);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_getsockname(UVTCPHandle handle, out SockAddr name, ref int namelen);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_tcp_getpeername(UVTCPHandle handle, out SockAddr name, ref int namelen);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uv_walk(UVLoopHandle loop, uv_walk_cb walk_cb, IntPtr arg);
        
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern long uv_now(UVLoopHandle loop);

        [DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi)]
        unsafe public static extern int WSAIoctl(
            IntPtr socket,
            int dwIoControlCode,
            int* lpvInBuffer,
            uint cbInBuffer,
            int* lpvOutBuffer,
            int cbOutBuffer,
            out uint lpcbBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine
        );

        [DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int WSAGetLastError();



        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Ansi)]
        public extern static long MoveMemory(IntPtr dest, IntPtr src, uint size);


        [DllImport("libm.so")]
        public static extern void memmove(IntPtr dest, IntPtr src, uint length);





#endregion
    }
}
