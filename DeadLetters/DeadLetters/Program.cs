using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeadLetters
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://backoffice:backoffice@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("DLX", ExchangeType.Direct, true, false);
            channel.QueueDeclare("deadLetters", true, false, false);
            channel.QueueBind("deadLetters", "DLX", "");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var mesage = System.Text.Encoding.UTF8.GetString(eventArgs.Body);
                var deathReasonBytes = eventArgs.BasicProperties.Headers["x-first-death-reason"] as byte[];
                var deathReason = System.Text.Encoding.UTF8.GetString(deathReasonBytes);
                Console.WriteLine($"Deadletter: {mesage}. Reason: {deathReason}");
            };

            channel.BasicConsume("deadLetters", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
