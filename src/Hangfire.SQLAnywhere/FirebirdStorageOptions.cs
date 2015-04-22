﻿// This file is part of Hangfire.Firebird

// Copyright © 2015 Rob Segerink <https://github.com/rsegerink/Hangfire.Firebird>.
// 
// Hangfire.Firebird is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire.Firebird is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Hangfire.Firebird. If not, see <http://www.gnu.org/licenses/>.
//
// This work is based on the work of Sergey Odinokov, author of 
// Hangfire. <http://hangfire.io/>
//   
//    Special thanks goes to him.

using System;

namespace Hangfire.Firebird
{
    public class FirebirdStorageOptions
    {
        private TimeSpan _queuePollInterval;

        public FirebirdStorageOptions()
        {
            QueuePollInterval = TimeSpan.FromSeconds(15);
            InvisibilityTimeout = TimeSpan.FromMinutes(30);
            Prefix = "HANGFIRE";
            UtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours;
            PrepareSchemaIfNecessary = true;
        }

        public TimeSpan QueuePollInterval
        {
            get { return _queuePollInterval; }
            set
            {
                var message = String.Format(
                    "The QueuePollInterval property value should be positive. Given: {0}.",
                    value);

                if (value == TimeSpan.Zero)
                {
                    throw new ArgumentException(message, "value");
                }
                if (value != value.Duration())
                {
                    throw new ArgumentException(message, "value");
                }

                _queuePollInterval = value;
            }
        }

        public TimeSpan InvisibilityTimeout { get; set; }

        public bool PrepareSchemaIfNecessary { get; set; }

        public string Prefix { get; private set; }

        public double UtcOffset { get; set; }
    }
}
