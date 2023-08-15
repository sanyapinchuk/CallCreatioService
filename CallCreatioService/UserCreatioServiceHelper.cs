using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace CallCreatioService
{
	public class UserCreatioServiceHelper
	{
		private readonly string _authServiceAddress;
		private readonly string _userServiceAddress;
		private readonly string _userName;
		private readonly string _userPassword;
		public UserCreatioServiceHelper(string crmAddress, string userName, string userPassword)
		{
			_authServiceAddress = crmAddress + "/ServiceModel/AuthService.svc/Login";
			_userServiceAddress = crmAddress + "/0/rest/{0}/{1}";
			_userName = userName;
			_userPassword = userPassword;
		}
		public async Task<string> CallService(string serviceName, string methodName, HttpMethod method,
			Dictionary<string, string> paramsValues = null)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				string json = JsonConvert.SerializeObject(new
				{
					UserName = _userName,
					UserPassword = _userPassword
				});
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
				HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

				HttpResponseMessage authResponse = await httpClient.PostAsync(_authServiceAddress, httpContent);
				if (!authResponse.IsSuccessStatusCode)
				{
					throw new Exception("Authentication failed");
				}
				var authResponseJson = await authResponse.Content.ReadAsStringAsync();
				if ((int)JObject.Parse(authResponseJson)["Code"] != 0)
				{
					throw new Exception("Authentication failed");
				}
				var url = string.Format(_userServiceAddress, serviceName, methodName);
				if(paramsValues != null && paramsValues.Count > 0)
				{
					url += "?";
					foreach (var paramValue in paramsValues)
					{
						url += $"{paramValue.Key}={paramValue.Value},";
					}
					url = url.Substring(0, url.LastIndexOf(','));
				}
				var requestMessage = new HttpRequestMessage(method, url);
				HttpResponseMessage responseService = await httpClient.SendAsync(requestMessage);
				if (!responseService.IsSuccessStatusCode)
				{
					throw new Exception("Call Creatio user service failed");
				}
				
				string content = await responseService.Content.ReadAsStringAsync();
				return content;
			}
		}
	}
}
