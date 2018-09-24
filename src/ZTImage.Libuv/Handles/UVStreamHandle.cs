using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{

    public abstract class UVStreamHandle : UVHandle
    {
        public UVStreamHandle():base(GCHandleType.Normal)
        {}

        public delegate UVIntrop.uv_buf_t AllocCallback(UVStreamHandle handle, Int32 suggestedSize,object allocState);
        public delegate void ReadCallback(UVStreamHandle handle, Int32 nread, UVException exception,ref UVIntrop.uv_buf_t buf, object readState);
        
        private readonly static UVIntrop.uv_read_cb mOnRead = UVReadCb;
        private readonly static UVIntrop.uv_alloc_cb mOnAlloc = UVAllocCb;
        private AllocCallback mAllocCallback;
        private ReadCallback mReadCallback;

        private object mAllocCallbackState;        
        private object mReadCallbackState;
        
        public void ReadStart(AllocCallback allocCallback, ReadCallback readCallback,object allocState,object readState)
        {
            this.mAllocCallback = allocCallback;
            this.mReadCallback = readCallback;
            this.mReadCallbackState = readState;
            this.mAllocCallbackState = allocState;
            UVIntrop.read_start(this, mOnAlloc, mOnRead);
        }

        public void ReadStop()
        {
            UVIntrop.read_stop(this);
        } 

        public void Write(byte[] datas)
        {
            Write(datas,0, datas.Length);
        }

        public unsafe void Write(byte[] datas,Int32 offset, Int32 count)
        {
            fixed (byte* p = datas)
            {
                UVIntrop.uv_buf_t[] mbuf = new UVIntrop.uv_buf_t[]{
                    UVIntrop.buf_init((IntPtr)(p+offset), count)
                };
                
                UVIntrop.try_write(this, mbuf, 1);
            }
        }
        
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public unsafe void Write(UVWriteRequest request, Action<UVWriteRequest, Int32, UVException, object> callback, object state)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((obj) => {
                request.Write(this, callback, state);
            });
        }

        #region UVCallback
        private static void UVAllocCb(IntPtr handle, int suggestedSize, out UVIntrop.uv_buf_t buf)
        {
            UVStreamHandle target=FromIntPtr<UVStreamHandle>(handle);
            if (target == null)
            {
                throw new UVException("流已释放");
            }
            try
            {
                buf = target.mAllocCallback(target, suggestedSize,target.mAllocCallbackState);
            }
            catch (Exception ex)
            {
                //todo:清理操作
                throw new UVException("分配内存出错,"+ex.Message);
            }
        }

        private unsafe static void UVReadCb(IntPtr handle, int nread, ref UVIntrop.uv_buf_t buf)
        {
            UVException ex;
            UVIntrop.Check(nread, out ex);

            UVStreamHandle target = FromIntPtr<UVStreamHandle>(handle);
            if (target == null)
            {
                throw new UVException("流已释放");
            }
            target.mReadCallback(target, nread, ex,ref buf, target.mReadCallbackState);
        }

        
        #endregion
    }
}
