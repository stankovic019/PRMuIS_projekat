using Klase.General.Modeli;
using System;
using System.Net;
using System.Net.Sockets;
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
        static void Main(string[] args)
        {
            Igrac igrac;
            bool prijavljen = false;
            #region UDP SERVER - PRIJAVA KORISNIKA

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001); //dimitrije port
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50002); //vojin port
            serverSocket.Bind(serverEP);
            Console.WriteLine($"Server je pokrenut i ceka poruku na: {serverEP}");

            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

            string prethodna = "";
            while (!prijavljen) // 1
            {
                byte[] prijemniBafer = new byte[1024]; // Inicijalizujemo bafer za prijem podataka sa pretpostavkom da poruka nece biti duza od 1024 bajta
                try
                {
                    int brBajta = serverSocket.ReceiveFrom(prijemniBafer, ref posiljaocEP); // Primamo poruku i podatke o posiljaocu
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    if (poruka == prethodna) //5
                        break;

                    Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    Console.WriteLine($"{posiljaocEP}: {poruka}");
                    prethodna = poruka;

                    //^([a-zA-Z0-9_]+)(,\s*[a-zA-Z0-9_]+)*$ regex za username samo bez liste igara - trebace za multiplayer


                    if (!Regex.IsMatch(poruka, "([a-zA-Z0-9_]+)(,\\s*[a-zA-Z0-9_]+)+"))
                    {
                        poruka = "Neispravan unos. Pokusajte ponovo.";
                    }
                    else {

                        igrac = new Igrac(poruka.Split(","));
                        poruka = "Ispravno prijavljen korisnik '" + igrac.username + "'";
                    }

                    byte[] binarnaPoruka = Encoding.UTF8.GetBytes("Server: " + poruka); // Dopisujemo Server eho cisto da znamo koja je poruka

                    brBajta = serverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, posiljaocEP); // 3.
                      
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom prijema poruke: \n{ex}");
                }

            }

            Console.WriteLine("Server zavrsava sa radom");
            serverSocket.Close(); // Zatvaramo soket na kraju rada
            Console.ReadKey();

            #endregion

        }
    }
}
