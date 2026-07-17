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
Console.WriteLine("10. Benchmark: Mining with Transactions");
Console.WriteLine("16. Connect to Peer");


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
            var transaction1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 10, walletAlice.PublicKey);
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
        case "10":
            Console.WriteLine("\n--- Starting Benchmark ---");

            int txCount = 9;

            if (walletService.GetBalance(walletAlice.Address) < (txCount * 2))
            {
                Console.WriteLine("Mining initial block to fund Alice...");
                blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            }

            Console.WriteLine($"Generating and adding {txCount} transactions to mempool...");
            for (int i = 0; i < txCount; i++)
            {
                try
                {
                    var txBench = transactionService.CreateTransaction(walletAlice, walletBob.Address, 1, walletAlice.PublicKey);
                    blockChainService.AddTransactionToMempool(txBench);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding tx #{i}: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine($"Mining block with {blockChainService.PendingTransactions.Count} transactions...");

            var sw = Stopwatch.StartNew();
            var benchmarkBlock = blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            sw.Stop();

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("[ BENCHMARK RESULTS ]");
            Console.WriteLine($"Difficulty: {blockChainService.Difficulty}");
            Console.WriteLine($"Transactions in block: {benchmarkBlock.Transactions.Count}");
            Console.WriteLine($"Time taken: {sw.Elapsed.TotalSeconds:F4} seconds ({sw.ElapsedMilliseconds} ms)");
            Console.WriteLine($"Nonce found: {benchmarkBlock.Nonce}");
            Console.WriteLine(new string('=', 50) + "\n");
            break;
        case "11":

            Console.WriteLine("\n Test Merkle Proof");

            for (int i = 1; i <= 5; i++)
            {
                var tx = transactionService.CreateTransaction(walletAlice, walletBob.Address, i, walletAlice.PublicKey);
                blockChainService.AddTransactionToMempool(tx);
            }

            Console.WriteLine("\n[Step 2] Mining block (expecting Merkle Tree pyramid output)...");
            var minedBlock = blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

            var targetTx = minedBlock.Transactions[2];
            Console.WriteLine($"Target Tx ID: {targetTx.Id}");
            var proof = hashingService.GetMerkleProof(minedBlock.Transactions, targetTx.Id);
            Console.WriteLine($"Generated proof with {proof.Count} hashes.");

            string targetTxHash = hashingService.ComputeTransactionHash(targetTx);
            Console.WriteLine("\n[Step 4] Printing Merkle Proof");
            string expectedRoot = hashingService.GetMerkleRoot(minedBlock.Transactions);

            bool isOriginalValid = hashingService.VerifyMerkleProof(targetTxHash, expectedRoot, proof);
            Console.WriteLine("\n[Step 5] Verifying Original Proof");
            Console.WriteLine($"Original Proof Valid: {isOriginalValid}");

            var tamperedProof = new List<(string Hash, bool IsLeft)>(proof);
            if (tamperedProof.Count > 0)
            {
                var firstElement = tamperedProof[0];
                char fakeChar = firstElement.Hash[0] == '0' ? '1' : '0';
                string tamperedHash = fakeChar + firstElement.Hash.Substring(1);
                tamperedProof[0] = (tamperedHash, firstElement.IsLeft);
                Console.WriteLine("Tampering with proof...");
            }
             
            bool isTamperedValid = hashingService.VerifyMerkleProof(targetTxHash, expectedRoot, tamperedProof);
            Console.WriteLine("\n[Step 6] Verifying Tampered Proof");
            Console.WriteLine($"Tampered Proof Valid: {isTamperedValid}\n");
            break;

        case "12":
            var tx1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 1, walletAlice.PublicKey);
            var tx2 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 2, walletAlice.PublicKey);
            var tx3 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 3, walletAlice.PublicKey);

            var maliciousTxs = new List<Transaction> { tx1, tx2, tx3, tx3 };

            try
            {
                hashingService.GetMerkleRoot(maliciousTxs);
                Console.WriteLine("Failed: Root calculated successfully (Protection bypassed).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Block Rejected: {ex.Message}");
            }

            break;
        case "13":
            Console.WriteLine("--- P2P Security Test ---");

            var txAttack = transactionService.CreateTransaction(walletAlice, walletBob.Address, 1, walletAlice.PublicKey);
            blockChainService.AddTransactionToMempool(txAttack);

            Console.WriteLine("Mining a valid block...");
            var maliciousBlock = blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

            Console.WriteLine("Hacker is modifying the transaction amount to 1,000,000 coins...");
            if (maliciousBlock.Transactions.Count > 0)
            {
                maliciousBlock.Transactions[0].Amount = 1000000;
            }

            Console.WriteLine("Broadcasting the fake block to the P2P network...");
            p2pService.BroadcastNewBlock(maliciousBlock);

            Console.WriteLine("Fake block broadcasted!");
            break;
        case "14":
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            Console.WriteLine("--- Merkle Root Integration Test ---");

            var mTx1 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 1, walletAlice.PublicKey);
            var mTx2 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 2, walletAlice.PublicKey);
            var mTx3 = transactionService.CreateTransaction(walletAlice, walletBob.Address, 3, walletAlice.PublicKey);

            blockChainService.AddTransactionToMempool(mTx1);
            blockChainService.AddTransactionToMempool(mTx2);
            blockChainService.AddTransactionToMempool(mTx3);

            Console.WriteLine("Mining block with transactions...");
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

            Console.WriteLine("\nChecking validity BEFORE attack...");
            displayService.PrintValidationResult(blockChainService.IsValid());

            Console.WriteLine("\nHacker alters the first transaction amount to 999...");
            int lastIndex = blockChainService.Chain.Count - 1;
            blockChainService.Chain[lastIndex].Transactions[0].Amount = 999;

            Console.WriteLine("Checking validity AFTER attack...");
            displayService.PrintValidationResult(blockChainService.IsValid());
            break;
        case "15":

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter peer port: ");
            var peerPort = int.Parse(Console.ReadLine());
            if (peerPort != null)
            {
                await p2pService.ConnectToPeerAsync("127.0.0.1", peerPort);
                Console.WriteLine("Connected to peer");
            }
            Console.ResetColor();
            break;
        case "16":
            p2pService.BroadcastSync();
            break;
        case "7":
            return;
    }
}
