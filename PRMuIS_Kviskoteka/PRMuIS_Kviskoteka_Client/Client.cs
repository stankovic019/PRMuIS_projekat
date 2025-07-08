using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

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

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPEndPoint destinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.4"), 50001); dimitrije IP
            IPEndPoint destinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.16"), 50001); //vojin IP
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);

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
                    int brBajta = clientSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, destinationEP); // Poruka koju saljemo u binarnom zapisu, pocetak poruke, duzina, flegovi, odrediste

                    Console.WriteLine($"Poslata prijava ka {destinationEP}...");
                    Console.WriteLine();
                    Thread.Sleep(1000);
                    brBajta = clientSocket.ReceiveFrom(prijemniBafer, ref posiljaocEP);

                    string ehoPoruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    Console.WriteLine($"{posiljaocEP} - {ehoPoruka}"); // 4
                    Console.WriteLine();
                    Thread.Sleep(1000);

                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom slanja poruke: \n{ex}");
                }
            }

            Console.WriteLine("Klijen zavrsava sa radom");
            clientSocket.Close(); // Zatvaramo soket na kraju rada
            #endregion
        }
    }
    
}

