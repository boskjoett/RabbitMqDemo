using System;
using System.Text;
using System.Windows;
using RabbitMQ.Client;

namespace Publisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectionFactory rabbitMqFactory;
        private IConnection rabbitMqConnection;
        private IModel rabbitMqChannel;
        private string exchange;

        public MainWindow()
        {
            InitializeComponent();

            ButtonDisconnect.IsEnabled = false;
            ButtonPublish.IsEnabled = false;
            ButtonDelete.IsEnabled = false;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                exchange = TextBoxExchange.Text;

                rabbitMqFactory = new ConnectionFactory() { HostName = TextBoxHost.Text, UserName = "zyuser", Password = "password" };

                rabbitMqConnection = rabbitMqFactory.CreateConnection();
                rabbitMqChannel = rabbitMqConnection.CreateModel();

                // Create an exchange of type "topic"
                rabbitMqChannel.ExchangeDeclare(exchange: exchange,
                                                type: "topic",
                                                durable: false);

                ButtonConnect.IsEnabled = false;
                ButtonDisconnect.IsEnabled = true;
                ButtonDelete.IsEnabled = true;
                ButtonPublish.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
        }

        private void ButtonPublish_Click(object sender, RoutedEventArgs e)
        {
            string routingKey = TextBoxRoutingKey.Text;
            var body = Encoding.UTF8.GetBytes(TextBoxMessage.Text);

            rabbitMqChannel.BasicPublish(exchange: exchange,
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            rabbitMqChannel.ExchangeDelete(exchange);
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            rabbitMqChannel?.Dispose();
            rabbitMqConnection?.Dispose();

            rabbitMqChannel = null;
            rabbitMqConnection = null;
            rabbitMqFactory = null;

            ButtonConnect.IsEnabled = true;
            ButtonDisconnect.IsEnabled = false;
            ButtonPublish.IsEnabled = false;
            ButtonDelete.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rabbitMqChannel?.Dispose();
            rabbitMqConnection?.Dispose();
        }
    }
}
