using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace toofz.Services
{
    public static class HttpClientFactory
    {
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
