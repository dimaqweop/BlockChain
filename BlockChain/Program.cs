using System;
using System.Diagnostics;
using System.Text;
using BlockChain.Models;
using BlockChain.Services;

Console.ForegroundColor = ConsoleColor.Blue;
Console.Write("Enter port for this node: ");
var port = Console.ReadLine();
Console.ResetColor();



// Connecting services

var displayService = new BlockChainDisplayService();
var hashingService = new HashingService();
var blockChainService = new BlockChainService();
var transactionService = new TransactionService(blockChainService.Chain);
var walletService = new WalletService(blockChainService.Chain);


// Starting P2P service

var p2pService = new TcpP2pService(blockChainService, int.Parse(port));
p2pService.Start();

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Enter peer port: ");
var peerPort = int.Parse(Console.ReadLine());
if (peerPort != null)
{
    await p2pService.ConnectToPeerAsync("127.0.0.1", peerPort);
    Console.WriteLine("Connected to peer");
}
Console.ResetColor();


Console.WriteLine("Blockchain Menu");
Console.WriteLine("1. Mine Block");
Console.WriteLine("2. Create Transaction");
Console.WriteLine("3. Show Alice Balance");
Console.WriteLine("4. Show Bob Balance");
Console.WriteLine("5. Validate Blockchain");
Console.WriteLine("6. Print Blockchain");
Console.WriteLine("7. Exit");
Console.WriteLine("8. Change Blockchain");
Console.WriteLine("9. Clear Blockchain");


var walletAlice = walletService.CreateWallet("Alice");
var walletBob = walletService.CreateWallet("Bob");
var walletCharlie = walletService.CreateWallet("Charlie");
var walletDave = walletService.CreateWallet("Dave");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.Write("Enter your choice: ");
    var choice = Console.ReadLine();
    Console.ResetColor();

    switch (choice)
    {
        case "1":
            var block = blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            p2pService.BroadcastNewBlock(block);
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
        case "9":
            blockChainService.Chain.Clear();
            new FileStorageService().ClearBlockChain();
            return;

        case "7":
            return;
    }
}


// HW_6

//Console.WriteLine("\n=== 1. Double Spend Demonstration ===");
//var alice = walletService.CreateWallet("Alice");
//var bob = walletService.CreateWallet("Bob");

//blockChainService.MineBlock(alice.Address, CancellationToken.None);

//var tx1 = transactionService.CreateTransaction(alice, bob.Address, 50, alice.PublicKey);
//var tx2 = transactionService.CreateTransaction(alice, bob.Address, 50, alice.PublicKey);

//blockChainService.AddTransactionToMempool(tx1);
//blockChainService.AddTransactionToMempool(tx2);

//try
//{
//    blockChainService.MineBlock(alice.Address, CancellationToken.None);
//}
//catch (InvalidOperationException ex)
//{
//    Console.WriteLine($"[SUCCESS] Double spend prevented: {ex.Message}");
//}

//blockChainService.PendingTransactions.Clear();


//Console.WriteLine("\n=== 2. Hard Cap Demonstration (Limit: 1000) ===");
//var miner = walletService.CreateWallet("Miner");

//for (int i = 0; i < 21; i++)
//{
//    blockChainService.MineBlock(miner.Address, CancellationToken.None);
//}

//Console.WriteLine($"Total Minted: {blockChainService.TotalMinted}");
//decimal balanceBefore = walletService.GetBalance(miner.Address);

//blockChainService.MineBlock(miner.Address, CancellationToken.None);
//decimal balanceAfter = walletService.GetBalance(miner.Address);

//Console.WriteLine($"Miner balance before extra block: {balanceBefore}");
//Console.WriteLine($"Miner balance after extra block:  {balanceAfter}");
//Console.WriteLine(balanceBefore == balanceAfter ? "[SUCCESS] Hard cap enforced. Balance did not increase." : "[FAILED] Hard cap breached.");


//Console.WriteLine("\n=== 3. Economy Audit ===");
//bool isEconomyValid = blockChainService.ValidateEconomy();
//Console.WriteLine($"[AUDIT RESULT] Is Economy Valid: {isEconomyValid}");



// HW_7

//var spammer = walletService.CreateWallet("Spammer");
//var alice = walletService.CreateWallet("Alice");
//var bob = walletService.CreateWallet("Bob");
//var charlie = walletService.CreateWallet("Charlie");
//var dave = walletService.CreateWallet("Dave");

//for (int i = 0; i < 5; i++) blockChainService.MineBlock(spammer.Address, CancellationToken.None);
//for (int i = 0; i < 2; i++) blockChainService.MineBlock(alice.Address, CancellationToken.None);

//blockChainService.MineBlock(charlie.Address, CancellationToken.None);
//var setupTx = transactionService.CreateTransaction(charlie, dave.Address, 10, charlie.PublicKey);
//blockChainService.AddTransactionToMempool(setupTx);
//blockChainService.MineBlock(dave.Address, CancellationToken.None);
//blockChainService.PendingTransactions.Clear();

//Console.WriteLine("\n=== 1. Spam Attack Test ===");
//int successfulSpamCount = 0;
//for (int i = 0; i < 10; i++)
//{
//    try
//    {
//        var tx = transactionService.CreateTransaction(spammer, bob.Address, 1 + i, spammer.PublicKey);
//        tx.Fee = 0;
//        tx.Signature = spammer.Sign(tx.GetDataToSign());

//        blockChainService.AddTransactionToMempool(tx);
//        successfulSpamCount++;
//    }
//    catch (InvalidOperationException) { }
//}
//Console.WriteLine($"Mempool size: {blockChainService.PendingTransactions.Count} / {blockChainService.MaxMempoolSize}");
//Console.WriteLine($"[RESULT] Accepted free transactions: {successfulSpamCount} (Expected: 5)");

//Console.WriteLine("\n=== 2. Eviction Test ===");
//try
//{
//    var highFeeTx = transactionService.CreateTransaction(spammer, alice.Address, 1, spammer.PublicKey);
//    highFeeTx.Fee = 10;
//    highFeeTx.Signature = spammer.Sign(highFeeTx.GetDataToSign());

//    blockChainService.AddTransactionToMempool(highFeeTx);
//    Console.WriteLine($"Mempool size after eviction: {blockChainService.PendingTransactions.Count}");
//    Console.WriteLine($"Count of free transactions left: {blockChainService.PendingTransactions.Count(t => t.Fee == 0)} (Expected: 4)");
//}
//catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }

//blockChainService.PendingTransactions.Clear();

//Console.WriteLine("\n=== 3. Replace-By-Fee (RBF) Test ===");
//var txLowFee = transactionService.CreateTransaction(alice, bob.Address, 5, alice.PublicKey);
//txLowFee.Fee = 1;
//txLowFee.Signature = alice.Sign(txLowFee.GetDataToSign());
//blockChainService.AddTransactionToMempool(txLowFee);

//var txHighFee = transactionService.CreateTransaction(alice, bob.Address, 5, alice.PublicKey);
//txHighFee.Fee = 15;
//txHighFee.Signature = alice.Sign(txHighFee.GetDataToSign());
//blockChainService.AddTransactionToMempool(txHighFee);

//Console.WriteLine($"Mempool size: {blockChainService.PendingTransactions.Count} (Expected: 1)");
//Console.WriteLine($"Current transaction Fee in mempool: {blockChainService.PendingTransactions.First().Fee} (Expected: 15)");

//blockChainService.PendingTransactions.Clear();

//Console.WriteLine("\n=== 4. Pending Balance Test ===");
//Console.WriteLine($"Charlie real balance: {walletService.GetBalance(charlie.Address)} (Expected: 40)");

//var tx1 = transactionService.CreateTransaction(charlie, dave.Address, 30, charlie.PublicKey);
//blockChainService.AddTransactionToMempool(tx1);
//Console.WriteLine($"[SUCCESS] First tx (30 coins) added. Charlie pending balance: {blockChainService.GetPendingBalance(charlie.Address)}");

//try
//{
//    var tx2 = transactionService.CreateTransaction(charlie, dave.Address, 20, charlie.PublicKey);
//    blockChainService.AddTransactionToMempool(tx2);
//    Console.WriteLine("[FAILED] Second tx bypassed pending balance security!");
//}
//catch (InvalidOperationException ex)
//{
//    Console.WriteLine($"[SUCCESS] Second tx (20 coins) rejected: {ex.Message}");
//}

//        blockChainService.AddTransactionToMempool(tx);
//        successfulSpamCount++;
//    }
//    catch (InvalidOperationException) { }
//}
//Console.WriteLine($"Mempool size: {blockChainService.PendingTransactions.Count} / {blockChainService.MaxMempoolSize}");
//Console.WriteLine($"[RESULT] Accepted free transactions: {successfulSpamCount} (Expected: 5)");

//Console.WriteLine("\n=== 2. Eviction Test ===");
//try
//{
//    var highFeeTx = transactionService.CreateTransaction(spammer, alice.Address, 1, spammer.PublicKey);
//    highFeeTx.Fee = 10;
//    highFeeTx.Signature = spammer.Sign(highFeeTx.GetDataToSign());

//    blockChainService.AddTransactionToMempool(highFeeTx);
//    Console.WriteLine($"Mempool size after eviction: {blockChainService.PendingTransactions.Count}");
//    Console.WriteLine($"Count of free transactions left: {blockChainService.PendingTransactions.Count(t => t.Fee == 0)} (Expected: 4)");
//}
//catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }

//blockChainService.PendingTransactions.Clear();
if (File.Exists("blockchain.json")) File.Delete("blockchain.json");
if (File.Exists("blockchain_backup.json")) File.Delete("blockchain_backup.json");

//Console.WriteLine("\n=== 3. Replace-By-Fee (RBF) Test ===");
//var txLowFee = transactionService.CreateTransaction(alice, bob.Address, 5, alice.PublicKey);
//txLowFee.Fee = 1;
//txLowFee.Signature = alice.Sign(txLowFee.GetDataToSign());
//blockChainService.AddTransactionToMempool(txLowFee);
var cts = CancellationToken.None;

//var txHighFee = transactionService.CreateTransaction(alice, bob.Address, 5, alice.PublicKey);
//txHighFee.Fee = 15;
//txHighFee.Signature = alice.Sign(txHighFee.GetDataToSign());
//blockChainService.AddTransactionToMempool(txHighFee);
Wallet miner1 = walletService.CreateWallet("Miner 1");
Wallet miner2 = walletService.CreateWallet("Miner 2");
Wallet alice = walletService.CreateWallet("Alice");
Wallet bob = walletService.CreateWallet("Bob");

//Console.WriteLine($"Mempool size: {blockChainService.PendingTransactions.Count} (Expected: 1)");
//Console.WriteLine($"Current transaction Fee in mempool: {blockChainService.PendingTransactions.First().Fee} (Expected: 15)");

for (int i = blockChainService.Chain.Count; blockChainService.GetCurrentReward() > 0; i++)
{
    Console.WriteLine($"Block #{i} -> Reward: {blockChainService.GetCurrentReward()} coins");
    blockChainService.MineBlock(miner1.Address, cts);
}
//blockChainService.PendingTransactions.Clear();

Console.WriteLine($"Emission stopped. Current reward: {blockChainService.GetCurrentReward()}. Total blocks: {blockChainService.Chain.Count}");
//Console.WriteLine("\n=== 4. Pending Balance Test ===");
//Console.WriteLine($"Charlie real balance: {walletService.GetBalance(charlie.Address)} (Expected: 40)");

Console.WriteLine("\n--- TEST 2: MINER'S DILEMMA ---");
//var tx1 = transactionService.CreateTransaction(charlie, dave.Address, 30, charlie.PublicKey);
//blockChainService.AddTransactionToMempool(tx1);
//Console.WriteLine($"[SUCCESS] First tx (30 coins) added. Charlie pending balance: {blockChainService.GetPendingBalance(charlie.Address)}");

try
{
    blockChainService.MineBlock(miner1.Address, cts);
}
catch (InvalidOperationException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[CAUGHT] Mining cancelled: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine("\n--- TEST 3: FEE BURNING ---");

var fundAliceTx = transactionService.CreateTransaction(miner1, alice.Address, 100m, miner1.PublicKey);
fundAliceTx.Fee = 5m;
fundAliceTx.Signature = miner1.Sign(fundAliceTx.GetDataToSign());

blockChainService.AddTransactionToMempool(fundAliceTx);
blockChainService.MineBlock(miner1.Address, cts);

var aliceToBobTx = transactionService.CreateTransaction(alice, bob.Address, 20m, alice.PublicKey);
aliceToBobTx.Fee = 10m;
aliceToBobTx.Signature = alice.Sign(aliceToBobTx.GetDataToSign());

blockChainService.AddTransactionToMempool(aliceToBobTx);

decimal miner2BalanceBefore = walletService.GetBalance(miner2.Address);
blockChainService.MineBlock(miner2.Address, cts);
decimal miner2BalanceAfter = walletService.GetBalance(miner2.Address);

decimal minerProfit = miner2BalanceAfter - miner2BalanceBefore;

Console.WriteLine($"Alice's original fee: 10 coins");
Console.WriteLine($"Miner 2 profit: {minerProfit} coins");
Console.WriteLine("Result: Miner received 50%, and the remaining 5 coins were BURNED!");
//try
//{
//    var tx2 = transactionService.CreateTransaction(charlie, dave.Address, 20, charlie.PublicKey);
//    blockChainService.AddTransactionToMempool(tx2);
//    Console.WriteLine("[FAILED] Second tx bypassed pending balance security!");
//}
//catch (InvalidOperationException ex)
//{
//    Console.WriteLine($"[SUCCESS] Second tx (20 coins) rejected: {ex.Message}");
//}
