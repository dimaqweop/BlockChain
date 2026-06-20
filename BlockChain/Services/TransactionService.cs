using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class TransactionService
    {
        public Transaction CreateTransaction(string from, string to, decimal amount)
        {
            var tx = new Transaction(from, to, amount);
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

            if (!IsValidCryptoAddress(transaction.From))
            {
                return (false, "Invalid Sender address. Must start with '0x', be 42 characters long, and contain only alphanumeric characters.");
            }

            if (!IsValidCryptoAddress(transaction.To))
            {
                return (false, "Invalid Recipient address. Must start with '0x', be 42 characters long, and contain only alphanumeric characters.");
            }

            if (transaction.Amount <= 0)
            {
                return (false, "Amount must be greater than zero.");
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
