// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggingExtensions
    {
        private static readonly Action<ILogger, Exception?> _noCertificate;
        private static readonly Action<ILogger, Exception?> _notHttps;
        private static readonly Action<ILogger, string, string, Exception?> _certRejected;
        private static readonly Action<ILogger, string, string, Exception?> _certFailedValidation;

        static LoggingExtensions()
        {
            _noCertificate = LoggerMessage.Define(
                eventId: new EventId(0, "NoCertificate"),
                logLevel: LogLevel.Debug,
                formatString: "No client certificate found.");

            _certRejected = LoggerMessage.Define<string, string>(
                eventId: new EventId(1, "CertificateRejected"),
                logLevel: LogLevel.Warning,
                formatString: "{CertificateType} certificate rejected, subject was {Subject}.");

            _certFailedValidation = LoggerMessage.Define<string, string>(
                eventId: new EventId(2, "CertificateFailedValidation"),
                logLevel: LogLevel.Warning,
                formatString: "Certificate validation failed, subject was {Subject}." + Environment.NewLine + "{ChainErrors}");

            _notHttps = LoggerMessage.Define(
                eventId: new EventId(3, "NotHttps"),
                logLevel: LogLevel.Debug,
                formatString: "Not https, skipping certificate authentication.");
        }

        public static void NoCertificate(this ILogger logger)
        {
            _noCertificate(logger, null);
        }

        public static void NotHttps(this ILogger logger)
        {
            _notHttps(logger, null);
        }

        public static void CertificateRejected(this ILogger logger, string certificateType, string subject)
        {
            _certRejected(logger, certificateType, subject, null);
        }

        public static void CertificateFailedValidation(this ILogger logger, string subject, IEnumerable<string> chainedErrors)
        {
            _certFailedValidation(logger, subject, String.Join(Environment.NewLine, chainedErrors), null);
        }
    }
}
