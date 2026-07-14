using CICD_Testing_Project.Api.Models.Features.Item;

namespace CICD_Testing_Project.Api.Domain.Features.Item;

public class BL_Item : IBL_Item
{
    private readonly IDA_Item _daItem;

    public BL_Item(IDA_Item daItem)
    {
        _daItem = daItem;
    }

    public async Task<List<ItemResponseModel>> GetAll(CancellationToken ct)
    {
        var lst = await _daItem.GetAll(ct);
        return lst;
    }

    public async Task<ItemResponseModel?> GetById(int id, CancellationToken ct)
    {
        var item = await _daItem.GetById(id, ct);
        return item;
    }

    public async Task<ItemResponseModel> Create(ItemRequestModel requestModel, CancellationToken ct)
    {
        var item = await _daItem.Create(requestModel, ct);
        return item;
    }

    public async Task<ItemResponseModel?> Update(int id, ItemRequestModel requestModel, CancellationToken ct)
    {
        var item = await _daItem.Update(id, requestModel, ct);
        return item;
    }

    public async Task<ItemResponseModel?> Patch(int id, ItemPatchModel patchModel, CancellationToken ct)
    {
        var item = await _daItem.Patch(id, patchModel, ct);
        return item;
    }

    public async Task<bool> Delete(int id, CancellationToken ct)
    {
        var result = await _daItem.Delete(id, ct);
        return result;
    }
}
