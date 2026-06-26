using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class TransactionService
    {
        private readonly WalletService _walletService;
        public TransactionService(List<Block> blockChain)
        {
            _walletService = new WalletService(blockChain);
        }

        public Transaction CreateTransaction(Wallet walletFrom, string to, decimal amount, byte[] senderPublicKey)
        {
            // Перевірка балансу
            var balance = _walletService.GetBalance(walletFrom.Address);
            if (balance < amount)
            {
                throw new ArgumentException("Insufficient funds.");
            }


            var tx = new Transaction(walletFrom.Address, to, amount, senderPublicKey);
            tx.Signature = walletFrom.Sign(tx.GetDataToSign());

            var validation = ValidateTransaction(tx);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            return tx;
        }

        public (bool IsValid, string ErrorMessage) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                return (false, "Transaction cannot be null.");
            }

            //if (!IsValidCryptoAddress(transaction.From)) return (false, "Invalid Sender address. Must start with '0x', be 42 characters long, and contain only alphanumeric characters.");

            //if (!IsValidCryptoAddress(transaction.To)) return (false, "Invalid Recipient address. Must start with '0x', be 42 characters long, and contain only alphanumeric characters.");

            if (string.IsNullOrWhiteSpace(transaction.From))
            {
                return (false, "Sender cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(transaction.To))
            {
                return (false, "Recipient cannot be empty.");
            }

            if (transaction.Amount <= 0)
            {
                return (false, "Amount must be greater than zero.");
            }

            if (transaction.From == "COINBASE")
            {
                return (true, string.Empty);
            }

            bool isSignatureValid = _walletService.VerifySignature(
                Convert.ToBase64String(transaction.SenderPublicKey), 
                transaction.GetDataToSign(),
                transaction.Signature
            );
            if (!isSignatureValid) {
                return (false, "Invalid wallet signature");
            }

            return (true, string.Empty);
        }

        private bool IsValidCryptoAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return false;

            return Regex.IsMatch(address, @"^0x[a-zA-Z0-9]{40}$");
        }
    }
}
