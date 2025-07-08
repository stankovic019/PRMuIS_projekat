using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

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
            
            Console.WriteLine("Klijent zavrsava sa radom");
            Console.ReadKey();
            TCPclientSocket.Close();

            
        }

    }
}