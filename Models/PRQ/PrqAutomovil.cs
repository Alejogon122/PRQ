using System;
using System.Collections.Generic;

namespace ParqueoMvc.Models.PRQ;

public partial class PrqAutomovil
{
    public int Id { get; set; }

    public string Color { get; set; } = null!;

    public short Anio { get; set; }

    public string Fabricante { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual ICollection<PrqIngresoAutomovil> IngresosAutomoviles { get; set; } = new List<PrqIngresoAutomovil>();
}
