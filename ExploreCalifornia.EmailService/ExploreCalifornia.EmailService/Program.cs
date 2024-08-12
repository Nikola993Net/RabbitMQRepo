using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace ExploreCalifornia.EmailService
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("emailServiceQueue", true, false, false);
            //channel.QueueBind("emailServiceQueue", "webappExchange", "");

            var headers = new Dictionary<string, object>
            {
                { "subject", "tour" },
                { "action", "booked" },
                { "x-match", "all" }
            };
            channel.QueueBind("emailServiceQueue", "webappExchange", "", headers);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(eventArgs.Body);

                var subject = System.Text.Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["subject"] as byte[]);
                var action = System.Text.Encoding.UTF8.GetString(eventArgs.BasicProperties.Headers["action"] as byte[]);

                Console.WriteLine($"{subject} {action} : {msg}");
            };

            //this is aknowladgement that queue now can delete message from the queue
            channel.BasicConsume("emailServiceQueue", true, consumer);

            Console.ReadLine();

            channel.Close();
            connection.Close();
        }
    }
}
