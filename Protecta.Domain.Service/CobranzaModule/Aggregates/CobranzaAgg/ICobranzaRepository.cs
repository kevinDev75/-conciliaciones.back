using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;



namespace Protecta.Domain.Service.CobranzaModule.Aggregates.CobranzaAgg
{
    public interface ICobranzaRepository
    {
        Task<List<Banco>> ListarBancos();
        Task<List<Cuenta>> ListarCuenta(int idBanco);
        Task<Conciliacion> ValidarTrama(Trama trama);
        Task<Planilla> ObtenerTrama(Trama trama);
        Task<bool> InsertarProceso(List<ListaConciliacion> listaConciliacions);
        Task<ResponseControl> GeneraPlanillaFactura(string idproceso, int idproducto, int idbanco, string tipooperacion, int usercode);
        Task<ResponseControl> Validar_Planilla_Factura(ListaConciliacion listaConciliacions);
        Task<ResponseControl> ObtenerLiquidacionManual(string idproceso, int idproducto, int idbanco, string StrProforma, string fechaInicio, string fechaFin, string usercode);
        Task<ResponseControl> ObtenerFormaPago(int idBanco, string idProceso);
        Task<bool> Insertar_Respuesta_FE(State_voucher _Voucher);
        Task<List<Tipo_Pago>> ListarTipoPago();
    }
}
