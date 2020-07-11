using Protecta.Domain.Service.ConsultaModule.Aggregates.ConsultaAgg;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protecta.Domain.Service.ReporteModule.Aggregates.ReporteAgg
{
    public interface IReporteRepository
    {
        Task<List<ReporteDepositoPendiente>> ReporteDepositosPendientes(DatosReporteConciliacionPendiente datosConsulta);
        Task<List<ReportePlanillaPendiente>> ReportePlanillasPendientes(DatosReporteConciliacionPendiente datosConsulta);

    }
}
