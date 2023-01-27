using Amazon.SQS;

namespace Customers.Api.Messaging;

public class SqsMessenger
{
    private readonly  IAmazonSQS _sqs;

    public SqsMessenger(IAmazonSQS sqs)
    {
        _sqs = sqs;
    }
}