using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrabHugo
{
    class Arquivo
    {
        private String nome;
        private int cont = 0;

        public string Nome { get => nome; set => nome = value; }

        public Arquivo()
        {
            this.nome = "bolsa.txt";
            this.cont = contarAcoes();
        }

        public Arquivo(String nome)
        {
            this.nome = nome;
            this.cont = contarAcoes();
        }

        // CONTA TODAS AS AÇÕES DO REGISTRO
        public int contarAcoes()
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

        // LE TODAS AS AÇÕES DO REGISTRO
        public void ler()
        {
            try
            {
                string[] linha = File.ReadAllLines(this.nome);
                for(int i = 0; i < linha.Length; i++)
                {
                    Console.WriteLine(linha[i]);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo nÃ£o existe!");
            }
        }

        // VERIFICA SE UMA AÇÃO EXISTE NO REGISTRO
        public bool existeAcao(String codigo)
        {
            bool resultado = false;
            try
            {
                string[] linha = File.ReadAllLines(this.nome);
                for (int i = 0; i < cont; i++)
                {
                    String[] trans = linha[i].Split(";");
                    if (trans[1] == codigo)
                    {
                        resultado = true;
                        break;
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo nÃ£o existe!");
            }
            return resultado;
        }

    }
}
