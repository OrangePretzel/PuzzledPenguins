using SlideCore.Data;
using UnityEngine;

namespace SlideUnity
{
	public class UnityDataStore : IDataStore
	{
		public string LoadDataForKey(string key, string defaultData)
		{
			return PlayerPrefs.GetString(key, defaultData);
		}

		public string LoadDataForKey(string key)
		{
			return PlayerPrefs.GetString(key);
		}

		public void StoreDataForKey(string key, string data)
		{
			PlayerPrefs.SetString(key, data);
			PlayerPrefs.Save();
		}
	}
}