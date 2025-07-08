using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Asocijacije.Enumeracije
{
    public enum PovratnaVrednostUnosa
    {
        Polje,
        KonacnoKolona,
        KonacnoAsocijacija,
        NeispravanUnos,
        Dalje
    }
}
