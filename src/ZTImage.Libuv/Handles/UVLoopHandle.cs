using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZTImage.Libuv
{
    public class UVLoopHandle : UVHandle
    {
        public static readonly UVLoopHandle Define = new UVLoopHandle();
        private Int32 mStateMutex = 0;
        
        public UVLoopHandle()
        {
            CreateHandle(UVIntrop.loop_size());
            UVIntrop.loop_init(this);
        }


        /// <summary>
        /// 同步开始
        /// </summary>
        public void Start()
        {
            if (Interlocked.CompareExchange(ref this.mStateMutex, 1, 0) == 0)
            {
                StartThread(null);
            }
            else
            {
                throw new Exception("已经开始，不可重复调用");
            }
            
        }

        /// <summary>
        /// 异步开始
        /// </summary>
        public void AsyncStart(Action<UVLoopHandle> callback)
        {
            if (Interlocked.CompareExchange(ref this.mStateMutex, 1, 0) == 0)
            {
                Thread thread = new Thread(StartThread);
                thread.IsBackground = true;
                thread.Start(callback);
            }            
        }

        private void StartThread(object state)
        {
            Action<UVLoopHandle> cb = state as Action<UVLoopHandle>;

            UVIntrop.run(this, (Int32)UVIntrop.UV_RUN_MODE.UV_RUN_DEFAULT);
            Volatile.Write(ref mStateMutex, 0);
            if (cb != null)
            {
                cb(this);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            UVIntrop.stop(this);
        }

        /// <summary>
        /// 循环关闭
        /// </summary>
        public void LoopClose()
        {
            UVIntrop.loop_close(this);
        }
    }
}
