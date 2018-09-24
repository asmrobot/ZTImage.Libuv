using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ZTImage.Libuv
{
    public abstract class UVHandle:UVMemory
    {
        private static readonly UVIntrop.uv_close_cb mDestroyMemory = DestroyMemory;
        public UVHandle():base(GCHandleType.Weak)
        {}

        public UVHandle(GCHandleType gcType) : base(gcType)
        { }

        protected void CreateHandle(Int32 size)
        {
            CreateMemory(size);
        }

        protected void CreateHandle(UVHandleType type)
        {
            CreateMemory(UVIntrop.handle_size(type));
        }

        protected override bool ReleaseHandle()
        {
            IntPtr memory = handle;
            if (memory != IntPtr.Zero)
            {
                UVIntrop.close(memory, mDestroyMemory);
                handle = IntPtr.Zero;
            }
            
            return true;
        }
    }
}
