﻿using System.Collections.Generic;
using System.IO;

namespace Nyan.Modules.Storage
{
    public interface IStorageProvider
    {
        Stream this[string key] { get; set; }
        EOperationalStatus OperationalStatus { get; }
        string Put(Stream source, string fileKey = null);
        FileStream Get(string key);
        string GetFullPath(string key);
        string GetBasePath();
        IEnumerable<string> GetKeys();
        bool Exists(string key);
        void Remove(string key);
        void RemoveAll();
        void Initialize();
        void Shutdown();
    }
}