using System;

namespace SlideCore.Data
{
	public interface IDataStore
	{
		void StoreDataForKey(string key, string data);
		string LoadDataForKey(string key, string defaultData);
		string LoadDataForKey(string key);
	}

	public static class DataManager
	{
		private static IDataStore _dataStore;

		public static void RegisterDataStore(IDataStore dataStore)
		{
			_dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
		}

		public static void UnregisterDataStore()
		{
			_dataStore = null;
		}

		public static void StoreData(string key, string data)
		{
			if (_dataStore == null) throw new Exception($"Please register a {nameof(IDataStore)} with the {nameof(DataManager)} before attempting to use it");
			_dataStore.StoreDataForKey(key, data);
		}

		public static string LoadData(string key, string defaultData)
		{
			if (_dataStore == null) throw new Exception($"Please register a {nameof(IDataStore)} with the {nameof(DataManager)} before attempting to use it");
			return _dataStore.LoadDataForKey(key, defaultData);
		}

		public static string LoadData(string key)
		{
			if (_dataStore == null) throw new Exception($"Please register a {nameof(IDataStore)} with the {nameof(DataManager)} before attempting to use it");
			return _dataStore.LoadDataForKey(key);
		}
	}
}
