
using Newtonsoft.Json;
using ServiceDashBoard1.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;


namespace ServiceDashBoard1.Services
{
    public class SapService
    {

        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;


        public SapService()
        {
            _cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };

            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri("https://a15.ubshanacloud.in:50000/b1s/v1/");
        }

        private async Task<bool> LoginToSapAsync()
        {
            var loginUrl = "Login";
            var loginData = new
            {
                CompanyDB = "SIL_TEST",
                UserName = "ubshanacloud\\sil.acc14",
                Password = "Gq$6nD2v"
            };

            var loginJson = JsonConvert.SerializeObject(loginData);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync(loginUrl, loginContent);

            if (!loginResponse.IsSuccessStatusCode)
            {
                var error = await loginResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed: {error}");
                return false;
            }

            return true;
        }
        public async Task<List<ViewServiceModel>?> GetServiceCallsAsync()
        {
            bool isLoggedIn = await LoginToSapAsync();
            if (!isLoggedIn) return null;

            var serviceCallUrl = $"ServiceCalls";
            var serviceCallResponse = await _client.GetAsync(serviceCallUrl);

            if (!serviceCallResponse.IsSuccessStatusCode)
            {
                var error = await serviceCallResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to fetch service calls: {error}");
                return null;
            }

            var responseContent = await serviceCallResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);


            var item = result?.value.ToObject<List<ViewServiceModel>>();
            Console.WriteLine("Item hai " + item);
            return item;
        }

        public async Task<string?> CreateServiceCallAsync(SapServiceModel model)
        {
            bool isLoggedIn = await LoginToSapAsync();
            if (!isLoggedIn) return null;

            var jsonBody = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("ServiceCalls", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("SAP POST failed: " + error);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        //public async Task<ViewServiceModel?> GetServiceCallBySerialAsync(string serialNo)
        //{
        //    bool isLoggedIn = await LoginToSapAsync();
        //    if (!isLoggedIn) return null;

        //    // SAP OData query with filter
        //    var filterUrl = $"ServiceCalls?$filter=InternalSerialNum eq '{serialNo}'";

        //    var response = await _client.GetAsync(filterUrl);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine("SAP filter fetch failed: " + error);
        //        return null;
        //    }

        //    var content = await response.Content.ReadAsStringAsync();
        //    var result = JsonConvert.DeserializeObject<dynamic>(content);

        //    var items = result?.value?.ToObject<List<ViewServiceModel>>();
        //    return items?.FirstOrDefault();
        //}

        public async Task<ViewServiceModel?> GetServiceCallBySerialAsync(string serialNo)
        {
            bool isLoggedIn = await LoginToSapAsync();
            if (!isLoggedIn) return null;

            var filterUrl = $"ServiceCalls?$filter=InternalSerialNum eq '{serialNo}'";

            var response = await _client.GetAsync(filterUrl);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("SAP filter fetch failed: " + error);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            // ✅ Deserialize to JObject first
            var result = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(content);

            // ✅ Then safely get .value and convert to List<ViewServiceModel>
            var items = result["value"]?.ToObject<List<ViewServiceModel>>();

            if (items == null || items.Count == 0)
                return null;

            // ✅ Get latest by UpdateDate
            var latest = items
                .Where(x => x.InternalSerialNum == serialNo)
                .OrderByDescending(x => x.UpdateDate)
                .FirstOrDefault();

            return latest;
        }


    }
}
