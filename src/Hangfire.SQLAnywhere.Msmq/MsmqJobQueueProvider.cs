﻿// This file is part of Hangfire.SQLAnywhere

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

using System.Collections.Generic;
using System.Data;

namespace Hangfire.SQLAnywhere.Msmq
{
    internal class MsmqJobQueueProvider : IPersistentJobQueueProvider
    {
        private readonly MsmqJobQueue _jobQueue;
        private readonly MsmqJobQueueMonitoringApi _monitoringApi;

        public MsmqJobQueueProvider(string pathPattern, IEnumerable<string> queues)
        {
            _jobQueue = new MsmqJobQueue(pathPattern);
            _monitoringApi = new MsmqJobQueueMonitoringApi(pathPattern, queues);
        }

        public IPersistentJobQueue GetJobQueue(IDbConnection connection)
        {
            return _jobQueue;
        }

        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi(IDbConnection connection)
        {
            return _monitoringApi;
        }
    }
}