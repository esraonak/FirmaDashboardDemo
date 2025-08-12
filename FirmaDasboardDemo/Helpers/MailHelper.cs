using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace FirmaDasboardDemo.Helpers
{
    public static class MailHelper
    {
        private static IConfiguration? _configuration;

        /// <summary>
        /// Program.cs içinde bir kez çağırın: MailHelper.Init(builder.Configuration);
        /// </summary>
        public static void Init(IConfiguration configuration)
            => _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        #region DTO
        private sealed class SmtpOptions
        {
            public required string Host { get; init; }
            public int Port { get; init; } = 587;
            public bool EnableSsl { get; init; } = true;
            public required string Username { get; init; }
            public required string Password { get; init; }
            public string? From { get; init; }          // yoksa Username kullanılır
            public string? FromName { get; init; }      // yoksa "TenteCRM"
            public int TimeoutMs { get; init; } = 30000;
        }
        #endregion

        #region Public API (sync)

        /// <summary>
        /// Basit kullanım: true/false döner. Hata metni gerekirse MailGonderDetay kullanın.
        /// </summary>
        public static bool MailGonder(
            string aliciEmail,
            string konu,
            string icerik,
            bool isHtml = false,
            string? replyTo = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<(string path, string? displayName)>? attachments = null)
        {
            var (ok, _) = MailGonderDetay(aliciEmail, konu, icerik, isHtml, replyTo, cc, bcc, attachments);
            return ok;
        }

        /// <summary>
        /// Ayrıntılı sonuç döner. Hata durumunda error doldurulur.
        /// </summary>
        public static (bool ok, string? error) MailGonderDetay(
            string aliciEmail,
            string konu,
            string icerik,
            bool isHtml = false,
            string? replyTo = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<(string path, string? displayName)>? attachments = null)
        {
            try
            {
                var o = GetOptions();

                using var client = new SmtpClient(o.Host, o.Port)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(o.Username, o.Password),
                    EnableSsl = o.EnableSsl,
                    Timeout = o.TimeoutMs
                };

                using var msg = BuildMessage(o, aliciEmail, konu, icerik, isHtml, replyTo, cc, bcc, attachments);
                client.Send(msg);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        #region Public API (async)

        public static async Task<(bool ok, string? error)> MailGonderAsync(
            string aliciEmail,
            string konu,
            string icerik,
            bool isHtml = false,
            string? replyTo = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<(string path, string? displayName)>? attachments = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var o = GetOptions();

                using var client = new SmtpClient(o.Host, o.Port)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(o.Username, o.Password),
                    EnableSsl = o.EnableSsl,
                    Timeout = o.TimeoutMs
                };

                using var msg = BuildMessage(o, aliciEmail, konu, icerik, isHtml, replyTo, cc, bcc, attachments);

                // SmtpClient SendMailAsync iptal token’ı doğrudan desteklemez; best-effort:
                using var _ = cancellationToken.Register(() => client.SendAsyncCancel());
                await client.SendMailAsync(msg);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        #region Internals

        private static SmtpOptions GetOptions()
        {
            if (_configuration is null)
                throw new InvalidOperationException("MailHelper.Init(configuration) çağrısı yapılmamış.");

            var s = _configuration.GetSection("Smtp");

            var host = s["Host"];
            var user = s["Username"];
            var pass = s["Password"]; // Prod'da env var ile gelebilir: Smtp__Password

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                throw new InvalidOperationException("SMTP ayarları eksik: Smtp:Host / Smtp:Username / Smtp:Password");
            }

            // Güvenli From: From boşsa Username; From dolu ve Username'den farklıysa Gmail 550'ye düşmemek için Username'e zorla
            var from = s["From"];
            if (!string.IsNullOrWhiteSpace(from) &&
                !string.Equals(from, user, StringComparison.OrdinalIgnoreCase))
            {
                // Eğer Gmail'de 'Send mail as' ile doğrulamadıysan 550 alırsın; bu yüzden zorla Username'e çeviriyoruz.
                from = user;
            }

            return new SmtpOptions
            {
                Host = host!,
                Username = user!,
                Password = pass!,
                Port = TryParse(s["Port"], 587),
                EnableSsl = TryParseBool(s["EnableSsl"], true),
                From = from, // null/empty ise BuildMessage'ta yine Username kullanılacak
                FromName = string.IsNullOrWhiteSpace(s["FromName"]) ? "TenteCRM" : s["FromName"],
                TimeoutMs = TryParse(s["TimeoutMs"], 30000)
            };
        }

        private static int TryParse(string? value, int fallback)
            => int.TryParse(value, out var x) ? x : fallback;

        private static bool TryParseBool(string? value, bool fallback)
            => bool.TryParse(value, out var b) ? b : fallback;

        private static MailMessage BuildMessage(
            SmtpOptions o,
            string to,
            string subject,
            string body,
            bool isHtml,
            string? replyTo,
            IEnumerable<string>? cc,
            IEnumerable<string>? bcc,
            IEnumerable<(string path, string? displayName)>? attachments)
        {
            var fromAddress = o.From ?? o.Username;

            var msg = new MailMessage
            {
                From = new MailAddress(fromAddress, o.FromName ?? "TenteCRM"),
                Subject = subject ?? string.Empty,
                Body = body ?? string.Empty,
                IsBodyHtml = isHtml,
                // Türkçe karakter sorunlarını önlemek için:
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8,
                HeadersEncoding = System.Text.Encoding.UTF8
            };

            msg.To.Add(to);

            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyToList.Add(new MailAddress(replyTo));

            if (cc != null)
                foreach (var c in cc)
                    if (!string.IsNullOrWhiteSpace(c))
                        msg.CC.Add(c);

            if (bcc != null)
                foreach (var b in bcc)
                    if (!string.IsNullOrWhiteSpace(b))
                        msg.Bcc.Add(b);

            if (attachments != null)
            {
                foreach (var (path, display) in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        var att = new Attachment(path);
                        if (!string.IsNullOrWhiteSpace(display))
                            att.Name = display!;
                        msg.Attachments.Add(att);
                    }
                }
            }

            return msg;
        }

        #endregion
    }
}
