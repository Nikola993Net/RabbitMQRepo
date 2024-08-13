using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace BackOffice
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://backoffice:backoffice@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var arguments = new Dictionary<string, object>
            {
                { "x-first-death-reason", "DLX"}
            };
            channel.QueueDeclare("backOfficeQueue", true, false, false, arguments);
            var headers = new Dictionary<string, object>
            {
                { "subject", "tour" },
                { "action", "booked" },
                { "x-match", "any" }
            };

            channel.QueueBind("backOfficeQueue", "webappExchange", "", headers);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(eventArgs.Body);

                var subject = System.Text.Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["subject"] as byte[]);
                var action = System.Text.Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["action"] as byte[]);
                var userId = eventArgs.BasicProperties.UserId;

                Console.WriteLine($"{userId} -> {eventArgs.BasicProperties.UserId} -> {subject} {action} : {msg}");
                channel.BasicReject(eventArgs.DeliveryTag, false);
            };

            channel.BasicConsume("backOfficeQueue", false, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
