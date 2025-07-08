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

        private bool kvisko;

        public Igrac()
        {

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

        public bool ulaganjeKviska()
        {
            if (!kvisko)
            {
                kvisko = true;
                return true;
            }
            return false;
        }

        public void dodeliPoene(int idx)
        {
            poeniUIgrama[idx] = poeniUTrenutnojIgri;
            poeniUTrenutnojIgri = 0;
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
                sb.Append($"\t{igre[i]}:{poeniUIgrama[i]}\n");

            return sb.ToString();

        }

    }
}
