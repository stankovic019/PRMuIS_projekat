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
        public string TekucePitanje { get;  set; }
        public bool TacanOdgovor { get;  set; }
        public Dictionary<string, bool> SvaPitanja { get; set; } = new Dictionary<string, bool>();

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

    }
}
