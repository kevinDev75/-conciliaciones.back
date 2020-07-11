using Protecta.Application.Service.Dtos.Consulta;
using Protecta.Application.Service.Dtos.Reporte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Protecta.Application.Service.Services.ReporteService
{
    public interface IReporteService
    {
        Task<IEnumerable<ReportePlanillaPendienteDto>> ReportePlanillasPendientes(DatosReporteConciliacionPendienteDto datosConciliacionPendienteDto);

        Task<IEnumerable<ReporteDepositoPendienteDto>> ReporteDepositosPendientes(DatosReporteConciliacionPendienteDto datosConciliacionPendienteDto);
    }
}
