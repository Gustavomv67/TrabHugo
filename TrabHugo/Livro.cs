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
            this.cont = contaOfertas();
        }

        public Livro(String nome)
        {
            this.nome = nome;
            this.cont = contaOfertas();
        }

        // LE TODAS AS OFERTAS DO REGISTRO
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

        // LE UMA OFERTA ESPECIFICA DO RESGISTRO
        public String lerOferta(int fila)
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

        // CONTA TODAS AS OFERTAS DO REGISTRO
        public int contaOfertas()
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

        // GRAVA UMA OFERTA NO REGISTRO
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
            checarTransacao(line);
        }

        // REGRAVA AS OFERTAS DO REGISTRO, DESCONSIDERANDO UMA OFERTA ESPECIFICA
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

        // DELETA OFERTAS MARCADAS PARA DELETAR DO REGISTRO
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

        // CONSULTA UMA TRANSAÇÃO DO REGISTRO DE TRANSAÇÕES
        public void consultarTransacao(String consulta)
        {
            try
            {
                String[] con = consulta.Split(";");
                String resultado = trans.lerTransacao(con[2], con[1]);
                String[] topicos = null;
                if (resultado != "")
                {
                    resultado = con[0] + "." + con[1] + "," + resultado;
                    topicos = resultado.Split(",");
                }
                else
                {
                    resultado += con[0] + "." + con[1];
                    topicos = new String[] { resultado };
                }
                Bolsa.enviarNotificacoes(topicos);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        // CHECA SE UMA TRANSAÇÃO É POSSÍVEL E REALIZA ELA, ARMAZENANDO NO REGISTRO DE
        // TRANSAÇÕES E ATUALIZANDO AS OFERTAS NO FINAL
        public void checarTransacao(String oferta)
        {
            bool alteracao = false;
            String[] atrib = oferta.Split(";");
            List<String> ofertas = ler();
            foreach (String o in ofertas)
            {
                int pos = ofertas.IndexOf(o);
                int end = ofertas.IndexOf(oferta);
                String[] of = o.Split(";");
                if (of[1] == atrib[1])
                {
                    if (of[0] == "venda" && atrib[0] == "compra" && (float.Parse(of[3]) <= float.Parse(atrib[3])))
                    {
                        alteracao = true;
                        if (int.Parse(of[2]) > int.Parse(atrib[2]))
                        {
                            int resp = int.Parse(of[2]) - int.Parse(atrib[2]);
                            of[2] = resp.ToString();
                            String novaoferta = joinStrings(of, ";", 0);
                            regravar(novaoferta, pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + atrib[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                            break;
                        }
                        else if (int.Parse(of[2]) < int.Parse(atrib[2]))
                        {
                            int resp = int.Parse(atrib[2]) - int.Parse(of[2]);
                            atrib[2] = resp.ToString();
                            String ofer = joinStrings(atrib, ";", 0);
                            regravar(ofer, end);
                            regravar("deletado", pos);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + of[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                        }
                        else
                        {
                            regravar("deletado", pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + atrib[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                            break;
                        }
                    }
                    else if (of[0] == "compra" && atrib[0] == "venda" && (float.Parse(of[3]) >= float.Parse(atrib[3])))
                    {
                        alteracao = true;
                        if (int.Parse(of[2]) > int.Parse(atrib[2]))
                        {
                            int resp = int.Parse(of[2]) - int.Parse(atrib[2]);
                            of[2] = resp.ToString();
                            String novaoferta = joinStrings(of, ";", 0);
                            regravar(novaoferta, pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + atrib[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                            break;
                        }
                        else if (int.Parse(of[2]) < int.Parse(atrib[2]))
                        {
                            int resp = int.Parse(atrib[2]) - int.Parse(of[2]);
                            atrib[2] = resp.ToString();
                            String ofer = joinStrings(atrib, ";", 0);
                            regravar(ofer, end);
                            regravar("deletado", pos);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + of[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                        }
                        else
                        {
                            regravar("deletado", pos);
                            regravar("deletado", end);
                            String data = getDateTime();
                            String transacao = data + ";" + of[1] + ";" + atrib[2] + ";" + of[3] + ";" + atrib[4] + ";"
                                    + of[4];
                            trans.gravar(transacao);
                            break;
                        }
                    }
                }
            }
            if (alteracao)
            {
                deletar();
            }
        }

        // MÉTODO AUXILIAR
        private String joinStrings(String[] strings, String delimiter, int startIndex)
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

        // MÉTODO AUXILIAR
        private String getDateTime()
        {
            DateTime date = new DateTime();
            return date.ToShortDateString();
        }
    }
}
