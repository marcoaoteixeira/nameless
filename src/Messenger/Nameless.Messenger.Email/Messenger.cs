using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Nameless.FileStorage;
using Nameless.Logging;
using Nameless.Services;

namespace Nameless.Messenger.Email {

    public sealed class Messenger : IMessenger {

        #region Public Constants

        public const string IS_BODY_HTML_KEY = "IsBodyHtml";
        public const string CARBON_COPY_KEY = "CC";
        public const string BLIND_CARBON_COPY_KEY = "BCC";

        #endregion

        #region Private Read-Only Fields

        private readonly IFileStorage _fileStorage;
        private readonly IClock _clock;
        private readonly MessengerOptions _opts;

        #endregion

        #region Public Properties

        private ILogger? _logger;
        public ILogger Logger {
            get { return _logger ??= NullLogger.Instance; }
            set { _logger = value ?? NullLogger.Instance; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="EmailService"/>.
        /// </summary>
        /// <param name="fileStorage">The file storage.</param>
        /// <param name="opts">The SMTP client settings.</param>
        public Messenger(IFileStorage fileStorage, IClock? clock = null, MessengerOptions? opts = null) {
            Ensure.NotNull(fileStorage, nameof(fileStorage));

            _fileStorage = fileStorage;
            _clock = clock ?? SystemClock.Instance;
            _opts = opts ?? new();
        }

        #endregion

        #region Private Methods

        private Task SendAsync(MimeMessage message, CancellationToken cancellationToken) {
            return _opts.DeliveryMethod switch {
                MessengerOptions.DeliveryMethods.PickupDirectory => SendOfflineAsync(message, cancellationToken),
                MessengerOptions.DeliveryMethods.Network => SendOnlineAsync(message, cancellationToken),
                _ => throw new InvalidOperationException("Invalid delivery method."),
            };
        }

        private async Task SendOfflineAsync(MimeMessage message, CancellationToken cancellationToken) {
            if (string.IsNullOrWhiteSpace(_opts.PickupDirectoryFolder) || !Directory.Exists(_opts.PickupDirectoryFolder)) {
                throw new InvalidOperationException("Pickup directory not specified or invalid.");
            }

            var now = _clock.UtcNow;
            var path = Path.Combine(_fileStorage.Root, _opts.PickupDirectoryFolder, $"{Guid.NewGuid():N}_{now:yyyyMMddHHmmssfff}.eml");
            using var stream = new FileStream(path, FileMode.Create);
            await message.WriteToAsync(stream, headersOnly: false, cancellationToken);
        }

        private async Task SendOnlineAsync(MimeMessage message, CancellationToken cancellationToken) {
            using var client = new SmtpClient();
            try {
                await client.ConnectAsync(_opts.Host, _opts.Port, _opts.EnableSsl, cancellationToken);

                // Authenticate if possible and needed.
                if (_opts.UseCredentials && !string.IsNullOrWhiteSpace(_opts.UserName) && client.Capabilities.HasFlag(SmtpCapabilities.Authentication)) {
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_opts.UserName, _opts.Password, cancellationToken);
                }
            } catch (Exception ex) { Logger.Error(ex, ex.Message); throw; }

            // Send message
            await client.SendAsync(message, cancellationToken: cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }

        #endregion

        #region IMessenger Members

        public async Task<MessengerResponse> DispatchAsync(MessengerRequest request, CancellationToken cancellationToken = default) {
            if (request.From == null || !request.From.Any()) {
                throw new InvalidOperationException("Missing sender address.");
            }
            var mail = new MimeMessage {
                Body = new TextPart((request.Properties.TryGet(IS_BODY_HTML_KEY, out bool isBodyHtml) && isBodyHtml) ? TextFormat.Html : TextFormat.Plain) {
                    Text = request.Message
                },
                Sender = MailboxAddress.Parse(request.From.First()),
                Subject = request.Subject,
                Priority = request.Priority switch {
                    MessagePriority.Low => MimeKit.MessagePriority.NonUrgent,
                    MessagePriority.Medium => MimeKit.MessagePriority.Normal,
                    MessagePriority.High => MimeKit.MessagePriority.Urgent,
                    _ => MimeKit.MessagePriority.Normal
                }
            };

            // Add recipients
            request.From.Each(_ => mail.From.Add(InternetAddress.Parse(_)));
            if (request.To != null) {
                request.To.Each(_ => mail.To.Add(InternetAddress.Parse(_)));
            }

            if (request.Properties.TryGetValue(CARBON_COPY_KEY, out string? cc)) {
                cc.Split(';').Each(_ => mail.Cc.Add(InternetAddress.Parse(_)));
            }
            if (request.Properties.TryGetValue(BLIND_CARBON_COPY_KEY, out string? bcc)) {
                bcc.Split(';').Each(_ => mail.Cc.Add(InternetAddress.Parse(_)));
            }

            try { await SendAsync(mail, cancellationToken); } catch (Exception ex) { return new MessengerResponse { Error = ex }; }

            return MessengerResponse.Successful;
        }

        #endregion
    }
}
