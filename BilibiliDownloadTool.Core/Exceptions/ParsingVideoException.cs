using System;

namespace BilibiliDownloadTool.Core.Exceptions
{

    [Serializable]
    public class ParsingVideoException : Exception
    {
        public ParsingVideoException() { }
        public ParsingVideoException(string message) : base(message) { }
        public ParsingVideoException(string message, Exception inner) : base(message, inner) { }
        protected ParsingVideoException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
