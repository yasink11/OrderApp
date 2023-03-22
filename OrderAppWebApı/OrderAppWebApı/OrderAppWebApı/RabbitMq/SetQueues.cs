using RabbitMQ.Client;

namespace OrderAppWebApı.RabbitMq
{
    public class SetQueues
    {
        public static void SendQueue(byte[] datas)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://byvqkrko:g4mt6J-nFOHybGzHZzpvlozl6prYVbWh@shark.rmq.cloudamqp.com/byvqkrko");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("sendTask", true, false, false);

            channel.BasicPublish(string.Empty, "sendTask", null, datas);

        }
    }
}
