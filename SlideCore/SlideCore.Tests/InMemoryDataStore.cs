using NUnit.Framework;
using SlideCore.Data;
using System;
using System.Collections.Generic;

namespace SlideCore.Tests
{
	public class InMemoryDataStore : IDataStore, IDisposable
	{
		private Dictionary<string, string> _dataStore;

		public InMemoryDataStore()
		{
			_dataStore = new Dictionary<string, string>();
			DataManager.RegisterDataStore(this);
		}

		public void Dispose()
		{
			DataManager.UnregisterDataStore();
		}

		public string LoadDataForKey(string key)
		{
			var uniqueKey = $"{TestContext.CurrentContext.Test.ID}\\{key}";
			if (!_dataStore.ContainsKey(uniqueKey))
				throw new Exception($"No data stored for key {key}");
			return _dataStore[uniqueKey];
		}

		public string LoadDataForKey(string key, string defaultData)
		{
			var uniqueKey = $"{TestContext.CurrentContext.Test.ID}\\{key}";
			if (!_dataStore.ContainsKey(uniqueKey))
				return defaultData;
			return _dataStore[uniqueKey];
		}

		public void StoreDataForKey(string key, string data)
		{
			var uniqueKey = $"{TestContext.CurrentContext.Test.ID}\\{key}";
			if (_dataStore.ContainsKey(uniqueKey))
				_dataStore[uniqueKey] = data;
			else
				_dataStore.Add(uniqueKey, data);
		}
	}
}
