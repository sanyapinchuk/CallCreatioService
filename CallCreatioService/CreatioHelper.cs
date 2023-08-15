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
	public class CreatioODataHelper
	{
		private readonly string _authServiceAddress;
		private readonly string _entityServiceAddress;
		private readonly string _userName;
		private readonly string _userPassword;
		public CreatioODataHelper(string crmAddress, string userName, string userPassword)
		{
			_authServiceAddress = crmAddress + "/ServiceModel/AuthService.svc/Login";
			_entityServiceAddress = crmAddress + "/0/odata/Account";
			_userName = userName;
			_userPassword = userPassword;
		}
		public async Task<string> GetEntities(string entityName, string[] columns = null)
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
				var url = string.Format(_entityServiceAddress, "Account");
				if(columns != null && columns.Length > 0 )
				{
					url += "?$select=";
					foreach (var column in columns)
					{
						url += column + ",";
					}
					url = url.Substring(0, url.LastIndexOf(','));
				}	
				HttpResponseMessage responseService = await httpClient.GetAsync(url);
				if (!responseService.IsSuccessStatusCode)
				{
					throw new Exception("Call Creatio OData service failed");
				}
				
				string content = await responseService.Content.ReadAsStringAsync();
				return content;
			}
		}
	}
}
