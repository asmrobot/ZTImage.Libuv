using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTImage.Utils
{
    internal class Validate
    {
        /// <summary>
        /// Null抛出异常
        /// </summary>
        /// <param name="val"></param>
        public static void ThrowIfNull(object val)
        {
            ThrowIfNull(val, "参数为空!~");
        }

        /// <summary>
        /// Null抛出异常
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        public static void ThrowIfNull(object val,string message)
        {
            if (val==null)
            {
                throw new ArgumentNullException(message);
            }
        }

        /// <summary>
        /// 空或空白字符抛出异常
        /// </summary>
        /// <param name="val"></param>
        public static void ThrowIfNullOrEmpty(string val)
        {
            ThrowIfNullOrEmpty(val, "参数为空");
        }

        /// <summary>
        /// 空或空白字符抛出异常
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        public static void ThrowIfNullOrEmpty(string val,string message)
        {
            if (string.IsNullOrEmpty(val))
            {
                throw new ArgumentNullException(message);
            }
        }

        /// <summary>
        /// 空或空白字符抛出异常
        /// </summary>
        /// <param name="val"></param>
        public static void ThrowIfNullOrWhite(string val)
        {
            ThrowIfNullOrWhite(val, "参数为无意义字符");
        }

        /// <summary>
        /// 空或空白字符抛出异常
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        public static void ThrowIfNullOrWhite(string val,string message)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                throw new ArgumentNullException(message);
            }
        }

        /// <summary>
        /// 等于0抛出异常
        /// </summary>
        /// <param name="val"></param>
        public static void ThrowIfZero(Int32 val)
        {
            ThrowIfZero(val, "参数不能为零");
        }

        /// <summary>
        /// 等于0抛出异常
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        public static void ThrowIfZero(Int32 val,string message)
        {
            if (val == 0)
            {
                throw new ArgumentNullException(message);
            }
        }

        /// <summary>
        /// 小于等于0抛出异常
        /// </summary>
        /// <param name="val"></param>
        public static void ThrowIfZeroOrMinus(Int32 val)
        {
            ThrowIfZeroOrMinus(val, "参数不能小于等于零");
        }

        /// <summary>
        /// 小于等于0抛出异常
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        public static void ThrowIfZeroOrMinus(Int32 val,string message)
        {
            if (val <= 0)
            {
                throw new ArgumentNullException(message);
            }
        }
    }
}
