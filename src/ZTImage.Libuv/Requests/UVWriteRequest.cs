using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    public unsafe class UVWriteRequest : UVRequest, IList<UVIntrop.uv_buf_t>
    {
        public const Int32 QUEUE_SIZE = 6;
        private readonly static UVIntrop.uv_write_cb mOnWrite = UVWriteCb;

        private Action<UVWriteRequest, Int32, UVException, object> mWriteCallback;
        private object mWriteCallbackState;
        private GCHandle[] mPins;

        private bool m_IsReadOnly = false;
        private int m_Updateing = 0;//进入队列数
        private UVIntrop.uv_buf_t* mBuffer;
        private int mOffset;
        

        public UVWriteRequest() 
        {
            Int32 requestSize = UVIntrop.req_size(UVRequestType.WRITE);
            var bufferSize = Marshal.SizeOf(typeof(UVIntrop.uv_buf_t)) * QUEUE_SIZE;
            this.mPins = new GCHandle[QUEUE_SIZE];
            CreateMemory(requestSize + bufferSize);
            this.mBuffer = (UVIntrop.uv_buf_t*)(this.handle + requestSize);
        }

        #region 队列操作 
        public void StartEnqueue()
        {
            m_IsReadOnly = false;
        }

        public void StopEnqueue()
        {
            if (m_IsReadOnly)
            {
                return;
            }

            m_IsReadOnly = true;
            if (m_Updateing < 0)
            {
                return;
            }

            SpinWait wait = new SpinWait();
            wait.SpinOnce();
            while (m_Updateing > 0)
            {
                wait.SpinOnce();
            }
        }
        
        public bool Enqueue(ArraySegment<byte> item)
        {
            if (m_IsReadOnly)
            {
                return false;
            }

            Interlocked.Increment(ref m_Updateing);
            while (!m_IsReadOnly)
            {
                bool conflict = false;
                if (TryEnqueue(item, out conflict))
                {
                    Interlocked.Decrement(ref m_Updateing);
                    return true;
                }

                if (!conflict)
                {
                    break;
                }
            }
            Interlocked.Decrement(ref m_Updateing);
            return false;
        }

        private unsafe bool TryEnqueue(ArraySegment<byte> item, out bool conflict)
        {
            conflict = false;
            int currentCount = mOffset;
            if (currentCount >= QUEUE_SIZE)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref mOffset, currentCount + 1, currentCount) != currentCount)
            {
                conflict = true;
                return false;
            }

            //添加
            var gcHandle = GCHandle.Alloc(item.Array, GCHandleType.Pinned);
            mPins[currentCount]=gcHandle;
            this.mBuffer[currentCount] = UVIntrop.buf_init(gcHandle.AddrOfPinnedObject() + item.Offset, item.Count);

            return true;
        }

        public bool Enqueue(IList<ArraySegment<byte>> items)
        {
            if (m_IsReadOnly)
            {
                return false;
            }

            Interlocked.Increment(ref m_Updateing);
            while (!m_IsReadOnly)
            {
                bool conflict = false;
                if (TryEnqueue(items, out conflict))
                {
                    Interlocked.Decrement(ref m_Updateing);
                    return true;
                }

                if (!conflict)
                {
                    break;
                }
            }
            Interlocked.Decrement(ref m_Updateing);
            return false;
        }

        private bool TryEnqueue(IList<ArraySegment<byte>> items, out bool conflict)
        {
            conflict = false;
            int oldCurrent = mOffset;
            if (oldCurrent + items.Count >= QUEUE_SIZE)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref mOffset, oldCurrent + items.Count, oldCurrent) != oldCurrent)
            {
                conflict = true;
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ArraySegment<byte> item = items[i];
                var gcHandle = GCHandle.Alloc(item.Array, GCHandleType.Pinned);
                mPins[oldCurrent+i] = gcHandle;
                this.mBuffer[oldCurrent+i] = UVIntrop.buf_init(gcHandle.AddrOfPinnedObject() + item.Offset, item.Count);
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        internal unsafe void Write(UVStreamHandle server, Action<UVWriteRequest, Int32, UVException, object> callback, object state)
        {
            try
            {
                this.mWriteCallback = callback;
                this.mWriteCallbackState = state;
                UVIntrop.write(this, server, this.mBuffer, this.mOffset, mOnWrite);
            }
            catch
            {
                this.UnpinGCHandles();
                throw;
            }
        }
        
        /// <summary>
        /// 触发已发送
        /// </summary>
        /// <param name="status"></param>
        /// <param name="error"></param>
        public void RaiseSended(Int32 status,UVException error)
        {
            try
            {
                if (this.mWriteCallback != null)
                {
                    this.mWriteCallback(this, status, error, this.mWriteCallbackState);
                }
            }
            catch (Exception ex)
            {
                this.mWriteCallback = null;
                this.mWriteCallbackState = null;
                UnpinGCHandles();
                //Trace.Error("UvWriteCb", ex);
                throw;
            }
        }
        
        // Safe handle has instance method called Unpin
        // so using UnpinGcHandles to avoid conflict
        private void UnpinGCHandles()
        {            
            for (var i = 0; i < this.mOffset; i++)
            {
                this.mPins[i].Free();
                this.mPins[i] = default(GCHandle);
            }
        }



        #region Callback
        private unsafe static void UVWriteCb(IntPtr reqHandle, Int32 status)
        {
            var req = FromIntPtr<UVWriteRequest>(reqHandle);

            UVException error = null;
            if (status < 0)
            {
                UVIntrop.Check(status, out error);
            }

            try
            {
                req.RaiseSended(status, error);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region IList

        public UVIntrop.uv_buf_t this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index 小于0");
                }
                
                var value = this.mBuffer[index];
                return value;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int IndexOf(UVIntrop.uv_buf_t item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, UVIntrop.uv_buf_t item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return this.mOffset;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.m_IsReadOnly;
            }
        }


        public void Add(UVIntrop.uv_buf_t item)
        {
            throw new NotSupportedException();
        }



        public void Clear()
        {
            this.UnpinGCHandles();
            this.mWriteCallback = null;
            this.mWriteCallbackState = null;

            this.mOffset = 0;

        }

        public bool Contains(UVIntrop.uv_buf_t item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(UVIntrop.uv_buf_t[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public bool Remove(UVIntrop.uv_buf_t item)
        {
            throw new NotImplementedException();
        }


        public unsafe IEnumerator<UVIntrop.uv_buf_t> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
