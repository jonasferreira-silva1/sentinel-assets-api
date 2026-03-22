using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SentinelAssetsAPI.Models;

namespace SentinelAssetsAPI.Services
{
  // ========================================
  // SERVICE NOTIFICATION - Integração AWS SNS (Serverless Notifications)
  // Chamado automaticamente ao criar asset
  // ========================================
  public class NotificationService
  {
    private readonly IAmazonSimpleNotificationService _snsClient;

    // DI - AWS client injetado (config via env vars ou ~/.aws)
    public NotificationService(IAmazonSimpleNotificationService snsClient)
    {
      _snsClient = snsClient;
    }

    // ========================================
    // ENVIA ALERTA SNS - Publish message para Topic (e-mail/SMS/fanout)
    // Try/catch para não quebrar API se AWS offline
    // ========================================
    public async Task SendAssetNotification(Asset asset)
    {
      var message = $"🔔 Novo ativo detectado: {asset.Name} " +
                   $"(IP: {asset.IPAddress ?? "N/A"}) - " +
                   $"Vulnerabilidade: {asset.VulnerabilityStatus}";

      var request = new PublishRequest
      {
        // SUBSTITUA pela ARN do seu SNS Topic (AWS Console → SNS → Topics)
        TopicArn = "arn:aws:sns:us-east-1:123456789012:SentinelAssetsTopic",
        Message = message
      };

      try
      {
        await _snsClient.PublishAsync(request);
        // Sucesso - envia para assinantes (e-mail, Lambda...)
      }
      catch (Exception ex)
      {
        // Fallback - log erro (futuro: ILogger ou DB log)
        Console.WriteLine($"❌ SNS Error: {ex.Message}");
      }
    }
  }
}