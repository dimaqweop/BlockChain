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


string choice;

do
{
    choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Enter data for block: ");
            var data = Console.ReadLine();
            blockChain.AddBlock(data);
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
