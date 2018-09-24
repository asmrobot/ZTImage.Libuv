using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZTImage.Libuv.Collections;

namespace ZTImage.Libuv
{
    public unsafe class UVConnectRquest : UVRequest
    {
        private static UVIntrop.uv_connect_cb UVConnectcb = UVConnectCallback;
        
        private Action<UVConnectRquest, Int32, UVException, object> mCallback;
        private object mState;

        public UVConnectRquest()
        {
            CreateRequest(UVRequestType.CONNECT);
        }

        public void Connect(UVTCPHandle tcp,string ip,Int32 port, Action<UVConnectRquest, Int32, UVException, object> callback,object state)
        {
            this.mCallback = callback;
            this.mState = state;
            UVException ex;
            SockAddr addr;
            UVIntrop.ip4_addr(ip, port, out addr, out ex);
            if (ex != null)
            {
                throw ex;
            }

            UVIntrop.tcp_connect(this, tcp, ref addr, UVConnectcb);
        }
        
        protected override bool ReleaseHandle()
        {
            DestroyMemory(handle);
            handle = IntPtr.Zero;
            return true;
        }

        #region Callback
        private static void UVConnectCallback(IntPtr reqHandle, Int32 status)
        {
            var req = FromIntPtr<UVConnectRquest>(reqHandle);
            var callback = req.mCallback;
            var state = req.mState;

            
            UVException error = null;
            if (status < 0)
            {
                UVIntrop.Check(status, out error);
            }

            try
            {
                if (callback != null)
                {
                    callback(req, status, error, state);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                req.Close();
            }
        }
        #endregion
    }
}
