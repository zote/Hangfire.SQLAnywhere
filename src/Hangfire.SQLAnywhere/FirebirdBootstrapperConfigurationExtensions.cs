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

namespace Hangfire.SQLAnywhere
{
    public static class SQLAnywhereBootstrapperConfigurationExtensions
    {
        /// <summary>
        /// Tells the bootstrapper to use Fireibrd as a job storage,
        /// that can be accessed using the given connection string or 
        /// its name.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="nameOrConnectionString">Connection string or its name</param>
        public static SQLAnywhereStorage UseSQLAnywhereStorage(
            this IBootstrapperConfiguration configuration,
            string nameOrConnectionString)
        {
            var storage = new SQLAnywhereStorage(nameOrConnectionString);
            configuration.UseStorage(storage);

            return storage;
        }

        /// <summary>
        /// Tells the bootstrapper to use SQLAnywhere as a job storage
        /// with the given options, that can be accessed using the specified
        /// connection string or its name.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="nameOrConnectionString">Connection string or its name</param>
        /// <param name="options">Advanced options</param>
        public static SQLAnywhereStorage UseSQLAnywhereStorage(
            this IBootstrapperConfiguration configuration,
            string nameOrConnectionString,
            SQLAnywhereStorageOptions options)
        {
            var storage = new SQLAnywhereStorage(nameOrConnectionString, options);
            configuration.UseStorage(storage);

            return storage;
        }
    }
}
