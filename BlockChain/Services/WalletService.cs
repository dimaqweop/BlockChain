using System.Security.Cryptography;
using System.Text;
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

            byte[] hash = SHA256.HashData(publicKey);
            string address = Convert.ToBase64String(hash);

            return new Wallet(name, address, publicKey, privateKey);
        }

        public bool VerifySignature(string publicKey, byte[] data, byte[] signature)
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
        }

        public Dictionary<string, decimal> GetBalance(string address)
        {
            var balances = new Dictionary<string, decimal>();
            balances["BASE"] = 0m;
            foreach (var block in blockChain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From == address)
                    {
                        balances["BASE"] -= transaction.Fee;

                        if (transaction.Type == TransactionType.ICO)
                        {
                            balances[transaction.Ticker] = balances.GetValueOrDefault(transaction.Ticker) + transaction.Emission;
                        }
                        else if (transaction.Type == TransactionType.Transfer)
                        {
                            balances[transaction.Ticker] = balances.GetValueOrDefault(transaction.Ticker) - transaction.Amount;
                        }
                    }
                    if (transaction.To == address)
                    {
                        if (transaction.Type == TransactionType.Transfer || transaction.From == "COINBASE")
                        {
                            balances[transaction.Ticker] = balances.GetValueOrDefault(transaction.Ticker) + transaction.Amount;
                        }
                    }
                }
            }
            return balances;
        }


        public byte[] SignMessage(Wallet wallet, string message)
        {
            byte[] dataToSign = Encoding.UTF8.GetBytes(message);
            return wallet.Sign(dataToSign);
        }

        public bool VerifyMessage(string claimedAddress, byte[] publicKey, string message, byte[] signature)
        {
            byte[] hash = SHA256.HashData(publicKey);
            string calculatedAddress = Convert.ToBase64String(hash);

            if (calculatedAddress != claimedAddress)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Autorization failed: public key does not match the claimed address!");
                Console.ResetColor();
                return false;
            }


            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            bool isSignatureValid = ecdsa.VerifyData(messageBytes, signature, HashAlgorithmName.SHA256);

            if (!isSignatureValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Autorization failed: invalid signature!");
                Console.ResetColor();
                return false;
            }

            return true;
        }
    }
}
