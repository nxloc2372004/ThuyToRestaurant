using System;
using System.Collections.Generic;

namespace ThuyTo.Models;

public partial class Commune
{
    public long CommuneId { get; set; }

    public string? CommuneName { get; set; }

    public string? CommuneType { get; set; }

    public long? DistrictId { get; set; }

    public virtual District? District { get; set; }

    public virtual ICollection<FeeShip> FeeShips { get; set; } = new List<FeeShip>();
}
