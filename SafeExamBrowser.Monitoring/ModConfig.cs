using System;
using System.Net;
using System.Net.Http;

namespace SafeExamBrowser.Monitoring
{
	public class ModConfig
	{
		private readonly HttpClient _httpClient;

		private readonly string _configUrl =
			"https://gist.githubusercontent.com/ISBachvarov21/d340eb96e76c3f855c2779f27cf568f9/raw/ModConfig.txt";

		private string _allowedUsers;
		
		public ModConfig(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public string GetConfig()
		{
			if (_allowedUsers == null)
			{
				_allowedUsers = GetConfigFromUrl();
			}

			return _allowedUsers;
		}

		public string GetConfigFromUrl()
		{
			try
			{
				var response = _httpClient.GetAsync(_configUrl).Result;
				if (response.StatusCode == HttpStatusCode.OK)
				{
					return response.Content.ReadAsStringAsync().Result;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return null;
		}
	}
}