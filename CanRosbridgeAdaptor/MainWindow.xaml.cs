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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace CanRosbridgeAdaptor
{
    
    public class ViewModelBase : INotifyPropertyChanged
    {
        // INotifyPropertyChanged を実装するためのイベントハンドラ
        public event PropertyChangedEventHandler PropertyChanged;

        // プロパティ名によって自動的にセットされる
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class MainWindowViewModel: ViewModelBase
    {
        private bool _rosConnected = false;
        public bool rosConnected
        {
            set
            {
                if (_rosConnected != value)
                {
                    _rosConnected = value;
                    OnPropertyChanged();
                }
            }
            get { return _rosConnected; }
        }

             

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //For ROS_bridge
        WebSocketDataTransporter stream;
        RosPublisher<sensor_msgs.msg.Joy> joyPublisher;
        RosSubscriber<can_plugins2.msg.Frame> canSubscriber;
        RosPublisher<can_plugins2.msg.Frame> canPublisher;

        //For USBCAN
        UsbCan usbcan;

        //For JoyStick
        GamePad gamePad;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }


        private async void rosConnect()
        {
            try
            {
                stream = await WebSocketDataTransporter.CreateClientAsync("localhost", 9090);
                if (stream.IsOpened)
                {
                    //
                    joyPublisher = RosPublisher<sensor_msgs.msg.Joy>.Create(stream, "joy");
                    canSubscriber = RosSubscriber<can_plugins2.msg.Frame>.Create(stream, "can_tx");
                    canPublisher = RosPublisher<can_plugins2.msg.Frame>.Create(stream, "can_rx");

                    canSubscriber.MessageReceived.Subscribe(frame => {
                        log(frame.ToString());
                    });
                    statusTextUpdate("Connect"); 
                    ((MainWindowViewModel)DataContext).rosConnected = true;
                }
                else
                {
                    statusTextUpdate("cannot open");
                    
                    stream.Dispose();
                }
            }catch(Exception ex)
            {
                statusTextUpdate("cannot connect");
            }
            
        }
        void statusTextUpdate(string msg)
        {
            Dispatcher.Invoke(() => { StatusText.Text = msg; });
        }
        void log(string msg)
        {
            //Dispatcher.Invoke(() => { LogPanel.Text += msg + "\n"; });
            Dispatcher.Invoke(() => { LogPanel.Text = msg + "\n"; });

        }
        void log<T>(ICollection<T> data)
        {
            foreach(var item in data)
            {
                log(item!.ToString()?? "");
                log("\n");
            }
        }

        private void Port_ButtonClick(object sender, RoutedEventArgs e)
        {
            PortsCombo.ItemsSource = SerialPort.GetPortNames();
        }
        private void ROS_ButtonClick(object sender, RoutedEventArgs e)
        {
            rosConnect();
        }

        Task canTask;
        private void USBCAN_ButtonClick(object sender, RoutedEventArgs e)
        {
            string portName = (string)PortsCombo.SelectedItem;
            usbcan = new(portName);
            try
            {
                usbcan.Open();
                statusTextUpdate("USB Connect");
            }catch(Exception ex) {
                statusTextUpdate("cannot connect (usbcan)");
            }

            canTask = new(async () =>
            {
                await foreach (var data in usbcan.ReadAsyc())
                {
                    //await canPublisher.WriteAsync(data);
                    log(data.ToString());
                }
            });
            canTask.Start();

            Task task = new(() =>
            {
                while (usbcan.IsActive)
                {
                    Task.Delay(100);
                }
                log("Handshake");
            });
            task.Start();
            
        }
        Task gamepadTask;
        private void Joy_ButtonClick(object sender, RoutedEventArgs e)
        {
            gamePad = new GamePad(10);
            gamepadTask = new(async () =>
            {
                await foreach (var data in gamePad.AsyncReadGamePad())
                {
                    await joyPublisher.WriteAsync(data);
                    log(data.ToString());
                }
            });

            gamepadTask.Start();
        }
    }
}
