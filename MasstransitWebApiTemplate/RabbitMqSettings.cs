namespace MasstransitWebApiTemplate
{
    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; }

        public ushort Port { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSLActive { get; set; }
        public string SSLThumbprint { get; set; }
    }
}
