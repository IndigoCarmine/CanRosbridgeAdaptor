using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CanRosbridgeAdaptor
{
    public class UsbCan {
        private SerialPort port;

        //If the class get handshake, it is true.
        public bool IsActive
            { get; private set; } = false;

        Task handshake;

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



            handshake = new(() =>
            {
                while (!IsActive)
                {
                    Handshake();
                    Task.Delay(100);
                }
            });
            handshake.Start();
            
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
        private void NegotiationProcess(uint buffer_index, byte[] buffer)
        {

            byte[] HelloSlcan = "HelloSLCAN"u8.ToArray();

            if (buffer_index != HelloSlcan.Length + 1 || buffer[0] != 1 << 4) return;
            
                

            for (int i = 0; i < HelloSlcan.Length; i++)
            {
                if (buffer[i + 1] != HelloSlcan[i]) return;
            }
            IsActive = true;

        }
        public async IAsyncEnumerable<can_plugins2.msg.Frame> ReadAsyc()
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

                    NegotiationProcess(buffer_index,buffer);

                    //generate frame
                    can_plugins2.msg.Frame frame;
                    try
                    {

                        var result = COBS.Decode(buffer);
                        if (result.Length < 6) continue;

                        frame = new(
                            new(),
                            (uint)(result[1] << 24 | result[2] << 16 | result[3] << 8 | result[4]),
                            Convert.ToBoolean((result[0]>>2)&1),
                            Convert.ToBoolean((result[1]>>1)&1),
                            Convert.ToBoolean((result[2])&1),
                            result[5],
                            new byte[result[5]]
                            );
                        //copy the data to frame 
                        for(int i = 0; i < result[5]; i++) {
                            frame.data[i] = result[i + 6];
                        }


                    }
                    catch {
                        continue;
                    }
                    yield return frame;

                    buffer_index = 0;

                    
                }
            }
            yield return new(new(), 1, true, true, true, 1,new byte[1]);
        }

        public void WriteAsync(can_plugins2.msg.Frame frame)
        {
            byte[] buffer = new byte[1 + 4 + 1 + frame.dlc];

            byte[] encoded_data = new byte[1 + 4 + 1 + frame.dlc + 2];

            for (int i = encoded_data.Length-1; i <= 0; i++)
            {
                if (encoded_data[i] == 0) ;
            }

        }
    } 
}