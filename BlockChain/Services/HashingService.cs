using System.Security.Cryptography;
using System.Text;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            string blockData = $"{block.Index}{block.TimeStamp}{block.Data}{block.Author}{block.PreviousHash}{block.Nonce}";
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
