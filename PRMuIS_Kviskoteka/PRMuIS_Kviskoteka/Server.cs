using Klase.General.Modeli;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace PRMuIS_Kviskoteka
{
 
    //PROJEKAT 25 - KVISKOTEKA
    //ČLANOVI TIMA: Dimitrije Stanković PR81/2022
    //              Vojin Jovanović PR82/2022
    //FTN, ŠKOLSKA 2024/25.
    
    internal class Server
    {
        static Igrac igrac;


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


        static void Main(string[] args)
        {
            bool prijavljen = false;
            #region UDP SERVER - PRIJAVA KORISNIKA

            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50001); //dimitrije port
            //IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50002); //vojin port
            UDPserverSocket.Bind(UDPserverEP);
            Console.WriteLine($"Server je pokrenut i ceka poruku na: {UDPserverEP}");

            EndPoint UDPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            string prethodna = "";
            while (!prijavljen) // 1
            {
                byte[] prijemniBafer = new byte[1024]; // Inicijalizujemo bafer za prijem podataka sa pretpostavkom da poruka nece biti duza od 1024 bajta
                try
                {
                    int brBajta = UDPserverSocket.ReceiveFrom(prijemniBafer, ref UDPposiljaocEP); // Primamo poruku i podatke o posiljaocu
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    if (poruka == prethodna) //5
                        break;

                    Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    Console.WriteLine($"{UDPposiljaocEP}: {poruka}");
                    prethodna = poruka;

                    //^([a-zA-Z0-9_]+)(,\s*[a-zA-Z0-9_]+)*$ regex za username samo bez liste igara - trebace za multiplayer


                    if (!Regex.IsMatch(poruka, "([a-zA-Z0-9_]+)(,\\s*[a-zA-Z0-9_]+)+") || !proveriUnos(poruka))
                    {
                        poruka = "Neispravan unos. Pokusajte ponovo.";
                        byte[] binarnaPoruka = Encoding.UTF8.GetBytes("0Server - " + poruka); // Dopisujemo Server eho cisto da znamo koja je poruka
                        brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP); // 3.
                        Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    }
                    else {

                        igrac = new Igrac(poruka.Split(","));
                        poruka = "Ispravno prijavljen korisnik '" + igrac.username + "'"; 
                        byte[] binarnaPoruka = Encoding.UTF8.GetBytes("1Server - " + poruka); // Dopisujemo Server eho cisto da znamo koja je poruka
                        brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP); // 3.
                        Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                        UpaliTCP();

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom prijema poruke: \n{ex}");
                }

            }

            Console.WriteLine("Server zavrsava sa radom");
            UDPserverSocket.Close(); // Zatvaramo soket na kraju rada
            Console.ReadKey();

            #endregion


        }

        static void UpaliTCP()
        {
            #region TCP SERVER

                Socket TCPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Any, 50001);

                TCPserverSocket.Bind(TCPserverEP);

                TCPserverSocket.Listen(5);


                Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {TCPserverEP}");

                Socket acceptedSocket = TCPserverSocket.Accept();

                IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
               
                Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");
                string poruka = $"Vasa TCP/IP adresa i port su: {clientEP}";
                byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);

                acceptedSocket.Send(binarnaPoruka);


                poruka = $"Dobrodosli u trening igru kviza \"KVISKOTEKA\". Danasnji takmicar je {igrac.username}";

            binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            acceptedSocket.Send(binarnaPoruka);
             

                Console.WriteLine("Server zavrsava sa radom");
                Console.ReadKey();
                acceptedSocket.Close();
                TCPserverSocket.Close();


            #endregion

        }

    }
}
