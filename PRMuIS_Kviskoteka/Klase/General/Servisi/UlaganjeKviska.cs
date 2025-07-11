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
            string odgovor = string.Empty;
            int brBajta = 0;
            int brojOdgovora = 0;

            string mozeKvisko = "1Da li zelite da ulozite kviska? (da/ne): ";
            string neMozeKvisko = "0Vec ste ulozili kviska u ovoj rundi...";

            foreach (Socket s in klijenti)
            {
                if (s == klijenti[0])
                    if (!igrac1.kvisko)
                        s.Send(Encoding.UTF8.GetBytes(mozeKvisko));
                    else
                        s.Send(Encoding.UTF8.GetBytes(neMozeKvisko));

                if(s == klijenti[1])
                    if (!igrac2.kvisko)
                        s.Send(Encoding.UTF8.GetBytes(mozeKvisko));
                    else
                        s.Send(Encoding.UTF8.GetBytes(neMozeKvisko));
            }

            while (brojOdgovora < 2)
            {
                List<Socket> checkRead = new List<Socket>();
                List<Socket> checkError = new List<Socket>();

                foreach(Socket s in klijenti)
                {
                    checkRead.Add(s);
                    checkError.Add(s);
                }

                Socket.Select(checkRead, null, checkError, 1000);

                if(checkRead.Count > 0)
                {
                    foreach(Socket s in checkRead)
                    {
                        brBajta = s.Receive(buffer);
                        brojOdgovora++;
                        odgovor = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        if (odgovor == "da")
                        {
                            if (s == klijenti[0])
                                igrac1.ulaganjeKviska(idx);
                            else
                                igrac2.ulaganjeKviska(idx);
                        }
                    }
                }

                checkRead.Clear();
                checkError.Clear();
            }

            Console.Clear();
        }
    }
}


            


            

            
