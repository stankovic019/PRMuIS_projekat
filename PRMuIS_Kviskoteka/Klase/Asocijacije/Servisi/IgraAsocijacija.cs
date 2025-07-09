using Klase.Asocijacije.Enumeracije;
using Klase.Asocijacije.Modeli;
using Klase.General.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Klase.Asocijacije.Servisi
{
    public class IgraAsocijacija
    {
        private Asocijacija asocijacija;
        private Igrac igrac1, igrac2;
        private int bodovi = 0;
        //konstruktor igre ako je trening igra
        public IgraAsocijacija(Igrac igrac)
        {

            string path = Directory.GetCurrentDirectory() + $"\\Files\\Asocijacije\\asocijacija{new Random().Next(1, 6)}.txt";
            string asocijacija_txt = string.Empty;
            try
            {
                asocijacija_txt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla.\n {ex}");
                return;
            }
            asocijacija = new Asocijacija(asocijacija_txt);
            igrac1 = igrac;
        }
        public IgraAsocijacija(Igrac igrac1, Igrac igrac2)
        {
            string path = Directory.GetCurrentDirectory() + $"\\Files\\Asocijacije\\asocijacija{new Random().Next(1, 6)}.txt";
            string asocijacija_txt = string.Empty;
            try
            {
                asocijacija_txt = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neuspesno otvaranje fajla.\n {ex}");
                return;
            }
            asocijacija = new Asocijacija(asocijacija_txt);
            this.igrac1 = igrac1;
            this.igrac2 = igrac2;
        }

        private PovratnaVrednostUnosa checkUnos(string unos)
        {
            if (unos == "DALJE")
                return PovratnaVrednostUnosa.Dalje;

            //ako je unos manji od 2, znaci da je neispravan unos
            if (unos.Length < 2) return PovratnaVrednostUnosa.NeispravanUnos;

            //ako nije uneta ni jedna oznaka kolone A,B,D,C niti K za konacno resenje
            if ((unos[0] < 65 || unos[0] > 68) && unos[0] != 'K') return PovratnaVrednostUnosa.NeispravanUnos;

            //ako nije unet broj iz opsega [1,4] niti dvotacka za pogadjanje konacnog resenja
            if ((unos[1] < 49 || unos[1] > 52) && unos[1] != ':') return PovratnaVrednostUnosa.NeispravanUnos;

            //ako je uneta oznaka konacnog resenja asocijacije, a nije uneta dvotacka u formatu (nego recimo broj, konacno resenje nema broj kolone)
            if (unos[0] == 'K' && unos[1] != ':') return PovratnaVrednostUnosa.NeispravanUnos;

            //ako nije pokusaj unosenja konacnog resenja
            if (unos[1] != ':')
                if (unos.Length != 2) //i duzina je razlicita od 2, znaci da je neispravan unos
                    return PovratnaVrednostUnosa.NeispravanUnos;

            //ako je uneto polje neke kolone
            if (unos[0] >= 65 && unos[0] <= 68 && unos[1] >= 49 && unos[1] <= 52)
                return PovratnaVrednostUnosa.Polje;
            //ako je uneto konacno resenje, proverava se da li kolone ili asocijacije
            if (unos[0] >= 65 && unos[0] <= 68 && unos[1] == ':')
                return PovratnaVrednostUnosa.KonacnoKolona;
            else
                return PovratnaVrednostUnosa.KonacnoAsocijacija;

            return PovratnaVrednostUnosa.NeispravanUnos;

        }

        public void treningIgra(Socket client)
        {
            //trening igra omogucava igracu da otvara polja do mile volje, bez prethodnog pogadjanja konacnog resenja kolone
            //konacno resenje asocijacije se moze pogadjati jedino nakon pogodjenog makar jednog konacnog resenja kolone

            StringBuilder sb = new StringBuilder();
            sb.Append("ZA OTVARANJE POLJA UNESITE KOMANDU U FORMATU: 'A1'\n");
            sb.Append("ZA POGADJANJE KONAČNOG REŠENJA KOLONE/ASOCIJACIJE UNESITE KOMANDU U FORMATU: " + "\n'A:[konacno_resenje]'/'K:[konacno_resenje]'\n");
            sb.Append("ZA IZLAZ IZ TRENING MODA UNESITE KOMANDU: 'IZLAZ'\n");
            Console.WriteLine();
            byte[] binarnaPoruka;
            binarnaPoruka = Encoding.UTF8.GetBytes(sb.ToString());
            client.Send(binarnaPoruka);

            bool otvorioPolje = false;
            bool otvorenaSvaPolja = false;
            while (!asocijacija.pogodjenoKonacno)
            {
                otvorenaSvaPolja = asocijacija.otvorenaSvaPolja();
                Console.WriteLine("TRENING");
                Console.WriteLine($"IGRAC: {igrac1.username} POENI: {igrac1.poeniUTrenutnojIgri} ");
                Console.WriteLine(asocijacija.ToString());
                Console.WriteLine();

                byte[] buffer = new byte[1024];
                int brojBajta = client.Receive(buffer);
                string komanda = Encoding.UTF8.GetString(buffer, 0, brojBajta).ToUpper();
                Console.Write("Uneta komanda: " + komanda);
                Console.WriteLine();
                Thread.Sleep(1000);

                if (komanda == "IZLAZ")
                {
                    asocijacija.endGame();
                    Console.Clear();
                    break;
                }

                PovratnaVrednostUnosa pvu = checkUnos(komanda);

                if (pvu == PovratnaVrednostUnosa.NeispravanUnos)
                {
                    Console.WriteLine("NEISPRAVAN UNOS. POKUSAJTE PONOVO.");
                    Thread.Sleep(1000);
                    Console.Clear();
                    binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                    client.Send(binarnaPoruka);
                    continue; //continue jer treba da ostane na istom igracu
                }

                //ako nije otvorio polje u ovom potezu, prvo mora to da uradi
                if (!otvorenaSvaPolja)
                {
                    if (pvu == PovratnaVrednostUnosa.Polje)
                    {
                        if (!asocijacija.findCollon(komanda))
                        {
                            Console.WriteLine("POLJE JE VEC OTVORENO. UNESITE PONOVO.");
                            Thread.Sleep(1000);
                            Console.Clear();
                            binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                            client.Send(binarnaPoruka);
                            continue; //continue jer treba da ostane na istom igracu
                        }
                        binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                        client.Send(binarnaPoruka);
                    }
                }

                if (pvu == PovratnaVrednostUnosa.KonacnoKolona)
                {
                    bool pogodjeno;
                    int poeni;
                    (pogodjeno, poeni) = asocijacija.guessKonacnoUKoloni(komanda);

                    //znaci nije otvoreno ni jedno polje u koloni
                    if (poeni == -1)
                    {
                        Console.WriteLine("NIJE OTVORENO NI JEDNO POLJE U KOLONI.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                        client.Send(binarnaPoruka);
                        continue;
                    }

                    if (poeni == -2)
                    {
                        Console.WriteLine("RESENJE JE VEC POGODJENO.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                        client.Send(binarnaPoruka);
                        continue;
                    }

                    if (!pogodjeno)
                    {
                        Console.WriteLine("Konačno resenje kolone nije pogodjeno.");
                        Thread.Sleep(1000);
                        binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                        client.Send(binarnaPoruka);

                    }

                    igrac1.poeniUTrenutnojIgri += poeni; //ako i nije pogodio, sabrace se sa nulom
                    binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                    client.Send(binarnaPoruka);
                }

                if (pvu == PovratnaVrednostUnosa.KonacnoAsocijacija)
                {
                    bool pogodjeno;
                    int poeni;
                    (pogodjeno, poeni) = asocijacija.tryKonacno(komanda);

                    if (!pogodjeno)
                    {
                        Console.WriteLine("Konačno resenje asocijacije nije pogodjeno.");
                        binarnaPoruka = Encoding.UTF8.GetBytes(komanda);
                        client.Send(binarnaPoruka);
                        Thread.Sleep(1000);
                    }

                    igrac1.poeniUTrenutnojIgri += poeni;
                }

                Console.Clear();
            }

            Console.WriteLine("TRENING");
            Console.WriteLine($"IGRAC: {igrac1.username} POENI: {igrac1.poeniUTrenutnojIgri} ");
            Console.WriteLine();
            Console.WriteLine(asocijacija.ToString());

            binarnaPoruka = Encoding.UTF8.GetBytes("izlaz");
            client.Send(binarnaPoruka);

        }


        public void Igraj()
        {
            bool otvorioPolje = false;
            bool otvorenaSvaPolja = false;
            Igrac trenIgrac = new Random().Next(1, 3) % 2 == 0 ? igrac1 : igrac2; //random ko igra prvi

            //jedna iteracija igre
            while (!asocijacija.pogodjenoKonacno)
            {
                otvorenaSvaPolja = asocijacija.otvorenaSvaPolja();
                Console.WriteLine($"IGRAC: {trenIgrac.username} POENI: {trenIgrac.poeniUTrenutnojIgri} ");
                Console.WriteLine(asocijacija.ToString());
                Console.WriteLine();
                Console.WriteLine("ZA OTVARANJE POLJA UNESITE KOMANDU U FORMATU: 'A1'");
                Console.WriteLine("ZA POGADJANJE KONAČNOG REŠENJA KOLONE/ASOCIJACIJE UNESITE KOMANDU U FORMATU: " + "\n'A:[konacno_resenje]'/'K:[konacno_resenje]'");
                Console.WriteLine("ZA PRESKAKANJE POTEZA UNESITE KOMANDU: 'DALJE'");
                Console.WriteLine();
                Console.Write("Unesite komandu: ");

                string komanda = Console.ReadLine().ToUpper();

                if (komanda == "EXIT")
                {
                    asocijacija.endGame();
                    Console.Clear();
                    break;
                }

                PovratnaVrednostUnosa pvu = checkUnos(komanda);

                if (pvu == PovratnaVrednostUnosa.NeispravanUnos)
                {
                    Console.WriteLine("NEISPRAVAN UNOS. POKUSAJTE PONOVO.");
                    Thread.Sleep(1000);
                    Console.Clear();
                    continue; //continue jer treba da ostane na istom igracu
                }

                //ako je otvorio polje ovaj potez
                if (otvorioPolje && pvu == PovratnaVrednostUnosa.Polje)
                {
                    Console.WriteLine("OTVORILI STE POLJE OVAJ POTEZ.");
                    Thread.Sleep(1000);
                    Console.Clear();
                    continue; //continue jer treba da bude isti igrac
                }

                //ako nije otvorio polje u ovom potezu, prvo mora to da uradi
                if (!otvorioPolje && !otvorenaSvaPolja)
                {
                    if (pvu == PovratnaVrednostUnosa.Polje)
                    {
                        if (!asocijacija.findCollon(komanda))
                        {
                            Console.WriteLine("POLJE JE VEC OTVORENO. UNESITE PONOVO.");
                            Thread.Sleep(1000);
                            Console.Clear();
                            continue; //continue jer treba da ostane na istom igracu
                        }
                        else
                            otvorioPolje = true;
                    }
                    else
                    {
                        Console.WriteLine("OTVORITE PRVO POLJE PRE POGADJANJA.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        continue; //continue jer treba da ostane na istom igracu
                    }
                }

                if (pvu == PovratnaVrednostUnosa.Dalje)
                {
                    otvorioPolje = false;
                    trenIgrac = trenIgrac.username == igrac1.username ? igrac2 : igrac1;
                }

                if (pvu == PovratnaVrednostUnosa.KonacnoKolona)
                {
                    bool pogodjeno;
                    int poeni;
                    (pogodjeno, poeni) = asocijacija.guessKonacnoUKoloni(komanda);

                    //znaci nije otvoreno ni jedno polje u koloni
                    if (poeni == -1)
                    {
                        Console.WriteLine("NIJE OTVORENO NI JEDNO POLJE U KOLONI.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        continue;
                    }

                    if (poeni == -2)
                    {
                        Console.WriteLine("RESENJE JE VEC POGODJENO.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        continue;
                    }

                    if (!pogodjeno)
                    {
                        Console.WriteLine("Konačno resenje kolone nije pogodjeno.");
                        Thread.Sleep(1000);
                        //sledeci igrac
                        trenIgrac = trenIgrac.username == igrac1.username ? igrac2 : igrac1;
                        otvorioPolje = false;

                    }

                    trenIgrac.poeniUTrenutnojIgri += poeni; //ako i nije pogodio, sabrace se sa nulom
                }

                if (pvu == PovratnaVrednostUnosa.KonacnoAsocijacija)
                {
                    bool pogodjeno;
                    int poeni;
                    (pogodjeno, poeni) = asocijacija.tryKonacno(komanda);

                    if (!pogodjeno)
                    {
                        Console.WriteLine("Konačno resenje asocijacije nije pogodjeno.");
                        //sledeci igrac
                        Thread.Sleep(1000);
                        trenIgrac = trenIgrac.username == igrac1.username ? igrac2 : igrac1;
                        otvorioPolje = false;
                    }

                    trenIgrac.poeniUTrenutnojIgri += poeni;
                }

                Console.Clear();
            }

            Console.WriteLine($"IGRAC: {igrac1.username} POENI: {igrac1.poeniUTrenutnojIgri} ");
            Console.WriteLine($"IGRAC: {igrac2.username} POENI: {igrac2.poeniUTrenutnojIgri} ");
            Console.WriteLine();
            Console.WriteLine(asocijacija.ToString());
        }
    }
}
