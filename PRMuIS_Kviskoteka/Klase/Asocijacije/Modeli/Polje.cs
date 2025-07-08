using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Asocijacije.Modeli
{
    public class Polje
    {
        public string sadrzaj { get; }
        public bool otvorenoPolje { get; set; }

        public Polje(string sadrzaj)
        {
            this.sadrzaj = sadrzaj;
            this.otvorenoPolje = false;
        }

        public bool otvoriPolje()
        {
            if (!otvorenoPolje)
            {
                otvorenoPolje = true;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return otvorenoPolje ? sadrzaj : "***";
        }

    }
}
