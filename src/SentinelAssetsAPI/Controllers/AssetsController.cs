using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentinelAssetsAPI.Data;
using SentinelAssetsAPI.Models;
using SentinelAssetsAPI.Services;

namespace SentinelAssetsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT required - Todos endpoints precisam token

// ========================================
// CONTROLLER ASSETS - Responsável por CRUD de ativos de segurança
// Protegido por JWT, injeção de dependências (DbContext + Service SNS)
// ========================================
public class AssetsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    // Construtor - DI injeta automaticamente DbContext e Service
    public AssetsController(AppDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ========================================
    // GET /api/assets - Lista TODOS os ativos salvos no banco
    // Autenticado via JWT, usa EF Core para query assíncrona
    // ========================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
    {
        return await _context.Assets.ToListAsync();
    }

    // ========================================
    // GET /api/assets/{id} - Busca ativo por ID
    // Retorna 404 se não encontrar
    // ========================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Asset>> GetAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        return asset;
    }

    // ========================================
    // POST /api/assets - CRIA novo ativo + ENVIA NOTIFICAÇÃO AWS SNS
    // DIFERENCIAL: Salva DB e dispara alerta serverless automático!
    // ========================================
    [HttpPost]
    public async Task<ActionResult<Asset>> CreateAsset(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        // Trigger notificação SNS (cloud)
        await _notificationService.SendAssetNotification(asset);

        return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
    }

    // ========================================
    // PUT /api/assets/{id} - ATUALIZA ativo existente
    // Valida ID, usa EntityState.Modified para EF track
    // ========================================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, Asset asset)
    {
        if (id != asset.Id) return BadRequest();

        _context.Entry(asset).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ========================================
    // DELETE /api/assets/{id} - Remove ativo
    // ========================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
