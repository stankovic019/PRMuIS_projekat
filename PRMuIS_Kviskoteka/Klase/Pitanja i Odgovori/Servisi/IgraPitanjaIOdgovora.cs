using Klase.Anagrami.Modeli;
using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using Klase.Pitanja_i_Odgovori.Modeli;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klase.Pitanja_i_Odgovori.Servisi
{
    public class IgraPitanjaIOdgovora
    {
        public PitanjaIOdgovori pitanjaIOdgr = new PitanjaIOdgovori();
        public PitanjaIOdgovori igra;
        private Igrac igrac1, igrac2;
        private int indeksTrenutnogPitanja = 0;
        private List<string> pitanjaRedosled = new List<string>();

        public IgraPitanjaIOdgovora(Igrac igrac)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\PitanjaIOdgovori\\pitanja.txt";
            string pitanja_txt = string.Empty;
            try
            {
                pitanja_txt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla. \n {ex}");
            }
            //igra = new PitanjaIOdgovori();
            UcitajPitanja(pitanja_txt);
            igrac1 = igrac;

        }
        public IgraPitanjaIOdgovora(Igrac igrac1, Igrac igrac2)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\PitanjaIOdgovori\\pitanja.txt";
            string pitanjaTxt = string.Empty;
            try
            {
                pitanjaTxt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla. \n {ex}");
            }
            //igra = new PitanjaIOdgovori(pitanjaTxt);
            UcitajPitanja(pitanjaTxt);
            this.igrac1 = igrac1;
            this.igrac2 = igrac2;

        }

        public IgraPitanjaIOdgovora()
        {

            try
            {
                UcitajPitanja("pitanja.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška prilikom učitavanja pitanja: " + ex.Message);
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
                return;
            }
        }

        private int poeni = 0;
        private const int poeniPoTacnom = 4;

        private int maksimalniPoeni = poeniPoTacnom * 5;

        public void UcitajPitanja(string putanjaDoFajla)
        {
            string[] linije = putanjaDoFajla.Split("\r\n");
            pitanjaIOdgr.SvaPitanja.Clear();

            foreach (var linija in linije)
            {
                if (string.IsNullOrWhiteSpace(linija))
                    continue;

                var delovi = linija.Split('|');
                if (delovi.Length != 2)
                    throw new FormatException("Svaka linija mora imati format: pitanje|a ili b");

                var pitanje = delovi[0].Trim();
                var odgovorSlovo = delovi[1].Trim().ToLower();

                if (odgovorSlovo != "a" && odgovorSlovo != "b")
                    throw new FormatException("Odgovor mora biti 'a' ili 'b'");

                bool tacan = odgovorSlovo == "a"; // 'a' = DA = true, 'b' = NE = false
                pitanjaIOdgr.SvaPitanja.Add(pitanje, tacan);
            }

            IzaberiIPromesajPitanja();
           
        }

        private void IzaberiIPromesajPitanja()
        {
            var rnd = new Random();
            var svaPitanjaLista = pitanjaIOdgr.SvaPitanja.Keys.ToList();

            if (svaPitanjaLista.Count > 5)
            {
                do
                {
                    pitanjaRedosled = svaPitanjaLista.OrderBy(_ => rnd.Next()).Take(5).ToList();
                }
                while (ImaTriIstaOdgovoraZaredom(pitanjaRedosled));
            }
            else
            {
                do
                {
                    pitanjaRedosled = svaPitanjaLista.OrderBy(_ => rnd.Next()).ToList();
                }
                while (ImaTriIstaOdgovoraZaredom(pitanjaRedosled));
            }
        }

        private bool ImaTriIstaOdgovoraZaredom(List<string> pitanja)
        {
            for (int i = 2; i < pitanja.Count; i++)
            {
                bool o1 = pitanjaIOdgr.SvaPitanja[pitanja[i - 2]];
                bool o2 = pitanjaIOdgr.SvaPitanja[pitanja[i - 1]];
                bool o3 = pitanjaIOdgr.SvaPitanja[pitanja[i]];

                if (o1 == o2 && o2 == o3)
                    return true;
            }
            return false;
        }

        public bool PostaviSledecePitanje()
        {
            
            if (indeksTrenutnogPitanja == pitanjaRedosled.Count)
                return false;

            pitanjaIOdgr.TekucePitanje = pitanjaRedosled[indeksTrenutnogPitanja];
            pitanjaIOdgr.TacanOdgovor = pitanjaIOdgr.SvaPitanja[pitanjaIOdgr.TekucePitanje];
            return true;
        }

        public bool ProveriOdgovor(string odgovorKlijenta)
        {
            odgovorKlijenta = odgovorKlijenta.ToLower();
            if (odgovorKlijenta[0] != 'a' && odgovorKlijenta[0] != 'b')
                throw new ArgumentException("Odgovor mora biti 'a' ili 'b'.");

            bool odgovorJeTacan = (odgovorKlijenta[0] == 'a');
            return odgovorJeTacan == pitanjaIOdgr.TacanOdgovor;
        }

        public void Igraj(Socket client)
        {
            byte[] binarnaPoruka;
            while (PostaviSledecePitanje())
            {
                Console.WriteLine("Pitanje: " + pitanjaIOdgr.TekucePitanje);
                Console.WriteLine("a) DA");
                Console.WriteLine("b) NE");
                byte[] buffer = new byte[1024];
                int brojBajta = client.Receive(buffer);
                string unos = Encoding.UTF8.GetString(buffer, 0, brojBajta);
                Console.Write("Uneti odgovor: " + unos);
                Thread.Sleep(1000);
              

                if (unos.ToLower() == "izlaz")
                {
                    binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
                    client.Send(binarnaPoruka);
                    Console.WriteLine();
                    break;
                }
                Console.WriteLine();

                try
                {
                    if (ProveriOdgovor(unos))
                    {
                        Console.WriteLine("Tačno! + " + poeniPoTacnom + " poena\n");
                        igrac1.poeniUTrenutnojIgri += poeniPoTacnom;
                    }
                    else
                    {
                        Console.WriteLine("Netačno.\n");
                    }
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message + " Pitanje se ne računa.\n");
                }

                if(indeksTrenutnogPitanja+1 == 5)
                {
                    binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
                    client.Send(binarnaPoruka);
                    continue;
                }

                binarnaPoruka = Encoding.UTF8.GetBytes(unos);
                client.Send(binarnaPoruka);
                Thread.Sleep(1000);
                Console.Clear();

            }

            Thread.Sleep(1000);
            Console.Clear();
            //binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
            //client.Send(binarnaPoruka);
            //Console.WriteLine("Kraj igre! Ukupno poena: " + poeni + " od mogucih " + maksimalniPoeni);
            //Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
            //Console.ReadKey();
        }

        public void IgrajDvaIgraca(List<Socket> klijenti, Socket ServerSocket)
        {
            byte[] buffer = new byte[1024];

            while (PostaviSledecePitanje())
            {
                Array.Clear(buffer, 0, buffer.Length);
                Console.Clear();

                // Ispis poena i pitanja
                Console.WriteLine("Poeni:");
                Console.WriteLine($"\t{igrac1.username}: {igrac1.poeniUTrenutnojIgri}");
                Console.WriteLine($"\t{igrac2.username}: {igrac2.poeniUTrenutnojIgri}");
                Console.WriteLine("Pitanje: " + pitanjaIOdgr.TekucePitanje);
                Console.WriteLine("a) DA");
                Console.WriteLine("b) NE");

                // Čekaj odgovore oba klijenta i sakupljaj ih
                Dictionary<Socket, string> odgovori = new Dictionary<Socket, string>();
                while (odgovori.Count < klijenti.Count)
                {
                    List<Socket> readyList = new List<Socket>();

                    foreach (Socket s in klijenti)
                        readyList.Add(s);

                    Socket.Select(readyList, null, null, 1000000);

                    foreach (Socket s in readyList)
                    {
                            int brBajta = s.Receive(buffer);
                            string odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta).Trim().ToLower();
                            odgovori[s] = odgovor;
                        
                       
                            string potvrda = "Odgovor primljen. Sačekaj dok drugi igrač odgovori...";
                            s.Send(Encoding.UTF8.GetBytes(potvrda));
                    }
                }

                // Obrađuj odgovore i ispisuj rezultate tek posle oba odgovora
                foreach (var par in odgovori)
                {
                    Socket s = par.Key;
                    string odgovor = par.Value;
                    Igrac igrac = s == klijenti[0] ? igrac1 : igrac2;

                    if (ProveriOdgovor(odgovor))
                    {
                        Console.WriteLine($"{igrac.username} je odgovorio: TACNO! + {poeniPoTacnom} poena");
                        igrac.poeniUTrenutnojIgri += poeniPoTacnom;
                    }
                    else
                    {
                        Console.WriteLine($"{igrac.username} je odgovorio: NETACNO!");
                    }
                }

                if(indeksTrenutnogPitanja++ < 4)
                    foreach (Socket s in klijenti)
                        s.Send(Encoding.UTF8.GetBytes("continue"));
                

               Thread.Sleep(2000);
            }
            foreach (Socket s in klijenti)
                s.Send(Encoding.UTF8.GetBytes("izlaz"));

        }

    }
}