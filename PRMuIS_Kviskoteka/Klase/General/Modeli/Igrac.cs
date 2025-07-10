using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.General.Modeli
{
    [Serializable]
    public class Igrac
    {
        public int id { get; }
        public string username { get; }
        public int brojIgara { get; }

        public int poeniUTrenutnojIgri { get; set; }

        private string[] igre;
        private int[] poeniUIgrama;
        public int ukupnoPoena { get; private set; } = 0;

        public bool kvisko { get; private set; } = false;
        public int kviskoIdx { get; private set; } = -1;
        public int poeniSaKviskom { get; private set; } = 0;

        private List<bool> penalties = new List<bool>();

        public void addPenalty()
        {
            penalties.Add(true);
        }

        public int getPenalties() { return penalties.Count; }

        public Igrac(string username) 
        {
            id = new Random().Next(0, 256);
            this.username = username;
            poeniUTrenutnojIgri = 0;
            brojIgara = 3;
            igre = new string[]
            {
                new string("an"),
                new string("po"),
                new string("as"),
            };
            poeniUIgrama = new int[brojIgara];
            kvisko = false;

        }

        public Igrac(string[] prijava)
        {
            id = new Random().Next(1, 256);
            this.username = prijava[0];
            poeniUTrenutnojIgri = 0;
            brojIgara = prijava.Length - 1;
            igre = new string[brojIgara];
            for(int i = 1; i< prijava.Length; ++i)
                igre[i-1] = prijava[i].Trim();
            poeniUIgrama = new int[brojIgara];
            kvisko = false;
        }

        public bool ulaganjeKviska(int idx)
        {
            if (!kvisko)
            {
                kvisko = true;
                kviskoIdx = idx;
                return true;
            }
            return false;
        }

        public void dodeliPoene(int idx)
        {
            poeniUIgrama[idx] = poeniUTrenutnojIgri;
            if (idx == kviskoIdx)
            {
                poeniUIgrama[idx] *= 2;
                poeniSaKviskom = poeniUIgrama[idx];
            }
            poeniUTrenutnojIgri = 0;
            penalties.Clear();
        }

        public string getIgra(int idx)
        {
            return igre[idx];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"IGRAC: {id} - {username}\n");
            sb.Append("Bodovi po igrama:\n");
            
            for (int i = 0; i < brojIgara; ++i)
            {
                if (igre[i] == "an")
                    sb.Append("\tanagram: ");
                else if (igre[i] == "po")
                    sb.Append("\tpitanja i odgovori: ");
                else if (igre[i] == "as")
                    sb.Append("\tasocijacija: ");

                sb.Append($"{poeniUIgrama[i]}\n");
                ukupnoPoena += poeniUIgrama[i];
            }

            sb.Append($"Ukupno poena: {ukupnoPoena} ");
            

            return sb.ToString();

        }

    }
}
