using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Klase.General.Modeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Klase.Anagrami.Servisi;
using Klase.Pitanja_i_Odgovori.Modeli;
using Klase.Asocijacije.Servisi;
using Klase.Pitanja_i_Odgovori.Servisi;

#pragma warning disable SYSLIB0011


namespace PRMuIS_Kviskoteka_Client
{

    //PROJEKAT 25 - KVISKOTEKA
    //ČLANOVI TIMA: Dimitrije Stanković PR81/2022
    //              Vojin Jovanović PR82/2022
    //FTN, ŠKOLSKA 2024/25.

    public class Client
    {
        static Igrac igrac;

        static void Main(string[] args)
        {
            UDPKonekcija();
            Console.ReadLine();

        }

        static void UDPKonekcija()
        {
            Socket UDPclientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.4"), 50001); //dimitrije IP:port
            //IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.16"), 50002); //vojin IP:port
            EndPoint UDPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] prijemniBafer = new byte[1024];
                Console.Write("Prijavite se: ");
                string poruka = Console.ReadLine();

                if (poruka.ToLower() == "izlaz") //izlaz iz aplikacije
                    break;

                byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                try
                {
                    int brBajta = UDPclientSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPdestinationEP); 
                    Console.WriteLine($"Poslata prijava ka {UDPdestinationEP}...");
                    Console.WriteLine();
                    Thread.Sleep(2000);

                    brBajta = UDPclientSocket.ReceiveFrom(prijemniBafer, ref UDPposiljaocEP);
                    poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    //povratna vrednost prijave, da li je uspesna ili ne
                    bool povratnaVrednost = poruka[0] == '1' ? true : false;
                    
                        Console.WriteLine($"{UDPposiljaocEP} - {poruka.Substring(1)}");
                        Thread.Sleep(2000);
                        Console.Clear();

                    if (povratnaVrednost)
                        TCPKonekcija();
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom slanja poruke: \n{ex}");
                }
            }

            Console.WriteLine("UDP Klijent zavrsava sa radom");
            UDPclientSocket.Close(); // Zatvaramo soket na kraju rada
        }


        static void TCPKonekcija() {


            Socket TCPclientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            byte[] buffer = new byte[1024];
            EndPoint TCPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);
            TCPclientSocket.Connect(TCPserverEP);
            
            //ispis poruke o tcp adresi i portu igre
            int brBajta = TCPclientSocket.Receive(buffer);
            string poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

            Console.WriteLine(poruka);
            Console.WriteLine();
            brBajta = TCPclientSocket.Receive(buffer);
            poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(poruka);
            Console.WriteLine();

            //saljemo serveru podatak da moze da krene sa igrom
            do
            {
                Console.Write("Unesite \"START\" za pocetak kviza: ");
                poruka = Console.ReadLine();

            } while (poruka.ToLower() != "start");

            byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            TCPclientSocket.Send(binarnaPoruka);

            BinaryFormatter formatter = new BinaryFormatter();
            igrac = new Igrac();

            //od servera trazimo da nam posalje podatke o igracu, kao i koje ce igre igrati

            try
            {
                brBajta = TCPclientSocket.Receive(buffer);
               
                using (MemoryStream ms = new MemoryStream(buffer, 0, brBajta))
                {
                    igrac = (Igrac)formatter.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Došlo je do greške: {ex.Message}"); 
            }


            //pokretanje igara

            Console.Clear();
            Console.WriteLine("Pokrecem igre...");
            

            for (int i = 0; i < igrac.brojIgara; ++i)
            {
                Thread.Sleep(2000);
                string igra = igrac.getIgra(i);
                Console.Clear();
                if (igra == "an")
                {
                    IgraAnagrama anagram = new IgraAnagrama(igrac);
                    anagram.treningIgra();
                    poruka = "Ukupni poeni u igri 'Anagram': " + igrac.poeniUTrenutnojIgri;
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    TCPclientSocket.Send(binarnaPoruka);
                    igrac.dodeliPoene(i);

                    continue;
                }

                if (igra == "po")
                {
                    IgraPitanjaIOdgovora po = new IgraPitanjaIOdgovora(igrac);
                    po.Igraj();
                    poruka = "Ukupni poeni u igri 'Pitanja i Odgovori': " + igrac.poeniUTrenutnojIgri;
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    TCPclientSocket.Send(binarnaPoruka);
                    igrac.dodeliPoene(i);

                    continue;
                }

                if (igra == "as")
                {
                    IgraAsocijacija asocijacija = new IgraAsocijacija(igrac);
                    asocijacija.treningIgra();
                    poruka = "Ukupni poeni u igri 'Asocijacije': " + igrac.poeniUTrenutnojIgri;
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    TCPclientSocket.Send(binarnaPoruka);
                    igrac.dodeliPoene(i);
                    continue;
                }
            }

           //kraj igranja
            binarnaPoruka = Encoding.UTF8.GetBytes("exit");
            TCPclientSocket.Send(binarnaPoruka);

            //slanje igraca nazad na server
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, igrac);
                byte[] data = ms.ToArray();

                TCPclientSocket.Send(data);
            }


            Console.WriteLine("TCP Klijent zavrsava sa radom");
            Console.WriteLine();
            TCPclientSocket.Close();
        }

    }
}