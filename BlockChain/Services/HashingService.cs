using System.Security.Cryptography;
using System.Text;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            var totalHash = "";
            foreach (var item in block.Transactions)
            {
                totalHash += ComputeHash(item.ToRowString());
            }
            string blockData = $"{block.Index}{block.TimeStamp.ToString("O")}{totalHash}{block.Author}{block.PreviousHash}{block.Nonce}{block.Difficulty}";
            return ComputeHash(blockData);
        }

        private string ComputeHash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
