using System;
using System.Diagnostics;
using System.Text;
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






//blockChainService.MineBlock(new List<Transaction>(), walletAlice.Address, CancellationToken.None);

//var transaction1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 50, walletBob.PublicKey);
//var transaction2 = transactionService.CreateTransaction(walletAlice, walletCharlie.Address, 50, walletCharlie.PublicKey);

//blockChainService.MineBlock(new List<Transaction> { transaction1, transaction2 }, walletAlice.Address, CancellationToken.None);

//Console.WriteLine($"Alice balance: {wallertService.GetBalance(walletAlice.Address)}");

//decimal previousBalance = wallertService.GetBalance(walletAlice.Address);
//int blocksMined = 0;

//while (true)
//{
//    blockChainService.MineBlock(new List<Transaction>(), walletAlice.Address, CancellationToken.None);
//    blocksMined++;

//    decimal currentBalance = wallertService.GetBalance(walletAlice.Address);
//    decimal reward = currentBalance - previousBalance;

//    if (reward < 50)
//    {
//        Console.WriteLine($"\n[Block {blocksMined}] Limit reached! Remaining reward given: {reward}");
//        Console.WriteLine($"Alice's current balance: {currentBalance}");
//        break;
//    }

//    previousBalance = currentBalance;
//    Console.Write(".");
//}

//blockChainService.MineBlock(new List<Transaction>(), walletAlice.Address, CancellationToken.None);

//decimal finalBalance = wallertService.GetBalance(walletAlice.Address);
//Console.WriteLine($"Alice's final balance: {finalBalance}");

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();
var blockChainService = new BlockChainService();
var transactionService = new TransactionService(blockChainService.Chain);

Console.WriteLine("================================");
displayService.PrintValidationResult(blockChainService.IsValid());
Console.WriteLine("================================");

Console.WriteLine("Blockchain Menu");
Console.WriteLine("1. Mine Block");
Console.WriteLine("2. Create Transaction");
Console.WriteLine("3. Show Alice Balance");
Console.WriteLine("4. Show Bob Balance");
Console.WriteLine("5. Validate Blockchain");
Console.WriteLine("6. Print Blockchain");
Console.WriteLine("7. Exit");
Console.WriteLine("8. Change Blockchain");



var walletService = new WalletService(blockChainService.Chain);

var walletAlice = walletService.CreateWallet("Alice");
var walletBob = walletService.CreateWallet("Bob");
var walletCharlie = walletService.CreateWallet("Charlie");
var walletDave = walletService.CreateWallet("Dave");


while (true)
{
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            Console.WriteLine("Blocks added successfuly");
            break;
        case "2":
            var transaction1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 10, walletBob.PublicKey);
            blockChainService.AddTransactionToMempool(transaction1);
            break;
        case "3":
            Console.WriteLine($"Alice balance: {walletService.GetBalance(walletAlice.Address)}");
            break;
        case "4":
            Console.WriteLine($"Bob balance: {walletService.GetBalance(walletBob.Address)}");
            break;
        case "5":
            displayService.PrintValidationResult(blockChainService.IsValid());
            break;
        case "6":
            displayService.PrintBlockChain(blockChainService.Chain);
            break;
        case "8":
            blockChainService.Chain[1].Transactions[0].Amount = 100;
            Console.WriteLine("Blockchain modified. Please validate");
            break;

        case "7":
            return;
    }
}


// Task4

//var attackTx1 = new Transaction("Ali", "ceBob", 10);
//var attackTx2 = new Transaction("Alice", "Bob", 10);

//Console.WriteLine("Transaction 1: " + attackTx1.ToRowString());
//Console.WriteLine("Transaction 2: " + attackTx2.ToRowString());

//if (attackTx1.Id == attackTx2.Id)
//{
//    Console.WriteLine("\nWarning! Found colision: Tx1.Id == Tx2.Id");
//    Console.WriteLine($"Generated Id from both: {attackTx1.Id}");
//}

//var pendingTransactions = new List<Transaction>
//{
//    new Transaction("Alice", "Bob", 10),
//    new Transaction("Bob", "Charlie", 20),
//    new Transaction("Charlie", "Dave", 30),
//    new Transaction("Dave", "Eve", 40),
//    new Transaction("Eve", "Frank", 50),
//    new Transaction("Frank", "Grace", 60),
//    new Transaction("Grace", "Heidi", 70),
//    new Transaction("Heidi", "Ivan", 80),
//    new Transaction("Ivan", "Judy", 90),
//    new Transaction("Judy", "Mallory", 100)
//};

//await blockChainService.AddBlockAsync(pendingTransactions, CancellationToken.None);

//var latestBlock = blockChainService.Chain.Last();

//int actualTransactionsSizeBytes = 0;
//foreach (var tx in latestBlock.Transactions)
//{
//    actualTransactionsSizeBytes += Encoding.UTF8.GetByteCount(tx.ToRowString());
//}

//Console.WriteLine($"Max Limit: {latestBlock.MaxBlockSizeBytes} bytes");
//Console.WriteLine($"Attempted: {pendingTransactions.Count} transactions");
//Console.WriteLine($"Accepted:  {latestBlock.Transactions.Count} transactions");
//Console.WriteLine($"Rejected:  {pendingTransactions.Count - latestBlock.Transactions.Count} transactions");
//Console.WriteLine($"Final Size:{actualTransactionsSizeBytes} bytes (Valid: {actualTransactionsSizeBytes <= latestBlock.MaxBlockSizeBytes})");



// Task5

//var vanityService = new VanityWalletService();

//Console.WriteLine("Mining wallet with prefix 'aa'...");
//var result1 = vanityService.MineWallet("aa");

//Console.WriteLine($"[Success] Address: {result1.wallet.Address}");
//Console.WriteLine($"Attempts: {result1.attempts:N0}");

//Console.WriteLine("Mining wallet with prefix '777'...");
//var result2 = vanityService.MineWallet("777");

//Console.WriteLine($"[Success] Address: {result2.wallet.Address}");
//Console.WriteLine($"Attempts: {result2.attempts:N0}");

//Console.WriteLine("Mining wallet with prefix 'abcd'...");
//var result3 = vanityService.MineWallet("abcd");

//Console.WriteLine($"[Success] Address: {result3.wallet.Address}");
//Console.WriteLine($"Attempts: {result3.attempts:N0}");





// Lesson6

