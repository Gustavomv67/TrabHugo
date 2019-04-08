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
            this.cont = contar();
        }

        public Arquivo(String nome)
        {
            this.nome = nome;
            this.cont = contar();
        }

        // CONTA TODAS AS AÇÕES
        public int contar()
        {
            string[] linhas = { "" };
            try
            {
                linhas = File.ReadAllLines(this.nome);
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo não existe!");
            }
            return linhas.Length;
        }

        // LE TODAS AS AÇÕES
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
                Console.WriteLine("Arquivo não existe!");
            }
        }

        // VERIFICA SE UMA AÇÃO EXISTE 
        public bool existe(String codigo)
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
                Console.WriteLine("Arquivo não existe!");
            }
            return resultado;
        }

    }
}
