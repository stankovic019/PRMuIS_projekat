namespace Klase.General.Modeli
{
    public class Igrac
    {
        public int id { get; }
        public string username { get; }

        public int poeniUTrenutnojIgri { get; set; }

        private int[] poeniUIgrama;

        private bool kvisko;

        public Igrac(string[] prijava)
        {
            id = new Random().Next(1, 256);
            this.username = prijava[0];
            poeniUTrenutnojIgri = 0;
            poeniUIgrama = new int[prijava.Length - 1];
            kvisko = false;
        }

        public bool ulaganjeKviska()
        {
            if (!kvisko)
            {
                kvisko = true;
                return true;
            }
            return false;
        }

        public void dodeliPoene(int idx)
        {
            poeniUIgrama[idx] = poeniUTrenutnojIgri;
            poeniUTrenutnojIgri = 0;
        }




    }
}
