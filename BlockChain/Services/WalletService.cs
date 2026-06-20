
using System.Security.Cryptography;
using BlockChain.Models;

namespace BlockChain.Services
{
    internal class WalletService
    {
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


    }
}
