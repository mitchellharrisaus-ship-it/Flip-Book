using Flipbook_App.Client.Models.DTOs;
using System.Net.Http.Json;

namespace Flipbook_App.Client.Services;

public class DrawActionApiService
{
	const string apiPath = "/api/canvas";
	const string writeActionToFilePath = $"{apiPath}/write-action-to-file";

	readonly HttpClient httpClient;

	public DrawActionApiService(HttpClient httpClient)
	{
		this.httpClient = httpClient;
	}

	public void SendDrawAction(DrawActionDTO? drawAction, string animationName)
	{
		//var content = new StringContent(JsonSerializer.Serialize(drawAction), System.Text.Encoding.UTF8, "application/json");

		//var urlParams = new Dictionary<string, string>()
		//{
		//	{ "animationName", animationName }
		//};
		//var queryStringParams = await new FormUrlEncodedContent(urlParams).ReadAsStringAsync();

		httpClient.PostAsJsonAsync($"{writeActionToFilePath}/{animationName}", drawAction);
	}
}
