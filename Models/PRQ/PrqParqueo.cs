using System;
using System.Collections.Generic;

namespace ParqueoMvc.Models.PRQ;

public partial class PrqParqueo
{
    public int Id { get; set; }

    public string NombreProvincia { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal PrecioPorHora { get; set; }

    public virtual ICollection<PrqIngresoAutomovil> IngresosAutomoviles { get; set; } = new List<PrqIngresoAutomovil>();
}
