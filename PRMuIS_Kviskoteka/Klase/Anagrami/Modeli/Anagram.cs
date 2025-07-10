using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Klase.Anagrami.Enumeracije;

namespace Klase.Anagrami.Modeli
{

    //Klasa Anagram, kao i sama igra su implementirane, pored onoga sto je trazeno u specifikaciji projekta,
    //sa dodatnom proverom smisla reci - odnosno, iz datoteke se citaju sve reci koje se mogu sastaviti od ponudjenih reci
    //da bi se izbeglo da korisnik (igrac) unese gluposti i dobije poene

    //primer: za reč MREŽA - korisnik može uneti AŽERM i dobiti poene, iako reč nema smisla, takodje može uneti i jedno jedino 
    //slovo i dobiti poene, zbog cega smo se odlucili na ovakvu implementaciju 

    public class Anagram
    {
        public string REC { get; set; }
        public List<PonudjenaRec> ponudjeneReci;
        
        Dictionary<char, int> rec = new Dictionary<char, int>();

        public Anagram(string anagramTxt)
        {
            string[] splits = anagramTxt.Split("\r\n");

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

        //metoda koja je trazena u zadatku, u nasem konkretnom primeru nema veliku svrhu ali smo je svakako odradili
        private PovratneVrednostiAnagrama proveriRec(string anagram)
        {

            Dictionary<char, int> ang = new Dictionary<char, int>();

            foreach (char c in anagram)
                if (ang.Keys.Contains(c))
                    ang[c]++;
                else
                    ang.Add(c, 1);


            foreach (KeyValuePair<char, int> kvp in ang)
                if (!rec.Keys.Contains(kvp.Key))
                    return PovratneVrednostiAnagrama.NePostojiSlovo; //ako uopste ne postoji slovo
                else if (rec[kvp.Key] < kvp.Value)
                    return PovratneVrednostiAnagrama.PreviseSlova; //ako u anagramu ima vise slova nego u pocetnoj reci

            return PovratneVrednostiAnagrama.IspravnaRec;

        }

        public PovratneVrednostiAnagrama postojiRec(string anagram)
        {

            PovratneVrednostiAnagrama pv = (proveriRec(anagram.ToUpper().Trim()));

            if (pv == PovratneVrednostiAnagrama.IspravnaRec)
                foreach (PonudjenaRec rec in ponudjeneReci)
                    if (rec.Rec.Equals(anagram.ToLower().Trim()))
                    {
                        if(rec.pogodiRec() && !rec.PogodjenaDrugiPut)
                            pv = PovratneVrednostiAnagrama.IspravnaRec;
                        else if (rec.pogodiRec2() && rec.PogodjenaPrviPut)
                            pv = PovratneVrednostiAnagrama.DrugiIgracPogodio;
                        else
                            pv = PovratneVrednostiAnagrama.VecPogodjeno;

                        return pv;
                    }
                    else
                        pv = PovratneVrednostiAnagrama.NeispravnaRec;

            return pv;

        }

        public int getPogodjene()
        {
            int n = 0;
            foreach (PonudjenaRec rec in ponudjeneReci)
                if (rec.PogodjenaPrviPut) n++;

            return n;
        }

        public void endGame()
        {
            foreach (PonudjenaRec rec in ponudjeneReci)
                rec.PogodjenaPrviPut = true;

        }

    }
}
