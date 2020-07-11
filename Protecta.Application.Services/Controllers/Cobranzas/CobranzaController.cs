using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Protecta.Application.Service.Dtos.Cobranzas;
using Protecta.Application.Service.Models;
using Protecta.Application.Service.Services.CobranzasModule;
using Protecta.CrossCuting.Log.Contracts;
using Protecta.CrossCuting.Utilities.Configuration;

namespace Protecta.Application.Service.Controllers.Cobranzas
{
    [Route("api/[controller]")]
    public class CobranzaController : Controller
    {
        private ICobranzasService _cobranzaService;
        private ILoggerManager _logger;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public CobranzaController(ILoggerManager logger,
            ICobranzasService _cobranzaService,           
            IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            this._cobranzaService = _cobranzaService;          
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ListarBancos()
        {
            _logger.LogInfo("Metodo Listar Bancos SCTR");

            var bancoResult = await _cobranzaService.ListarBancos();
            return Ok(bancoResult);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ListarTipoPago()
        {
            _logger.LogInfo("Metodo Listar Tipo Pago");

            var TipoPagoResult = await _cobranzaService.ListarTipoPago();
            return Ok(TipoPagoResult);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ListarCuentas(int idBanco)
        {
            _logger.LogInfo("Metodo Listar cuentas SCTR");

            var bancoResult = await _cobranzaService.ListarCuenta(idBanco);
            return Ok(bancoResult);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ValidarTrama([FromBody]Cargarlote cargarLoteJson)
        {
            var validarTramaResult = await _cobranzaService.ValidarTrama(cargarLoteJson.UserCode,
                                                                         cargarLoteJson.Data,
                                                                         cargarLoteJson.IdBanco,
                                                                         cargarLoteJson.IdProducto,
                                                                         cargarLoteJson.IdProceso,
                                                                         cargarLoteJson.FechaInicial,
                                                                         cargarLoteJson.FechaFinal,
                                                                         cargarLoteJson.CodProforma);
            return Ok(validarTramaResult);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ObtenerTrama([FromBody]TramaDto trama)
        {
            try {
                var obtenerTramaResult = await _cobranzaService.ObtenerTrama(trama);
                return Ok(obtenerTramaResult);
            }
            catch (Exception ex)
            {
                return Ok(ex);
            }
        }

       

        [HttpPost("[action]")]
        public async Task<IActionResult> InsertarFacturaFormaPago([FromBody]List<ListadoConciliacionDto> listado)
        {
            
            try
            {
                var result = await _cobranzaService.InsertarFacturaFormaPago(listado);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return Ok(new ResponseControl() { message = ex.Message.ToString() , Code = "1" });
            }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ValidarPlanillaFactura([FromBody]ListadoConciliacionDto listado)
        {
                var result = await _cobranzaService.Validar_Planilla_FacturaAsync(listado);
                return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ObtenerFormaPago([FromBody]Cargarlote cargarJson)
        {
            var result = await _cobranzaService.ObtenerFormaPago(cargarJson.IdBanco, cargarJson.IdProceso);
            return Ok(result);
        }
    }
    public class Cargarlote
    {
        public int IdBanco { get; set; }
        public int IdProducto { get; set; }
        public string IdProceso { get; set; }
        public string UserCode { get; set; }
        public string FechaInicial { get; set; }
        public string FechaFinal { get; set; }
        public string CodProforma { get; set; }
        public string Data { get; set; }
    } 
}