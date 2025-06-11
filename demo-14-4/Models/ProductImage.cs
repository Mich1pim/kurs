using System;
using System.Collections.Generic;

namespace demoModels;

public partial class ProductImage
{
    public int Id { get; set; }

    public string Imagepath { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;
}
