using NUnit.Framework;
using SlideCore.Data;
using System;
using System.IO;

namespace SlideCore.Tests
{
	public class TestContentStore : IContentStore, IDisposable
	{
		public const string TEST_PREFIX = "TestContent";

		public TestContentStore()
		{
			ContentManager.RegisterContentStore(this);
		}

		public void Dispose()
		{
			ContentManager.UnregisterContentStore();
		}

		public string LoadContentForKey(ContentTypes contentType, string key, string defaultData)
		{
			var filePath = GetFilePathFor(contentType, key);
			Console.WriteLine($"Loading content from {filePath}");
			if (File.Exists(filePath))
				return File.ReadAllText(filePath);
			return defaultData;
		}

		public string LoadContentForKey(ContentTypes contentType, string key)
		{
			var filePath = GetFilePathFor(contentType, key);
			Console.WriteLine($"Loading content from {filePath}");
			if (File.Exists(filePath))
				return File.ReadAllText(filePath);
			throw new Exception($"No content of type [{contentType}] available for key [{key}] [{filePath}]");
		}

		private string GetFilePathFor(ContentTypes contentType, string key)
		{
			if (key.StartsWith(TEST_PREFIX))
			{
				var keyWithoutPrefix = key.Replace(TEST_PREFIX, "").Replace('\\', '/');
				// This is a hack to support paths on OSX/Unix
				if (keyWithoutPrefix.StartsWith('/')) keyWithoutPrefix = keyWithoutPrefix.Substring(1);

				var filePathForTestContent = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestContent", contentType.ToString(), $"{keyWithoutPrefix}.json"));
				return filePathForTestContent;
			}

			// This is a hack to support paths on OSX/Unix
			if (key.StartsWith('/')) key = key.Substring(1);
			var filePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", contentType.ToString(), $"{key.Replace('\\', '/')}.json"));
			return filePath;
		}
	}
}
