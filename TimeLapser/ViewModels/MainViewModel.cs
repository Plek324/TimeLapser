using Accord.Video.FFMPEG;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TimeLapser.ViewModels
{
    class MainViewModel : Screen
    {
        BitmapImage _camFrame;
        string _camURL="rtsp://admin:admin@192.168.178.25/1/videoN";
        int _ticker = 1;
        private DispatcherTimer _clockTimer;

        public BitmapImage camImage { get { return _camFrame; } }

        public MainViewModel()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Tick += Clock_Tick;
            _clockTimer.Interval = TimeSpan.FromMilliseconds(3000);
            _clockTimer.Start();

        }

        private void Clock_Tick(object sender, EventArgs eventArgs)
        {
            _ticker++;
            NotifyOfPropertyChange(() => Ticker);

            loadPicture();
        }

        void loadPicture()
        {
            string address="";
            string[] urlParts = _camURL.Split('/');

            int adrPN = 0;
            while (adrPN < urlParts.Length)
            {
                if ((!urlParts[adrPN].EndsWith(":")) && (urlParts[adrPN].Length > 0))
                {
                    address = urlParts[adrPN];
                    if(address.Contains('@'))
                    {
                        string[] adrParts = address.Split('@');
                        if(adrParts.Length==2)
                        {
                            address = adrParts[1];
                        }
                    }
                    break;
                }
                adrPN++;
            }

            if (IsPortOpen(address, 554, TimeSpan.FromMilliseconds(250)))
            {
                using (var reader2 = new VideoFileReader())
                {
                    Bitmap frame;
                    reader2.Open(_camURL);
                    frame = reader2.ReadVideoFrame();

                    MemoryStream ms = new MemoryStream();
                    frame.Save(ms, ImageFormat.Jpeg);
                    var tempPict = new BitmapImage();
                    tempPict.BeginInit();
                    tempPict.StreamSource = new MemoryStream(ms.ToArray());
                    tempPict.EndInit();
                    ms.Close();

                    _camFrame = tempPict;
                    NotifyOfPropertyChange(() => camImage);
                }

            }
        }

        bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        public int Ticker
        {
            get { return _ticker; }
        }

        public string camURL
        {
            get { return _camURL; }
            set { _camURL = value; }
        }

    }
}
