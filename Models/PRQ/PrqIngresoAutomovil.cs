using System;
using System.Collections.Generic;

namespace ParqueoMvc.Models.PRQ;

public partial class PrqIngresoAutomovil
{
    public int Consecutivo { get; set; }

    public int IdParqueo { get; set; }

    public int IdAutomovil { get; set; }

    public DateTime FechaHoraEntrada { get; set; }

    public DateTime? FechaHoraSalida { get; set; }

    public virtual PrqAutomovil Automovil { get; set; } = null!;

    public virtual PrqParqueo Parqueo { get; set; } = null!;
}
