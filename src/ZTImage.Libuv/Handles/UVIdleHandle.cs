using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    public class UVIdleHandle:UVHandle
    {
        private static readonly UVIntrop.uv_idle_cb mIdleCallback = IdleCallback;
        public UVIdleHandle(UVLoopHandle loop)
        {
            this.CreateHandle(UVHandleType.IDLE);
            UVIntrop.idle_init(loop, this);
        }

        private Action<UVIdleHandle> mCallback;

        public void Start(Action<UVIdleHandle> callback)
        {
            this.mCallback = callback;
            UVIntrop.idle_start(this, mIdleCallback);
        }

        public void Stop()
        {
            UVIntrop.idle_stop(this);
        }

        private static void IdleCallback(IntPtr handle)
        {
            var idle=UVHandle.FromIntPtr<UVIdleHandle>(handle);
            idle.mCallback(idle);
        }




    }
}
