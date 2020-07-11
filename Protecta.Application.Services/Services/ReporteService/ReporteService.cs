using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Protecta.Application.Service.Dtos.Consulta;
using Protecta.Application.Service.Dtos.Reporte;
using Protecta.CrossCuting.Log.Contracts;
using Protecta.Domain.Service.ConsultaModule.Aggregates.ConsultaAgg;
using Protecta.Domain.Service.ReporteModule.Aggregates.ReporteAgg;

namespace Protecta.Application.Service.Services.ReporteService
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _reporteRepository;
        private ILoggerManager _logger;
        private IMapper _mapper;

        public ReporteService(IReporteRepository reporteRepository, ILoggerManager logger, IMapper mapper)
        {
            this._reporteRepository = reporteRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReporteDepositoPendienteDto>> ReporteDepositosPendientes(DatosReporteConciliacionPendienteDto datosConciliacionPendienteDto)
        {
            IEnumerable<ReporteDepositoPendienteDto> reporteDepositosPendientesDtos = null;

            try
            {
                var datosConsultaEntity = _mapper.Map<DatosReporteConciliacionPendiente>(datosConciliacionPendienteDto);

                var depositosResult = await _reporteRepository.ReporteDepositosPendientes(datosConsultaEntity);

                if (depositosResult == null || depositosResult.Count == 0)
                {
                    depositosResult = new List<ReporteDepositoPendiente>{
                        new ReporteDepositoPendiente()
                    };
                }

                reporteDepositosPendientesDtos = _mapper.Map<IEnumerable<ReporteDepositoPendienteDto>>(depositosResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);               
            }

            return reporteDepositosPendientesDtos;
        }

        public async Task<IEnumerable<ReportePlanillaPendienteDto>> ReportePlanillasPendientes(DatosReporteConciliacionPendienteDto datosConciliacionPendienteDto)
        {
            IEnumerable<ReportePlanillaPendienteDto> reportPlanillasPendientesDtos = null;

            try
            {
                var datosConsultaEntity = _mapper.Map<DatosReporteConciliacionPendiente>(datosConciliacionPendienteDto);

                var depositosResult = await _reporteRepository.ReportePlanillasPendientes(datosConsultaEntity);

                if (depositosResult == null || depositosResult.Count == 0)
                {
                    depositosResult = new List<ReportePlanillaPendiente>
                    {
                        new ReportePlanillaPendiente()
                    };
                }

                reportPlanillasPendientesDtos = _mapper.Map<IEnumerable<ReportePlanillaPendienteDto>>(depositosResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);               
            }

            return reportPlanillasPendientesDtos;
        }
    }
}
