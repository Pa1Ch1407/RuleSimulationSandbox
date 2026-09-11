using Microsoft.AspNetCore.Mvc;
using RuleSimulation.Api.Contracts;
using RuleSimulation.Api.Services;

namespace RuleSimulation.Api.Controllers;

[ApiController]
[Route("api/rule-sets")]
public sealed class RuleSetsController(RuleSetService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RuleSetDto>>> List(CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<RuleSetDto>> Get(long id, CancellationToken ct)
        => (await service.GetAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    public async Task<ActionResult<RuleSetDto>> Create(CreateRuleSetRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<RuleSetDto>> Update(long id, CreateRuleSetRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await service.UpdateAsync(id, request, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
