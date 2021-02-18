using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliDownloadTool.Core.Exceptions
{

    [Serializable]
    public class MangaNeedBuyException : Exception
    {
        public MangaNeedBuyException() { }
        public MangaNeedBuyException(string message) : base(message) { }
        public MangaNeedBuyException(string message, Exception inner) : base(message, inner) { }
        protected MangaNeedBuyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
