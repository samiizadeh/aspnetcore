// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.HttpLogging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for the HttpLogging middleware.
    /// </summary>
    public static class HttpLoggingServicesExtensions
    {
        /// <summary>
        /// Adds HTTP Logging services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="HttpLoggingOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddHttpLogging(this IServiceCollection services, Action<HttpLoggingOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            return services;
        }

        /// <summary>
        /// Adds W3C Logging services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="W3CLoggerOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddW3CLogging(this IServiceCollection services, Action<W3CLoggerOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            services.AddSingleton<W3CLogger>();
            return services;
        }
    }
}
