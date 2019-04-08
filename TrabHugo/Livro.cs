using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrabHugo
{
    class Livro
    {
        private int cont = 0;
        private String nome;

        public string Nome { get => nome; set => nome = value; }

        private Transacoes trans = new Transacoes();

        public Livro()
        {
            this.nome = "Livro.txt";
            this.cont = contar();
        }

        public Livro(String nome)
        {
            this.nome = nome;
            this.cont = contar();
        }

        // LE TODAS AS OFERTAS
        public List<String> ler()
        {
            List<String> ofertas = new List<String>();
            try
            {
                string[] linha = File.ReadAllLines(this.nome);
                for (int i = 0; i < cont; i++)
                {
                    ofertas.Add(linha[i]);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo não existe!");
            }
            return ofertas;
        }

        // LE UMA OFERTA
        public String ler(int fila)
        {
            String linha = null;
            try
            {
                string[] texto = File.ReadAllLines(this.nome);
                linha = texto[fila];
            }
            catch (IOException e)
            {
                Console.WriteLine("Arquivo não existe!");
            }
            return linha;
        }

        // CONTA TODAS AS OFERTAS
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

        // GRAVA UMA OFERTA
        public void gravar(String line)
        {
            try
            {
                File.AppendAllText(this.nome, line);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Arquivo não existe!");
            }
            checarTransacao(line);
        }

        // REGRAVA AS OFERTAS
        public void regravar(String fila, int pos)
        {
            try
            {
                string[] arrLine = File.ReadAllLines(this.nome);
                arrLine[pos - 1] = fila;
                File.WriteAllLines(this.nome, arrLine);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        // DELETA OFERTAS
        public void deletar()
        {
            try
            {
                string[] arrLine = File.ReadAllLines(this.nome);
                int deletados = 0;

                String linha = arrLine[0];

                for (int i = 0; i < cont; i++)
                {
                    if (linha != "deletado")
                    {
                        arrLine[i] = linha;
                    }
                    else
                    {
                        deletados++;
                    }
                    linha = arrLine[i];
                }
                File.WriteAllLines(this.nome, arrLine); ;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        // CONSULTA UMA TRANSAÇÃO
        public void consultar(String consulta)
        {
            try
            {
                String[] par = consulta.Split(";");
                String resultado = trans.ler(par[2], par[1]);
                String[] topicos = null;
                if (resultado != "")
                {
                    resultado = par[0] + "." + par[1] + "," + resultado;
                    topicos = resultado.Split(",");
                }
                else
                {
                    resultado += par[0] + "." + par[1];
                    topicos = new String[] { resultado };
                }
                Bolsa.enviar(topicos);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        // CHECA SE UMA TRANSAÇÃO É POSSÍVEL 
        public void checarTransacao(String oferta)
        {
            bool alterar = false;
            String[] atributo = oferta.Split(";");
            List<String> ofertas = ler();
            foreach (String var in ofertas)
            {
                int pos = ofertas.IndexOf(var);
                int end = ofertas.IndexOf(oferta);
                String[] ofert = var.Split(";");
                if (ofert[1] == atributo[1])
                {
                    if (ofert[0] == "venda" && atributo[0] == "compra" && (float.Parse(ofert[3]) <= float.Parse(atributo[3])))
                    {
                        alterar = true;
                        if (int.Parse(ofert[2]) > int.Parse(atributo[2]))
                        {
                            int resp = int.Parse(ofert[2]) - int.Parse(atributo[2]);
                            ofert[2] = resp.ToString();
                            String novaoferta = justaStrings(ofert, ";", 0);
                            regravar(novaoferta, pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + atributo[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                            break;
                        }
                        else if (int.Parse(ofert[2]) < int.Parse(atributo[2]))
                        {
                            int resp = int.Parse(atributo[2]) - int.Parse(ofert[2]);
                            atributo[2] = resp.ToString();
                            String oferter = justaStrings(atributo, ";", 0);
                            regravar(oferter, end);
                            regravar("deletado", pos);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + ofert[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                        }
                        else
                        {
                            regravar("deletado", pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + atributo[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                            break;
                        }
                    }
                    else if (ofert[0] == "compra" && atributo[0] == "venda" && (float.Parse(ofert[3]) >= float.Parse(atributo[3])))
                    {
                        alterar = true;
                        if (int.Parse(ofert[2]) > int.Parse(atributo[2]))
                        {
                            int resp = int.Parse(ofert[2]) - int.Parse(atributo[2]);
                            ofert[2] = resp.ToString();
                            String novaoferta = justaStrings(ofert, ";", 0);
                            regravar(novaoferta, pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + atributo[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                            break;
                        }
                        else if (int.Parse(ofert[2]) < int.Parse(atributo[2]))
                        {
                            int resp = int.Parse(atributo[2]) - int.Parse(ofert[2]);
                            atributo[2] = resp.ToString();
                            String oferter = justaStrings(atributo, ";", 0);
                            regravar(oferter, end);
                            regravar("deletado", pos);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + ofert[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                        }
                        else
                        {
                            regravar("deletado", pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + ofert[1] + ";" + atributo[2] + ";" + ofert[3] + ";" + atributo[4] + ";"
                                    + ofert[4];
                            trans.gravar(transacao);
                            break;
                        }
                    }
                }
            }
            if (alterar)
            {
                deletar();
            }
        }

        // MÉTODO AUXILIAR
        private String justaStrings(String[] strings, String delimitador, int startIndex)
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

        // MÉTODO AUXILIAR
        private String getDateTime()
        {
            DateTime date = new DateTime();
            return date.ToShortDateString();
        }
    }
}
