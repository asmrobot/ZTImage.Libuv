using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZTImage.Libuv.Collections;

namespace ZTImage.Libuv
{
    public class UVTCPHandle : UVStreamHandle
    {
        private readonly static UVIntrop.uv_connection_cb mOnConnection = UVConnectionCb;
        private Action<UVTCPHandle, Int32, UVException, Object> mConnectionCallback;
        private object mConnectionCallbackState;
        
        public UVTCPHandle(UVLoopHandle loop)
        {
            CreateHandle(UVHandleType.TCP);
            UVIntrop.tcp_init(loop, this);
        }

        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Bind(string ip, Int32 port)
        {
            UVException exception;
            SockAddr addr;
            UVIntrop.ip4_addr(ip, port, out addr, out exception);
            if (exception != null)
            {
                throw exception;
            }

            UVIntrop.tcp_bind(this, ref addr, 0);
        }

        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="backlog"></param>
        /// <param name="connectionCallback"></param>
        /// <param name="state"></param>
        public void Listen(int backlog, Action<UVTCPHandle, Int32, UVException, Object> connectionCallback, object state)
        {
            mConnectionCallbackState = state;
            mConnectionCallback = connectionCallback;
            UVIntrop.listen(this, backlog, mOnConnection);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="handle"></param>
        public void Accept(UVTCPHandle handle)
        {
            UVIntrop.accept(this, handle);
        }

        /// <summary>
        /// 主动连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="connectCallback"></param>
        /// <param name="state"></param>
        public void Connect(string ip,Int32 port,Action<UVException , object> connectCallback,object state)
        {
            ZTImage.Utils.Validate.ThrowIfNullOrWhite(ip,"ip error");
            ZTImage.Utils.Validate.ThrowIfZeroOrMinus(port, "port is error");

            UVConnectRquest request = new UVConnectRquest();
            request.Connect(this, ip, port, (req, status, ex, s) => {
                if (connectCallback != null)
                {
                    connectCallback(ex, s);
                }
            },state);
        }

        public IPEndPoint LocalIPEndPoint
        {
            get
            {
                SockAddr addr = default(SockAddr);
                Int32 namelen = Marshal.SizeOf(addr);
                try
                {
                    UVIntrop.tcp_getsockname(this, out addr, ref namelen);
                }
                catch (UVException ex)
                {
                    throw ex;
                }
                return addr.GetIPEndPoint();
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                SockAddr addr = default(SockAddr);
                Int32 namelen = Marshal.SizeOf(addr);
                try
                {
                    UVIntrop.tcp_getpeername(this, out addr, ref namelen);
                }
                catch (UVException ex)
                {
                    throw ex;
                }
                return addr.GetIPEndPoint();
            }
        }


        public void NoDelay(bool enable)
        {
            UVIntrop.tcp_nodelay(this, enable);
        }

        protected override bool ReleaseHandle()
        {
            this.mConnectionCallbackState = null;
            this.mConnectionCallback = null;
            return base.ReleaseHandle();
        }

        //todo:重载各种write

        #region  Callback
        private static void UVConnectionCb(IntPtr server, Int32 status)
        {
            UVException error;
            UVIntrop.Check(status, out error);
            UVTCPHandle handle = UVMemory.FromIntPtr<UVTCPHandle>(server);
            try
            {
                handle.mConnectionCallback(handle, status, error, handle.mConnectionCallbackState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
