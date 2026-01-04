using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;

namespace gutv_booker.Services.Telegram.Commands;

public class HelpCommand : ICommand
{
    public string Name => "ℹ️ Помощь";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var response = new StringBuilder();
        response.AppendLine("ℹ️ <b>Бот GUtv Booker</b>\n");

        response.AppendLine("<b>Команды:</b>");
        response.AppendLine("/start - Главное меню");
        response.AppendLine("/link КОД - Привязать аккаунт\n");

        response.AppendLine("Если что-то сломалось, введите /start\n\n");

        response.AppendLine("<b>Контакты для связи:</b>");
        response.AppendLine("<b>Директор студии</b>");
        response.AppendLine("Адельшин Джемильхан @pzr_enjoyer\n");

        response.AppendLine("<b>Технический директор</b>");
        response.AppendLine("Кон Владислав @Qineya\n");

        response.AppendLine("<b>Заместитель тех. директора</b>");
        response.AppendLine("Борисов Максим @mspieler\n");

        response.AppendLine("<b>Если что-то сломалось, но сильно</b>");
        response.AppendLine("Петров Дмитрий @s1ash2k");

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: response.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}