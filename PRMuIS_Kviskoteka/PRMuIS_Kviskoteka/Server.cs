using Klase.Anagrami.Modeli;
using Klase.Anagrami.Servisi;
using Klase.General.Modeli;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Klase.Pitanja_i_Odgovori.Servisi;
using Klase.Asocijacije.Servisi;
#pragma warning disable SYSLIB0011

namespace PRMuIS_Kviskoteka
{
 
    //PROJEKAT 25 - KVISKOTEKA
    //ČLANOVI TIMA: Dimitrije Stanković PR81/2022
    //              Vojin Jovanović PR82/2022
    //FTN, ŠKOLSKA 2024/25.
    
    internal class Server
    {
        static Igrac igrac;
        static bool prijavljen = false;

        static void Main(string[] args)
        {
            UDPKonekcija();
            Console.ReadLine();

        }

        static void UDPKonekcija() {

            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50001); //dimitrije ip/port
            IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50001); //test ip
            //IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50002); //vojin ip/port
            UDPserverSocket.Bind(UDPserverEP);

            Console.WriteLine($"UDP Server je pokrenut i ceka poruku na: {UDPserverEP}");

            EndPoint UDPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            while (!prijavljen) 
            {
                byte[] prijemniBafer = new byte[1024]; 

                try
                {
                    int brBajta = UDPserverSocket.ReceiveFrom(prijemniBafer, ref UDPposiljaocEP);
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    Console.WriteLine($"{UDPposiljaocEP}: {poruka}");

                    //^([a-zA-Z0-9_]+)(,\s*[a-zA-Z0-9_]+)*$ regex za username samo bez liste igara - trebace za multiplayer

                    //proveravamo da li je lepo poslata prijava korisnika

                    if (!Regex.IsMatch(poruka, "([a-zA-Z0-9_]+)(,\\s*[a-zA-Z0-9_]+)+") || !proveriUnos(poruka))
                    {
                        poruka = "Neispravan unos. Pokusajte ponovo.";
                        byte[] binarnaPoruka = Encoding.UTF8.GetBytes("0Server - " + poruka); 
                        brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP);
                        Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    }
                    else
                    {

                        igrac = new Igrac(poruka.Split(","));
                        poruka = "Ispravno prijavljen korisnik '" + igrac.username + "'";
                        byte[] binarnaPoruka = Encoding.UTF8.GetBytes("1Server - " + poruka); 
                        brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP);
                        Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                        TCPKonekcija();
                        break;

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom prijema poruke: \n{ex}");
                }

            }

            Console.WriteLine("UDP Server zavrsava sa radom");
            UDPserverSocket.Close(); 
            Console.ReadKey();

        }

        static void TCPKonekcija()
        {


            Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Any, 50001);
            TCPserverSocket.Bind(TCPserverEP);
            TCPserverSocket.Listen(5);


            Console.WriteLine($"TCP Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {TCPserverEP}");

            Socket acceptedSocket = TCPserverSocket.Accept();

            IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
               
            Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");
            
            //saljemo podatke o adresi i portu klijentu
            string poruka = $"Vasa TCP/IP adresa i port su: {clientEP}";
            byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            acceptedSocket.Send(binarnaPoruka);

            poruka = $"Dobrodosli u trening igru kviza \"KVISKOTEKA\". Danasnji takmicar je {igrac.username}";
            binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            acceptedSocket.Send(binarnaPoruka);


            //primamo start poruku od klijenta
            byte[] buffer = new byte[1024];    
            int brojBajta = acceptedSocket.Receive(buffer);
            poruka = Encoding.UTF8.GetString(buffer,0,brojBajta);

            poruka = Convert.ToString(igrac.brojIgara);
            binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            acceptedSocket.Send(binarnaPoruka);


            for (int i = 0; i < igrac.brojIgara; ++i)
            {
                Thread.Sleep(2000);
                string igra = igrac.getIgra(i);
                Console.Clear();
                if (igra == "an")
                {
                    IgraAnagrama anagram = new IgraAnagrama(igrac);
                    poruka = "ANAGRAM";
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    acceptedSocket.Send(binarnaPoruka);
                    anagram.treningIgra(acceptedSocket);
                    Console.WriteLine("Ukupni poeni u igri 'Anagram': " + igrac.poeniUTrenutnojIgri);
                    igrac.dodeliPoene(i);
                    continue;
                }

                if (igra == "po")
                {
                    IgraPitanjaIOdgovora po = new IgraPitanjaIOdgovora(igrac);
                    poruka = "PITANJA I ODGOVORI";
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    acceptedSocket.Send(binarnaPoruka);
                    po.Igraj(acceptedSocket);
                    Console.WriteLine("Ukupni poeni u igri 'Pitanja i Odgovori': " + igrac.poeniUTrenutnojIgri); 
                    igrac.dodeliPoene(i);
                    continue;
                }

                if (igra == "as")
                {
                    IgraAsocijacija asocijacija = new IgraAsocijacija(igrac);
                    poruka = "ASOCIJACIJE";
                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    acceptedSocket.Send(binarnaPoruka);
                    asocijacija.treningIgra(acceptedSocket);
                    Console.WriteLine("Ukupni poeni u igri 'Asocijacije': " + igrac.poeniUTrenutnojIgri);
                    igrac.dodeliPoene(i);
                    continue;
                }

            }

            Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine("PODACI O IGRACU NAKON IGRANJA:");
            Console.WriteLine(igrac);

            Console.WriteLine();
            Console.WriteLine("TCP Server zavrsava sa radom");
            Console.ReadKey();
            acceptedSocket.Close();
            TCPserverSocket.Close();

        }

        static bool proveriUnos(string unos)
        {
            string[] skracenice = unos.Split(",");
            int poCounter = 0;
            for (int i = 1; i < skracenice.Length; ++i) //krece se od 1 jer je prvi unos username
            {
                if (skracenice[i].Trim().ToLower() == "po")
                {
                    poCounter++;
                    if (poCounter > 2)
                        return false;
                }

                else if (skracenice[i].Trim().ToLower() != "an" && skracenice[i].Trim().ToLower() != "as")
                    return false;

            }

            return true;

        }
    }
}
