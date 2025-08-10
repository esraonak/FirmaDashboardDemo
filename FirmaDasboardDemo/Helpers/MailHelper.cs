    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;
    using System.Net.Mail;

    namespace FirmaDasboardDemo.Helpers
    {
        public static class MailHelper
        {
            private static IConfiguration _configuration;

            /// <summary>
            /// MailHelper sınıfını başlatır. Genellikle Program.cs içinde çağrılmalıdır.
            /// </summary>
            public static void Init(IConfiguration configuration)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            /// <summary>
            /// Belirtilen alıcıya e-posta gönderir.
            /// </summary>
            public static bool MailGonder(string aliciEmail, string konu, string icerik)
            {
                try
                {
                    if (_configuration == null)
                        throw new InvalidOperationException("MailHelper.Init(configuration) çağrısı yapılmamış.");

                    var smtpSection = _configuration.GetSection("Smtp");

                    string host = smtpSection["Host"];
                    string username = smtpSection["Username"];
                    string password = smtpSection["Password"];
                    bool enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var ssl) && ssl;
                    int port = int.TryParse(smtpSection["Port"], out var p) ? p : 587;

                    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                        throw new InvalidOperationException("SMTP ayarları eksik.");

                    using (var smtpClient = new SmtpClient(host, port))
                    {
                        smtpClient.Credentials = new NetworkCredential(username, password);
                        smtpClient.EnableSsl = enableSsl;

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(username, "TenteCRM"),
                            Subject = konu,
                            Body = icerik,
                            IsBodyHtml = false
                        };

                        mailMessage.To.Add(aliciEmail);
                        smtpClient.Send(mailMessage);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    // 📄 Hataları loglamak istersen buraya yazabilirsin.
                    return false;
                }
            }
        }
    }
