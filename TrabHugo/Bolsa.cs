using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace TrabHugo
{
    class Bolsa
    {
        private static string EXCHANGE = "BOLSADEVALORES";
        private Livro livro = new Livro();

        static public void Main()
        {
            receber();
        }
        public static void enviar(String[] topicos)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(EXCHANGE, "topic");
            string routingKey = "";
            string message = "";
            if (topicos.Length < 1)
                routingKey = "Anonimo";
            else
                routingKey = topicos[0];

            if (topicos.Length < 2)
                message = "Nada";
            else
                message = juntarStrings(topicos, ";", 1);
            channel.BasicPublish(EXCHANGE, routingKey, null, Encoding.UTF8.GetBytes(message));
            Console.WriteLine("Enviada: '" + routingKey + " <" + message + ">'");
        }

        private static String juntarStrings(String[] strings, String delimitador, int startIndex)
        {
            int length = strings.Length;
            if (length == 0)
                return "";
            if (length < startIndex)
                return "";
            StringBuilder words = new StringBuilder(strings[startIndex]);
            for (int i = startIndex + 1; i < length; i++)
            {
                words.Append(delimitador).Append(strings[i]);
            }
            return words.ToString();
        }

        // RECEBE AS OPERAÇÕES DOS BROKERS
        public static void receber()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("BROKER", true, false, false, null);
            Console.WriteLine("ONLINE. Para parar pressione CTRL+C");
            var consumidor = new EventingBasicConsumer(channel);
            consumidor.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                String[] topic = message.Split("_");
                if (topic.Length > 1)
                {
                    Console.WriteLine("Recebida: '<" + message + ">'");
                    try
                    {
                        if (topic[0].Contains("info"))
                        {
                            consultar(topic[0], topic[1]);
                        }
                        else
                        {
                            try
                            {
                                enviar(topic);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.StackTrace);
                            }
                            registrar(topic[0], topic[1]);
                        }
                    }
                    finally
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                else
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }
            };
            channel.BasicConsume(queue: "BROKER",
                                 autoAck: true,
                                 consumer: consumidor);
        }

        private static void registrar(String topico, String oferta)
        {
            topico = topico.Replace(".", ";");
            String operacao = topico + ";" + oferta;
            Livro livro = new Livro();
            livro.gravar(operacao);
        }

        private static void consultar(String topico, String consulta)
        {
            topico = topico.Replace(".", ";");
            String con = topico + ";" + consulta;
            Livro livro = new Livro();
            livro.consultar(con);
        }
    }
}
