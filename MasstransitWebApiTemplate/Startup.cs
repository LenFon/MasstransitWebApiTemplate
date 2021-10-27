using HealthChecks.UI.Client;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MasstransitWebApiTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //services.Configure<HealthCheckPublisherOptions>(options =>
            //{
            //    options.Delay = TimeSpan.FromSeconds(2);
            //    options.Predicate = (check) => check.Tags.Contains("ready");
            //});
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MasstransitWebApiTemplate", Version = "v1" });
            });
            services.Configure<RabbitMqSettings>(Configuration.GetSection("RabbitMqSettings"));
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq(ConfigureBus);
                x.AddConsumers(typeof(Startup).Assembly);
            });

            services.AddMassTransitHostedService(true);

            //services.AddHealthChecks();
            services
                .AddHealthChecksUI(settings =>
                {
                    settings.AddHealthCheckEndpoint("MasstransitDemo", "/hc");
                })
                .AddInMemoryStorage();
        }

        private void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg)
        {

            var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            cfg.Host(settings.Host, settings.Port, settings.VirtualHost, h =>
            {
                h.Username(settings.Username);
                h.Password(settings.Password);

                if (settings.SSLActive)
                {
                    h.UseSsl(ssl =>
                    {
                        ssl.ServerName = Dns.GetHostName();
                        ssl.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateNameMismatch);
                        ssl.Certificate = GetX509Certificate(settings);
                        ssl.Protocol = SslProtocols.Tls12;
                        ssl.CertificateSelectionCallback = CertificateSelectionCallback;
                    });
                }
            });

            cfg.UseSerilogMessagePropertiesEnricher();
            cfg.ConfigureEndpoints(context);

            X509Certificate CertificateSelectionCallback(object sender, string targethost, X509CertificateCollection localcertificates, X509Certificate remotecertificate, string[] acceptableissuers)
            {
                var serverCertificate = localcertificates.OfType<X509Certificate2>()
                                        .FirstOrDefault(cert => cert.Thumbprint.ToLower() == settings.SSLThumbprint.ToLower());

                return serverCertificate ?? throw new Exception("Wrong certificate");
            }

            X509Certificate GetX509Certificate(RabbitMqSettings settings)
            {
                X509Certificate2 x509Certificate = null;

                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                try
                {
                    X509Certificate2Collection certificatesInStore = store.Certificates;

                    x509Certificate = certificatesInStore.OfType<X509Certificate2>()
                        .FirstOrDefault(cert => cert.Thumbprint?.ToLower() == settings.SSLThumbprint?.ToLower());
                }
                finally
                {
                    store.Close();
                }

                return x509Certificate;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MasstransitWebApiTemplate v1"));
            }
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/hc-ui";
                });
            });
        }
    }
}
