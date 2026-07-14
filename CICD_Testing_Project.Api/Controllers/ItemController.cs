using CICD_Testing_Project.Api.Domain.Features.Item;
using CICD_Testing_Project.Api.Models.Features.Item;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CICD_Testing_Project.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemController : ControllerBase
{
    private readonly IBL_Item _blItem;

    public ItemController(IBL_Item blItem)
    {
        _blItem = blItem;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lst = await _blItem.GetAll(ct);
        return Ok(lst);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _blItem.GetById(id, ct);
        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemRequestModel requestModel, CancellationToken ct)
    {
        var item = await _blItem.Create(requestModel, ct);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ItemRequestModel requestModel, CancellationToken ct)
    {
        var item = await _blItem.Update(id, requestModel, ct);
        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] ItemPatchModel patchModel, CancellationToken ct)
    {
        var item = await _blItem.Patch(id, patchModel, ct);
        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _blItem.Delete(id, ct);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
