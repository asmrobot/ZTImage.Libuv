using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    /// <summary>
    /// 基本内存分配
    /// </summary>
    public unsafe abstract class UVMemory : SafeHandle
    {
        private GCHandleType mHandleType;
        public UVMemory(GCHandleType type=GCHandleType.Weak):base(IntPtr.Zero,true)
        {
            this.mHandleType = type;
        }
        
        /// <summary>
        /// 分配需要的内存并把本对象GCHand放入
        /// </summary>
        /// <param name="size"></param>
        protected void CreateMemory(Int32 size)
        {
            handle = Marshal.AllocHGlobal(size);
            //todo:那种分配内存的方式更好
            //handle = Marshal.AllocCoTaskMem(size);
            GCHandle gcHandlePtr = GCHandle.Alloc(this, this.mHandleType);
            *(IntPtr*)handle = GCHandle.ToIntPtr(gcHandlePtr);
        }

        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="memory"></param>
        protected static void DestroyMemory(IntPtr memory)
        {
            IntPtr gcHandlePtr = *(IntPtr*)memory;
            DestroyMemory(memory, gcHandlePtr);
        }

        /// <summary>
        /// 释放GCHandle及其占用的内存
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="gcHandlePtr"></param>
        protected static void DestroyMemory(IntPtr memory, IntPtr gcHandlePtr)
        {
            if (gcHandlePtr != IntPtr.Zero)
            {
                GCHandle gcHandle=GCHandle.FromIntPtr(gcHandlePtr);
                gcHandle.Free();
            }
            Marshal.FreeHGlobal(memory);
        }


        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }
        
        public void Validate(Boolean closed = false)
        {

        }

        public IntPtr InternalGetHandle()
        {
            return handle;
        }


        public static T FromIntPtr<T>(IntPtr memory)
        {
            GCHandle gcHandle=GCHandle.FromIntPtr(*(IntPtr*)memory);
            return (T)gcHandle.Target;
        }
    }
}
