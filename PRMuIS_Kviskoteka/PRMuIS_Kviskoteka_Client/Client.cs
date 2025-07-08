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

#pragma warning disable SYSLIB0011


namespace PRMuIS_Kviskoteka_Client
{

    //PROJEKAT 25 - KVISKOTEKA
    //ČLANOVI TIMA: Dimitrije Stanković PR81/2022
    //              Vojin Jovanović PR82/2022
    //FTN, ŠKOLSKA 2024/25.

    public class Client
    {
        static void Main(string[] args)
        {
            #region UDP SERVER - PRIJAVA KORISNIKA

            Socket UDPclientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.4"), 50001); //dimitrije IP
            //IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.16"), 50002); //vojin IP
            EndPoint UDPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            while (true) // 1.
            {
                byte[] prijemniBafer = new byte[1024];
                Console.Write("Prijavite se: ");
                string poruka = Console.ReadLine();

                if (poruka.ToLower() == "exit")
                    break;

                byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                try
                {
                    int brBajta = UDPclientSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPdestinationEP); // Poruka koju saljemo u binarnom zapisu, pocetak poruke, duzina, flegovi, odrediste

                    Console.WriteLine($"Poslata prijava ka {UDPdestinationEP}...");
                    Console.WriteLine();
                    Thread.Sleep(1000);
                    brBajta = UDPclientSocket.ReceiveFrom(prijemniBafer, ref UDPposiljaocEP);

                    string ehoPoruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    bool povratnaVrednost = ehoPoruka[0] == '1' ? true : false;

                    if (!povratnaVrednost)
                    {
                        Console.WriteLine($"{UDPposiljaocEP} - {ehoPoruka.Substring(1)}"); 
                        Thread.Sleep(2000);
                        Console.Clear();
                    }
                    else
                    {
                        
                        Console.WriteLine($"{UDPposiljaocEP} - {ehoPoruka.Substring(1)}");
                        Thread.Sleep(2000);
                        Console.Clear();
                        upaliTCP();
                    }
                    
;

                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom slanja poruke: \n{ex}");
                }
            }

            Console.WriteLine("Klijen zavrsava sa radom");
            UDPclientSocket.Close(); // Zatvaramo soket na kraju rada
            #endregion
        }


        static void upaliTCP() {


            Socket TCPclientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            byte[] buffer = new byte[1024];
            EndPoint TCPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);
            TCPclientSocket.Connect(TCPserverEP);
            
            int brBajta = TCPclientSocket.Receive(buffer);
            string ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(ehoPoruka);
            Console.WriteLine();
            
            brBajta = TCPclientSocket.Receive(buffer);
            ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(ehoPoruka);
            Console.WriteLine();
            string start;
            do
            {
                Console.Write("Unesite \"START\" za pocetak kviza: ");
                start = Console.ReadLine();
            } while (start.ToLower() != "start");

            byte[] binarnaPoruka = Encoding.UTF8.GetBytes(start);
            TCPclientSocket.Send(binarnaPoruka);

            BinaryFormatter formatter = new BinaryFormatter();
            Igrac igrac = new Igrac();

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

            string poruka;

            for (int i = 0; i < igrac.brojIgara; ++i)
            {
                string igra = igrac.getIgra(i);
                Console.Clear();
                Thread.Sleep(2000);
                if (igra == "an")
                {
                    IgraAnagrama anagram = new IgraAnagrama(igrac);
                    anagram.treningIgra();
                    poruka = "Poeni u igri anagram: " + igrac.poeniUTrenutnojIgri;
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    TCPclientSocket.Send(binarnaPoruka);
                    igrac.dodeliPoene(i);

                    continue;
                }

                //if (igra == "po")
                //{
                //   PitanjaIOdgovori po = new PitanjaIOdgovori()
                //    anagrami.treningIgra();
                //    continue;
                //}

                if (igra == "as")
                {
                    IgraAsocijacija asocijacija = new IgraAsocijacija(igrac);
                    asocijacija.treningIgra();
                    poruka = "Poeni u igri asocijacije: " + igrac.poeniUTrenutnojIgri;
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    TCPclientSocket.Send(binarnaPoruka);
                    igrac.dodeliPoene(i);
                    continue;
                }
            }

            binarnaPoruka = Encoding.UTF8.GetBytes(igrac.ToString());
            TCPclientSocket.Send(binarnaPoruka);

            binarnaPoruka = Encoding.UTF8.GetBytes("exit");
            TCPclientSocket.Send(binarnaPoruka);

            Console.WriteLine("Klijent zavrsava sa radom");
            Console.ReadKey();
            TCPclientSocket.Close();

            
        }

    }
}