using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.AccountApi
{

    public class AccessTokenInvalidException : Exception
    {
        public AccessTokenInvalidException() { }
        public AccessTokenInvalidException(string message) : base(message) { }
        public AccessTokenInvalidException(string message, Exception inner) : base(message, inner) { }
    }
}
