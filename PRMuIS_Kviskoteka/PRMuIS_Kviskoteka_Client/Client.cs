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
            //IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.4"), 50001); //dimitrije IP:port
            IPEndPoint UDPdestinationEP = new IPEndPoint(IPAddress.Parse("192.168.0.16"), 50002); //vojin IP:port
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
                    int povratnaVrednost = poruka[0] - 48;
                    
                        Console.WriteLine($"{UDPposiljaocEP} - {poruka.Substring(1)}");
                        Thread.Sleep(2000);
                        Console.Clear();

                    if (povratnaVrednost == 1)
                    {
                        TCPKonekcijaJedanKorisnik();
                        break;
                    }
                    else if(povratnaVrednost == 2)
                    {
                        TCPKonekcijaDvaKorisnika();
                        break;
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom slanja poruke: \n{ex}");
                }
            }

            Console.WriteLine("UDP Klijent zavrsava sa radom");
            UDPclientSocket.Close(); // Zatvaramo soket na kraju rada
        }


        static void TCPKonekcijaJedanKorisnik() {


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

        
            //pokretanje igara

            Console.Clear();
   

            brBajta = TCPclientSocket.Receive(buffer);
            int brojIgara = Convert.ToInt32(Encoding.UTF8.GetString(buffer, 0, brBajta));

            for (int i = 0; i < brojIgara; ++i)
            {

                brBajta = TCPclientSocket.Receive(buffer);
                string trenutnaIgra = Encoding.UTF8.GetString(buffer, 0, brBajta);

                Console.WriteLine("Trenutna igra broj " + (i + 1) + ": " + trenutnaIgra);

                if (trenutnaIgra == "ANAGRAM")
                    while (true)
                    {
                        Console.Write("Unesite rec: ");
                        poruka = Console.ReadLine().Trim().ToLower();
                        binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                        TCPclientSocket.Send(binarnaPoruka);
                        brBajta = TCPclientSocket.Receive(buffer);
                        poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        if (poruka == "izlaz") break;
                    }
                else if (trenutnaIgra == "PITANJA I ODGOVORI")
                    while (true)
                    {
                        Console.Write("Unesite odgovor (a/b): ");
                        poruka = Console.ReadLine().Trim().ToLower();
                        binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                        TCPclientSocket.Send(binarnaPoruka);
                        brBajta = TCPclientSocket.Receive(buffer);
                        poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        if (poruka == "izlaz") break;
                    }
                else if (trenutnaIgra == "ASOCIJACIJE")
                {
                    brBajta = TCPclientSocket.Receive(buffer);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, brBajta));
                    Console.WriteLine();
                    while (true)
                    {
                        Console.Write("Unesite komandu: ");
                        poruka = Console.ReadLine().Trim().ToLower();
                        binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                        TCPclientSocket.Send(binarnaPoruka);
                        brBajta = TCPclientSocket.Receive(buffer);
                        poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        if (poruka == "izlaz") break;
                    }
                }
            
                Thread.Sleep(1000);
                Console.Clear();
            }

            Thread.Sleep(1000);
            Console.Clear();

            Console.WriteLine("TCP Klijent zavrsava sa radom");
            Console.WriteLine();
            TCPclientSocket.Close();
        }

        static void TCPKonekcijaDvaKorisnika()
        {
            Socket TCPclientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint TCPserverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            TCPclientSocket.Connect(TCPserverEP);
            byte[] buffer = new byte[1024];
            string poruka = string.Empty;
       


            int brBajta = TCPclientSocket.Receive(buffer);
            poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
            Console.WriteLine(poruka);
            Console.WriteLine();

            do
            {
                Console.Write("Unesite \"START\" za pocetak kviza: ");
                poruka = Console.ReadLine();

            } while (poruka.ToLower() != "start");

            byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
            TCPclientSocket.Send(binarnaPoruka);


            //pokretanje igara

            Console.Clear();


            brBajta = TCPclientSocket.Receive(buffer);
            


            for (int i = 1; i < 2; ++i)
            {

                brBajta = TCPclientSocket.Receive(buffer);
                string trenutnaIgra = Encoding.UTF8.GetString(buffer, 0, brBajta);

                Console.WriteLine("Trenutna igra broj " + (i + 1) + ": " + trenutnaIgra);

                if (trenutnaIgra == "ANAGRAM")
                {
                    while (true)
                    {
                        Console.Write("Unesite rec: ");
                        poruka = Console.ReadLine().Trim().ToLower();
                        binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                        TCPclientSocket.Send(binarnaPoruka);
                        brBajta = TCPclientSocket.Receive(buffer);
                        poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        if (poruka == "izlaz")
                        {
                            Console.WriteLine("cekam izlaz...");
                            Console.ReadLine();
                        }
                    }
                }
                //else if (trenutnaIgra == "PITANJA I ODGOVORI")
                //{
                //    while (true)
                //    {
                //        Console.Write("Unesite slovo(a/b): ");
                //        poruka = Console.ReadLine().Trim().ToLower();
                //        binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                //        TCPclientSocket.Send(binarnaPoruka);
                //        brBajta = TCPclientSocket.Receive(buffer);
                //        poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                //        if (poruka == "izlaz")
                //        {
                //            Console.WriteLine("cekam izlaz...");
                //            Console.ReadLine();
                //        }
                //    }
                //}
            }
                // ===================== OVO NE IDE ====================================
                //try
                //{
                //    while (true)
                //    {

                //        Console.WriteLine("Unesite ime i prezime studenta");
                //        string ImeIPrezime = Console.ReadLine();
                //        //Console.WriteLine("Unesite broj poena studenta");
                //        //student.Poeni = Convert.ToInt32(Console.ReadLine());


                //        using (MemoryStream ms = new MemoryStream())
                //        {
                //            BinaryFormatter bf = new BinaryFormatter();
                //            bf.Serialize(ms, ImeIPrezime);
                //            buffer = ms.ToArray();
                //            clientSocket.Send(buffer);
                //        }

                //        Console.WriteLine("Podaci su uspesno poslati! \n\nDa li zelite da posaljete jos? da/ne");

                //        if (Console.ReadLine().ToLower() == "ne")
                //        {
                //            break;
                //        }

                //    }

                //}
                //catch (SocketException ex)
                //{
                //    Console.WriteLine($"Doslo je do greske tokom slanja:\n{ex}");
                //}
                Console.WriteLine("Klijent zavrsava sa radom");
            Console.ReadKey();
            //TCPclientSocket.Close();
        }

    }
}