using System;
using BlockChain.Models;
using BlockChain.Services;

var blockChain = new BlockChainService();
var displayService = new BlockChainDisplayService();

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



string choice;

do
{
    Console.WriteLine("BlockChain Menu");
    Console.WriteLine("1. Add Block");
    Console.WriteLine("2. Validate Chain");
    Console.WriteLine("3. Print Chain");
    Console.WriteLine("0. Exit");
    Console.Write("Choice: ");


    choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Enter data for block: ");
            var data = Console.ReadLine();


            using (var cts = new CancellationTokenSource())
            {
                //var networkTask = Task.Run(async () =>
                //{
                //    int delay = new Random().Next(2000, 8000);
                //    await Task.Delay(delay);

                //    if (!cts.IsCancellationRequested)
                //    {
                //        Console.WriteLine($"\nAnother chain found a block rather than {delay} ms!");
                //        cts.Cancel();
                //    }
                //});

                try
                {
                    Console.WriteLine("Starting Mining...");

                    await blockChain.AddBlockAsync(data, cts.Token);
                    cts.Cancel();
                    Console.WriteLine("\nSuccess! Block has found!");
                }
                catch
                {
                    Console.WriteLine("\nRejected! Local mining rejected!");
                }
            }

            break;
        case "2":
            displayService.PrintValidationResult(blockChain.IsValid());
            break;
        case "3":
            displayService.PrintBlockChain(blockChain.Chain);
            break;
        default:
            Console.WriteLine("Incorrect choice; Select 1 or 2");
            break;
    }
}

while (choice != "0");
