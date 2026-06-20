using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;

namespace BlockChain.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }

        public byte[] SenderPublicKey { get; set; }
        public byte[] Signature { get; set; }


        public string ToRowString()
        {
            if (Signature != null)
            {
                return $"{Id} | {From} -> {To} | Amount: {Amount} | Time: {TimeStamp.ToString("O")} {Convert.ToHexString(Signature)}";
            }

            return $"{Id} | {From} -> {To} | Amount: {Amount} | Time: {TimeStamp.ToString("O")}";
        }

        public byte[] GetDataToSign()
        {
            string row = $"{Id}{From}{To}{Amount}{TimeStamp.ToString("O")}";
            return Encoding.UTF8.GetBytes(row);
        }

        public Transaction(string from, string to, decimal amount, byte[] senderPublicKey)
        {
            Id = GenerateHashId(from, to, amount);
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.UtcNow;
            this.SenderPublicKey = senderPublicKey;
        }

        private string GenerateHashId(string from, string to, decimal amount)
        {
            var dataToHash = new {From = from, To = to, Amount = amount};

            string jsonString = JsonSerializer.Serialize(dataToHash);

            byte[] inputBytes = Encoding.UTF8.GetBytes(jsonString);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
