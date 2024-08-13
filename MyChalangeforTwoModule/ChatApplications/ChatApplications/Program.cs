using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatApplications
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pleas enter a username: ");
            var user = Console.ReadLine();

            Console.WriteLine("Please specify a chat room name:");
            var roomName = Console.ReadLine();

            var exchangeName = "chat2";

            // Create unique queue name for this instance
            var queueName = Guid.NewGuid().ToString();

            var factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{user}:{user}@localhost:5672");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, true, true, true);
            channel.QueueBind(queueName, exchangeName, roomName);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(eventArgs.Body);
                var userId = eventArgs.BasicProperties.UserId;
                Console.WriteLine($"{userId} -> {msg}");
            };

            channel.BasicConsume(queueName, true, consumer);


            var message = Console.ReadLine();
            while (message != "")
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                var props = channel.CreateBasicProperties();
                props.UserId = user;
                channel.BasicPublish(exchangeName, roomName, props, bytes);
                message = Console.ReadLine();
            }

            channel.Close();
            connection.Close();

            //Console.ReadLine();
        }


    }
}
