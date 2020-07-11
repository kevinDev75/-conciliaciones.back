using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Protecta.Application.Service.Dtos.Consulta;
using Protecta.Application.Service.Services.ReporteService;
using Protecta.CrossCuting.Log.Contracts;
using Protecta.CrossCuting.Utilities.Configuration;

namespace Protecta.Application.Service.Controllers.Reporte
{
    [Route("api/[controller]")]
    public class ReporteController : Controller
    {
        private IReporteService _reporteService;
        private ILoggerManager _logger;
        private IMapper _mapper;

        private readonly AppSettings _appSettings;

        public ReporteController(ILoggerManager logger, IReporteService _reporteService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            this._reporteService = _reporteService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ReportePlanillasPendientes([FromBody] DatosReporteConciliacionPendienteDto datosReporteDto)
        //public async Task<IActionResult> ReportePlanillasPendientes()
        {
            _logger.LogInfo("Metodo ReportePlanillasPendientes");

            //DatosReporteConciliacionPendienteDto datosReporteDto = new DatosReporteConciliacionPendienteDto();
            //datosReporteDto.IdProducto = 1000;

            var conciliacionResult = await _reporteService.ReportePlanillasPendientes(datosReporteDto);          

            return Ok(conciliacionResult);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ReporteDepositosPendientes([FromBody] DatosReporteConciliacionPendienteDto datosReporteDto)
        //public async Task<IActionResult> ReporteDepositosPendientes()
        {
            _logger.LogInfo("Metodo ReporteDepositosPendientes");
            //DatosReporteConciliacionPendienteDto datosReporteDto = new DatosReporteConciliacionPendienteDto();
            var conciliacionResult = await _reporteService.ReporteDepositosPendientes(datosReporteDto);           

            return Ok(conciliacionResult);
        }
    }
}