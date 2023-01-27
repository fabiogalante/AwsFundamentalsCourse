using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumers.Messages;
using MediatR;
using Microsoft.Extensions.Options;

namespace Customers.Consumers;

public class QueueConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly IOptions<QueueSettings> _settings;
    private readonly IMediator _mediator;
    private readonly ILogger<QueueConsumerService> _logger;

    public QueueConsumerService(IAmazonSQS sqs, IOptions<QueueSettings> settings, IMediator mediator, ILogger<QueueConsumerService> logger)
    {
        _sqs = sqs;
        _settings = settings;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrlResponse = await _sqs.GetQueueUrlAsync("customers", stoppingToken);

        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl  = queueUrlResponse.QueueUrl,
            AttributeNames = new List<string>{"All"},
            MessageAttributeNames = new List<string>{"All"},
            MaxNumberOfMessages = 1
  
        };
        
        while(!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in response.Messages)
            {
                var messageType = message.MessageAttributes["MessageType"].StringValue;
                var type = Type.GetType($"Customers.Consumers.Messages.{messageType}");
                
                if(type is null)
                {
                    _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                    continue;
                }
                
                var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type)!;
                

                try
                {
                    await _mediator.Send(typedMessage, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message failed during processing");
                   continue;
                }
                
                // switch (messageType)
                // {
                //     case nameof(CustomerCreated):
                //         var created = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
                //         break;
                //     
                //     case nameof(CustomerUpdate):
                //         break;
                //     
                //     case nameof(CustomerDeleted):
                //         break;
                // }
        
                await _sqs.DeleteMessageAsync(queueUrlResponse.QueueUrl,message.ReceiptHandle, stoppingToken);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}