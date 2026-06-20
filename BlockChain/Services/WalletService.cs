
using System.Security.Cryptography;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class WalletService
    {
        public List<Block> blockChain;

        public WalletService(List<Block> blockChain) { 
            this.blockChain = blockChain;
        }

        public Wallet CreateWallet(string name)
        {
            using var ecdsa = ECDsa.Create();

            byte[] privateKey = ecdsa.ExportECPrivateKey();
            byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

            string address = Convert.ToBase64String(publicKey);
            return new Wallet(name, address, publicKey, privateKey);
        }

        public bool VerifySignature(string publicKey, byte[] data, byte[] signature)
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
        }

        public decimal GetBalance(string address)
        {
            decimal balance = 0;
            foreach (var block in blockChain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From == address)
                    {
                        balance -= transaction.Amount;
                    }
                    if (transaction.To == address)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }
    }
}
