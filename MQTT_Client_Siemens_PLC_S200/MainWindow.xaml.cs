using System;
using System.Threading.Tasks;
using System.Windows;


using S7.Net;
using MQTTnet.Client;
using MQTTnet;

namespace MQTT_Client_Siemens_PLC_S200
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Plc plc;
        private bool isListening;

        // MQTT client instance
        private IMqttClient mqttClient;

        public MainWindow()
        {
            InitializeComponent();

            // Create MQTT client instance
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
        }




        private async void StartListeningButton_Click(object sender, RoutedEventArgs e)
        {
            if (plc == null)
            {
                // Create an instance of the S7.Net Plc object
                plc = new Plc(CpuType.S7200, "192.168.0.25", 0, 1);

                // Open the connection to the PLC
                plc.Open();
            }

            isListening = true;

            await Task.Run(async () =>
            {
                while (isListening)
                {
                    // Read the Q0.0 output
                    bool outputStatus = (bool)plc.Read("Q0.0");

                    // Publish MQTT topic based on output status
                    await PublishMqttTopic(outputStatus);
                    // Update UI or perform any actions based on the output status
                    UpdateUI(outputStatus);

                    // Delay before the next read
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }




        private async Task PublishMqttTopic(bool outputStatus)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("status")
                    .WithPayload(outputStatus ? "Open" : "Close")
                    .Build();

                var mqttOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("X.X.X.X", 1883) // Replace with your MQTT broker details
                    .Build();

                await mqttClient.ConnectAsync(mqttOptions);
                await mqttClient.PublishAsync(message);
                await mqttClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                // Handle MQTT publishing errors here
            }
        }


        private void StopListeningButton_Click(object sender, RoutedEventArgs e)
        {
            isListening = false;

            if (plc != null)
            {
                // Close the connection to the PLC
                plc.Close();
                plc = null;
            }
        }

        private void UpdateUI(bool outputStatus)
        {
            // Update UI elements or perform actions based on the PLC output status
            Dispatcher.Invoke(() =>
            {
                if (outputStatus)
                {
                    StatusTextBlock.Text = "Current Status: Open"; // Update the text block to show "Open"
                }
                else
                {
                    StatusTextBlock.Text = "Current Status: Close"; // Update the text block to show "Close"
                }
            });
        }

    }
}
