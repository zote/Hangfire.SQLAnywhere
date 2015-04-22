// This file is part of Hangfire.SQLAnywhere

// Copyright © 2015 Rob Segerink <https://github.com/rsegerink/Hangfire.SQLAnywhere>.
// 
// Hangfire.SQLAnywhere is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire.SQLAnywhere is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire.SQLAnywhere. If not, see <http://www.gnu.org/licenses/>.
//
// This work is based on the work of Sergey Odinokov, author of 
// Hangfire. <http://hangfire.io/>
//   
//    Special thanks goes to him.

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Hangfire.Logging;
using System.Data.Common;

namespace Hangfire.SQLAnywhere
{
    [ExcludeFromCodeCoverage]
    internal static class SQLAnywhereObjectsInstaller
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(SQLAnywhereStorage));

        public static void Install(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            Log.Info("Start installing Hangfire SQL objects...");

            int version = 1;
            bool scriptFound = true;

            do
            {
                try
                {
                    var script = GetStringResource(typeof(SQLAnywhereObjectsInstaller).Assembly, string.Format("Hangfire.SQLAnywhere.Install.v{0}.sql", version.ToString(CultureInfo.InvariantCulture)));

                    if (!VersionAlreadyApplied(connection, version))
                    {
                        FbScript fbScript = new FbScript(script);
                        fbScript.Parse();

                        FbBatchExecution fbBatch = new FbBatchExecution(connection, fbScript);
                        fbBatch.Execute(true);

                        UpdateVersion(connection, version);
                    }
                }
                catch (DbException)
                {
                    throw;
                }
                catch (Exception)
                {
                    scriptFound = false;
                }

                version++;
            } while (scriptFound);

            Log.Info("Hangfire SQL objects installed.");
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "Requested resource '{0}' was not found in the assembly `{1}`.",
                        resourceName,
                        assembly));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static bool VersionAlreadyApplied(DbConnection connection, int version)
        {
            using (var cmd = connection.CreateCommand())
            {
                bool alreadyApplied = false;

                cmd.CommandText = @"SELECT count(*) FROM sys.systables where table_name = 'HANGFIRE_SCHEMA';";

                bool tableExists = Convert.ToBoolean(cmd.ExecuteScalar());

                if (tableExists)
                {
                    cmd.CommandText = string.Format(@"SELECT 1 FROM HANGFIRE_SCHEMA WHERE VERSION = {0};", version);

                    alreadyApplied = Convert.ToBoolean(cmd.ExecuteScalar());
                }

                return alreadyApplied;
            }
        }

        private static void UpdateVersion(DbConnection connection, int version)
        {
            using (var cmd = connection.CreateCommand())
            {
                if (version == 1)
                {
                    cmd.CommandText = string.Format(@"INSERT INTO HANGFIRE_SCHEMA (VERSION) VALUES ({0});", version);
                }
                else
                {
                    cmd.CommandText = string.Format(@"UPDATE HANGFIRE_SCHEMA SET VERSION = {0};", version);
                }

                cmd.ExecuteNonQuery();
            }
        }
    }
}
