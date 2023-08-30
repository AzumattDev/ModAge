using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Thunderstore
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<List<Dictionary<string, object>>> FetchOnline()
    {
        var response = await client.GetStringAsync("https://valheim.thunderstore.io/api/v1/package/");
        return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
    }
    
    
    /*
     * var packages = await FetchOnline();

     */
}