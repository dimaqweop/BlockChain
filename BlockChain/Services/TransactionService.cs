using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (string.IsNullOrWhiteSpace(transaction.From)) // Перевірка на null, порожній рядок або рядок, що складається з пробілів
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

            return (true, string.Empty);
        }
    }
}
