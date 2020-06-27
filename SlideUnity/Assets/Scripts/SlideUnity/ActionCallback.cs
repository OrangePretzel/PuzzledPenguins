namespace SlideUnity
{
	public class ActionCallback
	{
		private System.Action _callbackAction;

		public void ClearCallback() => _callbackAction = null;

		public void RegisterCallback(System.Action callback)
		{
			System.Action callbackAction = null;
			callbackAction = () =>
			{
				callback?.Invoke();
				_callbackAction -= callbackAction;
			};
			_callbackAction += callbackAction;
		}

		public void Invoke()
		{
			_callbackAction?.Invoke();
			_callbackAction = null;
		}
	}
}
