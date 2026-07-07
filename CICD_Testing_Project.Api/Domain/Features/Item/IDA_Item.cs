using CICD_Testing_Project.Api.Models.Features.Item;

namespace CICD_Testing_Project.Api.Domain.Features.Item
{
    public interface IDA_Item
    {
        Task<List<ItemResponseModel>> GetAll(CancellationToken ct);

        Task<ItemResponseModel?> GetById(int id, CancellationToken ct);

        Task<ItemResponseModel?> Update(int id, ItemRequestModel requestModel, CancellationToken ct);

        Task<ItemResponseModel?> Patch(int id, ItemPatchModel patchModel, CancellationToken ct);

        Task<bool> Delete(int id, CancellationToken ct);
    }
}