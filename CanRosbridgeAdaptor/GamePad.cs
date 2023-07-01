using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Interop;
using Husty.RosBridge;
using System.Threading;
using System.Runtime.CompilerServices;
namespace CanRosbridgeAdaptor
{
    class GamePad
    {
        int JoyID = 0;
        int Delay = 10;

        public GamePad(int DelayMicroSecond) { Delay = DelayMicroSecond; }

        public async IAsyncEnumerable<sensor_msgs.msg.Joy> AsyncReadGamePad([EnumeratorCancellation] CancellationToken token = default)
        {
            JOYINFOEX data = new();
            int[] buttons = new int[11];
            float[] axes = new float[8];

            int lasthash = 0;



            while (true)
            {

                await Task.Delay(Delay);
                int a = NativeMethods.joyGetPosEx(JoyID, ref data);
                if (data.GetHashCode() == lasthash) continue;
                lasthash = data.GetHashCode();

                for(int i = 0; i < 11; i++)
                {
                    buttons[i] = (int)(0x1 & (data.dwButtons >> i));
                }

                axes[0] = AxeDatatoFloat(data.dwXpos);
                axes[1] = AxeDatatoFloat(data.dwYpos);

                axes[2] = AxeDatatoFloat(data.dwRpos);
                axes[3] = AxeDatatoFloat(data.dwZpos);

                switch(data.dwPOV)
                {
                    case 0:
                        axes[6] = 0;
                        axes[7] = 1;
                        break;
                    case 4500:
                        axes[7] = 1;
                        axes[6] = -1;
                        break;
                    case 9000:
                        axes[6] = -1;
                        axes[7] = 0;
                        break;
                    case 13500:
                        axes[6] = -1;
                        axes[7] = -1;
                        break;
                    case 18000:
                        axes[6] = 0;
                        axes[7] = -1;
                        break;
                    case 22500:
                        axes[6] = 1;
                        axes[7] = -1;
                        break;
                    case 27000:
                        axes[6] = 1;
                        axes[7] = 0;
                        break;
                    case 31500:
                        axes[6] = 1;
                        axes[7] = 1;
                        break;
                    default:
                        axes[6] = 0;
                        axes[7] = 0;
                        break;
                }


                yield return new sensor_msgs.msg.Joy(new(), axes, buttons);
            }


        }

        private float AxeDatatoFloat(uint a)
        {
            //0-65536 to -1 to 1
            return (float)a /(2^15 -1) -1;
        }

        public async IAsyncEnumerable<string> AsyncReadGamePadTest([EnumeratorCancellation] CancellationToken token = default)
        {
            JOYINFOEX data = new();
            int[] buttons = new int[11];
            float[] axes = new float[8];



            while (!token.IsCancellationRequested)
            {
                int a = NativeMethods.joyGetPosEx(JoyID, ref data);
                yield return data.ToString();
                await Task.Delay(Delay);
            }


        }
        public static GamePad[] SearchGamePad()
        {
            List<GamePad> gamePad = new List<GamePad>();

            for (int i = 0; i < NativeMethods.joyGetNumDevs(); i++)
            {

            }
            return gamePad.ToArray();
        }
        static class NativeMethods
        {
            const int JOYSTICKID1 = 0;

            const int MMSYSERR_BADDEVICEID = 2; // The specified joystick identifier is invalid.
            const int MMSYSERR_NODRIVER = 6;    // The joystick driver is not present.
            const int MMSYSERR_INVALPARAM = 11; // An invalid parameter was passed.
            const int JOYERR_PARMS = 165;       // The specified joystick identifier is invalid.
            const int JOYERR_NOCANDO = 166;     // Cannot capture joystick input because a required service (such as a Windows timer) is unavailable.
            const int JOYERR_UNPLUGGED = 167;   // The specified joystick is not connected to the system.

            const int JOY_RETURNX = 0x001;
            const int JOY_RETURNY = 0x002;
            const int JOY_RETURNZ = 0x004;
            const int JOY_RETURNR = 0x008;
            const int JOY_RETURNU = 0x010;
            const int JOY_RETURNV = 0x020;
            const int JOY_RETURNPOV = 0x040;
            const int JOY_RETURNBUTTONS = 0x080;
            const int JOY_RETURNALL = 0x0FF;

            const int JOY_RETURNRAWDATA = 0x100;
            const int JOY_RETURNPOVCTS = 0x200;
            const int JOY_RETURNCENTERED = 0x400;

            [DllImport("winmm.dll")]
            public static extern int joyGetNumDevs();

            [DllImport("winmm.dll")]
            public static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOYINFOEX
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwXpos;
            public uint dwYpos;
            public uint dwZpos;
            public uint dwRpos;
            public uint dwUpos;
            public uint dwVpos;
            public uint dwButtons;
            public uint dwButtonNumber;
            public uint dwPOV;
            public uint dwReserved1;
            public uint dwReserved2;

            public JOYINFOEX()
            {
                dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX));
                dwFlags = (uint)0x80|0x40;
            }
            public override string ToString()
            {
                return "x:" + dwXpos.ToString() + " y:" + dwYpos.ToString() + " z:" + dwZpos.ToString() +
                      " r:" + dwRpos.ToString() + " u:" + dwUpos.ToString() + " v:" + dwVpos.ToString() +
                      "\nbutton:" + Convert.ToString(dwButtons, 2) + " BNum:" + dwButtonNumber.ToString() + " POV:" + dwPOV.ToString();
            }
        }
        public static UInt32 JOY_RETURNX = 0x00000001;
        public static UInt32 JOY_RETURNY = 0x00000002;
        public static UInt32 JOY_RETURNZ = 0x00000004;
        public static UInt32 JOY_RETURNR = 0x00000008;
        public static UInt32 JOY_RETURNU = 0x00000010;
        public static UInt32 JOY_RETURNV = 0x00000020;
        public static UInt32 JOY_RETURNPOV = 0x00000040;
        public static UInt32 JOY_RETURNBUTTONS = 0x00000080;
        public static UInt32 JOY_RETURNALL = (JOY_RETURNX | JOY_RETURNY | JOY_RETURNZ | JOY_RETURNR | JOY_RETURNU | JOY_RETURNV | JOY_RETURNPOV | JOY_RETURNBUTTONS);


    }
}
