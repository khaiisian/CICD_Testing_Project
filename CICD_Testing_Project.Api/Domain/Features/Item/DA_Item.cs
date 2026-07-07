using CICD_Testing_Project.Api.Models.Features.Item;
using CICD_Testing_Project.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CICD_Testing_Project.Api.Domain.Features.Item;

public class DA_Item : IDA_Item
{
    private readonly AppDbContext _dbContext;

    public DA_Item(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ItemResponseModel>> GetAll(CancellationToken ct)
    {
        var lst = await _dbContext.TblItems.Select(x => new ItemResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            Price = x.Price,
            Qty = x.Qty,
        }).ToListAsync(ct);
        return lst;
    }

    public async Task<ItemResponseModel?> GetById(int id, CancellationToken ct)
    {
        var item = await _dbContext.TblItems
            .Where(x => x.Id == id)
            .Select(x => new ItemResponseModel
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Qty = x.Qty,
            }).FirstOrDefaultAsync(ct);
        return item;
    }

    public async Task<ItemResponseModel?> Update(int id, ItemRequestModel requestModel, CancellationToken ct)
    {
        var item = await _dbContext.TblItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return null;

        item.Name = requestModel.Name;
        item.Qty = requestModel.Qty;
        item.Price = requestModel.Price;

        await _dbContext.SaveChangesAsync(ct);

        return new ItemResponseModel
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            Qty = item.Qty,
        };
    }

    public async Task<ItemResponseModel?> Patch(int id, ItemPatchModel patchModel, CancellationToken ct)
    {
        var item = await _dbContext.TblItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return null;

        if (patchModel.Name is not null)
            item.Name = patchModel.Name;

        if (patchModel.Qty is not null)
            item.Qty = patchModel.Qty.Value;

        if (patchModel.Price is not null)
            item.Price = patchModel.Price.Value;

        await _dbContext.SaveChangesAsync(ct);

        return new ItemResponseModel
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            Qty = item.Qty,
        };
    }

    public async Task<bool> Delete(int id, CancellationToken ct)
    {
        var item = await _dbContext.TblItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return false;

        _dbContext.TblItems.Remove(item);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}
