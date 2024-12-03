//using System;
//using System.Threading.Tasks;
//using Antares.Service.History.Core.Domain.History;
//using Antares.Service.History.Tests.Init;
//using Autofac;
//using Lykke.Cqrs;
//using Lykke.Service.PostProcessing.Contracts.Cqrs;
//using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
//using Xunit;

//namespace Antares.Service.History.Tests
//{
//    [Collection("history-tests")]
//    public class TransferTests
//    {
//        public TransferTests(TestInitialization initialization)
//        {
//            _container = initialization.Container;
//        }

//        private readonly IContainer _container;

//        private CashTransferProcessedEvent CreateTransferRecord()
//        {
//            var cqrs = _container.Resolve<ICqrsEngine>();

//            var id = Guid.NewGuid();
//            var walletFrom = Guid.NewGuid();
//            var walletTo = Guid.NewGuid();
//            var volume = new Random().Next(1, 100);

//            var @event = new CashTransferProcessedEvent
//            {
//                OperationId = id,
//                FromWalletId = walletFrom,
//                ToWalletId = walletTo,
//                Volume = volume,
//                AssetId = "EUR",
//                Timestamp = DateTime.UtcNow,
//                FeeSize = 0.5M,
//                FeeSourceWalletId = walletTo
//            };

//            cqrs.PublishEvent(@event, PostProcessingBoundedContext.Name);

//            return @event;
//        }

//        [Fact]
//        public async Task SaveTransfer_Test()
//        {
//            var command = CreateTransferRecord();

//            await Task.Delay(3000);

//            var repo = _container.Resolve<IHistoryRecordsRepository>();

//            var itemFrom = await repo.GetAsync(command.OperationId, command.FromWalletId);
//            var itemTo = await repo.GetAsync(command.OperationId, command.ToWalletId);

//            Assert.NotNull(itemFrom);
//            Assert.NotNull(itemTo);

//            Assert.True(itemFrom is Cashout);
//            Assert.True(itemTo is Cashin);

//            var transferFrom = itemFrom as Cashout;
//            var transferTo = itemTo as Cashin;

//            Assert.Equal(-Math.Abs(command.Volume), transferFrom.Volume);
//            Assert.Equal(Math.Abs(command.Volume), transferTo.Volume);

//            Assert.Null(transferFrom.FeeSize);
//            Assert.Equal(command.FeeSize, transferTo.FeeSize);
//        }
//    }
//}
