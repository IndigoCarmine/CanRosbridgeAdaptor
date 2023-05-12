using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Husty.Extensions;
using Husty.Communication;
using Husty.RosBridge;
using System.Runtime.CompilerServices;
using System.IO.Ports;
using HidSharp;

namespace CanRosbridgeAdaptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //For ROS_bridge
        WebSocketDataTransporter stream;
        RosSubscriber<sensor_msgs.msg.Joy> joySubscriber;
        RosSubscriber<can_plugins2.msg.Frame> canSubscriber;
        RosPublisher<can_plugins2.msg.Frame> canPublisher;

        //For USBCAN
        SerialPort usbcan;

        //For JoyStick
        
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void rosConnect()
        {
            try
            {
                stream = await WebSocketDataTransporter.CreateClientAsync("localhost", 9090);
                if (stream.IsOpened)
                {
                    //
                    joySubscriber = RosSubscriber<sensor_msgs.msg.Joy>.Create(stream, "/Joy");
                    canSubscriber = RosSubscriber<can_plugins2.msg.Frame>.Create(stream, "/can_tx");
                    canPublisher = RosPublisher<can_plugins2.msg.Frame>.Create(stream, "/can_tx");

                    canSubscriber.MessageReceived.Subscribe(frame => { Console.WriteLine(frame); });

                }
                else
                {
                    log("cannot open");
                    
                    stream.Dispose();
                }
            }catch(Exception ex)
            {
                log("cannot connect");
            }
        }
        void log(string msg)
        {
            Dispatcher.Invoke(() => { LogText.Text = msg; });
        }

        private void USBCAN_ButtonClick(object sender, RoutedEventArgs e)
        {
            PortsCombo.ItemsSource = SerialPort.GetPortNames();
        }
        private void ROS_ButtonClick(object sender, RoutedEventArgs e)
        {
            rosConnect();
        }

        private void Port_ButtonClick(object sender, RoutedEventArgs e)
        {
            string portName = (string)PortsCombo.SelectedItem;
            usbcan = new(portName);
            try
            {
                usbcan.Open();
            }catch(Exception ex) {
                log("cannot connect (usbcan)");
            }
        }
    }
}
