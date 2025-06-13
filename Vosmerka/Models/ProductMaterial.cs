﻿using System;
using System.Collections.Generic;

namespace Vosmerka.Models;

public partial class ProductMaterial
{
    public int ProductId { get; set; }

    public int MaterialId { get; set; }

    public double? Count { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
