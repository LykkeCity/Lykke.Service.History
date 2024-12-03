# Antares.Service.History 

# Purpose

  - Keeping the read-model of history data and orders of each wallet.

# Contracts

Input (v1, Antares.Service.PostProcessing events; RabbitMQ, protobuf):
  - CashInProcessedEvent, CashOutProcessedEvent, CashTransferProcessedEvent, ExecutionProcessedEvent, OrderPlacedEvent, OrderCancelledEvent.

Input (v2, Antares.Service.BlockchainCashinDetector; RabbitMQ, messagepack):
  - CashinCompletedEvent.
  
Input (v3, Antares.Service.BlockchainCashoutProcessor; RabbitMQ, messagepack):
  - CashoutCompletedEvent.
  
Input (v4, bitcoinservice; RabbitMQ, messagepack):
  - CashinCompletedEvent, CashoutCompletedEvent.

Input (v5, "tx-handler.ethereum.commands" context; RabbitMQ, messagepack):
  - SaveEthInHistoryCommand, ProcessEthCoinEventCommand, ProcessHotWalletErc20EventCommand.
  
Output (HTTP):
  - get order by id;
  - get orders by wallet id;
  - get history by wallet id;

# Scaling

| Image | Resources | Default instances number | Max instances |
| ------ | ------ | ------ | ------ |
| Lykke.Service.History | C2-R2 | 1 | 2* |

 \* Service can be scaled by changing prefetch and batch settings (table below, measured on ExecutionProcessedEvent)

| Prefetch | Batch | 1 replica (ops) | 2 replicas (ops) |
| ------ | ------ | ------ | ------ |
| 200 | 100 | 500 | 700 |
| 500 | 100 | 650 | 750 |
| 2000 | 500 | 2100 | 3500 |
| 10000 | 1000 | 3200 | 4400 |

# Dependencies
  - PostgreSQL (data);
  - Azure Table Storage (logs);
  - RabbitMQ (Cqrs);

# Service owners
Swisschain team
