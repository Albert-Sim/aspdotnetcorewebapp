namespace Client.Models
{
    public class Token
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;

        public Int64 TotalSupply { get; set; }

        public string ContractAddress { get; set; } = string.Empty;

        public int TotalHolders { get; set; }

        public decimal Price { get; set; }
    }
}
