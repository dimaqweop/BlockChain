using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class VanityWalletService
    {
        public (Wallet wallet, int attempts) MineWallet(string desiredPrefix)
        {
            int attempt = 0;
            while (true)
            {
                using var ecdsa = ECDsa.Create();
                byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

                byte[] hash = SHA256.HashData(publicKey);
                string address = Convert.ToBase64String(hash);

                attempt++;
                if (address.StartsWith(desiredPrefix))
                {
                    byte[] privateKey = ecdsa.ExportECPrivateKey();

                    var wallet = new Wallet($"Vanity-{desiredPrefix}", address, publicKey, privateKey);

                    return (wallet, attempt);
                }
            }
        }
    }
}
