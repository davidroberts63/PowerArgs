﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerArgs
{
    /// <summary>
    /// An exception that should be thrown when the error condition is caused because of bad user input.
    /// </summary>
    public class ArgException : Exception
    {
        /// <summary>
        /// Creates a new ArgException given a user friendly message
        /// </summary>
        /// <param name="msg">A user friendly message.</param>
        public ArgException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new ArgException given a user friendly message
        /// </summary>
        /// <param name="msg">A user friendly message.</param>
        /// <param name="inner">The inner exception that caused the problem</param>
        public ArgException(string msg, Exception inner) : base(msg, inner) { }
    }

    /// <summary>
    /// An exception that should be thrown when the error condition is caused by an improperly formed
    /// argument scaffold type.  For example if the user specified the same shortcut value for more than one property.
    /// </summary>
    public class InvalidArgDefinitionException : Exception
    {
        /// <summary>
        /// Creates a new InvalidArgDefinitionException given a message.
        /// </summary>
        /// <param name="msg">An error message.</param>
        public InvalidArgDefinitionException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new InvalidArgDefinitionException given a message.
        /// </summary>
        /// <param name="msg">An error message.</param>
        /// <param name="inner">The inner exception that caused the problem</param>
        public InvalidArgDefinitionException(string msg, Exception inner) : base(msg, inner) { }
    }
}
