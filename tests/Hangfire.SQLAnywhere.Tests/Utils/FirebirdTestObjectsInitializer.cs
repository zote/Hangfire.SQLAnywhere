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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Dapper;
using SQLAnywhereSql.Data.SQLAnywhereClient;
using SQLAnywhereSql.Data.Isql;

namespace Hangfire.SQLAnywhere.Tests
{
    [ExcludeFromCodeCoverage]
    internal static class SQLAnywhereTestObjectsInitializer
    {
        public static void CleanTables(FbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var script = GetStringResource(
                typeof(SQLAnywhereTestObjectsInitializer).Assembly,
                "Hangfire.SQLAnywhere.Tests.Clean.sql");

            FbScript fbScript = new FbScript(script);
            fbScript.Parse();

            FbBatchExecution fbBatch = new FbBatchExecution(connection, fbScript);
            fbBatch.Execute(true);
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "Requested resource `{0}` was not found in the assembly `{1}`.",
                        resourceName,
                        assembly));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
