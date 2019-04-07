using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrabHugo
{
    class Transacoes
    {
        private int cont = 0;
        private String nome;
        public string Nome { get => nome; set => nome = value; }

        public Transacoes()
        {
            this.nome = "Transacoes.txt";
            this.cont = contarTrans();
        }

        public Transacoes(String nome)
        {
            this.nome = nome;
            this.cont = contarTrans();
        }


        // CONTA TODAS AS TRANSAÇÕES DO REGISTRO
        public int contarTrans()
        {
            string[] linhas = { "" };
            try
            {
                linhas = File.ReadAllLines(this.nome);
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo nÃ£o existe!");
            }
            return linhas.Length;
        }

        // LE UMA TRANSAÇÃO ESPECIFICA DO REGISTRO
        public String lerTransacao(String data, String acao)
        {
            String resultado = "";
            try
            {
                string[] linhas = File.ReadAllLines(this.nome);
                for (int i = 0; i < cont; i++)
                {
                    String[] trans = linhas[i].Split(";");
                    if (trans[1] == acao && trans[0] == data)
                    {
                        if (i == cont - 1)
                        {
                            resultado += "{Transação " + (i + 1) + " = " + linhas[i] + "}";
                        }
                        else
                        {
                            resultado += "{Transação " + (i + 1) + " = " + linhas[i] + "},";
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo nÃ£o existe!");
            }
            return resultado;
        }

        // ENVIA A TRANSAÇÃO ATRAVÉS DA BOLSA DE VALORES AOS BROKERS INTERESSADOS
        public void publicarTransacao(String transacao)
        {
            try
            {
                String[] trans = transacao.Split(";");
                String message = trans[2] + ";" + trans[3];
                String topic = "transacao." + trans[1];
                Bolsa.enviarNotificacoes(new String[] { topic, message });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        // GRAVA UMA TRANSAÇÃO NO REGISTRO
        public void gravar(String line)
        {
            try
            {
                File.AppendAllText(this.nome, line);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Arquivo nÃ£o existe!");
            }
            publicarTransacao(line);
        }
    }
}
