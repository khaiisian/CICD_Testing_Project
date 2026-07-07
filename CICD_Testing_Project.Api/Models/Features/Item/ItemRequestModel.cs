namespace CICD_Testing_Project.Api.Models.Features.Item;

public class ItemRequestModel
{
    public string Name { get; set; } = null!;

    public int Qty { get; set; }

    public decimal Price { get; set; }
}
