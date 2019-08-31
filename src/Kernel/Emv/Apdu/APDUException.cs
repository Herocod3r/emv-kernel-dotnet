using System;

namespace Kernel.Emv.Apdu
{
    public class APDUException : Exception
    {
        public APDUException(string message ,Exception internalException) : base(message,internalException)
        {
            
        }
        public APDUException(string message) : base(message)
        {
            
        }

        public APDUException(string message,APDUCommand command) : this(message)
        {
            Command = command;
        }

        public APDUCommand Command { get; }
        
    }
}