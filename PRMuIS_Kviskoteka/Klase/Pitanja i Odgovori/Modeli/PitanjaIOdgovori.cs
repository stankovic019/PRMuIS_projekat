using Klase.Anagrami.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Pitanja_i_Odgovori.Modeli
{
    public class PitanjaIOdgovori
    {
        public string TekucePitanje { get;  set; }
        public bool TacanOdgovor { get;  set; }
        public Dictionary<string, bool> SvaPitanja { get; set; } = new Dictionary<string, bool>();

        private List<string> pitanjaRedosled = new List<string>();
        private int indeksTrenutnogPitanja = -1;
        public string REC { get; set; }
        public List<PonudjenaRec> ponudjeneReci;
        Dictionary<char, int> rec = new Dictionary<char, int>();

        public PitanjaIOdgovori(string tekucePitanje,  bool tacanOdgovor, Dictionary<string, bool> svaPitanja)
        {
            this.TekucePitanje = tekucePitanje;
            this.TacanOdgovor = tacanOdgovor;
            this.SvaPitanja = svaPitanja;
        }

        public PitanjaIOdgovori()
        {

        }
        public PitanjaIOdgovori(string pitanjaTxt)
        {
            string[] splits = pitanjaTxt.Split("\r\n");

            this.REC = splits[0];

            this.ponudjeneReci = new List<PonudjenaRec>();

            for (int i = 1; i < splits.Length; i++)
                ponudjeneReci.Add(new PonudjenaRec(splits[i].Trim()));

            foreach (char c in REC)
                if (rec.Keys.Contains(c))
                    rec[c]++;
                else
                    rec.Add(c, 1);

        }

    }
}
