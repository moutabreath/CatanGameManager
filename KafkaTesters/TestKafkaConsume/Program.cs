using Confluent.Kafka;
using System.Collections.Generic;

namespace TestkafkaConsume
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            IList<string> topics = new List<string>
            {
                "player-points"
            };
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(topics);
                var consumeResult = consumer.Consume(5000);

            }

        }

    }
}

