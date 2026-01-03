// eely.GitMoH - (c) 2025-26 by Joerg Plenert, Voerde
using System;

namespace eely.gitmoh
{
    internal class GitMohException : Exception
    {
        public GitMohException() : base() { }
        public GitMohException(string message) : base(message) { }
        public GitMohException(string message, Exception innerException) : base(message, innerException) { }
    }
}
