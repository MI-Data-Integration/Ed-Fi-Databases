// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DbUp.Engine.Output;
using EdFi.Db.Deploy.Helpers;
using log4net;
using System;

namespace EdFi.Db.Deploy
{
    public class DbUpLogger : IUpgradeLog
    {
        private readonly ILog _logger;

        public DbUpLogger(ILog logger)
        {
            _logger = Preconditions.ThrowIfNull(logger, nameof(logger));
        }

        public void LogInformation(string format, params object[] args)
        {
            Preconditions.ThrowIfNull(format, nameof(format));
            Preconditions.ThrowIfNull(args, nameof(args));

            _logger.InfoFormat(format, args);
        }

        public void LogError(string format, params object[] args)
        {
            Preconditions.ThrowIfNull(format, nameof(format));
            Preconditions.ThrowIfNull(args, nameof(args));

            _logger.ErrorFormat(format, args);
        }

		public void LogError(Exception ex, string format, params object[] args)
		{
			Preconditions.ThrowIfNull(format, nameof(format));
			Preconditions.ThrowIfNull(args, nameof(args));

			_logger.Error(string.Format(format, args), ex);
		}

		public void LogWarning(string format, params object[] args)
        {
            Preconditions.ThrowIfNull(format, nameof(format));
            Preconditions.ThrowIfNull(args, nameof(args));

            _logger.WarnFormat(format, args);
        }

		public void LogTrace(string format, params object[] args)
		{
			Preconditions.ThrowIfNull(format, nameof(format));
			Preconditions.ThrowIfNull(args, nameof(args));
			
            _logger.DebugFormat(format, args);
		}

		public void LogDebug(string format, params object[] args)
		{
			Preconditions.ThrowIfNull(format, nameof(format));
			Preconditions.ThrowIfNull(args, nameof(args));

			_logger.DebugFormat(format, args);
		}

	}
}
