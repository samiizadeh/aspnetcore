// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggingExtensions
    {
        private static readonly Action<ILogger, Exception?> _errorClosingTheSession;
        private static readonly Action<ILogger, string, Exception?> _accessingExpiredSession;
        private static readonly Action<ILogger, string, string, Exception?> _sessionStarted;
        private static readonly Action<ILogger, string, string, int, Exception?> _sessionLoaded;
        private static readonly Action<ILogger, string, string, int, Exception?> _sessionStored;
        private static readonly Action<ILogger, string, Exception?> _sessionCacheReadException;
        private static readonly Action<ILogger, Exception?> _errorUnprotectingCookie;
        private static readonly Action<ILogger, Exception?> _sessionLoadingTimeout;
        private static readonly Action<ILogger, Exception?> _sessionCommitTimeout;
        private static readonly Action<ILogger, Exception?> _sessionCommitCanceled;
        private static readonly Action<ILogger, Exception?> _sessionRefreshTimeout;
        private static readonly Action<ILogger, Exception?> _sessionRefreshCanceled;
        private static readonly Action<ILogger, Exception?> _sessionNotAvailable;

        private static readonly LogDefineOptions SkipEnabledCheckLogOptions = new() { SkipEnabledCheck = true };

        static LoggingExtensions()
        {
            _errorClosingTheSession = LoggerMessage.Define(
                eventId: new EventId(1, "ErrorClosingTheSession"),
                logLevel: LogLevel.Error,
                formatString: "Error closing the session.");
            _accessingExpiredSession = LoggerMessage.Define<string>(
                eventId: new EventId(2, "AccessingExpiredSession"),
                logLevel: LogLevel.Information,
                formatString: "Accessing expired session, Key:{sessionKey}");
            _sessionStarted = LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(3, "SessionStarted"),
                formatString: "Session started; Key:{sessionKey}, Id:{sessionId}",
                SkipEnabledCheckLogOptions);
            _sessionLoaded = LoggerMessage.Define<string, string, int>(
                logLevel: LogLevel.Debug,
                eventId: new EventId(4, "SessionLoaded"),
                formatString: "Session loaded; Key:{sessionKey}, Id:{sessionId}, Count:{count}",
                SkipEnabledCheckLogOptions);
            _sessionStored = LoggerMessage.Define<string, string, int>(
                eventId: new EventId(5, "SessionStored"),
                logLevel: LogLevel.Debug,
                formatString: "Session stored; Key:{sessionKey}, Id:{sessionId}, Count:{count}");
            _sessionCacheReadException = LoggerMessage.Define<string>(
                logLevel: LogLevel.Error,
                eventId: new EventId(6, "SessionCacheReadException"),
                formatString: "Session cache read exception, Key:{sessionKey}",
                SkipEnabledCheckLogOptions);
            _errorUnprotectingCookie = LoggerMessage.Define(
                eventId: new EventId(7, "ErrorUnprotectingCookie"),
                logLevel: LogLevel.Warning,
                formatString: "Error unprotecting the session cookie.");
            _sessionLoadingTimeout = LoggerMessage.Define(
                eventId: new EventId(8, "SessionLoadingTimeout"),
                logLevel: LogLevel.Warning,
                formatString: "Loading the session timed out.");
            _sessionCommitTimeout = LoggerMessage.Define(
                eventId: new EventId(9, "SessionCommitTimeout"),
                logLevel: LogLevel.Warning,
                formatString: "Committing the session timed out.");
            _sessionCommitCanceled = LoggerMessage.Define(
                eventId: new EventId(10, "SessionCommitCanceled"),
                logLevel: LogLevel.Information,
                formatString: "Committing the session was canceled.");
            _sessionRefreshTimeout = LoggerMessage.Define(
                eventId: new EventId(11, "SessionRefreshTimeout"),
                logLevel: LogLevel.Warning,
                formatString: "Refreshing the session timed out.");
            _sessionRefreshCanceled = LoggerMessage.Define(
                eventId: new EventId(12, "SessionRefreshCanceled"),
                logLevel: LogLevel.Information,
                formatString: "Refreshing the session was canceled.");
            _sessionNotAvailable = LoggerMessage.Define(
                eventId: new EventId(13, "SessionCommitNotAvailable"),
                logLevel: LogLevel.Information,
                formatString: "Session cannot be committed since it is unavailable.");
        }

        public static void ErrorClosingTheSession(this ILogger logger, Exception exception)
        {
            _errorClosingTheSession(logger, exception);
        }

        public static void AccessingExpiredSession(this ILogger logger, string sessionKey)
        {
            _accessingExpiredSession(logger, sessionKey, null);
        }

        public static void SessionStarted(this ILogger logger, string sessionKey, string sessionId)
        {
            _sessionStarted(logger, sessionKey, sessionId, null);
        }

        public static void SessionLoaded(this ILogger logger, string sessionKey, string sessionId, int count)
        {
            _sessionLoaded(logger, sessionKey, sessionId, count, null);
        }

        public static void SessionStored(this ILogger logger, string sessionKey, string sessionId, int count)
        {
            _sessionStored(logger, sessionKey, sessionId, count, null);
        }

        public static void SessionCacheReadException(this ILogger logger, string sessionKey, Exception exception)
        {
            _sessionCacheReadException(logger, sessionKey, exception);
        }

        public static void ErrorUnprotectingSessionCookie(this ILogger logger, Exception exception)
        {
            _errorUnprotectingCookie(logger, exception);
        }

        public static void SessionLoadingTimeout(this ILogger logger)
        {
            _sessionLoadingTimeout(logger, null);
        }

        public static void SessionCommitTimeout(this ILogger logger)
        {
            _sessionCommitTimeout(logger, null);
        }

        public static void SessionCommitCanceled(this ILogger logger)
        {
            _sessionCommitCanceled(logger, null);
        }

        public static void SessionRefreshTimeout(this ILogger logger)
        {
            _sessionRefreshTimeout(logger, null);
        }

        public static void SessionRefreshCanceled(this ILogger logger)
        {
            _sessionRefreshCanceled(logger, null);
        }

        public static void SessionNotAvailable(this ILogger logger) => _sessionNotAvailable(logger, null);
    }
}
