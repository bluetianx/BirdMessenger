using System;

namespace BirdMessenger.Infrastructure
{
    public class TusException : Exception
    {
        public TusException(string message) : base(message)
        {

        }
    }
}