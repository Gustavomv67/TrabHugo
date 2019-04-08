using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TrabHugo
{
    class Broker
    {
        private static string EXCHANGE = "BOLSADEVALORES";
        private static String codigo = null;

        static public void Main()
        {
            int opcao = 0, quant = 0;
            float valor = 0;
            string data = null, nome = null, hora = null, opera = null, lixo = null;
            Arquivo arq = new Arquivo();

            Console.WriteLine("Digite o nome do Broker: ");
            nome = Console.ReadLine();
            do
            {
                Console.WriteLine("Qual operação deseja realizar?");
                Console.WriteLine("1. Monitorar Ações");
                Console.WriteLine("2. Comprar");
                Console.WriteLine("3. Vender");
                Console.WriteLine("4. Informação");
                Console.WriteLine("0. Sair");
                opcao = int.Parse(Console.ReadLine());

                switch (opcao)
                {
                    case 1:
                        lixo = Console.ReadLine();
                        arq.ler();
                        Console.WriteLine("\nDigite o código da ação: ");
                        codigo = Console.ReadLine();
                        while (!arq.existe(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        Console.WriteLine("\nDigite a operação (compra, venda, transacao, *): ");
                        opera = Console.ReadLine();
                        while (opera != "compra" && opera != "venda" && opera != "transacao" && opera == "*")
                        {
                            Console.WriteLine("\nEssa operação não existe, digite outro: ");
                            opera = Console.ReadLine();
                        }
                        codigo = opera + "." + codigo;
                        new Thread(thread).Start();
                        break;
                    case 2:
                        lixo = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação: ");
                        codigo = Console.ReadLine();
                        while (!arq.existe(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "compra." + codigo;

                        Console.WriteLine("\nDigite a quantidade: ");
                        quant = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nDigite o valor das ações:");
                        valor = float.Parse(Console.ReadLine());

                        enviar(new String[] { codigo, quant.ToString(), valor.ToString(), nome });
                        break;
                    case 3:
                        lixo = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação: ");
                        codigo = Console.ReadLine();
                        while (!arq.existe(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "venda." + codigo;

                        Console.WriteLine("\nDigite a quantidade: ");
                        quant = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nDigite o valor das ações: ");
                        valor = float.Parse(Console.ReadLine());

                        enviar(new String[] { codigo, quant.ToString(), valor.ToString(), nome });
                        break;
                    case 4:
                        lixo = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação: ");
                        codigo = Console.ReadLine();
                        while (!arq.existe(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "info." + codigo;

                        Console.WriteLine("\nDigite a data (dd/mm/aa): ");
                        data = Console.ReadLine();

                        Console.WriteLine("\nDigite a hora (hh:mm): ");
                        hora = Console.ReadLine();

                        String datahora = data + " " + hora;
                        enviar(new String[] { codigo, datahora });
                        break;
                    default:
                        break;
                }


            } while (opcao != 0);
        }

        // RECEBE NOTIFICAÇÕES
        public static void receber(String[] topicos)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(EXCHANGE, "topic");
            string queueName = channel.QueueDeclare().QueueName;


            if (topicos.Length < 1)
            {
                Console.Error.WriteLine("Erro");
                Environment.Exit(1);
            }

            foreach (String bindingKey in topicos)
            {
                channel.QueueBind(queueName, EXCHANGE, bindingKey);
            }

            Console.WriteLine("MONITORANDO. Para parar pressione CTRL+C");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                if (!codigo.Contains("info") && ea.RoutingKey.Contains("info"))
                {

                }
                else
                {
                    Console.WriteLine("Recebida: '" + ea.RoutingKey + "_<" + message + ">'");
                    codigo = "";
                }
            };
            channel.BasicConsume(queue: "BROKER",
                                 autoAck: true,
                                 consumer: consumer);
        }

        // REQUISITA OPERAÇÕES
        public static void enviar(String[] operacao)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare("BROKER", true, false, false, null);

            string message = "";
            if (operacao.Length < 1)
                message = "Anonimo";
            else
                message = operacao[0];

            if (operacao.Length < 2)
                message += "_" + "Nada";
            else
                message += "_" + juntasStrings(operacao, ";", 1);
            channel.BasicPublish("", "BROKER", null, Encoding.UTF8.GetBytes(message));

            Console.WriteLine("Enviada: '<" + message + ">'");
        }

        // AUXILIAR
        private static string juntasStrings(String[] strings, String delimitador, int startIndex)
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

        private static void thread()
        {
            try
            {
                receber(new String[] { codigo });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
