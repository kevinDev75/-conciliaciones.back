using Protecta.Application.Service.Dtos.Cobranzas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Protecta.Application.Service.Models;

namespace Protecta.Application.Service.Services.CobranzasModule
{
    public interface ICobranzasService
    {
        Task<IEnumerable<BancoDto>> ListarBancos();
        Task<IEnumerable<CuentaDto>> ListarCuenta(int Idbanco);
        Task<TramaDto> ValidarTrama(string usercode, string base64String, int idbanco, int idproducto, string idproceso, string fechaInicio, string fechaFinal, string CodProforma);
        Task<PlanillaDto> ObtenerTrama(TramaDto trama);
        Task<ResponseControl> InsertarFacturaFormaPago(List<ListadoConciliacionDto> listadoConciliacionDtos);
        Task<ResponseControl> Validar_Planilla_FacturaAsync(ListadoConciliacionDto Listado);
        Task<ResponseControl> ObtenerFormaPago(int idbanco,string idproceso);
        Task<IEnumerable<Tipo_PagoDto>> ListarTipoPago();
       
    }
}

