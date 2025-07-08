using Klase.Pitanja_i_Odgovori.Modeli;

namespace Klase.Pitanja_i_Odgovori.Servisi
{
    public class IgraPitanjaIOdgovora
    {
        PitanjaIOdgovori igra = new PitanjaIOdgovori();

        public IgraPitanjaIOdgovora()
        {

            try
            {
                igra.UcitajPitanja("pitanja.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška prilikom učitavanja pitanja: " + ex.Message);
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
                return;
            }
        }

        private int poeni = 0;
        private const int poeniPoTacnom = 4;

        private int maksimalniPoeni = poeniPoTacnom * 5;

        public void Igraj()
        {

            while (igra.PostaviSledecePitanje())
            {
                Console.WriteLine("Pitanje: " + igra.TekucePitanje);
                Console.WriteLine("a) DA");
                Console.WriteLine("b) NE");
                Console.Write("Unesite odgovor (a/b): ");
                var unos = Console.ReadKey().KeyChar;
                Console.WriteLine();

                try
                {
                    if (igra.ProveriOdgovor(unos))
                    {
                        Console.WriteLine("Tačno! + " + poeniPoTacnom + " poena\n");
                        poeni += poeniPoTacnom;
                    }
                    else
                    {
                        Console.WriteLine("Netačno.\n");
                    }
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message + " Pitanje se ne računa.\n");
                }
            }

            Console.WriteLine("Kraj igre! Ukupno poena: " + poeni + " od mogucih " + maksimalniPoeni);
            Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }
    }
}
