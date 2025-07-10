using Klase.General.Modeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klase.General.Servisi
{
    public class UlaganjeKviska
    {
        Igrac igrac1, igrac2;

        public UlaganjeKviska(Igrac igrac1, Igrac igrac2)
        {
            this.igrac1 = igrac1;
            this.igrac2 = igrac2;
        }

        public void Ulozi(List<Socket> klijenti, Socket server, int idx)
        {
            Console.Clear();
            Console.WriteLine("Ulaganje Kviska...");
            byte[] buffer = new byte[1024];
            string poruka = string.Empty;
            int pristigliOdgovori = 0;

        
            if (igrac1.kvisko)
            {
                klijenti[0].Send(Encoding.UTF8.GetBytes("0"));
                pristigliOdgovori++;
            }
            else
                klijenti[0].Send(Encoding.UTF8.GetBytes("1"));


            if (igrac2.kvisko)
            {
                klijenti[1].Send(Encoding.UTF8.GetBytes("0"));
                pristigliOdgovori++;
            }
            else
                klijenti[1].Send(Encoding.UTF8.GetBytes("1"));

            try
            {

                while (true)
                {
                    List<Socket> checkRead = new List<Socket>();
                    List<Socket> checkError = new List<Socket>();


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

                            int brBajta = s.Receive(buffer);
                            poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                            if (poruka == "da")
                            {
                                pristigliOdgovori++;
                                if (s == klijenti[0])
                                    igrac1.ulaganjeKviska(idx);
                                else
                                    igrac2.ulaganjeKviska(idx);
                            }
                            else if (poruka == "ne")
                                pristigliOdgovori++;

                            if (pristigliOdgovori == 2)
                                return;
                        }

                    }
                }

            }
            catch (Exception e) { }

            Console.Clear();
        }
    }
}


            


            

            
