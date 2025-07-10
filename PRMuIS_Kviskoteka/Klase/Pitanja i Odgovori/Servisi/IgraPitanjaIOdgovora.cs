using Klase.Anagrami.Modeli;
using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using Klase.Pitanja_i_Odgovori.Modeli;
using System;
using System.Collections.Generic;
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
            indeksTrenutnogPitanja = -1;
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
            indeksTrenutnogPitanja++;
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

            while (true)
            {
                bool imaSledecePitanje = PostaviSledecePitanje();
                if (!imaSledecePitanje)
                    break;  // Nema više pitanja, izlaz iz igre

                Console.Clear();

                // Ispis poena i pitanja
                Console.WriteLine("Poeni:");
                Console.WriteLine($"\t{igrac1.username}: {igrac1.poeniUTrenutnojIgri}");
                Console.WriteLine($"\t{igrac2.username}: {igrac2.poeniUTrenutnojIgri}");
                Console.WriteLine("Pitanje: " + pitanjaIOdgr.TekucePitanje);
                Console.WriteLine("a) DA");
                Console.WriteLine("b) NE");

                // Šalji pitanje oba klijenta
                byte[] pitanjeBajtovi = Encoding.UTF8.GetBytes($"Pitanje: {pitanjaIOdgr.TekucePitanje}\na) DA\nb) NE");
                foreach (var klijent in klijenti)
                {
                    klijent.Send(pitanjeBajtovi);
                }

                // Čekaj odgovore oba klijenta i sakupljaj ih
                Dictionary<Socket, string> odgovori = new Dictionary<Socket, string>();
                while (odgovori.Count < klijenti.Count)
                {
                    List<Socket> readyList = new List<Socket>(klijenti);
                    Socket.Select(readyList, null, null, 1000000);

                    foreach (var s in readyList)
                    {
                        if (!odgovori.ContainsKey(s))
                        {
                            int brBajta = s.Receive(buffer);
                            string odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta).Trim().ToLower();
                            odgovori[s] = odgovor;
                            string potvrda = "Odgovor primljen. Sačekaj dok drugi igrač odgovori...";
                            s.Send(Encoding.UTF8.GetBytes(potvrda));

                        }
                        else
                        {
                            if (s.Available > 0)
                                s.Receive(buffer);
                        }
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
                
                Thread.Sleep(2000);
            }
        }


        //public void IgrajDvaIgraca(List<Socket> klijenti, Socket ServerSocket)
        //{
        //    byte[] binarnaPoruka;
        //    byte[] buffer = new byte[1024];
        //    string rec = string.Empty;
        //    Igrac trenutniIgrac = null;
        //    Socket trenutniSocket = null;
        //    StringBuilder sb = new StringBuilder();
        //    int izaslaOba = 0;

        //    while (true)
        //    {
        //        bool imaSledecePitanje = PostaviSledecePitanje();
        //        if (!imaSledecePitanje)
        //            break;  // Nema više pitanja, izlaz iz igre

        //        Console.Clear();

        //        // Ispis poena i pitanja
        //        Console.WriteLine("Poeni:");
        //        Console.WriteLine($"\t{igrac1.username}: {igrac1.poeniUTrenutnojIgri}");
        //        Console.WriteLine($"\t{igrac2.username}: {igrac2.poeniUTrenutnojIgri}");
        //        Console.WriteLine("Pitanje: " + pitanjaIOdgr.TekucePitanje);
        //        Console.WriteLine("a) DA");
        //        Console.WriteLine("b) NE");

        //        // Šalji pitanje oba klijenta
        //        byte[] pitanjeBajtovi = Encoding.UTF8.GetBytes($"Pitanje: {pitanjaIOdgr.TekucePitanje}\na) DA\nb) NE");
        //        foreach (var klijent in klijenti)
        //        {
        //            klijent.Send(pitanjeBajtovi);
        //        }

        //        // Primaš odgovore oba klijenta, dodeljuj poene
        //        Dictionary<Socket, string> odgovori = new Dictionary<Socket, string>();

        //        while (odgovori.Count < klijenti.Count)
        //        {
        //            List<Socket> readyList = new List<Socket>(klijenti);
        //            Socket.Select(readyList, null, null, 1000000);

        //            foreach (var s in readyList)
        //            {
        //                if (!odgovori.ContainsKey(s))
        //                {
        //                    int brBajta = s.Receive(buffer);
        //                    string odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta).Trim().ToLower();

        //                    odgovori[s] = odgovor;

        //                    Igrac igrac = s == klijenti[0] ? igrac1 : igrac2;

        //                    if (ProveriOdgovor(odgovor))
        //                    {
        //                        Console.WriteLine($"{igrac.username} je  odgovorio: TACNO! + {poeniPoTacnom} poena");
        //                        igrac.poeniUTrenutnojIgri += poeniPoTacnom;
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"{igrac.username} je  odgovorio: NETACNO!");
        //                    }
        //                }
        //            }
        //        }

        //        Thread.Sleep(2000);
        //    }
        //}

        // ========================================================================================================================
        //while (PostaviSledecePitanje())
        //{
        //    Console.Clear();

        //    Console.WriteLine("Poeni:");
        //    Console.WriteLine($"\t{igrac1.username}: {igrac1.poeniUTrenutnojIgri}");
        //    Console.WriteLine($"\t{igrac2.username}: {igrac2.poeniUTrenutnojIgri}");
        //    Console.WriteLine("Pitanje: " + pitanjaIOdgr.TekucePitanje);
        //    Console.WriteLine("a) DA");
        //    Console.WriteLine("b) NE");
        //    while (izaslaOba != 2)
        //    {
        //        Dictionary<Socket, string> odgovori = new Dictionary<Socket, string>();

        //        while (odgovori.Count < 2)
        //        {
        //            List<Socket> ready = new List<Socket>(klijenti);
        //            Socket.Select(ready, null, null, 1000000); // čekaj do 1 sekunde

        //            foreach (Socket s in ready)
        //            {
        //                if (s.Available > 0 && !odgovori.ContainsKey(s))
        //                {
        //                    try
        //                    {
        //                        int br = s.Receive(buffer);
        //                        if (br > 0)
        //                        {
        //                            string odgovor = Encoding.UTF8.GetString(buffer, 0, br).Trim().ToLower();
        //                            odgovori[s] = odgovor;
        //                        }
        //                    }
        //                    catch (SocketException se)
        //                    {
        //                        if (se.SocketErrorCode == SocketError.WouldBlock)
        //                        {
        //                            // Nema podataka trenutno, ignorisi i nastavi cekanje
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine("Greška u primanju: " + se.Message);
        //                            // po potrebi prekini igru ili obradi gresku
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // Kada su oba igrača odgovorila:
        //        foreach (var par in odgovori)
        //        {
        //            Socket s = par.Key;
        //            string odgovor = par.Value;
        //            Igrac igrac = (s == klijenti[0]) ? igrac1 : igrac2;

        //            try
        //            {
        //                if (ProveriOdgovor(odgovor))
        //                {
        //                    igrac.poeniUTrenutnojIgri += poeniPoTacnom;
        //                    Console.WriteLine($"{igrac.username}: Tačno! +{poeniPoTacnom} poena");
        //                    s.Send(Encoding.UTF8.GetBytes("Tačno! +" + poeniPoTacnom + " poena"));
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"{igrac.username}: Netačno.");
        //                    s.Send(Encoding.UTF8.GetBytes("Netačno."));
        //                }
        //            }
        //            catch (ArgumentException ex)
        //            {
        //                Console.WriteLine($"{igrac.username}: Nevažeći odgovor. Pitanje se ne računa.");
        //                s.Send(Encoding.UTF8.GetBytes("Nevažeći odgovor."));
        //            }
        //        }

        //        // Prikaz bodova
        //        string rezultat = $"REZULTAT:\n{igrac1.username}: {igrac1.poeniUTrenutnojIgri} poena\n{igrac2.username}: {igrac2.poeniUTrenutnojIgri} poena\n";
        //        Console.WriteLine(rezultat);
        //        byte[] rezBin = Encoding.UTF8.GetBytes(rezultat);

        //        foreach (Socket klijent in klijenti)
        //            klijent.Send(rezBin);

        //        Thread.Sleep(2000);
        //        //Console.Clear();
        //    }
        //}
    }
}