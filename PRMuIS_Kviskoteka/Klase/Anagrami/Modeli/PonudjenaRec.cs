using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Klase.Anagrami.Modeli
{
    public class PonudjenaRec
    {
        public string Rec { get; }
        public bool PogodjenaPrviPut { get; set; }
        public bool PogodjenaDrugiPut { get; set; }

        public PonudjenaRec(string Rec)
        {
            this.Rec = Rec;
            this.PogodjenaPrviPut = false;
            this.PogodjenaDrugiPut = false;
        }

        public bool pogodiRec()
        {
            if (!this.PogodjenaPrviPut)
            {
                PogodjenaPrviPut = true;
                return true;
            }
            return false;
        }

        public bool pogodiRec2()
        {
            if (!this.PogodjenaDrugiPut)
            {
                PogodjenaDrugiPut = true;
                return true;
            }
            return false;
        }

    }
}
