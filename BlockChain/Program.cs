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
Console.WriteLine("11. Merkle Proof Test");
Console.WriteLine("12. Merkle Tree Malicious Test");
Console.WriteLine("13. P2P Security Test");
Console.WriteLine("14. Merkle Root Integration Test");
Console.WriteLine("15. Connect to Peer");
Console.WriteLine("16. Broadcast Sync");
Console.WriteLine("17. Run Multi-Currency Economy Demo (ICO & Transfer)");


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
        //case "10":
        //    Console.WriteLine("\n--- Starting Benchmark ---");

        //    int txCount = 9;

        //    if (walletService.GetBalance(walletAlice.Address) < (txCount * 2))
        //    {
        //        Console.WriteLine("Mining initial block to fund Alice...");
        //        blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
        //    }

        //    Console.WriteLine($"Generating and adding {txCount} transactions to mempool...");
        //    for (int i = 0; i < txCount; i++)
        //    {
        //        try
        //        {
        //            var txBench = transactionService.CreateTransaction(walletAlice, walletBob.Address, 1, walletAlice.PublicKey);
        //            blockChainService.AddTransactionToMempool(txBench);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error adding tx #{i}: {ex.Message}");
        //            break;
        //        }
        //    }

        //    Console.WriteLine($"Mining block with {blockChainService.PendingTransactions.Count} transactions...");

        //    var sw = Stopwatch.StartNew();
        //    var benchmarkBlock = blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
        //    sw.Stop();

        //    Console.WriteLine("\n" + new string('=', 50));
        //    Console.WriteLine("[ BENCHMARK RESULTS ]");
        //    Console.WriteLine($"Difficulty: {blockChainService.Difficulty}");
        //    Console.WriteLine($"Transactions in block: {benchmarkBlock.Transactions.Count}");
        //    Console.WriteLine($"Time taken: {sw.Elapsed.TotalSeconds:F4} seconds ({sw.ElapsedMilliseconds} ms)");
        //    Console.WriteLine($"Nonce found: {benchmarkBlock.Nonce}");
        //    Console.WriteLine(new string('=', 50) + "\n");
        //    break;
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
        case "17":
            Console.WriteLine("\n=== MULTI-CURRENCY ECONOMY DEMONSTRATION ===");

            // 1. Аліса майнить блоки
            Console.WriteLine("\n1. Alice mines blocks to accumulate 'BASE' capital...");
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            Console.WriteLine($"Alice's balance: {walletService.GetBalance(walletAlice.Address).GetValueOrDefault("BASE")} BASE");

            // 2. Аліса проводить ICO
            Console.WriteLine("\n2. Alice successfully conducts an ICO for 1000 'ALICE_COIN'...");
            var icoAlice = new Transaction(walletAlice.Address, walletAlice.Address, 0, walletAlice.PublicKey)
            {
                Type = TransactionType.ICO,
                Ticker = "ALICE_COIN",
                Emission = 1000,
                Fee = 100
            };
            icoAlice.Signature = walletService.SignMessage(walletAlice, Encoding.UTF8.GetString(icoAlice.GetDataToSign()));
            blockChainService.AddTransactionToMempool(icoAlice);
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            Console.WriteLine("Success!");

            // 3. Боб без грошей
            Console.WriteLine("\n3. Bob tries to issue 'BOB_COIN' while being broke...");
            var icoBob = new Transaction(walletBob.Address, walletBob.Address, 0, walletBob.PublicKey)
            {
                Type = TransactionType.ICO,
                Ticker = "BOB_COIN",
                Emission = 5000,
                Fee = 100
            };
            icoBob.Signature = walletService.SignMessage(walletBob, Encoding.UTF8.GetString(icoBob.GetDataToSign()));
            try
            {
                blockChainService.AddTransactionToMempool(icoBob);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"NETWORK REJECTION: {ex.Message}");
                Console.ResetColor();
            }

            // 4. Плагіат
            Console.WriteLine("\n4. Bob tries to steal the brand 'ALICE_COIN' (Plagiarism)...");
            var giveBobBase = new Transaction(walletAlice.Address, walletBob.Address, 110, walletAlice.PublicKey) { Type = TransactionType.Transfer, Ticker = "BASE", Amount = 110, Fee = 1 };
            giveBobBase.Signature = walletService.SignMessage(walletAlice, Encoding.UTF8.GetString(giveBobBase.GetDataToSign()));
            blockChainService.AddTransactionToMempool(giveBobBase);
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

            var stealAliceCoin = new Transaction(walletBob.Address, walletBob.Address, 0, walletBob.PublicKey)
            {
                Type = TransactionType.ICO,
                Ticker = "ALICE_COIN",
                Emission = 999,
                Fee = 100
            };
            stealAliceCoin.Signature = walletService.SignMessage(walletBob, Encoding.UTF8.GetString(stealAliceCoin.GetDataToSign()));
            try
            {
                blockChainService.AddTransactionToMempool(stealAliceCoin);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"NETWORK REJECTION: {ex.Message}");
                Console.ResetColor();
            }

            // 5. Переказ нових токенів
            Console.WriteLine("\n5. Alice successfully transfers 300 'ALICE_COIN' to Bob...");
            var transferToken = new Transaction(walletAlice.Address, walletBob.Address, 300, walletAlice.PublicKey)
            {
                Type = TransactionType.Transfer,
                Ticker = "ALICE_COIN",
                Amount = 300,
                Fee = 5
            };
            transferToken.Signature = walletService.SignMessage(walletAlice, Encoding.UTF8.GetString(transferToken.GetDataToSign()));
            blockChainService.AddTransactionToMempool(transferToken);
            blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
            Console.WriteLine("Success!");

            // 6. Фінальний результат
            Console.WriteLine("\n6. FINAL MULTI-CURRENCY PORTFOLIOS:");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("Alice's Wallet:");
            foreach (var asset in walletService.GetBalance(walletAlice.Address))
                Console.WriteLine($"  [{asset.Key}]: {asset.Value}");

            Console.WriteLine("\nBob's Wallet:");
            foreach (var asset in walletService.GetBalance(walletBob.Address))
                Console.WriteLine($"  [{asset.Key}]: {asset.Value}");
            Console.WriteLine(new string('-', 30) + "\n");
            break;
        case "18":
            Console.WriteLine("\n=== EXTRA TASK: BATTLE FOR THE TICKER (MANUAL FORK) ===");
            Console.Write("Is this Node A (shorter chain) or Node B (longer chain)? (Enter A or B): ");
            var nodeRole = Console.ReadLine()?.Trim().ToUpper();

            if (nodeRole == "A")
            {
                Console.WriteLine("\n[NODE A] Mining initial blocks for 'BASE' capital...");
                blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);
                blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

                Console.WriteLine("[NODE A] Alice issues 'MEME' token (1000 supply) and mines 1 block.");
                var icoA = new Transaction(walletAlice.Address, walletAlice.Address, 0, walletAlice.PublicKey)
                {
                    Type = TransactionType.ICO,
                    Ticker = "MEME",
                    Emission = 1000,
                    Fee = 100
                };
                icoA.Signature = walletService.SignMessage(walletAlice, Encoding.UTF8.GetString(icoA.GetDataToSign()));

                blockChainService.AddTransactionToMempool(icoA);
                blockChainService.MineBlock(walletAlice.Address, CancellationToken.None);

                Console.WriteLine($"\n[NODE A READY] Current chain length: {blockChainService.Chain.Count}");
                Console.WriteLine("Wait for Node B to be ready, then connect them (Option 15).");
            }
            else if (nodeRole == "B")
            {
                Console.WriteLine("\n[NODE B] Mining initial blocks for 'BASE' capital...");
                blockChainService.MineBlock(walletBob.Address, CancellationToken.None);
                blockChainService.MineBlock(walletBob.Address, CancellationToken.None);

                Console.WriteLine("[NODE B] Bob issues 'MEME' token (5000 supply) and mines 2 blocks (longer chain!).");
                var icoB = new Transaction(walletBob.Address, walletBob.Address, 0, walletBob.PublicKey)
                {
                    Type = TransactionType.ICO,
                    Ticker = "MEME",
                    Emission = 5000,
                    Fee = 100
                };
                icoB.Signature = walletService.SignMessage(walletBob, Encoding.UTF8.GetString(icoB.GetDataToSign()));

                blockChainService.AddTransactionToMempool(icoB);
                blockChainService.MineBlock(walletBob.Address, CancellationToken.None); // Блок з ICO Боба
                blockChainService.MineBlock(walletBob.Address, CancellationToken.None); // Порожній блок для переваги у вазі

                Console.WriteLine($"\n[NODE B READY] Current chain length: {blockChainService.Chain.Count}");
                Console.WriteLine("Now connect Node A to this Node (using Option 15 on Node A).");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter 'A' or 'B'.");
            }
            break;
        case "7":
            return;
    }
}
