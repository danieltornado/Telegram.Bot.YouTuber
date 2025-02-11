using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.YouTuber.SetWebhook.Extensions;

var configuration = ConfigurationExtensions.GetConfiguration("Development");
var botConfig = configuration.GetBotConfiguration();

string token = botConfig.Token;

HttpClient httpClient = new();
TelegramBotClientOptions options = new(token);

TelegramBotClient bot = new(options, httpClient);

if (args.Length > 0 && args.Contains("--unset"))
{
    await bot.SetWebhookAsync(url: "");

    Console.WriteLine("Unset successfully");
}
else
{
    const string publicKey = "public.pem";
    string webhookAddress = botConfig.Url;

    string certPath = Path.Combine(Directory.GetCurrentDirectory(), "Security", publicKey);

    if (System.IO.File.Exists(certPath) is false)
        throw new InvalidOperationException($"Не найден файл {publicKey}");
    
    Stream fileStream = new FileStream(certPath, FileMode.Open);
    InputFileStream certificate = new(fileStream);

    await bot.SetWebhookAsync(url: webhookAddress, certificate: certificate, allowedUpdates: [ UpdateType.Message, UpdateType.CallbackQuery ]);

    Console.WriteLine("Set successfully");
}