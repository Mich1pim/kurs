using System;
using System.Collections.Generic;
using System.Linq;

namespace demoModels;

public partial class Product
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? ProductTypeId { get; set; }

    public string ArticleNumber { get; set; } = null!;

    public string? Description { get; set; }

    public string? Images { get; set; }

    public int? ProductionPersonCount { get; set; }

    public int? ProductionWorkshopNumber { get; set; }

    public decimal MinCostForAgent { get; set; }

	public decimal getSum
	{
		get
		{
			if (ProductMaterials == null || !ProductMaterials.Any())
				return 0;

			return ProductMaterials.Sum(pm =>
			{
				if (pm.Material != null && pm.Count.HasValue)
				{
					return pm.Material.Cost * (decimal)pm.Count.Value;
				}
				return 0;
			});
		}
	}

	public virtual ICollection<ProductCostHistory> ProductCostHistories { get; set; } = new List<ProductCostHistory>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();

    public virtual ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();

    public virtual ProductType? ProductType { get; set; }
}
