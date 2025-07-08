using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace Klase.Pitanja_i_Odgovori.Modeli
{
    public class PitanjaIOdgovori
    {
        public string TekucePitanje { get; private set; }
        public bool TacanOdgovor { get; private set; }
        public Dictionary<string, bool> SvaPitanja { get; private set; } = new Dictionary<string, bool>();

        private List<string> pitanjaRedosled = new List<string>();
        private int indeksTrenutnogPitanja = -1;

        public PitanjaIOdgovori(string tekucePitanje,  bool tacanOdgovor, Dictionary<string, bool> svaPitanja)
        {
            this.TekucePitanje = tekucePitanje;
            this.TacanOdgovor = tacanOdgovor;
            this.SvaPitanja = svaPitanja;
        }

        public PitanjaIOdgovori()
        {

        }

        public void UcitajPitanja(string putanjaDoFajla)
        {
            string[] linije = putanjaDoFajla.Split("\r\n");
            SvaPitanja.Clear();

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
                SvaPitanja[pitanje] = tacan;
            }

            IzaberiIPromesajPitanja();
            indeksTrenutnogPitanja = -1;
        }

        private void IzaberiIPromesajPitanja()
        {
            var rnd = new Random();
            var svaPitanjaLista = SvaPitanja.Keys.ToList();

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
                bool o1 = SvaPitanja[pitanja[i - 2]];
                bool o2 = SvaPitanja[pitanja[i - 1]];
                bool o3 = SvaPitanja[pitanja[i]];

                if (o1 == o2 && o2 == o3)
                    return true;
            }
            return false;
        }

        public bool PostaviSledecePitanje()
        {
            indeksTrenutnogPitanja++;
            if (indeksTrenutnogPitanja >= pitanjaRedosled.Count)
                return false;

            TekucePitanje = pitanjaRedosled[indeksTrenutnogPitanja];
            TacanOdgovor = SvaPitanja[TekucePitanje];
            return true;
        }

        public bool ProveriOdgovor(string odgovorKlijenta)
        {
            odgovorKlijenta = odgovorKlijenta.ToLower();
            if (odgovorKlijenta[0] != 'a' && odgovorKlijenta[0] != 'b')
                throw new ArgumentException("Odgovor mora biti 'a' ili 'b'.");

            bool odgovorJeTacan = (odgovorKlijenta[0] == 'a');
            return odgovorJeTacan == TacanOdgovor;
        }

    }
}
