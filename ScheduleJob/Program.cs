using System.Text.Json;
using MySql.Data.MySqlClient;

MySqlConnection connection = new("server=localhost;port=3306;database=etherscan;uid=etherscan_user;password=Etherscan_pass123");

connection.Open();

int limitPerQuery = 1000;
int currectOffset = 0;
int parallelCount = 8;

List<Token> tokens = new();

MySqlCommand commandGetTotalCount = new("SELECT COUNT(id) FROM Token", connection);

int totalTokenCount = Convert.ToInt32(commandGetTotalCount.ExecuteScalar());

while (totalTokenCount > currectOffset)
{
    string sqlCommandGetTokenSymbol = string.Format("SELECT id, symbol FROM Token LIMIT {0} OFFSET {1}", limitPerQuery, currectOffset);
    MySqlCommand commandGetTokenSymbol = new(sqlCommandGetTokenSymbol, connection);

    MySqlDataReader reader = commandGetTokenSymbol.ExecuteReader();

    try
    {
        while (reader.Read())
        {
            tokens.Add(new Token() { id = reader.GetInt32(0), symbol = reader.GetString(1) });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        break;
    }
    finally
    {
        currectOffset = tokens.Count;
        reader.Close();
    }
}

await Parallel.ForEachAsync(tokens, new ParallelOptions() { MaxDegreeOfParallelism = parallelCount }, async (token, _) =>
{
    await Task.Run(async () =>
    {
        try
        {
            HttpClient httpClient = new();
            string content = await httpClient.GetStringAsync(String.Format("https://min-api.cryptocompare.com/data/price?fsym={0}&tsyms=USD", token.symbol));
            Price? price = JsonSerializer.Deserialize<Price>(content);
            if (price != null && price.Response == string.Empty)
            {
                lock ("lock")
                {
                    using MySqlCommand commandUpdateTokenPrice = new(string.Format("UPDATE Token SET price = {0} WHERE id = {1}", price.USD, token.id), connection);
                    commandUpdateTokenPrice.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("parallel: " + ex.Message);
        }
    }, CancellationToken.None);
});

connection.Close();

class Token
{
    public int id { get; set; }

    public string symbol { get; set; } = string.Empty;
}

class Price
{
    public string Response { get; set; } = string.Empty;

    public decimal USD { get; set; }
}