using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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
            var balances = _walletService.GetBalance(walletFrom.Address);
            if (balances.GetValueOrDefault("BASE") < amount)
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

            if (transaction.Amount < 0)
            {
                return (false, "Amount must be greater than zero.");
            }

            if (transaction.From == "COINBASE")
            {
                return (true, string.Empty);
            }

            var existingTokens = new HashSet<string> { "BASE" };
            foreach (var block in _walletService.blockChain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.Type == TransactionType.ICO)
                    {
                        existingTokens.Add(tx.Ticker);
                    }
                }
            }

            if (transaction.Type == TransactionType.ICO)
            {
                if (transaction.Fee < 100)
                    return (false, "ICO Tax rejected: Fee must be at least 100.");
                if (existingTokens.Contains(transaction.Ticker))
                    return (false, "Anti-Plagiarism: Token already exists.");
                if (transaction.Emission <= 0)
                    return (false, "Math: Emission must be greater than zero.");
            }

            else if (transaction.Type == TransactionType.Transfer)
            {
                if (!existingTokens.Contains(transaction.Ticker))
                    return (false, $"Air Protect: Token '{transaction.Ticker}' does not exist.");
                if (transaction.Amount < 0)
                    return (false, "Math: Amount must be greater than zero.");
            }

            var senderBalances = _walletService.GetBalance(transaction.From);
            decimal baseBalance = senderBalances.GetValueOrDefault("BASE");
            decimal tokenBalance = senderBalances.GetValueOrDefault(transaction.Ticker);

            if (transaction.Type == TransactionType.Transfer)
            {
                if (baseBalance < transaction.Fee)
                {
                    return (false, "Insufficient BASE balance to cover the fee.");
                }
            }
            else
            {
                if (transaction.Ticker == "BASE")
                {
                    if (baseBalance < transaction.Amount + transaction.Fee)
                        return (false, "Insufficient BASE balance to cover the emission and fee.");
                }
                else
                {
                    if (tokenBalance < transaction.Amount)
                    {
                        return (false, $"Insufficient {transaction.Ticker} balance to cover the transfer amount.");
                    }
                    if (baseBalance < transaction.Fee)
                    {
                        return (false, "Insufficient BASE balance to cover the fee.");
                    }
                }
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
