using System;
using BlockChain.Models;
using BlockChain.Services;

//var blockChain = new BlockChainService();
//var displayService = new BlockChainDisplayService();

//Console.WriteLine("Додаємо блоки до блокчейну...");
//blockChain.AddBlock("Alice->Bob: 10", "Alice");
//blockChain.AddBlock("Bob->Charlie: 5", "Bob");
//blockChain.AddBlock("Charlie->David: 15", "Charlie");

//displayService.PrintBlockChain(blockChain.Chain);
//displayService.PrintValidationResult(blockChain.IsValid());

//string hashToSeach = blockChain.Chain[2].Hash;

//Block foundBlook = blockChain.FindBlockByHash(hashToSeach);

//if (foundBlook != null)
//{
//    Console.WriteLine("Found! Data: ");
//    displayService.PrintBlockChain(new List<Block> { foundBlook });
//}
//else
//{
//    Console.WriteLine("Error");
//}

//Block missingBlock = blockChain.FindBlockByHash("FAKE");

//if (missingBlock == null)
//{
//    Console.WriteLine("Didn't found");
//}
//else
//{
//    Console.WriteLine("Error");
//}

//    //blockChain.Chain[1].Data = "Bob->Charlie: 999999"; 
//    blockChain.Chain[2].Author = "Alex";
//displayService.PrintValidationResult(blockChain.IsValid()); 

// HW_1

//Console.WriteLine("Adding blocks to chain: ");
//blockChain.AddBlock("Transaction 1");
//blockChain.AddBlock("Transaction 2");
//blockChain.AddBlock("Transaction 3");
//blockChain.AddBlock("Transaction 4");

//displayService.PrintBlockChain(blockChain.Chain);

//int initialCheck = blockChain.GetInvalidBlockIndex();
//if (initialCheck == -1)
//{
//    Console.WriteLine("Starting validation: Chain fully valid");
//}

//blockChain.Chain[2].Data = "Hacker data";
//int invalidIndex = blockChain.GetInvalidBlockIndex();

//if (invalidIndex != -1)
//{
//    Console.WriteLine("Attention! An integrity violation has been detected. The counterfeit block is number 2.");
//}
//else
//{
//    Console.WriteLine("Chain valid");
//}



//string choice;

//do
//{
//    Console.WriteLine("BlockChain Menu");
//    Console.WriteLine("1. Add Block");
//    Console.WriteLine("2. Validate Chain");
//    Console.WriteLine("3. Print Chain");
//    Console.WriteLine("0. Exit");
//    Console.Write("Choice: ");


//    choice = Console.ReadLine();

//    switch (choice)
//    {
//        case "1":
//            Console.WriteLine("Enter data for block: ");
//            var data = Console.ReadLine();


//            using (var cts = new CancellationTokenSource())
//            {
//                //var networkTask = Task.Run(async () =>
//                //{
//                //    int delay = new Random().Next(2000, 8000);
//                //    await Task.Delay(delay);

//                //    if (!cts.IsCancellationRequested)
//                //    {
//                //        Console.WriteLine($"\nAnother chain found a block rather than {delay} ms!");
//                //        cts.Cancel();
//                //    }
//                //});

//                try
//                {
//                    Console.WriteLine("Starting Mining...");

//                    await blockChain.AddBlockAsync(data, cts.Token);
//                    cts.Cancel();
//                    Console.WriteLine("\nSuccess! Block has found!");
//                }
//                catch
//                {
//                    Console.WriteLine("\nRejected! Local mining rejected!");
//                }
//            }

//            break;
//        case "2":
//            displayService.PrintValidationResult(blockChain.IsValid());
//            break;
//        case "3":
//            displayService.PrintBlockChain(blockChain.Chain);
//            break;
//        default:
//            Console.WriteLine("Incorrect choice; Select 1 or 2");
//            break;
//    }
//}

//while (choice != "0");





// Lesson_4

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();
var blockChainService = new BlockChainService();
var transactionService = new TransactionService();

Console.WriteLine("Blockchain Menu");
Console.WriteLine("1. Add Block");
Console.WriteLine("2. Validate Blockchain");
Console.WriteLine("3. Print Blockchain");
Console.WriteLine("4. Exit");

var transaction1 = new Transaction("Alice", "Bob", 10);
var transaction2 = new Transaction("Bob", "Charlie", 5);
var transaction3 = new Transaction("Charlie", "Dave", 2);
var transaction4 = new Transaction("Dave", "Alice", 1);



while (true)
{
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            blockChainService.AddBlockAsync(new List<Transaction> { transaction1, transaction2, transaction3, transaction4 }, CancellationToken.None);
            Console.WriteLine("Blocks added successfuly");
            break;
        case "2":
            bool isValid = blockChainService.IsValid();
            displayService.PrintValidationResult(isValid);
            break;
        case "3":
            displayService.PrintBlockChain(blockChainService.Chain);
            break;
        case "4":
            return;
    }
}
