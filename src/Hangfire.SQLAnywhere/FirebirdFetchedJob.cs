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
using Dapper;
using Hangfire.Storage;

namespace Hangfire.SQLAnywhere
{
    internal class SQLAnywhereFetchedJob : IFetchedJob
    {
        private readonly IDbConnection _connection;
        private readonly SQLAnywhereStorageOptions _options;
        private bool _disposed;
        private bool _removedFromQueue;
        private bool _requeued;

        public SQLAnywhereFetchedJob(
            IDbConnection connection, 
            SQLAnywhereStorageOptions options,
            int id, 
            string jobId, 
            string queue)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (jobId == null) throw new ArgumentNullException("jobId");
            if (queue == null) throw new ArgumentNullException("queue");
            if (options == null) throw new ArgumentNullException("options");

            _connection = connection;
            _options = options;

            Id = id;
            JobId = jobId;
            Queue = queue;
        }

        public int Id { get; private set; }
        public string JobId { get; private set; }
        public string Queue { get; private set; }

        public void RemoveFromQueue()
        {
            _connection.Execute(string.Format(@"
                DELETE FROM ""{0}.JOBQUEUE"" 
                WHERE id = @id;", _options.Prefix),
                new { id = Id });

            _removedFromQueue = true;
        }

        public void Requeue()
        {
            _connection.Execute(string.Format(@"
                UPDATE ""{0}.JOBQUEUE"" 
                SET fetchedat = NULL 
                WHERE id = @id;", _options.Prefix),
                new { id = Id });

            _requeued = true;
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (!_removedFromQueue && !_requeued)
            {
                Requeue();
            }

            _disposed = true;
        }
    }
}
