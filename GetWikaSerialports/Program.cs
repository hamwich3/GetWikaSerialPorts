using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WikaTest;

namespace GetWikaSerialports
{
    class Program
    {
        static List<Wika> Wikas = new List<Wika>();

        static void Main(string[] args)
        {
            Console.WriteLine("--Galiso Wika Transducer Port Finder--");
            OpenPorts();
            Wait();
            GetIDs();
            ClosePorts();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void OpenPorts()
        {
            String[] portnames = SerialPort.GetPortNames();
            var sortedportnames = portnames.OrderBy(s => Convert.ToInt32(s.Substring(3)));
            Console.WriteLine("COM Ports available:");
            foreach (string p in sortedportnames)
            {
                Wika wika = new Wika();
                try
                {
                    wika.OpenPort(p);
                    Wikas.Add(wika);
                    Console.WriteLine(p);
                }
                catch (Exception)
                {
                    Console.WriteLine(p + ":\tfailed to open!");
                }
            }
        }

        static void Wait()
        {
            Console.Write("Waiting for transducers to power up");
            for (int i = 0; i <= 5; i++)
            {
                Thread.Sleep(500);
                Console.Write(".");
                Thread.Sleep(500);
            }
            Console.WriteLine("");
        }

        static void GetIDs()
        {
            foreach (Wika wika in Wikas)
            {
                if (wika.GetID())
                {
                    Console.WriteLine(wika.Transducer.PortName + ":\t" + wika.ID);
                }
                else
                {
                    Console.WriteLine(wika.Transducer.PortName + ":\tnone");
                }
            }
        }

        static void ClosePorts()
        {
            foreach (Wika wika in Wikas)
            {
                wika.Transducer.Close();
            }
        }

    }
}
