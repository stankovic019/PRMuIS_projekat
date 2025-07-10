using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Anagrami.Enumeracije
{
    public enum PovratneVrednostiAnagrama
    {
        NePostojiSlovo,
        PreviseSlova,
        IspravnaRec,
        NeispravnaRec,
        VecPogodjeno,
        DrugiIgracPogodio
        
    }
}
