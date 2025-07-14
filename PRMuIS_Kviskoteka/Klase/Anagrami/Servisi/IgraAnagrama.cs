using Klase.Anagrami.Enumeracije;
using Klase.Anagrami.Modeli;
using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Anagrami.Servisi
{
    public class IgraAnagrama
    {
        public Anagram anagram;
        private Igrac igrac1, igrac2;
        private int bodovi = 0;
        public IgraAnagrama(Igrac igrac)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\Anagrami\\anagram{new Random().Next(1, 9)}.txt";
            string anagramTxt = string.Empty;
            try
            {
                anagramTxt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla. \n {ex}");
            }
            anagram = new Anagram(anagramTxt);
            igrac1 = igrac;

        }
        public IgraAnagrama(Igrac igrac1, Igrac igrac2)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\Anagrami\\anagram{new Random().Next(1, 9)}.txt";
            string anagramTxt = string.Empty;
            try
            {
                anagramTxt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla. \n {ex}");
            }
            anagram = new Anagram(anagramTxt);
            this.igrac1 = igrac1;
            this.igrac2 = igrac2;

        }

        public void treningIgra(Socket client)
        {
            byte[] binarnaPoruka;

            while (anagram.getPogodjene() < anagram.ponudjeneReci.Count)
            {
                Console.WriteLine("TRENING");
                Console.WriteLine("Poeni: " + igrac1.poeniUTrenutnojIgri);
                Console.WriteLine("Pogodjene reci: " + anagram.getPogodjene() + "/" + anagram.ponudjeneReci.Count);
                Console.WriteLine("PONUDJENA REC: " + anagram.REC);
                byte[] buffer = new byte[1024];
                int brojBajta = client.Receive(buffer);
                string rec = Encoding.UTF8.GetString(buffer, 0, brojBajta);
                Console.Write("Primljena rec: " + rec);
                Console.WriteLine();
                Thread.Sleep(1000);

                //string rec = Console.ReadLine().Trim().ToLower();

                if (rec == "izlaz")
                {
                    anagram.endGame();
                    Console.Clear();
                    break;
                }

                PovratneVrednostiAnagrama pv = anagram.postojiRec(rec);

                if (pv == PovratneVrednostiAnagrama.IspravnaRec)
                {
                    igrac1.poeniUTrenutnojIgri += rec.Length;
                    Console.Clear();
                    if (anagram.getPogodjene() < anagram.ponudjeneReci.Count)
                    {
                        binarnaPoruka = Encoding.UTF8.GetBytes(rec);
                        client.Send(binarnaPoruka);
                    }
                    else
                    {
                        binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
                        client.Send(binarnaPoruka);
                        return;
                    }
                        continue;
                }



                if (pv == PovratneVrednostiAnagrama.NePostojiSlovo)
                    Console.WriteLine("Vasa rec koristi slovo koje ne postoji u ponudjenoj reci / ponudjenim recima.");
                else if (pv == PovratneVrednostiAnagrama.PreviseSlova)
                    Console.WriteLine("Vasa rec koristi vise slova nego sto postoji u ponudjenoj reci / ponudjenim recima.");
                else if (pv == PovratneVrednostiAnagrama.VecPogodjeno)
                    Console.WriteLine("Uneta rec je vec pogodjena.");
                else if (pv == PovratneVrednostiAnagrama.NeispravnaRec)
                    Console.WriteLine("Uneta rec nije ispravna.");

                binarnaPoruka = Encoding.UTF8.GetBytes(rec);
                client.Send(binarnaPoruka);

                Thread.Sleep(1000);
                Console.Clear();
            }

            binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
            client.Send(binarnaPoruka);

        }


        public void Igraj(List<Socket> klijenti, Socket serverSocket)
        {

            byte[] buffer = new byte[1024];
            string rec = string.Empty;
            Igrac trenutniIgrac = null;
            Socket trenutniSocket = null;
            StringBuilder sb = new StringBuilder();
            int izaslaOba = 0;
            while (izaslaOba != 2) 
            {
                try
                {

                    rec = string.Empty;
                    Console.WriteLine("Poeni: ");
                    Console.WriteLine($"\t{igrac1.username}: {igrac1.poeniUTrenutnojIgri} ");
                    Console.WriteLine($"\t{igrac2.username}: {igrac2.poeniUTrenutnojIgri} ");
                    Console.WriteLine("PONUDJENA REC: " + anagram.REC);
                    Console.WriteLine();
                    Console.WriteLine(sb.ToString());
                    do
                    {
                        List<Socket> checkRead = new List<Socket>();
                        List<Socket> checkError = new List<Socket>();

                        foreach (Socket s in klijenti)
                        {
                            checkRead.Add(s);
                            checkError.Add(s);
                        }

                        Socket.Select(checkRead, null, checkError, 1000);

                        if (checkRead.Count > 0)
                        {

                            foreach (Socket s in checkRead)
                            {
                                {
                                    int brBajta = s.Receive(buffer);
                                    rec = Encoding.UTF8.GetString(buffer, 0, brBajta);
                                    trenutniSocket = s;
                                    trenutniIgrac = s == klijenti[0] ? igrac1 : igrac2;
                                }
                            }
                        }
                    }
                    while (rec == string.Empty);

                    sb.Append($"{trenutniIgrac.username}: {rec}\n");
                    Console.WriteLine($"{trenutniIgrac.username}: {rec}\n");

                    PovratneVrednostiAnagrama pv = anagram.postojiRec(rec);

                    if (pv == PovratneVrednostiAnagrama.IspravnaRec)
                         trenutniIgrac.poeniUTrenutnojIgri += rec.Length;
                    else if(pv == PovratneVrednostiAnagrama.DrugiIgracPogodio)
                        trenutniIgrac.poeniUTrenutnojIgri += Convert.ToInt32(Math.Floor(rec.Length * 0.8));
                    else if (pv == PovratneVrednostiAnagrama.NePostojiSlovo)
                        Console.WriteLine("Vasa rec koristi slovo koje ne postoji u ponudjenoj reci / ponudjenim recima.");
                    else if (pv == PovratneVrednostiAnagrama.PreviseSlova)
                        Console.WriteLine("Vasa rec koristi vise slova nego sto postoji u ponudjenoj reci / ponudjenim recima.");
                    else if (pv == PovratneVrednostiAnagrama.VecPogodjeno)
                        Console.WriteLine("Uneta rec je vec pogodjena.");
                    else if (pv == PovratneVrednostiAnagrama.NeispravnaRec)
                        Console.WriteLine("Uneta rec nije ispravna.");

                    if (pv != PovratneVrednostiAnagrama.IspravnaRec && pv != PovratneVrednostiAnagrama.DrugiIgracPogodio)
                        trenutniIgrac.addPenalty();

                    if (trenutniIgrac.getPenalties() == 3 || rec == "izlaz")
                    {
                        trenutniSocket.Send(Encoding.UTF8.GetBytes("izlaz"));
                        izaslaOba++;
                    }
                    else
                        trenutniSocket.Send(Encoding.UTF8.GetBytes(rec));
                    


                    Thread.Sleep(1000);
                    Console.Clear();
                }

                catch (Exception e) { }
            }

            
        }
    }
}


