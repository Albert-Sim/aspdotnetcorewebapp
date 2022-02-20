using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length == 0)
{
    Console.WriteLine("API key is not provided, please provide api key as parameter while running this executable.");
    return;
}

string api = "https://eth-mainnet.alchemyapi.io/v2/";
string apiKey = args[0];
int parallelCount = 8;

await Parallel.ForEachAsync(Enumerable.Range(12100001, 500), new ParallelOptions() { MaxDegreeOfParallelism = parallelCount }, async (blockNumber, _) =>
{
    await Task.Run(async () =>
    {
        try
        {
            RequestBody bodyGetBlock = new()
            {
                Method = "eth_getBlockByNumber",
                Params = new object[]
                {
                    "0x" + blockNumber.ToString("X"),
                    false
                },
                Id = 0
            };

            StringContent contentGetBlock = new(JsonSerializer.Serialize(bodyGetBlock), Encoding.UTF8, "application/json");
            
            HttpResponseMessage responseMessageGetBlock = await new HttpClient().PostAsync(api + apiKey, contentGetBlock);
            if (responseMessageGetBlock.StatusCode != HttpStatusCode.OK)
                return;

            ResponseBody? responseBodyGetBlock = JsonSerializer.Deserialize<ResponseBody>(responseMessageGetBlock.Content.ReadAsStringAsync().Result);
            if (responseBodyGetBlock == null || responseBodyGetBlock.Result == null)
            {
                Console.WriteLine(string.Format("Block {0} is not found", blockNumber.ToString()));
                return;
            }

            //Insert block record into database here

            RequestBody bodyGetBlockTransactionCount = new()
            {
                Method = "eth_getBlockByNumber",
                Params = new object[]
                {
                    "0x" + blockNumber.ToString("X"),
                    false
                },
                Id = 0
            };

            StringContent contentGetBlockTransactionCount = new(JsonSerializer.Serialize(bodyGetBlockTransactionCount), Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessageGetBlockTransactionCount = await new HttpClient().PostAsync(api + apiKey, contentGetBlockTransactionCount);
            if (responseMessageGetBlockTransactionCount.StatusCode != HttpStatusCode.OK)
                return;

            ResponseBody? responseBodyGetBlockTransactionCount = JsonSerializer.Deserialize<ResponseBody>(responseMessageGetBlockTransactionCount.Content.ReadAsStringAsync().Result);
            if (responseBodyGetBlockTransactionCount == null || responseBodyGetBlockTransactionCount.Result == null)
                return;
            
            string? transactionCountInHex = responseBodyGetBlockTransactionCount.Result.ToString();
            if (transactionCountInHex == null)
                return;

            int transactionCount = int.Parse(transactionCountInHex[2..], System.Globalization.NumberStyles.HexNumber);

            if (transactionCount == 0)
            {
                Console.WriteLine(string.Format("Block {0} has 0 transaction", blockNumber.ToString()));
                return;
            }

            await Parallel.ForEachAsync(Enumerable.Range(0, transactionCount), new ParallelOptions() { MaxDegreeOfParallelism = parallelCount }, async (index, _) =>
            {
                await Task.Run(async () =>
                {
                    RequestBody bodyGetTransaction = new()
                    {
                        Method = "eth_getTransactionByBlockNumberAndIndex",
                        Params = new object[]
                        {
                            "0x" + blockNumber.ToString("X"),
                            "0x" + index.ToString("X")
                        },
                        Id = 0
                    };

                    StringContent contentGetTransaction = new(JsonSerializer.Serialize(bodyGetTransaction), Encoding.UTF8, "application/json");

                    HttpResponseMessage responseMessageGetTransaction = await new HttpClient().PostAsync(api + apiKey, contentGetTransaction);
                    if (responseMessageGetTransaction.StatusCode != HttpStatusCode.OK)
                        return;

                    ResponseBody? responseBodyGetTransaction = JsonSerializer.Deserialize<ResponseBody>(responseMessageGetTransaction.Content.ReadAsStringAsync().Result);
                    if (responseBodyGetTransaction == null || responseBodyGetTransaction.Result == null)
                        return;

                    //Insert transaction record into database here

                }, CancellationToken.None);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }, CancellationToken.None);
});

public class RequestBody
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";
    
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object[] Params { get; set; } = Array.Empty<object>();

    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class ResponseBody
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("result")]
    public Object? Result { get; set; }
}