
using Helper;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

Console.WriteLine("Hello, Please enter user phone number:");
var phoneNumber = Console.ReadLine();
Console.WriteLine("Please enter user email:");
var email = Console.ReadLine();
string exchangeName = "userAlertExchange";

var connectionFactory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};
IConnection connection = await connectionFactory.CreateConnectionAsync();
IChannel channel = await connection.CreateChannelAsync();
await channel.ExchangeDeclareAsync(exchangeName, type: ExchangeType.Headers, durable: true, autoDelete: false, arguments: null);

var user = new User
{
    PhoneNumber = phoneNumber,
    Email = email
};
var userJSON = JsonConvert.SerializeObject(user);

Console.WriteLine("Enter log type: ");
var logType = Console.ReadLine();
Console.WriteLine("Enter platform: ");
var platform = Console.ReadLine();
var headerProperties = new BasicProperties();
var headers = new Dictionary<string, object>
{
    { "LogType" ,  logType},
    { "PlatForm" , platform}
};
headerProperties.Headers = headers;

await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "", mandatory: false,
    body: Encoding.UTF8.GetBytes(userJSON), basicProperties: headerProperties);

Console.WriteLine("User informations sent to queue.");
Console.ReadKey();
