using Confluent.Kafka;
using System.Reflection.Metadata.Ecma335;
using static Confluent.Kafka.ConfigPropertyNames;

namespace MessageBusLib
{
    public sealed class MessageBus : IDisposable
    {
        private readonly IProducer<Null, string> Producer;
        private IConsumer<Null, string>? Consumer;
        private readonly IDictionary<string, string> ProducerConfig;
        private readonly IDictionary<string, string> ConsumerConfig;

        public MessageBus() : this("localhost:9092") { }

        public MessageBus(string host)
        {
            ProducerConfig = new Dictionary<string, string> { { "bootstrap.servers", host } };
            ConsumerConfig = new Dictionary<string, string>
            {
                { "group.id", "custom-group"},
                { "bootstrap.servers", host }
            };

            Producer = new ProducerBuilder<Null, string>(ProducerConfig).Build();
        }

        public void SendMessage(string topic, string message)
        {
            try
            {
                Producer.Produce(topic, new Message<Null, string> { Value = message });
                Producer.Flush();
            }
            catch (ProduceException<Null, string> err)
            {
                Console.WriteLine($"Failed to deliver msg: {err.Error.Reason}");
            }

        }

        public void SubscribeOnTopic<T>(string topic, Action<T> action, CancellationToken cancellationToken) where T : class
        {
            using (Consumer = new ConsumerBuilder<Null, string>(ConsumerConfig).Build())
            {
                Consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, -1) }); //topic.Length-1
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = Consumer.Consume(TimeSpan.FromMilliseconds(10));//TimeSpan.FromMilliseconds(10)
                    if (result != null)
                        action(result.Message.Value as T);
                }
            }
        }

        public void Dispose()
        {
            Producer?.Dispose();
            Consumer?.Dispose();
        }
    }
}