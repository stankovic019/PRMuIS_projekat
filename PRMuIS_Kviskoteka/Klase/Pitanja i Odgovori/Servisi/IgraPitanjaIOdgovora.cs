using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using Klase.Pitanja_i_Odgovori.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klase.Pitanja_i_Odgovori.Servisi
{
    public class IgraPitanjaIOdgovora
    {
        private PitanjaIOdgovori pitanja;
        private PitanjaIOdgovori igra;
        private Igrac igrac1, igrac2;

        public IgraPitanjaIOdgovora(Igrac igrac)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\PitanjaIOdgovori\\pitanja.txt";

            string pitanja_txt = File.ReadAllText(path);
            igra = new PitanjaIOdgovori();
            igra.UcitajPitanja(pitanja_txt);
            this.igrac1 = igrac;

        }

        public IgraPitanjaIOdgovora()
        {

            try
            {
                igra.UcitajPitanja("pitanja.txt");
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

        public void Igraj(Socket client)
        {
            byte[] binarnaPoruka;
            while (igra.PostaviSledecePitanje())
            {
                Console.WriteLine("Pitanje: " + igra.TekucePitanje);
                Console.WriteLine("a) DA");
                Console.WriteLine("b) NE");
                byte[] buffer = new byte[1024];
                int brojBajta = client.Receive(buffer);
                string unos = Encoding.UTF8.GetString(buffer, 0, brojBajta);
                Console.Write("Uneti odgovor: " + unos);
                Thread.Sleep(1000);
              

                if (unos.ToLower() == "izlaz")
                {
                    break;
                }
                Console.WriteLine();

                try
                {
                    if (igra.ProveriOdgovor(unos))
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

                binarnaPoruka = Encoding.UTF8.GetBytes(unos);
                client.Send(binarnaPoruka);

                Thread.Sleep(1000);
                Console.Clear();
            }

            binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
            client.Send(binarnaPoruka);
            //Console.WriteLine("Kraj igre! Ukupno poena: " + poeni + " od mogucih " + maksimalniPoeni);
            //Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
            //Console.ReadKey();
        }
    }
}
