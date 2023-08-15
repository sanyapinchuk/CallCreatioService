using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CallCreatioService
{
	internal class Program
	{
		private const string CRM_ADDRESS = "https://pinchuk.beesender.com";
		private const string USER_LOGIN = "Supervisor";
		private const string USER_PASSWORD = "Supervisor";
		static async Task Main()
		{
			await CallODataService();
			await CallUserService();

			Console.ReadLine();
		}
		private static async Task CallODataService()
		{
			var creatioHelper = new CreatioODataHelper(CRM_ADDRESS, USER_LOGIN, USER_PASSWORD);
			try
			{
				var content = await creatioHelper.GetEntities( "Account", new string[] { "Name" });
				var list = JsonConvert.DeserializeAnonymousType(content, new
				{
					value = new[]
						{
						new
						{
							Name= ""
						}
					}
				});
				var count = list.value.Where(i => i.Name.ToUpper().Contains("A")).Count();
				Console.WriteLine("[ODataService] Count of accounts with \"A\" in the Name: " + count);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private static async Task CallUserService()
		{
			var creatioHelper = new UserCreatioServiceHelper(CRM_ADDRESS, USER_LOGIN, USER_PASSWORD);
			try
			{
				var paramValues = new Dictionary<string, string>()
				{
					{"name", "A" }
				};
				var content = await creatioHelper.CallService("AccountStatisticsService",
					"GetCountWithStringInName", HttpMethod.Get, paramValues);

				var result = (int)JObject.Parse(content)["GetCountWithStringInNameResult"];
				Console.WriteLine("[UserService] Count of accounts with \"A\" in the Name: " + result);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}
	}
}
