using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace OrderAppWebApı.BackgroundService
{
    public class SendMailService : IHostedService
    {
        private Timer timer;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(SendQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public void SendQueue(object state)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://byvqkrko:g4mt6J-nFOHybGzHZzpvlozl6prYVbWh@shark.rmq.cloudamqp.com/byvqkrko");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();
            channel.QueueDeclare("sendTask", true, false, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume("sendTask", false, consumer);
            consumer.Received += (obj, e) =>
            {
                var result = Encoding.UTF8.GetString(e.Body.ToArray());
                SendMail(result);
            };
        }
 
  

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
            

            // throw new NotImplementedException();
        }



        public void SendMail(string mail)
        {
            var from = "username";
            var to = mail;
            var subject = "Test Mail";
            var body = "";


            var username = "username"; //get from Mailtrap
            var password = "password"; //get from Mailtrap

            var host = "smtp.office365.com";
            var port = 587;

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            client.Send(from,to,subject, body);

        }


    }
}
