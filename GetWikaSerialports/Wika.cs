using System;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;

namespace WikaTest
{
    class Wika
	{
		public SerialPort Transducer = new SerialPort();
		public string ID = "";
		
		public Wika()
		{

		}

		private double pressure = 0;
		public double Pressure
        {
            get
            {
                return pressure;
            }

            set
            {
                pressure = value;
            }
        }

		public bool GetID()
		{
			//Transducer.DiscardInBuffer();
			byte[] RequestIDNo = { 0x4B, 0x4E, 0x00, 0x67, 0x0D };
			Transducer.Write(RequestIDNo, 0, 5);

			const int K0x4B = 0;
			const int sign1 = 1;
			const int sign2 = 2;
			const int sign3 = 3;
			const int sign4 = 4;
			const int CS	 = 5;
			const int CR	 = 6;

			byte[] read = new byte[7];
			int NBytes = 0;

			for (int i = 0; i < 7; i++)
			{
				try
				{
					NBytes += Transducer.Read(read, i, 1);
					Thread.Sleep(5);
				}
				catch (Exception)
				{
					return false;
				}
			}

			// If bytes aren't right, discard and return.
			if (NBytes != 7 || read[CR] != 0x0D)
			{
				Transducer.DiscardInBuffer();
				return false;
			}

			// Calculate Checksum
			int sum = read[K0x4B] + read[sign1] + read[sign2] + read[sign3] + read[sign4];
			int checksum = ((sum & 0xFF) ^ 0xFF) + 1;
			// If Checksum is bad, discard and return.
			if (checksum != read[CS])
			{
				Transducer.DiscardInBuffer();
				return false;
			}

			byte[] IDBytes = new byte[4];
			Array.Copy(read, 1, IDBytes, 0, 4);

			ID = System.Text.Encoding.ASCII.GetString(IDBytes);
			return true;
		}

		public void GetPressure()
		{
			const int P0x50 =		0;
			const int HB =			1;
			const int LB =			2;
			const int PFactor =	    3;
			const int CS =			4;
			const int CR =			5;

			int checksum;
			bool pressureIsNeg;
			double PValue;
			bool ExpIsNeg;
			double ExpToBase10;
			double press;

            byte[] RequestPressure = { 0x50, 0x5A, 0x00, 0x56, 0x0D };
            Transducer.Write(RequestPressure, 0, 5);

            byte[] read = new byte[6];
			int NBytes = 0;

			for (int i = 0; i < 6; i++)
			{
				try
				{
					NBytes += Transducer.Read(read, i, 1);
				}
				catch (Exception)
				{
                    Transducer.DiscardInBuffer();
                    return;
				}
			}

			// If bytes aren't right, discard and return.
			if (NBytes != 6 || read[CR] != 0x0D)
			{
				Transducer.DiscardInBuffer();
				return;
			}

			// Calculate Checksum
			int sum = read[P0x50] + read[HB] + read[LB] + read[PFactor];
			checksum = ((sum & 0xFF) ^ 0xFF) + 1;

			// If Checksum is bad, discard and return.
			if (checksum != read[CS])
			{
				Transducer.DiscardInBuffer();
				return;
			}

			// Get Pressure Value
			pressureIsNeg = Convert.ToBoolean((read[HB] & 0x80) >> 7);
			if (pressureIsNeg)
			{
				PValue = 0 - (((read[HB] << 8) + read[LB]) & 0x7FFF);
			}
			else
			{
				PValue = ((read[HB] << 8) + read[LB]) & 0x7FFF;
			}

			// Get exponent value
			ExpIsNeg = Convert.ToBoolean((read[PFactor] & 0x80) >> 7);
			if (ExpIsNeg)
			{
				ExpToBase10 = 0 - ((read[PFactor] & 0x38) >> 3);
			}
			else
			{
				ExpToBase10 = ((read[PFactor] & 0x38) >> 3);
			}

			press = Math.Pow(10, ExpToBase10) * PValue;
			Pressure = press;
		}

        public bool IsOpen()
        {
            return Transducer.IsOpen;
        }

		public void OpenPort(string portName)
		{
            Transducer.PortName = portName;
			Transducer.BaudRate = 9600;
			Transducer.DataBits = 8;
			Transducer.StopBits = StopBits.One;
			Transducer.Parity = Parity.None;
			Transducer.RtsEnable = true;
			Transducer.DtrEnable = true;
			Transducer.ReadTimeout = 50;
			Transducer.WriteTimeout = 50;
			Transducer.NewLine = "\r";
			try
			{
				Transducer.Open();
			}
			catch (Exception e)
			{
                throw e;
			}
		}
	}

}
