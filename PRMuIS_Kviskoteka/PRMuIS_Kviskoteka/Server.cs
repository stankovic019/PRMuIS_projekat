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
using System.Diagnostics;
using Klase.General.Servisi;
#pragma warning disable SYSLIB0011

namespace PRMuIS_Kviskoteka
{
 
    //PROJEKAT 25 - KVISKOTEKA
    //ČLANOVI TIMA: Dimitrije Stanković PR81/2022
    //              Vojin Jovanović PR82/2022
    //FTN, ŠKOLSKA 2024/25.
    
    internal class Server
    {
        static List<Igrac> igraci = new List<Igrac>();
        static List<EndPoint> multiplayerEndPoints = new List<EndPoint>();
        static Igrac igrac;
        static bool prijavljen = false;

        static void Main(string[] args)
        {
   
            UDPKonekcija();
            Console.ReadLine();

        }

        static void UDPKonekcija() {

            Socket UDPserverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50001); //dimitrije ip/port
            //IPEndPoint UDPserverEP = new IPEndPoint(IPAddress.Any, 50002); //vojin ip/port
            UDPserverSocket.Bind(UDPserverEP);

            Console.WriteLine($"UDP Server je pokrenut i ceka poruku na: {UDPserverEP}");

            EndPoint UDPposiljaocEP = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine(UDPposiljaocEP.ToString() );
            PokreniKlijente(1); //singleplayer
            PokreniKlijente(1); //multiplayer


            while (!prijavljen) 
            {
                byte[] prijemniBafer = new byte[1024];

                try
                {
                    int brBajta = UDPserverSocket.ReceiveFrom(prijemniBafer, ref UDPposiljaocEP);
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                    Console.WriteLine($"{UDPposiljaocEP}: {poruka}");

                    //proveravamo da li je lepo poslata prijava korisnika
                    
                    if (Regex.IsMatch(poruka, "([a-zA-Z0-9_]+)(,\\s*[a-zA-Z0-9_]+)+") && proveriUnos(poruka))
                    {
                        igrac = new Igrac(poruka.Split(","));
                        poruka = "Ispravno prijavljen korisnik '" + igrac.username + "'";
                        byte[] binarnaPoruka = Encoding.UTF8.GetBytes("1Server - " + poruka);
                        brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP);
                        Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                        TCPKonekcijaJedanKorisnik();
                        break;
                    }
                    else if (Regex.IsMatch(poruka, "([a-zA-Z0-9_]+)"))
                    {
                        igraci.Add(new Igrac(poruka));
                        multiplayerEndPoints.Add(UDPposiljaocEP);
                        
                        if (igraci.Count == 2)
                        {
                            Console.WriteLine("pokrecem igru...");
                            poruka = "2";
                            byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                            foreach(EndPoint ep in multiplayerEndPoints)
                                brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, ep);
                            TCPKonekcijaDvaKorisnika();
                            break;

                        }
                        else
                        {
                            Console.WriteLine("Cekam prijavu drugog igraca...");

                        }
                    }
                    else
                        {
                            poruka = "Neispravan unos. Pokusajte ponovo.";
                            byte[] binarnaPoruka = Encoding.UTF8.GetBytes("0Server - " + poruka);
                            brBajta = UDPserverSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, UDPposiljaocEP);
                            Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
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

        static void TCPKonekcijaJedanKorisnik()
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


        static void TCPKonekcijaDvaKorisnika()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

            serverSocket.Bind(serverEP);
            serverSocket.Blocking = false;
            int maxKlijenata = 2;
            serverSocket.Listen(maxKlijenata);


            Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");



            List<Socket> klijenti = new List<Socket>(); // Pravimo posebnu listu za klijentske sokete 

            byte[] buffer = new byte[1024];
            string poruka = string.Empty;
            byte[] binarnaPoruka;
            int starting = 0;
            try
            {

                while (true)
                {
                    List<Socket> checkRead = new List<Socket>();
                    List<Socket> checkError = new List<Socket>();

                    if (klijenti.Count < maxKlijenata)
                    {
                        checkRead.Add(serverSocket);

                    }
                    checkError.Add(serverSocket);

                    foreach (Socket s in klijenti)
                    {
                        checkRead.Add(s);
                        checkError.Add(s);
                    }


                    Socket.Select(checkRead, null, checkError, 1000);


                    if (checkRead.Count > 0)
                    {
                        foreach (Socket s in checkRead)
                        {
                            if (s == serverSocket)
                            {

                                Socket client = serverSocket.Accept();
                                client.Blocking = false;
                                klijenti.Add(client);
                                Console.WriteLine($"Klijent se povezao sa {client.RemoteEndPoint}");
                                poruka = $"Vasa TCP/IP adresa i port su: {client.RemoteEndPoint}";
                                binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                                client.Send(binarnaPoruka);
                            }

                            else if (starting != 2)
                            {
                                int brBajta = s.Receive(buffer);
                                if (brBajta == 0)
                                {
                                    Console.WriteLine("Klijent je prekinuo komunikaciju");
                                    s.Close();
                                    klijenti.Remove(s);

                                    continue;
                                }
                                else
                                {
                                    poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                                    if (poruka == "start")
                                        starting++;

                                    if (starting == 2)
                                    {
                                        Console.WriteLine("Pokrecem igre...");
                                        Thread.Sleep(1000);
                                    }
                                    else
                                        Console.WriteLine("Cekam drugog igraca...");
                                }
                            }
    
                        }
                        if (starting == 2)
                        {
                            foreach (Socket s in klijenti)
                                s.Send(Encoding.UTF8.GetBytes("start"));

                           UlaganjeKviska kvisko = new UlaganjeKviska(igraci[0], igraci[1]);
                            for (int i = 0; i < 3; ++i)
                            {

                                //kvisko.Ulozi(klijenti, serverSocket, i);

                                string igra = igraci[0].getIgra(i); //sve jedno je, iste igre igraju
                                Console.Clear();
                                if (igra == "an")
                                {
                                    IgraAnagrama anagram = new IgraAnagrama(igraci[0], igraci[1]);
                                    poruka = "ANAGRAM";
                                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                                    foreach (Socket s in klijenti)
                                        s.Send(binarnaPoruka);
                                    anagram.Igraj(klijenti, serverSocket);
                                    Console.WriteLine("Ukupni poeni u igri 'Anagram':");
                                    foreach (Igrac ig in igraci)
                                    {
                                        Console.WriteLine($"\t{ig.username} :  {ig.poeniUTrenutnojIgri}");
                                        ig.dodeliPoene(i);
                                    }
                                    Thread.Sleep(2000);
                                    continue;
                                }
                                else if (igra == "po")
                                {
                                    IgraPitanjaIOdgovora pitanjaIodg = new IgraPitanjaIOdgovora(igraci[0], igraci[1]);
                                    poruka = "PITANJA I ODGOVORI";
                                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                                    foreach (Socket s in klijenti)
                                        s.Send(binarnaPoruka);
                                    pitanjaIodg.IgrajDvaIgraca(klijenti, serverSocket);
                                    Console.WriteLine("Ukupni poeni u igri 'Pitanja i Odgovori':");
                                    foreach (Igrac ig in igraci)
                                    {
                                        Console.WriteLine($"\t{ig.username} :  {ig.poeniUTrenutnojIgri}");
                                        ig.dodeliPoene(i);
                                    }
                                    Thread.Sleep(2000);
                                    continue;
                                }

                                else if (igra == "as")
                                {
                                    IgraAsocijacija asocijacija = new IgraAsocijacija(igraci[0], igraci[1]);
                                    poruka = "ASOCIJACIJE";
                                    binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                                    foreach (Socket s in klijenti)
                                        s.Send(binarnaPoruka);
                                    asocijacija.Igraj(klijenti, serverSocket);
                                    Console.Clear();
                                    Console.WriteLine("Ukupni poeni u igri 'Asocijacije': ");
                                    foreach (Igrac ig in igraci)
                                    {
                                        Console.WriteLine($"\t{ig.username} :  {ig.poeniUTrenutnojIgri}");
                                        ig.dodeliPoene(i);
                                    }
                                    Thread.Sleep(2000);
                                    Console.Clear();
                                    continue;
                                }
                            }
                            break;
                        }
                        checkRead.Clear();
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Doslo je do greske {ex}");
            }


            foreach (Igrac i in igraci)
                Console.WriteLine(i + "\n");


            if (igraci[0].ukupnoPoena > igraci[1].ukupnoPoena)
            {
                klijenti[0].Send(Encoding.UTF8.GetBytes("Pobedili ste!"));
                klijenti[1].Send(Encoding.UTF8.GetBytes("Izgubili ste."));
            }
            else if(igraci[0].ukupnoPoena > igraci[1].ukupnoPoena)
            {
                klijenti[1].Send(Encoding.UTF8.GetBytes("Pobedili ste!"));
                klijenti[0].Send(Encoding.UTF8.GetBytes("Izgubili ste."));
            }
            else
            {
                if (igraci[0].poeniSaKviskom > igraci[1].poeniSaKviskom)
                {
                    klijenti[0].Send(Encoding.UTF8.GetBytes("Pobedili ste!"));
                    klijenti[1].Send(Encoding.UTF8.GetBytes("Izgubili ste."));
                }
                else if (igraci[0].poeniSaKviskom < igraci[1].poeniSaKviskom)
                {
                    klijenti[1].Send(Encoding.UTF8.GetBytes("Pobedili ste!"));
                    klijenti[0].Send(Encoding.UTF8.GetBytes("Izgubili ste."));
                }
                else
                {
                    klijenti[1].Send(Encoding.UTF8.GetBytes("Nereseno je."));
                    klijenti[0].Send(Encoding.UTF8.GetBytes("Nereseno je."));
                }
            }
                Console.WriteLine("Server zavrsava sa radom");
            Console.ReadKey();
            serverSocket.Close();

        }

        static void PokreniKlijente(int brojKlijenata)
        {
            for (int i = 0; i < brojKlijenata; i++)
            {
                // Putanja do izvršnog fajla klijenta (potrebno je kompajlirati ga)
                string trenutniDir = AppDomain.CurrentDomain.BaseDirectory;
               
                // Relativna putanja do klijenta
                string relativnaPutanja = Path.Combine("..", "..", "..", "..", "PRMuIS_Kviskoteka_Client", "bin", "Debug", "net5.0", "PRMuIS_Kviskoteka_Client.exe");
        
                // Kombinuj da dobiješ punu putanju
                string clientPath = Path.GetFullPath(Path.Combine(trenutniDir, relativnaPutanja));
                //Console.WriteLine(clientPath);
                //Console.ReadKey();
                if (!File.Exists(clientPath))
                {
                    Console.WriteLine("Fajl nije pronađen: " + clientPath);
                    break;
                }

                Process klijentProces = new Process(); // Stvaranje novog procesa
                klijentProces.StartInfo.FileName = clientPath; //Zadavanje putanje za pokretanje
                klijentProces.StartInfo.Arguments = $"{i + 1}"; // Argument - broj klijenta
                klijentProces.StartInfo.UseShellExecute = true; //otvara u novoj konzoli
                klijentProces.Start(); // Pokretanje klijenta
            }
        }
    }
}
