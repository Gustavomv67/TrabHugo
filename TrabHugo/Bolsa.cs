using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace TrabHugo
{
    class Bolsa
    {
        private static string EXCHANGE_NAME = "BOLSADEVALORES";
        private Livro livro = new Livro();

        static public void Main(String[] args)
        {
            string trash = null;
            receberOperacao();
        }
        public static void enviarNotificacoes(String[] topicos)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(EXCHANGE_NAME, "topic");
            String routingKey = getRouting(topicos);
            String message = getMessage(topicos);
            channel.BasicPublish(EXCHANGE_NAME, routingKey, null, Encoding.UTF8.GetBytes(message));
            Console.WriteLine(" [x] Enviada -> '" + routingKey + "_<" + message + ">'");
        }

        private static String getRouting(String[] strings)
        {
            if (strings.Length < 1)
                return "anonymous.info";
            return strings[0];
        }

        private static String getMessage(String[] strings)
        {
            if (strings.Length < 2)
                return "Nenhuma";
            return joinStrings(strings, ";", 1);
        }

        private static String joinStrings(String[] strings, String delimiter, int startIndex)
        {
            int length = strings.Length;
            if (length == 0)
                return "";
            if (length < startIndex)
                return "";
            StringBuilder words = new StringBuilder(strings[startIndex]);
            for (int i = startIndex + 1; i < length; i++)
            {
                words.Append(delimiter).Append(strings[i]);
            }
            return words.ToString();
        }

        // RECEBE AS OPERAÇÕES DOS BROKERS
        public static void receberOperacao()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare("BROKER", true, false, false, null);
            Console.WriteLine(" [*] Servidor ONLINE. Para parar pressione CTRL+C");
            //channel.BasicQos(1000, 1, true);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                String[] topic = message.Split("_");
                if (topic.Length > 1)
                {
                    Console.WriteLine(" [x] Recebida -> '<" + message + ">'");
                    try
                    {
                        doWork(topic);
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
                                 consumer: consumer);
        }

        // AUXILIAR
        private static void doWork(String[] topic)
        {
            if (topic[0].Contains("info"))
            {
                consultarTransacao(topic[0], topic[1]);
            }
            else
            {
                try
                {
                    enviarNotificacoes(topic);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                registrarOperacao(topic[0], topic[1]);
            }
        }

        // MÉTODO AUXILIAR
        private static void registrarOperacao(String topico, String oferta)
        {
            topico = topico.Replace(".", ";");
            String operacao = topico + ";" + oferta;
            Livro livro = new Livro();
            livro.gravar(operacao);
        }

        // MÉTODO AUXILIAR
        private static void consultarTransacao(String topico, String consulta)
        {
            topico = topico.Replace(".", ";");
            String con = topico + ";" + consulta;
            Livro livro = new Livro();
            livro.consultarTransacao(con);
        }
    }
}
