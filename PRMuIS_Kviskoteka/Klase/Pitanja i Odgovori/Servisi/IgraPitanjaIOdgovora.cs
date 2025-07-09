using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using Klase.Pitanja_i_Odgovori.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klase.Pitanja_i_Odgovori.Servisi
{
    public class IgraPitanjaIOdgovora
    {
        private PitanjaIOdgovori pitanjaIOdgr = new PitanjaIOdgovori();
        private PitanjaIOdgovori igra;
        private Igrac igrac1, igrac2;
        private int indeksTrenutnogPitanja = 0;
        private List<string> pitanjaRedosled = new List<string>();

        public IgraPitanjaIOdgovora(Igrac igrac)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\PitanjaIOdgovori\\pitanja.txt";

            string pitanja_txt = File.ReadAllText(path);
            igra = new PitanjaIOdgovori();
            UcitajPitanja(pitanja_txt);
            this.igrac1 = igrac;

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
    }
}
