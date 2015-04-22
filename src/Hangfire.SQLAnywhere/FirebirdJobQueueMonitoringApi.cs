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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Hangfire.SQLAnywhere
{
    internal class SQLAnywhereJobQueueMonitoringApi : IPersistentJobQueueMonitoringApi
    {
        private readonly IDbConnection _connection;
        private readonly SQLAnywhereStorageOptions _options;

        public SQLAnywhereJobQueueMonitoringApi(IDbConnection connection, SQLAnywhereStorageOptions options)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (options == null) throw new ArgumentNullException("options");

            _connection = connection;
            _options = options;
        }

        public IEnumerable<string> GetQueues()
        {
            string sqlQuery = string.Format(@"
                SELECT DISTINCT QUEUE 
                FROM ""{0}.JOBQUEUE"";", _options.Prefix); 

            return _connection.Query(sqlQuery).Select(x => (string)x.queue).ToList();
        }

        public IEnumerable<int> GetEnqueuedJobIds(string queue, int @from, int perPage)
        {
            return GetQueuedOrFetchedJobIds(queue, false, @from, perPage);
        }

        private IEnumerable<int> GetQueuedOrFetchedJobIds(string queue, bool fetched, int @from, int perPage)
        {
            string sqlQuery = string.Format(@"
                SELECT j.id 
                FROM ""{0}.JOBQUEUE"" jq
                LEFT JOIN ""{0}.JOB"" j ON jq.jobid = j.id
                WHERE jq.queue = @queue 
                AND jq.fetchedat {1}
                AND j.id IS NOT NULL
                ROWS @start TO @end;
                ", _options.Prefix, fetched ? "IS NOT NULL" : "IS NULL");

            return _connection.Query<int>(
                sqlQuery,
                new { queue = queue, start = @from + 1, end = @from + perPage })
                .ToList();
        }

        public IEnumerable<int> GetFetchedJobIds(string queue, int @from, int perPage)
        {
            return GetQueuedOrFetchedJobIds(queue, true, @from, perPage);
        }

        public EnqueuedAndFetchedCountDto GetEnqueuedAndFetchedCount(string queue)
        {
            string sqlQuery = string.Format(@"
                SELECT (
                        SELECT COUNT(*) 
                        FROM ""{0}.JOBQUEUE"" 
                        WHERE fetchedat IS NULL 
                        AND queue = @queue
                    ) ""EnqueuedCount"", 
                    (
                        SELECT COUNT(*) 
                        FROM ""{0}.JOBQUEUE"" 
                        WHERE fetchedat IS NOT NULL 
                        AND queue = @queue
                    ) ""FetchedCount""
                FROM rdb$database;", _options.Prefix);

            var result = _connection.Query(sqlQuery, new { queue = queue }).Single();

            return new EnqueuedAndFetchedCountDto
            {
                EnqueuedCount = result.EnqueuedCount,
                FetchedCount = result.FetchedCount
            };
        }
    }
}
