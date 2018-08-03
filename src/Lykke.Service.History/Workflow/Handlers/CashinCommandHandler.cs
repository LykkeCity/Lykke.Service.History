﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Service.History.Workflow.Handlers
{
    public class CashinCommandHandler
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public CashinCommandHandler(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(SaveCashinCommand command)
        {
            var entity = Mapper.Map<Cashin>(command);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Skipped duplicated cashin record", context: new { id = command.Id });
            }

            return CommandHandlingResult.Ok();
        }
    }
}
