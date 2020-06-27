using System;

namespace SlideCore.Data
{
	public enum ContentTypes
	{
		LevelPackList,
		LevelPack,
		Level
	}

	public interface IContentStore
	{
		string LoadContentForKey(ContentTypes contentType, string key, string defaultData);
		string LoadContentForKey(ContentTypes contentType, string key);
	}

	/// <summary>Exception thrown when the string was not able to parsable as content</summary>
	[Serializable]
	public class InvalidSerializedContentException : Exception
	{
		public InvalidSerializedContentException() { }

		public InvalidSerializedContentException(string message) : base(message) { }

		public InvalidSerializedContentException(string message, Exception innerException) : base(message, innerException) { }

		protected InvalidSerializedContentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	public static class ContentManager
	{
		private static IContentStore _contentStore;

		public static void RegisterContentStore(IContentStore contentStore)
		{
			_contentStore = contentStore ?? throw new ArgumentNullException(nameof(contentStore));
		}

		public static void UnregisterContentStore()
		{
			_contentStore = null;
		}

		public static string LoadContent(ContentTypes contentType, string key, string defaultData)
		{
			if (_contentStore == null) throw new Exception($"Please register a {nameof(IContentStore)} with the {nameof(ContentManager)} before attempting to use it");
			return _contentStore.LoadContentForKey(contentType, key, defaultData);
		}

		public static string LoadContent(ContentTypes contentType, string key)
		{
			if (_contentStore == null) throw new Exception($"Please register a {nameof(IContentStore)} with the {nameof(ContentManager)} before attempting to use it");
			return _contentStore.LoadContentForKey(contentType, key);
		}
	}
}
