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

namespace Hangfire.SQLAnywhere
{
    internal class SQLAnywhereJobQueueProvider : IPersistentJobQueueProvider
    {
        private readonly SQLAnywhereStorageOptions _options;

        public SQLAnywhereJobQueueProvider(SQLAnywhereStorageOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            _options = options;
        }

        public SQLAnywhereStorageOptions Options { get { return _options; } }

        public IPersistentJobQueue GetJobQueue(IDbConnection connection)
        {
            return new SQLAnywhereJobQueue(connection, _options);
        }

        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi(IDbConnection connection)
        {
            return new SQLAnywhereJobQueueMonitoringApi(connection, _options);
        }
    }
}
