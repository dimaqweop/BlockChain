using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class BlockChainDisplayService
    {
        public void PrintBlockChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"TimeStamp: {block.TimeStamp}");
                Console.WriteLine($"PreviousHash: {block.PreviousHash}");
                Console.WriteLine($"Hash: {block.Hash:10}");
                //Console.WriteLine($"Author: {block.Author}");
                Console.WriteLine($"Nonce: {block.Nonce}");
                Console.WriteLine(new string('-', 50));
                foreach (var transaction in block.Transactions)
                {
                    PrintTransaction(transaction);
                }
                Console.WriteLine(new string('-', 50));
            }
        }

        public void PrintTransaction(Transaction transaction)
        {
            Console.WriteLine($"Id: {transaction.Id}");
            Console.WriteLine($"From: {transaction.From:10}");
            Console.WriteLine($"To: {transaction.To:10}");
            Console.WriteLine($"Amount: {transaction.Amount}");
            Console.WriteLine($"TimeStamp: {transaction.TimeStamp}");
            Console.WriteLine(new string('-', 50));
        }

        public void PrintValidationResult(bool isValid)
        {
            if (isValid)
            {
                Console.WriteLine("The blockchain is valid.");
            }
            else
            {
                Console.WriteLine("The blockchain is invalid.");
            }
        }
    }
}
