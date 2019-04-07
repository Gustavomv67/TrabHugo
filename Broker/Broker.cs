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
        private static string EXCHANGE_NAME = "BOLSADEVALORES";
        private static String codigo = null;

        static public void Main(String[] args)
        {
            int opcao = 0;
            int quant = 0;
            float valor = 0;
            String data = null;
            String nome = null;
            String hora = null;
            String opera = null;
            Arquivo arq = new Arquivo();
            String trash = null;

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
                        trash = Console.ReadLine();
                        arq.ler();
                        Console.WriteLine("\nDigite o código da ação a ser monitorada: ");
                        codigo = Console.ReadLine();
                        while (!arq.existeAcao(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe no sistema, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        Console.WriteLine("\nDigite a operação a ser monitorada (compra, venda, transacao, *): ");
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
                        trash = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação a ser comprada: ");
                        codigo = Console.ReadLine();
                        while (!arq.existeAcao(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe no sistema, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "compra." + codigo;

                        Console.WriteLine("\nDigite a quantidade desejada: ");
                        quant = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nDigite o valor de cada ação: ");
                        valor = float.Parse(Console.ReadLine());

                        enviarOperacao(new String[] { codigo, quant.ToString(), valor.ToString(), nome });
                        break;
                    case 3:
                        trash = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação a ser vendida: ");
                        codigo = Console.ReadLine();
                        while (!arq.existeAcao(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe no sistema, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "venda." + codigo;

                        Console.WriteLine("\nDigite a quantidade desejada: ");
                        quant = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nDigite o valor de cada ação: ");
                        valor = float.Parse(Console.ReadLine());

                        enviarOperacao(new String[] { codigo, quant.ToString(), valor.ToString(), nome });
                        break;
                    case 4:
                        trash = Console.ReadLine();
                        arq.ler();

                        Console.WriteLine("\nDigite o código da ação da qual deseja informações: ");
                        codigo = Console.ReadLine();
                        while (!arq.existeAcao(codigo))
                        {
                            Console.WriteLine("\nEsse código não existe no sistema, digite outro: ");
                            codigo = Console.ReadLine();
                        }
                        codigo = "info." + codigo;

                        Console.WriteLine("\nDigite a data (dd/mm/aa): ");
                        data = Console.ReadLine();

                        Console.WriteLine("\nDigite a hora (hh:mm): ");
                        hora = Console.ReadLine();

                        String datahora = data + " " + hora;
                        enviarOperacao(new String[] { codigo, datahora });
                        break;
                    default:
                        break;
                }


            } while (opcao != 0);
        }

        // RECEBE NOTIFICAÇÕES DA BOLSA PERTINENTE AS AÇÕES DESEJADAS
        public static void receberNotificacoes(String[] topicos)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(EXCHANGE_NAME, "topic");
            string queueName = channel.QueueDeclare().QueueName;


            if (topicos.Length < 1)
            {
                Console.Error.WriteLine("Usage: ReceiveLogsTopic [binding_key]...");
                Environment.Exit(1);
            }

            foreach (String bindingKey in topicos)
            {
                channel.QueueBind(queueName, EXCHANGE_NAME, bindingKey);
            }

            Console.WriteLine(" [*] MONITORANDO. Para parar pressione CTRL+C");

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
                    Console.WriteLine(" [x] Recebida -> '" + ea.RoutingKey + "_<" + message + ">'");
                    codigo = "";
                }
            };
            channel.BasicConsume(queue: "BROKER",
                                 autoAck: true,
                                 consumer: consumer);
        }

        // REQUISITA OPERAÇÕES PARA A BOLSA
        public static void enviarOperacao(String[] operacao)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare("BROKER", true, false, false, null);

            String message = getRouting(operacao);
            message += "_" + getMessage(operacao);

            channel.BasicPublish("", "BROKER", null, Encoding.UTF8.GetBytes(message));

            Console.WriteLine(" [x] Enviada -> '<" + message + ">'");
        }


        // AUXILIAR
        private static string getRouting(String[] strings)
        {
            if (strings.Length < 1)
                return "anonymous.info";
            return strings[0];
        }

        // AUXILIAR
        private static string getMessage(String[] strings)
        {
            if (strings.Length < 2)
                return "Nenhuma";
            return joinStrings(strings, ";", 1);
        }

        // AUXILIAR
        private static string joinStrings(String[] strings, String delimiter, int startIndex)
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

        private static void thread()
        {
            try
            {
                receberNotificacoes(new String[] { codigo });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
