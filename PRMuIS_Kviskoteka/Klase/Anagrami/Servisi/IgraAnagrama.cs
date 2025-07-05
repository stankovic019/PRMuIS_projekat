using Klase.Anagrami.Enumeracije;
using Klase.Anagrami.Modeli;
using Klase.General.Modeli;

namespace Klase.Anagrami.Servisi
{
    public class IgraAnagrama
    {
        public Anagram anagram;
        private Igrac igrac1, igrac2;
        int bodovi = 0; //temp
        public IgraAnagrama(string anagramTxt, Igrac igrac)
        {
            anagram = new Anagram(anagramTxt);
            igrac1 = igrac;

        }
        public IgraAnagrama(string anagramTxt, Igrac igrac1, Igrac igrac2)
        {
            anagram = new Anagram(anagramTxt);
            this.igrac1 = igrac1;
            this.igrac2 = igrac2;

        }

        public void treningIgra()
        {
            while (true)
            {
                Console.WriteLine("TRENING");
                Console.WriteLine("Poeni: " + igrac1.poeniUTrenutnojIgri);
                Console.WriteLine("Pogodjene reci: " + anagram.getPogodjene() + "/" + anagram.ponudjeneReci.Count);
                Console.WriteLine("PONUDJENA REC: " + anagram.REC);
                Console.Write("Unesite rec: ");
                string rec = Console.ReadLine().Trim().ToLower();

                PovratneVrednostiAnagrama pv = anagram.postojiRec(rec);

                if (pv == PovratneVrednostiAnagrama.IspravnaRec)
                {
                    igrac1.poeniUTrenutnojIgri += rec.Length;
                    Console.Clear();
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


                Thread.Sleep(1000);
                Console.Clear();
            }
        }


        public void Igraj()
        {

            while (true)
            {
                Console.WriteLine("Bodovi: " + bodovi);
                Console.WriteLine("Pogodjene reci: " + anagram.getPogodjene() + "/" + anagram.ponudjeneReci.Count);
                Console.WriteLine("PONUDJENA REC: " + anagram.REC);
                Console.Write("Unesite rec: ");
                string rec = Console.ReadLine().Trim().ToLower();

                PovratneVrednostiAnagrama pv = anagram.postojiRec(rec);

                if (pv == PovratneVrednostiAnagrama.IspravnaRec)
                {
                    bodovi += rec.Length;
                    Console.Clear();
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


                Thread.Sleep(1000);
                Console.Clear();
            }

        }
    }
}
