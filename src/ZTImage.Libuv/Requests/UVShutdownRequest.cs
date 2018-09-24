using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv.Requests
{
    public class UVShutdownRequest: UVRequest
    {
        public UVShutdownRequest()
        {
            CreateRequest(UVRequestType.SHUTDOWN);
        }


    }
}
