using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Libuv
{
    public class UVException : Exception
    {
        private string message;
        public UVException(string message, Int32 statusCode) : base(message)
        {
            this.message = message;
        }

        public UVException(string message):base(message)
        {
            this.message = message;
        }
    }
}
