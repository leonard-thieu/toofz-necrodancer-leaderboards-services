/*
    Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
    Microsoft Open Technologies would like to thank its contributors, a list
    of whom are at http://aspnetwebstack.codeplex.com/wikipage?title=Contributors.

    Licensed under the Apache License, Version 2.0 (the "License"); you
    may not use this file except in compliance with the License. You may
    obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
    implied. See the License for the specific language governing permissions
    and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace toofz.Services
{
    public static class HttpClientFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the <see cref="DelegatingHandler"/> instances
        /// provided by <paramref name="handlers"/>. The resulting pipeline can be used to manually create <see cref="HttpClient"/>
        /// or <see cref="HttpMessageInvoker"/> instances with customized message handlers.
        /// </summary>
        /// <param name="innerHandler">The inner handler represents the destination of the HTTP message channel.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as part 
        /// of sending an <see cref="HttpRequestMessage"/> and receiving an <see cref="HttpResponseMessage"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for 
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>The HTTP message channel.</returns>
        public static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<DelegatingHandler> handlers)
        {
            if (innerHandler == null)
                throw new ArgumentNullException(nameof(innerHandler));
            if (handlers == null)
                return innerHandler;

            var httpMessageHandler = innerHandler;

            foreach (var delegatingHandler in handlers.Reverse())
            {
                if (delegatingHandler == null)
                    throw new ArgumentNullException(nameof(handlers));
                if (delegatingHandler.InnerHandler != null)
                    throw new ArgumentException("DelegatingHandler has non-null inner handler.", nameof(handlers));

                delegatingHandler.InnerHandler = httpMessageHandler;
                httpMessageHandler = delegatingHandler;
            }

            return httpMessageHandler;
        }
    }
}
