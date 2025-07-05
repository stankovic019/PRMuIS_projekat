using System.Text;

namespace Klase.Asocijacije.Modeli
{
    public class Asocijacija
    {
        public Dictionary<char, Kolona> kolone = new Dictionary<char, Kolona>();

        public string konacnoResenje;
        public bool canGuessKonacno;
        public bool pogodjenoKonacno = false;

        public Asocijacija(string asocijacija_txt)
        {
            string[] splits = asocijacija_txt.Split("\r\n");

            char kolona = 'A';

            for (int i = 0; i < splits.Length - 1; i += 5)
            {

                Kolona k = new Kolona(kolona.ToString(), splits[i], splits[i + 1], splits[i + 2], splits[i + 3], splits[i + 4]);
                kolone.Add(kolona, k);
                kolona++;
            }

            konacnoResenje = splits.Last();

        }

        public bool findCollon(string nazivKolone)
        {
            char c = nazivKolone[0];
            Kolona k = kolone[c];
            int i = Convert.ToInt32(nazivKolone[1]) - 48;
            return k.otvoriPolje(i);
        }

        public (bool, int) guessKonacnoUKoloni(string nazivKolone)
        {
            char c = nazivKolone[0];
            Kolona k = kolone[c];

            bool retVal;
            int poeni;
            (retVal, poeni) = k.tryKonacno(nazivKolone.Substring(2).Trim());
            if (retVal) canGuessKonacno = true; //ako je uspesno pogodjeno bilo koje konacno resenje kolone, omoguciti pogadjanje
                                                //konacnog resenja asocijacije
            return (retVal, poeni);
        }

        public bool otvorenaSvaPolja()
        {
            foreach (KeyValuePair<char, Kolona> k in kolone)
                if (!k.Value.allOpen())
                    return false;

            return true;
        }

        public void endGame()
        {
            pogodjenoKonacno = true;
            foreach (KeyValuePair<char, Kolona> c in kolone)
                c.Value.endGame();
        }


        public (bool, int) tryKonacno(string pokusaj)
        {
            int poeni = 0;
            if (canGuessKonacno)
            {
                if (pokusaj.Substring(2).Trim() == konacnoResenje)
                {
                    pogodjenoKonacno = true;
                    poeni += 10; //za pogodjeno konacno resenje asocijacije
                    foreach (KeyValuePair<char, Kolona> c in kolone)
                        poeni += c.Value.endGame();
                    return (true, poeni);
                }

            }

            return (false, 0);
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            int[] nums = { 1, 2, 3, 4 };


            foreach (KeyValuePair<char, Kolona> a in kolone)
            {
                sb.Append(string.Format("{0,18}|{1,15}|{2,15}|{3,15}\n", nums[0], nums[1], nums[2], nums[3]));
                sb.Append(a.Value.ToString());
                sb.Append("\nRešenje kolone " + a.Key.ToString().ToUpper() + ": " + a.Value.getKonacno() + "\n");
                for (int i = 0; i < 66; ++i) sb.Append("-");
                sb.Append("\n");
            }
            //sb.Append("\n");
            sb.Append("KONAČNO REŠENJE:" + string.Format("{0,15}", pogodjenoKonacno ? konacnoResenje : "***"));


            return sb.ToString();
        }
    }
}
