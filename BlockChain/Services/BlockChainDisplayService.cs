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
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine(new string('=', 70));

            foreach (var block in chain)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[ BLOCK #{block.Index} ]");
                Console.ResetColor();
                Console.WriteLine($" Hash:     {block.Hash}");
                Console.WriteLine($" Prev:     {block.PreviousHash}");
                Console.WriteLine($" Nonce:    {block.Nonce}");
                Console.WriteLine($" Time:     {block.TimeStamp:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($" Transactions Count: {block.Transactions.Count}");
                Console.WriteLine(new string('-', 70));

                foreach (var tx in block.Transactions)
                {
                    PrintTransaction(tx);
                }
            }
            Console.WriteLine("\n" + new string('=', 70));
        }

        public void PrintTransaction(Transaction transaction)
        {
            Console.WriteLine($"  >> TRANSACTION: {transaction.Id.Substring(0, 16)}...");
            Console.WriteLine($"     From:   {transaction.From}");
            Console.WriteLine($"     To:     {transaction.To}");
            Console.WriteLine($"     Amount: {transaction.Amount:F2} coins");
            Console.WriteLine($"     Time:   {transaction.TimeStamp:HH:mm:ss}");
            Console.WriteLine(new string('.', 40));
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
