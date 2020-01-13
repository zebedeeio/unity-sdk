// <copyright file="BitPayException.cs" company="Zebedee Inc.">
//
//  Copyright (c) Zebedee, Inc. and its affiliates.
//
//  ZEBEDEE Unity SDK can not be copied and/or distributed without the express
//  permission of Zebedee, Inc. and its affiliates.
//
// </copyright>

namespace ZbdUnitySDK.Exception
{
    using System;

    /// <summary>
    /// Provides an API specific exception handler.
    /// </summary>
    public class ZedebeeException : Exception
    {
            private readonly string message = "Exception information not provided";
            private Exception inner = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitPayException"/> class.
        /// </summary>
            public ZedebeeException()
            {
            }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitPayException"/> class.
        /// Constructor.  Creates an exception with a message only.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
            public ZedebeeException(string message)
                : base(message)
            {
            this.message = message;
            }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitPayException"/> class.
        /// Constructor.  Creates an exception with a message and root cause exception.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="inner">The root cause exception.</param>
            public ZedebeeException(string message, Exception inner)
                : base(message, inner)
            {
            this.message = message;
            this.inner = inner;
            }

        /// <summary>
        /// The exception message text.
        /// </summary>
        /// <returns>The exception message text.</returns>
            public string GetMessage()
            {
                return this.message;
            }

        /// <summary>
        /// The root cause exception.
        /// </summary>
        /// <returns>The root cause exception.</returns>
            public Exception GetInner()
            {
                return inner;
            }
        }
}
