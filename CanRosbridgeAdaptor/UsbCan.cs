using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CanRosbridgeAdaptor
{
    public class UsbCan {
        private SerialPort port;

        //If the class get handshake, it is true.
        public bool IsActive{ get; private set; } = false;

        //If the port is open, it is true
        public bool IsOpen
        {
            get
            {
                return port.IsOpen;
            }
        }

        public UsbCan(string portName)
        {
            port = new SerialPort(portName);   
            
        }
        public void Open()
        {
            port.Open();
        }
        public async void Handshake()
        {
            byte[] HelloUSBCAN = "HelloUSBCAN/0"u8.ToArray();

            while (IsOpen&&IsActive)
            {
                port.Write(HelloUSBCAN, 0, HelloUSBCAN.Length);
                await Task.Delay(100);
            }   

        }
        async IAsyncEnumerable<can_plugins2.msg.Frame> ReadAsyc()
        {
            byte[] buffer = new byte[64];
            uint buffer_index = 0;
            while (!port.IsOpen)
            {
                await Task.Delay(100);
            }
            while(port.IsOpen)
            {
                // data structure
                /*
                uint8_t command & frame_type: (command: if it is normal can frame, it is 0x00.)<<4 | is_rtr << 2 | is_extended << 1 | is_error
                uint8_t id[4] : can id
                uint8_t dlc : data length
                uint8_t data[8] : data
                */

                buffer[buffer_index+1] = (byte)port.ReadByte();
                if(buffer[buffer_index + 1] == 0)
                {
                    //finish or error

                }
            }
            yield return new(new(), 1, true, true, true, 1,new byte[1]);
        }
    } 
}