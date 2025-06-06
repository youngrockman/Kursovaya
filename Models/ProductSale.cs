﻿using System;
using System.Collections.Generic;

namespace Vosmerka.Models;

public partial class ProductSale
{
    public int Id { get; set; }

    public int AgentId { get; set; }

    public int ProductId { get; set; }

    public DateOnly SaleDate { get; set; }

    public int ProductCount { get; set; }

    public virtual Agent Agent { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
