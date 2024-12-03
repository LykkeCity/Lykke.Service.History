using System;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Cqrs;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Tests.Init;
using Autofac;
using Lykke.Cqrs;
using Xunit;

namespace Antares.Service.History.Tests
{
    [Collection("history-tests")]
    public class ForwardWithdrawalTests
    {
        public ForwardWithdrawalTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        private readonly IContainer _container;

        private CreateForwardCashinCommand CreateForwardCashinRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var volume = 54.31M;

            var command = new CreateForwardCashinCommand
            {
                OperationId = id,
                WalletId = clientId,
                Volume = volume,
                AssetId = "LKK",
                Timestamp = DateTime.UtcNow
            };

            cqrs.SendCommand(command, HistoryBoundedContext.Name, HistoryBoundedContext.Name);

            return command;
        }

        private void DeleteForwardCashinRecord(Guid id, Guid walletId)
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var command = new DeleteForwardCashinCommand
            {
                OperationId = id,
                WalletId = walletId
            };

            cqrs.SendCommand(command, HistoryBoundedContext.Name, HistoryBoundedContext.Name);
        }

        [Fact]
        public async Task ForwardTemporaryCashin_Test()
        {
            var command = CreateForwardCashinRecord();

            await Task.Delay(3000);

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var item = await repo.GetAsync(command.OperationId, command.WalletId);

            Assert.NotNull(item);
            Assert.True(item is Cashin);

            var cashin = item as Cashin;

            Assert.Equal(command.Volume, cashin.Volume);
            Assert.Equal(command.AssetId, cashin.AssetId);

            DeleteForwardCashinRecord(command.OperationId, command.WalletId);

            await Task.Delay(3000);

            item = await repo.GetAsync(command.OperationId, command.WalletId);

            Assert.Null(item);
        }
    }
}
