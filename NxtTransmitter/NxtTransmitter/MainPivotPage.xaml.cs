using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace NxtTransmitter
{
    public partial class MainPivotPage : PhoneApplicationPage
    {
        private static readonly Color[] forward = { Colors.White, Colors.LightGray };
        private static readonly Color[] reverse = { Colors.White, Colors.DarkGray };
        private static readonly Color[] left = { Colors.White, Colors.Gray };
        private static readonly Color[] right = { Colors.LightGray, Colors.White };
        private static readonly Color[] stop = { Colors.LightGray, Colors.DarkGray };
        private static readonly Color[] grab = { Colors.LightGray, Colors.Gray };
        private static readonly Color[] drop = { Colors.DarkGray, Colors.White };

        private static readonly Dictionary<string, Color[]> actionToTokens = new Dictionary<string, Color[]>()
            {
                {"forward", forward},
                {"reverse", reverse},
                {"left", left},
                {"right", right},
                {"stop", stop},
                {"grab", grab},
                {"drop", drop}
            };
        private static readonly Dictionary<Color, SolidColorBrush> tokenBrush = new Dictionary<Color, SolidColorBrush>() 
            {
                { Colors.Black, new SolidColorBrush(Colors.Black) },
                { Colors.White, new SolidColorBrush(Colors.White) },
                { Colors.LightGray, new SolidColorBrush(Colors.LightGray) },
                { Colors.DarkGray, new SolidColorBrush(Colors.DarkGray) },
                { Colors.Gray, new SolidColorBrush(Colors.Gray) } 
            };
         



        private readonly DispatcherTimer pollTimer = new DispatcherTimer();
        private readonly WebClient client = new WebClient();
        private ProgressIndicator progressIndicator;
        private ConfigurationModel configuration;

        private Queue<Color> actions = new Queue<Color>();
        private Queue<string> actionNames = new Queue<string>();
        private int seconds;

        public MainPivotPage()
        {
            InitializeComponent();
            
            configuration = (ConfigurationModel)Resources["appSettings"];
            infoRectangle.Fill = tokenBrush[Colors.Black];

            InitializeTimer();
            InitializeWebClient();
        }

        private void InitializeWebClient()
        {
            client.OpenReadCompleted += OnOpenReadCompleted;
            client.DownloadStringCompleted += OnDownloadStringCompletedDebugJson;
        }

        private void InitializeTimer()
        {
            // timer interval specified as 1 second             
            pollTimer.Interval = TimeSpan.FromSeconds(1);
            // Sub-routine OnTimerTick will be called at every 1 second             
            pollTimer.Tick += OnTimerTick;
        }

        void OnTimerTick(Object sender, EventArgs args)
        {
            currentCommandTextBlock.Text = actionNames.Count != 0 ? actionNames.Dequeue() : "";
            infoRectangle.Fill = actions.Count != 0 ? tokenBrush[actions.Dequeue()] : tokenBrush[Colors.Black];
            if (--seconds == 0)
            {
                seconds = configuration.PollingIntervalSeconds;
                if (!configuration.DebugMode)
                {
                    downloadStatus();
                }
            }
        }

        void OnOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            hideProgressIndicator();

            if (e.Error == null)
            {
                parseAndTransmit(e.Result);
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void hideProgressIndicator()
        {
            SystemTray.IsVisible = false;
            progressIndicator.IsVisible = false;
        }

        void OnDownloadStringCompletedDebugJson(object sender, DownloadStringCompletedEventArgs e)
        {
            hideProgressIndicator();

            if (e.Error == null)
            {
                MessageBox.Show(e.Result);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(e.Result)))
                {
                    parseAndTransmit(stream);
                }
            }
            else
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void parseAndTransmit(Stream stream)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(Winner));
            var winnerReply = jsonSerializer.ReadObject(stream) as Winner;
            string[] winners = winnerReply.winner.Split(';');
            foreach (string winner in winners)
            {
                transmitMessage(winner);
            }
        }

        private void downloadStatus()
        {
            if (client.IsBusy)
            {
                Title = "Busy";
                return;
            }
            Title = "Not Busy";
            SystemTray.IsVisible = true;
            progressIndicator.IsVisible = true;
            Uri uri = new Uri(configuration.ProductionMode ? configuration.ProductionUrl : configuration.DevelopmentUrl);
            if (configuration.DebugJson)
            {
                client.DownloadStringAsync(uri);
            }
            else
            {
                client.OpenReadAsync(uri);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            seconds = configuration.PollingIntervalSeconds;
            actions.Clear();
            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
            pollTimer.Start();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopPolling();
        }

        private void stopPolling()
        {
            actions.Clear();
            stopButton.IsEnabled = false;
            startButton.IsEnabled = true;
            pollTimer.Stop();
            hideProgressIndicator();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            progressIndicator = SystemTray.ProgressIndicator;
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            transmitMessage((string)((Button)sender).Tag);
        }

        private void transmitMessage(string direction)
        {
            if (actionToTokens.ContainsKey(direction))
            {
                actionNames.Enqueue(direction);
                push(actionToTokens[direction]);
            }
        }

        private void push(Color[] colors)
        {
            if (colors != null)
            {
                foreach (Color color in colors)
                {
                    actions.Enqueue(color);
                }
            }

            actions.Enqueue(Colors.Black);
        }

        private void whiteRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            lightGrayRectangle.Visibility = Visibility.Visible;
            whiteRectangle.Visibility = Visibility.Collapsed;
        }

        private void lightGrayRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            darkGrayRectangle.Visibility = Visibility.Visible;
            lightGrayRectangle.Visibility = Visibility.Collapsed;
        }

        private void darkGrayRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            grayRectangle.Visibility = Visibility.Visible;
            darkGrayRectangle.Visibility = Visibility.Collapsed;
        }

        private void grayRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            darkSlateGrayRectangle.Visibility = Visibility.Visible;
            grayRectangle.Visibility = Visibility.Collapsed;
        }

        private void darkSlateGrayRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            blackRectangle.Visibility = Visibility.Visible;
            darkSlateGrayRectangle.Visibility = Visibility.Collapsed;
        }

        private void blackRectangle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            whiteRectangle.Visibility = Visibility.Visible;
            blackRectangle.Visibility = Visibility.Collapsed;
        }


    }

    [DataContract] 
    public class Winner
    {
        [DataMember]
        public string winner { get; set; }
    }
}