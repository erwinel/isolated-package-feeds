﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 
using System;
using System.Collections.Generic;
using NuGet.Server.Core.Infrastructure;

namespace ExampleNuGetHost
{
    public class DictionarySettingsProvider : ISettingsProvider
    {
        private readonly Dictionary<string, object> _settings;

        public DictionarySettingsProvider(Dictionary<string, object> settings)
        {
            _settings = settings;
        }

        public bool GetBoolSetting(string key, bool defaultValue)
        {
            return _settings.ContainsKey(key) ? Convert.ToBoolean(_settings[key]) : defaultValue;
        }

        public int GetIntSetting(string key, int defaultValue)
        {
            return _settings.ContainsKey(key) ? Convert.ToInt32(_settings[key]) : defaultValue;
        }

        public string GetStringSetting(string key, string defaultValue)
        {
            return _settings.ContainsKey(key) ? Convert.ToString(_settings[key]) : defaultValue;
        }
    }
}
