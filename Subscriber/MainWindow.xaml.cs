using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Subscriber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectionFactory rabbitMqFactory;
        private IConnection rabbitMqConnection;
        private IModel rabbitMqChannel;

        public MainWindow()
        {
            InitializeComponent();

            ButtonDisconnect.IsEnabled = false;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string exchange = TextBoxExchange.Text;
                string bindingKey = TextBoxBindingKey.Text;

                rabbitMqFactory = new ConnectionFactory() { HostName = TextBoxHost.Text, UserName = "zyuser", Password = "password" };

                rabbitMqConnection = rabbitMqFactory.CreateConnection();
                rabbitMqChannel = rabbitMqConnection.CreateModel();

                // Create an exchange of type "topic"
                rabbitMqChannel.ExchangeDeclare(exchange: exchange,
                                                type: "topic",
                                                durable: false);

                // Declare a queue and bind it to the exchange
                var queueName = rabbitMqChannel.QueueDeclare().QueueName;

                rabbitMqChannel.QueueBind(queue: queueName,
                                          exchange: exchange,
                                          routingKey: bindingKey);

                var consumer = new EventingBasicConsumer(rabbitMqChannel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    string listBoxText = $"{ea.RoutingKey}: {message}";
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ListBoxMessages.Items.Add(listBoxText)));
                };

                rabbitMqChannel.BasicConsume(queue: queueName,
                                             autoAck: true,
                                             consumer: consumer);

                ButtonConnect.IsEnabled = false;
                ButtonDisconnect.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
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
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            ListBoxMessages.Items.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rabbitMqChannel?.Dispose();
            rabbitMqConnection?.Dispose();
        }
    }
}
