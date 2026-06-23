using System;
using System.Diagnostics;
using System.Text;
using BlockChain.Models;
using BlockChain.Services;


// Connecting services

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();
var blockChainService = new BlockChainService();
var transactionService = new TransactionService();
var walletService = new WalletService();


// Lesson_4



//Console.WriteLine("Blockchain Menu");
//Console.WriteLine("1. Add Block");
//Console.WriteLine("2. Validate Blockchain");
//Console.WriteLine("3. Print Blockchain");
//Console.WriteLine("4. Exit");
//Console.WriteLine("5. Change Blockchain");

//var walletAlice = new WalletService().CreateWallet("Alice");
//var walletBob = new WalletService().CreateWallet("Bob");
//var walletCharlie = new WalletService().CreateWallet("Charlie");
//var walletDave = new WalletService().CreateWallet("Dave");

//var transaction1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 10, walletAlice.PublicKey);
//var transaction2 = transactionService.CreateTransaction(walletBob, walletCharlie.Address, 10, walletBob.PublicKey);
//var transaction3 = transactionService.CreateTransaction(walletCharlie, walletDave.Address, 10, walletCharlie.PublicKey);
//var transaction4 = transactionService.CreateTransaction(walletDave, walletAlice.Address, 10, walletDave.PublicKey);


//while (true)
//{
//    var choice = Console.ReadLine();

//    switch (choice)
//    {
//        case "1":
//            blockChainService.AddBlockAsync(new List<Transaction> { transaction1, transaction2, transaction3, transaction4 }, CancellationToken.None);
//            Console.WriteLine("Blocks added successfuly");
//            break;
//        case "2":
//            bool isValid = blockChainService.IsValid();
//            displayService.PrintValidationResult(isValid);
//            break;
//        case "3":
//            displayService.PrintBlockChain(blockChainService.Chain);
//            break;
//        case "5":
//            blockChainService.Chain[1].Transactions[0].Amount = 100;
//            Console.WriteLine("Blockchain modified. Please validate");
//            break;

//        case "4":
//            return;
//    }
//}


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

//blockChainService.AddBlock(pendingTransactions, CancellationToken.None);

//var latestBlock = blockChainService.Chain.Last();

//int actualTransactionsSizeBytes = 0;
//foreach (var tx in latestBlock.Transactions)
//{
//    Console.WriteLine(tx.From);
//    actualTransactionsSizeBytes += Encoding.UTF8.GetByteCount(tx.ToRowString());
//}

//Console.WriteLine($"Max Limit: {latestBlock.MaxBlockSizeBytes} bytes");
//Console.WriteLine($"Attempted: {pendingTransactions.Count} transactions");
//Console.WriteLine($"Accepted:  {latestBlock.Transactions.Count} transactions");
//Console.WriteLine($"Rejected:  {pendingTransactions.Count - latestBlock.Transactions.Count} transactions");
//Console.WriteLine($"Final Size:{actualTransactionsSizeBytes} bytes (Valid: {actualTransactionsSizeBytes <= latestBlock.MaxBlockSizeBytes})");

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

// HW_3


//blockChainService.AddBlock(new List<Transaction>(), CancellationToken.None);
//blockChainService.AddBlock(new List<Transaction>(), CancellationToken.None);
//blockChainService.AddBlock(new List<Transaction>(), CancellationToken.None);

//Console.WriteLine($"Block: {block.Index} | Hash {block.Hash}");
//Console.WriteLine($"Is chain valid? {blockChainService.IsValid()}");


// HW_4

//static string GenerateValidAddress()
//{
//    byte[] randomBytes = new byte[20];
//    using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
//    {
//        rng.GetBytes(randomBytes);
//    }
//    return "0x" + Convert.ToHexString(randomBytes).ToLower();
//}

//var mempool = new List<Transaction>();

//Console.WriteLine("===== 15 Transactions =====");
//for (int i = 0; i < 15; i++)
//foreach (var block in blockChainService.Chain.ToList())
//{
//    string from = GenerateValidAddress();
//    string to = GenerateValidAddress();
//    var tx = transactionService.CreateTransaction(from, to, (i + 1) * 10);
//    displayService.PrintTransaction(tx);
//    mempool.Add(tx);
//}


//Console.ForegroundColor = ConsoleColor.Yellow;
//Console.WriteLine("\n=== Creating transaction on address 'Bob' ===");
//var invalidTx = new Transaction(GenerateValidAddress(), "Bob", 20);
//mempool.Add(invalidTx);
//displayService.PrintTransaction(invalidTx);
//Console.ResetColor();

//Console.WriteLine("=== Starting Processing ===");
//blockChainService.ProcessTransactions(mempool, CancellationToken.None);

//displayService.PrintBlockChain(blockChainService.Chain);



// HW_5

var vanityService = new VanityWalletService();

Console.WriteLine("Mining wallet with prefix 'aa'...");
var (myWallet, myAttempts) = vanityService.MineWallet("aa");

Console.WriteLine($"[Success] Address: {myWallet.Address}");
Console.WriteLine($"Attempts: {myAttempts:N0}");


string authMessage = "This is my custom wallet!";

byte[] mySignature = walletService.SignMessage(myWallet, authMessage);
Console.WriteLine("My authentication:");
bool isAuthSuccess = walletService.VerifyMessage(myWallet.Address, myWallet.PublicKey, authMessage, mySignature);
if (isAuthSuccess)
{
    Console.WriteLine("Auth successful!");
}

Wallet hackerWallet = walletService.CreateWallet("Hacker");

byte[] hackerSignature = walletService.SignMessage(hackerWallet, authMessage);
Console.WriteLine("Hacker authentication:");
bool isHackerAuthSuccess = walletService.VerifyMessage(myWallet.Address, hackerWallet.PublicKey, authMessage, hackerSignature);
if (!isHackerAuthSuccess)
{
    Console.WriteLine("Hacker auth failed!");
}


