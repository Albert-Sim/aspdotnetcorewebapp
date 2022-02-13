using Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages
{
    public class DetailModel : PageModel
    {
        public Token Token { get; private set; } = new Token();

        public void OnGet(int id)
        {
            if (id == 1)
            {
                Token = new Token()
                {
                    Id = 1,
                    Name = "Vechain",
                    Symbol = "VEN",
                    TotalSupply = 35987133,
                    TotalHolders = 65,
                    ContractAddress = "0xd850942ef8811f2a866692a623011bde52a462c1",
                    Price = 100
                };
            }
            else
            {
                Token = new Token()
                {
                    Id = 2,
                    Name = "Zilliqa",
                    Symbol = "ZIR",
                    TotalSupply = 53272942,
                    TotalHolders = 54,
                    ContractAddress = "0x05f4a42e251f2d52b8ed15e9fedaacfcef1fad27"
                };
            }
        }
    }
}
