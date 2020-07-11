using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Protecta.Application.Service.Dtos.Reporte
{
    public class ReportePlanillaPendienteDto
    {
        public int IdPlanilla
        {
            get; set;
        }
        /*
        public string Descripcion
        {
            get;
            set;
        }*/

        public string Monto
        {
            get;
            set;
        }

        public string FechaPlanilla
        {
            get;
            set;
        }

        public string FechaProceso
        {
            get;
            set;
        }

        public string NumeroOperacion { get; set; }
        /*
        public int IdTipoMedioPago { get; set; }
        */
        public string DescripcionMedioPago
        {
            get;
            set;
        }


    }
}
