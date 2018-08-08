﻿using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public class CustomQueueItem<T>
    {
        private readonly ulong _deliveryTag;

        private readonly IModel _model;

        public T Value { get; }

        public CustomQueueItem(T value, ulong deliveryTag, IModel model)
        {
            Value = value;
            _deliveryTag = deliveryTag;
            _model = model;
        }

        public void Accept()
        {
            _model.BasicAck(_deliveryTag, false);
        }

        public void Reject(bool requeue = true)
        {
            _model.BasicReject(_deliveryTag, requeue);
        }
    }
}