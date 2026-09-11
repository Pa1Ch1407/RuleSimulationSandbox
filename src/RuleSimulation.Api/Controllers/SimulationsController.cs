using Microsoft.AspNetCore.Mvc;
using RuleSimulation.Api.Contracts;
using RuleSimulation.Api.Services;

namespace RuleSimulation.Api.Controllers;

[ApiController]
[Route("api/simulations")]
public sealed class SimulationsController(SimulationService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SimulationResponse>> Run(SimulationRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await service.RunAsync(request, ct));
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}
