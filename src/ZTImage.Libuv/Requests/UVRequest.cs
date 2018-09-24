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
    public unsafe class UVRequest : UVMemory
    {
        

        public UVRequest() : base(GCHandleType.Normal)
        {}


        protected void CreateRequest(Int32 size)
        {
            CreateMemory(size);
        }

        protected void CreateRequest(UVRequestType type)
        {
            CreateMemory(UVIntrop.req_size(type));
        }
        
        protected override bool ReleaseHandle()
        {
            IntPtr memory = handle;
            if (memory != IntPtr.Zero)
            {
                DestroyMemory(handle);
                handle = IntPtr.Zero;
            }

            return true;
        }
    }
}
