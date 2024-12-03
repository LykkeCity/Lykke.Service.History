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
//    public class CashoutTests
//    {
//        public CashoutTests(TestInitialization initialization)
//        {
//            _container = initialization.Container;
//        }

//        private readonly IContainer _container;

//        private CashOutProcessedEvent CreateCashoutRecord()
//        {
//            var cqrs = _container.Resolve<ICqrsEngine>();

//            var id = Guid.NewGuid();
//            var clientId = Guid.NewGuid();
//            var volume = 54.31M;

//            var @event = new CashOutProcessedEvent
//            {
//                OperationId = id,
//                WalletId = clientId,
//                Volume = volume,
//                AssetId = "EUR",
//                Timestamp = DateTime.UtcNow
//            };

//            cqrs.PublishEvent(@event, PostProcessingBoundedContext.Name);

//            return @event;
//        }

//        [Fact]
//        public async Task SaveCashout_Test()
//        {
//            var command = CreateCashoutRecord();

//            await Task.Delay(3000);

//            var repo = _container.Resolve<IHistoryRecordsRepository>();

//            var item = await repo.GetAsync(command.OperationId, command.WalletId);

//            Assert.NotNull(item);
//            Assert.True(item is Cashout);
//            Assert.Equal(-Math.Abs(command.Volume), (item as Cashout).Volume);
//        }
//    }
//}
