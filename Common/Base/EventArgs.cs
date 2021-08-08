using System;

namespace TeaTime
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            this.Value = value;
        }

        public T Value { get; set; }
    }

	/// <summary>
	/// Allows compact formulation of event handler calling:<br/>
	/// 
	/// OnTrackTextEditorChanged.SafeFire(this, value);
	/// </summary>
	public static class EventHandlerExtensions
	{
		public static void SafeFire<T>(this EventHandler<EventArgs<T>> handler, object sender, T value)
		{
			if (handler == null) return;
			handler(sender, new EventArgs<T>(value));
		}
	}
}
