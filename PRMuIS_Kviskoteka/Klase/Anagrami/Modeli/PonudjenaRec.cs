namespace Klase.Anagrami.Modeli
{
    public class PonudjenaRec
    {
        public string Rec { get; }
        public bool Pogodjena { get; set; }

        public PonudjenaRec(string Rec)
        {
            this.Rec = Rec;
            this.Pogodjena = false;
        }

        public bool pogodiRec()
        {
            if (!this.Pogodjena)
            {
                Pogodjena = true;
                return true;
            }
            return false;
        }
    }
}
