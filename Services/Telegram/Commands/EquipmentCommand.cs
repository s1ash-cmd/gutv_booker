using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text;

namespace gutv_booker.Services.Telegram.Commands;

public class EquipmentCommand : ICommand
{
    private readonly EquipmentService _equipmentService;

    public EquipmentCommand(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    public string Name => "📋 Оборудование";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var equipment = await _equipmentService.GetAllEquipmentModels();

            if (!equipment.Any())
            {
                await botClient.SendMessage(message.Chat.Id, "Нет доступного оборудования", cancellationToken: cancellationToken);
                return;
            }

            var response = new StringBuilder("📋 Доступное оборудование:\n\n");
            foreach (var item in equipment)
            {
                response.AppendLine($"🔹 {item.Name}");
                response.AppendLine($"   ID: {item.Id}");
            }

            await botClient.SendMessage(message.Chat.Id, response.ToString(), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(message.Chat.Id, "Ошибка получения данных", cancellationToken: cancellationToken);
        }
    }
}