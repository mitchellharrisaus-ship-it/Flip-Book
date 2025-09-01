namespace Flipbook_App.Client.Services;

public class ToastService
{
	public event Func<ToastMessage, Task>? OnShow;

	public async Task ShowToast(string message, ToastType type = ToastType.Info, int duration = 5000)
	{
		if (OnShow != null)
		{
			await OnShow.Invoke(new ToastMessage(message, type, duration));
		}
	}
}

public record ToastMessage(string Message, ToastType Type, int Duration);

public enum ToastType
{
	Info,
	Success,
	Error
}
