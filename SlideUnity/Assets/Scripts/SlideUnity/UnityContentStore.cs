using SlideCore.Data;
using UnityEngine;

namespace SlideUnity
{
	public class UnityContentStore : IContentStore
	{
		public string LoadContentForKey(ContentTypes contentType, string key, string defaultData)
		{
			try
			{
				var content = Resources.Load<TextAsset>($@"{contentType}\{key}");
				return content.text;
			}
			catch
			{
				return defaultData;
			}
		}

		public string LoadContentForKey(ContentTypes contentType, string key)
		{
			try
			{
				var content = Resources.Load<TextAsset>($@"{contentType}\{key}");
				return content.text;
			}
			catch
			{
				throw new System.Exception($"No content of type [{contentType}] available for key [{key}]");
			}
		}
	}
}