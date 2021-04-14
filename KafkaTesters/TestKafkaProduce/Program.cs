using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace TestApi
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.

            using (IProducer<Null, string> producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    DeliveryResult<Null, string> directoryResult = await producer.ProduceAsync("player-points", new Message<Null, string> { Value = $"{Guid.NewGuid()}" });
                }
                catch (ProduceException<Null, string> ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Console.WriteLine("Hello World!");
        }
    }
}
