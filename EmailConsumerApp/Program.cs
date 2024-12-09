
using Helper;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "userAlertExchange";
string queueName = "EmailToUser";
var connectionFactory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};
var connection = await connectionFactory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();
await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

var headers = new Dictionary<string, object>
{
    { "LogType" ,  "error"},
    { "PlatForm" , "windows"},
    { "x-match" , "any" }
};

await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: "", arguments: headers);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, args) => {
    var body = args.Body.ToArray();
    var bodyString = Encoding.UTF8.GetString(body);
    var user = JsonConvert.DeserializeObject<User>(bodyString);
    if (user != null)
    {
        Console.WriteLine($"Email sent to: {user.Email}");
        await channel.BasicAckAsync(args.DeliveryTag, false);
    }
};

await channel.BasicConsumeAsync(queueName, false, consumer);
Console.ReadLine();