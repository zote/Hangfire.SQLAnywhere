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
﻿
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;
using Dapper;
using SQLAnywhereSql.Data.SQLAnywhereClient;
using Xunit;

namespace Hangfire.SQLAnywhere.Tests
{
    public class CleanDatabaseAttribute : BeforeAfterTestAttribute
    {
        private static readonly object GlobalLock = new object();
        private static bool _sqlObjectInstalled;

        public CleanDatabaseAttribute()
        {

        }

        public override void Before(MethodInfo methodUnderTest)
        {
            Monitor.Enter(GlobalLock);

            if (!_sqlObjectInstalled)
            {
                RecreateDatabaseAndInstallObjects();
                _sqlObjectInstalled = true;
            }
            CleanTables();
        }

        public override void After(MethodInfo methodUnderTest)
        {
            try
            {
                FbConnection.ClearAllPools();
            }
            finally
            {
                Monitor.Exit(GlobalLock);
            }

        }

        private static void RecreateDatabaseAndInstallObjects()
        {
            FbConnection.ClearAllPools();

            var connectionStringBuilder = new FbConnectionStringBuilder(ConnectionUtils.GetConnectionString());
            if (File.Exists(connectionStringBuilder.Database))
                FbConnection.DropDatabase(connectionStringBuilder.ConnectionString);

            FbConnection.CreateDatabase(connectionStringBuilder.ToString(), 16384, true, false);

            using (var connection = new FbConnection(connectionStringBuilder.ToString()))
            {
                SQLAnywhereObjectsInstaller.Install(connection);
            }
        }

        private static void CleanTables()
        {
            using (var connection = new FbConnection(
                ConnectionUtils.GetConnectionString()))
            {
                SQLAnywhereTestObjectsInitializer.CleanTables(connection);
            }
        }
    }
}
