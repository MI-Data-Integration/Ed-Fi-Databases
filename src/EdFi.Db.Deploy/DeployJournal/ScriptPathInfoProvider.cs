// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Db.Deploy.Adapters;
using EdFi.Db.Deploy.Helpers;
using EdFi.Db.Deploy.Parameters;
using EdFi.Db.Deploy.ScriptPathResolvers;

namespace EdFi.Db.Deploy.DeployJournal
{
    public class ScriptPathInfoProvider : IScriptPathInfoProvider
    {
        private IOptions _options;
        private readonly IFileSystem _fileSystemAdapter;

        public ScriptPathInfoProvider(IFileSystem fileSystemAdapter)
        {
            _fileSystemAdapter = Preconditions.ThrowIfNull(fileSystemAdapter, nameof(fileSystemAdapter));
        }

        public IReadOnlyList<IScriptPathInfo> FindAllScriptsInFileSystem(IOptions options)
        {
            _options = Preconditions.ThrowIfNull(options, nameof(options));

            var scriptPaths = new List<ScriptPathInfo>();

            scriptPaths.AddRange(FindCoreScripts());
            scriptPaths.AddRange(FindLegacyExtensionScripts());
            scriptPaths.AddRange(FindFeatureScripts());
            scriptPaths.AddRange(FindLegacyExtensionFeatureScripts());

            return scriptPaths;
        }

        private IEnumerable<ScriptPathInfo> FindCoreScripts()
        {
            var filePaths = _options.FilePaths.ToList();
            var databaseType = _options.DatabaseType;
            var engineType = _options.Engine;
            var scriptPaths = new List<ScriptPathInfo>();

            foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
            {
                scriptPaths.AddRange(
                    from path in filePaths
                    let scriptsPath = GetScriptsPathFromDatabaseDeployPath(path, scriptType)
                    where _fileSystemAdapter.DirectoryExists(scriptsPath)
                    select new ScriptPathInfo(_fileSystemAdapter, path, scriptsPath));
            }

            return scriptPaths;

            string GetScriptsPathFromDatabaseDeployPath(string path, ScriptType scriptType)
            {
                string scriptsPath = GetScriptsPath(new ScriptPathResolver(path, databaseType, engineType), scriptType);

                if (!_fileSystemAdapter.DirectoryExists(scriptsPath) && engineType == EngineType.SqlServer)
                {
                    scriptsPath = GetScriptsPath(new LegacyScriptPathResolver(path, databaseType), scriptType);
                }

                return scriptsPath;
            }
        }

        private IEnumerable<ScriptPathInfo> FindLegacyExtensionScripts()
        {
            if (_options.Engine != EngineType.SqlServer)
            {
                return new List<ScriptPathInfo>();
            }

            var filePaths = _options.FilePaths.ToList();
            var databaseType = _options.DatabaseType;
            var scriptPaths = new List<ScriptPathInfo>();

            foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
            {
                scriptPaths.AddRange(
                    from path in filePaths
                    let scriptsPath = GetScriptPathsFromLegacyExtensionsPath(path, scriptType)
                    where _fileSystemAdapter.DirectoryExists(scriptsPath)
                    select new ScriptPathInfo(_fileSystemAdapter, path, scriptsPath));
            }
            return scriptPaths;

            string GetScriptPathsFromLegacyExtensionsPath(string path, ScriptType scriptType)
            {
                return GetScriptsPath(new LegacyExtensionsScriptPathResolver(path, databaseType), scriptType);
            }
        }

        private IEnumerable<ScriptPathInfo> FindFeatureScripts()
        {
            var filePaths = _options.FilePaths.ToList();
            var features = _options.Features.ToList();
            var databaseType = _options.DatabaseType;
            var engineType = _options.Engine;
            var scriptPaths = new List<ScriptPathInfo>();

            foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
            {
                scriptPaths.AddRange(
                    from path in filePaths
                    from feature in features
                    let scriptPath = GetScriptsPathFromFeatureDeployPath(path, feature, scriptType)
                    where _fileSystemAdapter.DirectoryExists(scriptPath)
                    select new ScriptPathInfo(_fileSystemAdapter, path, scriptPath));
            }
            return scriptPaths;

            string GetScriptsPathFromFeatureDeployPath(string path, string feature, ScriptType scriptType)
            {
                string scriptsPath = GetScriptsPath(new ScriptPathResolver(path, databaseType, EngineType.SqlServer, feature), scriptType);

                if (!_fileSystemAdapter.DirectoryExists(scriptsPath) && engineType == EngineType.SqlServer)
                {
                    scriptsPath = GetScriptsPath(new LegacyScriptPathResolver(path, databaseType, feature), scriptType);
                }

                return scriptsPath;
            }
        }

        private IEnumerable<ScriptPathInfo> FindLegacyExtensionFeatureScripts()
        {
            if (_options.Engine != EngineType.SqlServer)
            {
                return new List<ScriptPathInfo>();
            }

            var filePaths = _options.FilePaths.ToList();
            var features = _options.Features.ToList();
            var databaseType = _options.DatabaseType;

            var scriptPaths = new List<ScriptPathInfo>();

            foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
            {
                scriptPaths.AddRange(
                    from path in filePaths
                    from feature in features
                    let scriptsPath = GetScriptPathsFromLegacyExtensionFeaturesPath(path, feature, scriptType)
                    where _fileSystemAdapter.DirectoryExists(scriptsPath)
                    select new ScriptPathInfo(_fileSystemAdapter, path, scriptsPath));
            }
            return scriptPaths;

            string GetScriptPathsFromLegacyExtensionFeaturesPath(string path, string feature, ScriptType scriptType)
            {
                return GetScriptsPath(new LegacyExtensionsScriptPathResolver(path, databaseType, feature), scriptType);
            }
        }

        private static string GetScriptsPath(IScriptPathResolver fileInfoProvider, ScriptType scriptType)
                    => scriptType switch
                    {
                        ScriptType.Migration => fileInfoProvider.MigrationScriptPath(),
                        ScriptType.Structure => fileInfoProvider.StructureScriptPath(),
                        _ => fileInfoProvider.DataScriptPath()
                    };
    }
}