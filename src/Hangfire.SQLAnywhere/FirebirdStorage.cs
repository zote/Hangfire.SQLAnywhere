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
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.IO;
using Hangfire.Annotations;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.Storage;
using SQLAnywhereSql.Data.SQLAnywhereClient;

namespace Hangfire.SQLAnywhere
{
    public class SQLAnywhereStorage : JobStorage
    {
        private readonly FbConnection _existingConnection;
        private readonly SQLAnywhereStorageOptions _options;
        private readonly string _connectionString;

        public SQLAnywhereStorage(string nameOrConnectionString)
            : this(nameOrConnectionString, new SQLAnywhereStorageOptions())
        {
        }

        /// <summary>
        /// Initializes SQLAnywhereStorage from the provided SQLAnywhereStorageOptions and either the provided connection
        /// string or the connection string with provided name pulled from the application config file.       
        /// </summary>
        /// <param name="nameOrConnectionString">Either a SQLAnywhere connection string or the name of 
        /// a SQLAnywhere connection string located in the connectionStrings node in the application config</param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"><paramref name="nameOrConnectionString"/> argument is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> argument is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="nameOrConnectionString"/> argument is neither 
        /// a valid SQLAnywhere connection string nor the name of a connection string in the application
        /// config file.</exception>
        public SQLAnywhereStorage(string nameOrConnectionString, SQLAnywhereStorageOptions options)
        {
            if (nameOrConnectionString == null) throw new ArgumentNullException("nameOrConnectionString");
            if (options == null) throw new ArgumentNullException("options");

            _options = options;

            if (IsConnectionString(nameOrConnectionString))
            {
                _connectionString = nameOrConnectionString;
            }
            else if (IsConnectionStringInConfiguration(nameOrConnectionString))
            {
                _connectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Could not find connection string with name '{0}' in application config file",
                                  nameOrConnectionString));
            }

            if (options.PrepareSchemaIfNecessary)
            {
                var connectionStringBuilder = new FbConnectionStringBuilder(_connectionString);
                if (!File.Exists(connectionStringBuilder.Database))
                    FbConnection.CreateDatabase(_connectionString, 16384, true, false);

                using (var connection = CreateAndOpenConnection())
                {
                    SQLAnywhereObjectsInstaller.Install(connection);
                }
            }

            InitializeQueueProviders();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLAnywhereStorage"/> class with
        /// explicit instance of the <see cref="FbConnection"/> class that will be used
        /// to query the data.
        /// </summary>
        /// <param name="existingConnection">Existing connection</param>
        /// <param name="options">SQLAnywhereStorageOptions</param>
        public SQLAnywhereStorage(FbConnection existingConnection, SQLAnywhereStorageOptions options)
        {
            if (existingConnection == null) throw new ArgumentNullException("existingConnection");
            if (options == null) throw new ArgumentNullException("options");
            //var connectionStringBuilder = new FbConnectionStringBuilder(existingConnection.ConnectionString);
            //if (connectionStringBuilder.Enlist) throw new ArgumentException("SQLAnywhereSql is not fully compatible with TransactionScope yet, only connections without Enlist = true are accepted.");

            _existingConnection = existingConnection;
            _options = new SQLAnywhereStorageOptions();

            InitializeQueueProviders();
        }

        public PersistentJobQueueProviderCollection QueueProviders { get; private set; }

        public override IMonitoringApi GetMonitoringApi()
        {
            return new SQLAnywhereMonitoringApi(_connectionString, _options, QueueProviders);
        }

        public override IStorageConnection GetConnection()
        {
            var connection = _existingConnection ?? CreateAndOpenConnection();
            return new SQLAnywhereConnection(connection, QueueProviders, _options, _existingConnection == null);
        }

        public override IEnumerable<IServerComponent> GetComponents()
        {
            yield return new ExpirationManager(this, _options);
        }

        public override void WriteOptionsToLog(ILog logger)
        {
            logger.Info("Using the following options for SQL Server job storage:");
            logger.InfoFormat("    Queue poll interval: {0}.", _options.QueuePollInterval);
            logger.InfoFormat("    Invisibility timeout: {0}.", _options.InvisibilityTimeout);
        }

        public override string ToString()
        {
            const string canNotParseMessage = "<Connection string can not be parsed>";

            try
            {
                var connectionStringBuilder = new FbConnectionStringBuilder(_connectionString);
                var builder = new StringBuilder();

                builder.Append("Data Source: ");
                builder.Append(connectionStringBuilder.DataSource);
                builder.Append(", Server Type: ");
                builder.Append(connectionStringBuilder.ServerType);
                builder.Append(", Database: ");
                builder.Append(connectionStringBuilder.Database);

                return builder.Length != 0
                    ? string.Format("SQLAnywhere Server: {0}", builder)
                    : canNotParseMessage;
            }
            catch (Exception)
            {
                return canNotParseMessage;
            }
        }

        internal FbConnection CreateAndOpenConnection()
        {
            var connection = new FbConnection(_connectionString);
            connection.Open();

            return connection;
        }

        private void InitializeQueueProviders()
        {
            var defaultQueueProvider = new SQLAnywhereJobQueueProvider(_options);
            QueueProviders = new PersistentJobQueueProviderCollection(defaultQueueProvider);
        }

        private bool IsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.Contains(";");
        }

        private bool IsConnectionStringInConfiguration(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionStringSetting != null;
        }
    }
}
