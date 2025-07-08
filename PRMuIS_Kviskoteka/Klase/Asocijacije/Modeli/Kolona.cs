using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Klase.Asocijacije.Modeli
{
    public class Kolona
    {
        public string Naziv { get; set; }
        public Polje _1 { get; set; }
        public Polje _2 { get; set; }
        public Polje _3 { get; set; }
        public Polje _4 { get; set; }
        public string konacnoResenje { get; set; }
        private bool canGuessKonacno { get; set; } = false;
        private bool pogodjenoKonacno { get; set; }

        public Kolona(string naziv, Polje _1, Polje _2, Polje _3, Polje _4, string konacnoResenje)
        {
            Naziv = naziv;
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
            this.konacnoResenje = konacnoResenje;
            pogodjenoKonacno = false;
        }

        public Kolona(string naziv, string _1, string _2, string _3, string _4, string konacnoResenje)
        {
            Naziv = naziv;
            this._1 = new Polje(_1);
            this._2 = new Polje(_2);
            this._3 = new Polje(_3);
            this._4 = new Polje(_4);
            this.konacnoResenje = konacnoResenje;
            pogodjenoKonacno = false;
        }


        public string getKonacno()
        {
            return pogodjenoKonacno ? konacnoResenje : "***";
        }

        public bool otvoriPolje(int oznaka)
        {
            canGuessKonacno = true; //cim je jedno polje otvoreno, tj. cim se doslo do ove metode moze da pogadja konacno resenje
            switch (oznaka)
            {
                case 1: return _1.otvoriPolje(); break;
                case 2: return _2.otvoriPolje(); break;
                case 3: return _3.otvoriPolje(); break;
                case 4: return _4.otvoriPolje(); break;
            }
            return false;

        }

        //metoda koja vraca da li su sva polja otvorena
        public bool allOpen()
        {
            return _1.otvorenoPolje && _2.otvorenoPolje && _3.otvorenoPolje && _4.otvorenoPolje;
        }

        public (bool, int) tryKonacno(string pokusaj)
        {
            int bodovi = 0;
            if (canGuessKonacno)
            {
                if (pogodjenoKonacno)
                    return (false, -2); //kod greske da je vec pogodjeno konacno
                if (pokusaj == konacnoResenje)
                {
                    //otvaraju se sva polja ako je pogodjeno konacno resenje
                    pogodjenoKonacno = true;
                    bodovi += 2; //ako je pogodjeno konacno resenje kolone, dobija se 5 poena
                    //i po dva poena za svako neotvoreno polje
                    if (!_1.otvorenoPolje)
                    {
                        _1.otvorenoPolje = true;
                        bodovi += 1;
                    }
                    if (!_2.otvorenoPolje)
                    {
                        _2.otvorenoPolje = true;
                        bodovi += 1;
                    }
                    if (!_3.otvorenoPolje)
                    {
                        _3.otvorenoPolje = true;
                        bodovi += 1;
                    }
                    if (!_4.otvorenoPolje)
                    {
                        _4.otvorenoPolje = true;
                        bodovi += 1;
                    }
                    return (true, bodovi);
                }
                return (false, 0);

            }
            return (false, -1); //ako nije otvoreno ni jedno polje u koloni

        }

        public int endGame()
        {
            int bodovi = 0;
            if (!pogodjenoKonacno)
            {
                pogodjenoKonacno = true;
                bodovi += 5;
            }
            if (!_1.otvorenoPolje)
            {
                _1.otvorenoPolje = true;
                bodovi += 2;
            }
            if (!_2.otvorenoPolje)
            {
                _2.otvorenoPolje = true;
                bodovi += 2;
            }
            if (!_3.otvorenoPolje)
            {
                _3.otvorenoPolje = true;
                bodovi += 2;
            }
            if (!_4.otvorenoPolje)
            {
                _4.otvorenoPolje = true;
                bodovi += 2;
            }

            return bodovi;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Naziv}: ");
            sb.Append(string.Format("{0, 15}|{1, 15}|{2, 15}|{3, 15}", _1, _2, _3, _4));

            sb.Append("\n");

            return sb.ToString();

        }
    }
}
