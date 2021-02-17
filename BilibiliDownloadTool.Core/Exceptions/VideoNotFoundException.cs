using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliDownloadTool.Core.Exceptions
{

    [Serializable]
    public class VideoNotFoundException : Exception
    {
        public VideoNotFoundException() { }
        public VideoNotFoundException(string message) : base(message) { }
        public VideoNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected VideoNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
