using System;
using System.Collections.Generic;
using System.Text;

namespace Protecta.Domain.Service.ReporteModule.Aggregates.ReporteAgg
{
    public class ReportePlanillaPendiente
    {
        public int IdPlanilla
        {
            get; set;
        }

        public string Descripcion
        {
            get;
            set;
        }

        public string Monto
        {
            get;
            set;
        }

        public DateTime FechaPlanilla
        {
            get;
            set;
        }

        public DateTime FechaProceso
        {
            get;
            set;
        }

        public string NumeroOperacion { get; set; }


        public int IdProducto
        {
            get;
            set;
        }

        public int IdTipoMedioPago { get; set; }

        public string DescripcionMedioPago { get; set; }

    }
}
