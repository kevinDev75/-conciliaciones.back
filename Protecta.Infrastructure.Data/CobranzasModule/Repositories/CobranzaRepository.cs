using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Protecta.CrossCuting.Utilities.Configuration;
using Protecta.Domain.Service.CobranzaModule.Aggregates;
using Protecta.Domain.Service.CobranzaModule.Aggregates.CobranzaAgg;
using Protecta.Infrastructure.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Protecta.Infrastructure.Data.CobranzasModule.Repositories
{
    public class CobranzaRepository : ICobranzaRepository
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly IConnectionBase _connectionBase;

        public CobranzaRepository(IOptions<AppSettings> appSettings, IConnectionBase ConnectionBase)
        {
            this.appSettings = appSettings;
            _connectionBase = ConnectionBase;
        }


        public Task<List<Banco>> ListarBancos()
        {
            Banco entities = null;
            List<Banco> listaBancos = new List<Banco>();
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output));
            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_PAYROLL.PA_SEL_BANK", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
            {
                while (dr.Read())
                {
                    entities = new Banco();
                    entities.IdBanco = Convert.ToInt32(dr["NIDBANK"]);
                    entities.DescripcionBanco = Convert.ToString(dr["SDESCRIPTION"]);
                    listaBancos.Add(entities);
                }
            }

            return Task.FromResult<List<Banco>>(listaBancos);
        }


        public Task<Conciliacion> ValidarTrama(Trama trama)
        {

            Conciliacion _conciliacion = new Conciliacion();
            string response = string.Empty, valid = string.Empty, mensaje = string.Empty;
            List<OracleParameter> parameters = new List<OracleParameter>();
            try
            {
                parameters.Add(new OracleParameter("P_TRAMA", OracleDbType.Varchar2, 4000, trama.StringTrama, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NFILA", OracleDbType.Varchar2, trama.Fila, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NID_BANK", OracleDbType.Int32, trama.IdBanco, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NSEGMEN", OracleDbType.Int32, trama.Segmento, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_STIPOINGRESO", OracleDbType.Varchar2, trama.TipoIngreso, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NPRODUCT", OracleDbType.Int32, trama.IdProducto, ParameterDirection.Input));
                //Parámetro de Salida
                //var pRecibo = new OracleParameter("P_NRECEIPT", OracleDbType.Varchar2, ParameterDirection.Output)
                //{
                //    Size = 2000
                //};
                var pRecibo = new OracleParameter("P_SPROFORMA", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pImporte = new OracleParameter("P_NPREMIUM", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pImporteOrigen = new OracleParameter("P_NPREMIUM_ORIG", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };

                var pFechaPago = new OracleParameter("P_FECHAPAGO", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pNombreCliente = new OracleParameter("P_NOMCLIENTE", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pFechaVencimiento = new OracleParameter("P_FECHAVENC", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pNumeroCuenta = new OracleParameter("P_NUMCUENTA", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pNumeroDocumento = new OracleParameter("P_NUMDOCUMENTO", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pMoneda = new OracleParameter("P_NIDCURRENCY", OracleDbType.Int32, ParameterDirection.Output);

                var pcantidad = new OracleParameter("P_CANT_REGIST", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var Fechaoperation = new OracleParameter("P_OPERATION_DATE", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var numeroOperation = new OracleParameter("P_OPERATION_NUMBER", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var reference = new OracleParameter("P_REFERENCE", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pmonto = new OracleParameter("P_MONTO_TOTAL", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                var pmontoOrigen = new OracleParameter("P_ORIG_TOTAL", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };
                

                var pExtorno = new OracleParameter("P_EXTORNO", OracleDbType.Varchar2, ParameterDirection.Output)
                {
                    Size = 1
                };
                var pValid = new OracleParameter("P_VALID", OracleDbType.Int32, ParameterDirection.Output);
                var pMensaje = new OracleParameter("P_MENSAJE", OracleDbType.NVarchar2, ParameterDirection.Output)
                {
                    Size = 2000
                };


                parameters.Add(pRecibo);
                parameters.Add(pImporte);
                parameters.Add(pImporteOrigen);
                parameters.Add(pFechaPago);
                parameters.Add(pNombreCliente);
                parameters.Add(pFechaVencimiento);
                parameters.Add(pNumeroCuenta);
                parameters.Add(pNumeroDocumento);
                parameters.Add(pMoneda);
                parameters.Add(Fechaoperation);
                parameters.Add(numeroOperation);
                parameters.Add(reference);
                parameters.Add(pcantidad);
                parameters.Add(pmonto);
                parameters.Add(pmontoOrigen);
                parameters.Add(pExtorno);
                parameters.Add(pValid);
                parameters.Add(pMensaje);

                OracleParameterCollection dr = (OracleParameterCollection)_connectionBase.ExecuteByStoredProcedureNonQuery("PKG_TRAMA_CONFIG.VAL_TRAMA", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);

                _conciliacion.IsValido = dr["P_VALID"].Value == DBNull.Value ? false : int.Parse(dr["P_VALID"].Value.ToString()) == 0 ? false : true;
                _conciliacion.Mensaje = dr["P_MENSAJE"].Value.ToString().Equals("null") ? string.Empty : dr["P_MENSAJE"].Value.ToString();
                _conciliacion.NumeroRecibo = dr["P_SPROFORMA"].Value.ToString().Equals("null") ? string.Empty : dr["P_SPROFORMA"].Value.ToString();
                _conciliacion.Importe = dr["P_NPREMIUM"].Value.ToString().Equals("null") ? string.Empty : dr["P_NPREMIUM"].Value.ToString();
                _conciliacion.NombreCliente = dr["P_NOMCLIENTE"].Value.ToString().Equals("null") ? string.Empty : dr["P_NOMCLIENTE"].Value.ToString();
                _conciliacion.NumeroCuenta = dr["P_NUMCUENTA"].Value.ToString().Equals("null") ? string.Empty : dr["P_NUMCUENTA"].Value.ToString();
                _conciliacion.FechaVencimiento = dr["P_FECHAVENC"].Value.ToString().Equals("null") ? string.Empty : dr["P_FECHAVENC"].Value.ToString();
                _conciliacion.FechaPago = dr["P_FECHAPAGO"].Value.ToString().Equals("null") ? string.Empty : dr["P_FECHAPAGO"].Value.ToString();
                _conciliacion.IdMoneda = dr["P_NIDCURRENCY"].Value.ToString().Equals("null") ? string.Empty : dr["P_NIDCURRENCY"].Value.ToString();
                _conciliacion.FlagExtorno = dr["P_EXTORNO"].Value.ToString().Equals("null") || dr["P_EXTORNO"].Value.ToString().Trim().Equals("") ? "2" : "1";
                _conciliacion.NumeroDocuento = dr["P_NUMDOCUMENTO"].Value.ToString().Equals("null") ? string.Empty : dr["P_NUMDOCUMENTO"].Value.ToString();
                _conciliacion.CantTotal = dr["P_CANT_REGIST"].Value.ToString().Equals("null") ? string.Empty : dr["P_CANT_REGIST"].Value.ToString();
                _conciliacion.MontoTotal = dr["P_MONTO_TOTAL"].Value.ToString().Equals("null") ? string.Empty : dr["P_MONTO_TOTAL"].Value.ToString();
                _conciliacion.NumeroOperacion = dr["P_OPERATION_NUMBER"].Value.ToString().Equals("null") ? string.Empty : dr["P_OPERATION_NUMBER"].Value.ToString();
                _conciliacion.Referencia = dr["P_REFERENCE"].Value.ToString().Equals("null") ? string.Empty : dr["P_REFERENCE"].Value.ToString();
                _conciliacion.FechaOperacion = dr["P_FECHAPAGO"].Value.ToString().Equals("null") ? string.Empty : dr["P_FECHAPAGO"].Value.ToString();
                _conciliacion.ImporteOrigen = dr["P_NPREMIUM_ORIG"].Value.ToString().Equals("null") ? string.Empty : dr["P_NPREMIUM_ORIG"].Value.ToString();
                _conciliacion.MontoTotalOrigen = dr["P_ORIG_TOTAL"].Value.ToString().Equals("null") ? string.Empty : dr["P_ORIG_TOTAL"].Value.ToString();

            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            return Task.FromResult<Conciliacion>(_conciliacion);
        }
        public Task<Planilla> ObtenerTrama(Trama trama)
        {
            Planilla planilla = new Planilla();
            Proforma proforma;
            planilla.ListaProforma = new List<Proforma>();

            List<OracleParameter> parameters = new List<OracleParameter>();
            CultureInfo cultureInfo = new CultureInfo("es-PE");
            parameters.Add(new OracleParameter("P_NIDBANK", OracleDbType.Int32, trama.IdBanco, ParameterDirection.Input));
            parameters.Add(new OracleParameter("P_NPRODUCT", OracleDbType.Int32, trama.IdProducto, ParameterDirection.Input));
            parameters.Add(new OracleParameter("P_DFECHAINI", OracleDbType.Date, DateTime.Parse(trama.FechaInicial, cultureInfo), ParameterDirection.Input));
            parameters.Add(new OracleParameter("P_DFECHAFIN", OracleDbType.Date, DateTime.Parse(trama.FechaFinal, cultureInfo), ParameterDirection.Input));
            parameters.Add(new OracleParameter("P_NUSERCODE", OracleDbType.Int32, trama.CodigoUsuario, ParameterDirection.Input));

            //Parámetro de Salida
            var pValid = new OracleParameter("P_NCODE", OracleDbType.Int32, ParameterDirection.Output);
            var pRuta = new OracleParameter("P_SRUTA", OracleDbType.NVarchar2, ParameterDirection.Output);
            pRuta.Size = 2500;


            // var pRuta = new OracleParameter("P_SRUTA", OracleDbType.Varchar2,4000, ParameterDirection.Output);
            var pMensaje = new OracleParameter("P_SMESSAGE", OracleDbType.Varchar2, 4000, ParameterDirection.Output);
            parameters.Add(pRuta);
            parameters.Add(new OracleParameter("LISTA", OracleDbType.RefCursor, ParameterDirection.Output));
            parameters.Add(pValid);
            parameters.Add(pMensaje);

            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_TRAMA_ENVIO_BANCO", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
            {
                while (dr.Read())
                {
                    proforma = new Proforma();
                  //  proforma.NumeroCuenta = dr["NRECEIPT"] == DBNull.Value ? string.Empty : dr["NRECEIPT"].ToString();
                    proforma.NombreContratante = dr["SCLIENAME"] == DBNull.Value ? string.Empty : dr["SCLIENAME"].ToString();
                    proforma.Documento = dr["SIDDOC"] == DBNull.Value ? string.Empty : dr["SIDDOC"].ToString();
                    proforma.DescTipoDoc = dr["DESC_TIPODOC"] == DBNull.Value ? string.Empty : dr["DESC_TIPODOC"].ToString();
                    proforma.CodigoProforma = dr["SPROFORMA"] == DBNull.Value ? string.Empty : dr["SPROFORMA"].ToString();
                    proforma.NumeroRecibo = dr["NRECEIPT"] == DBNull.Value ? string.Empty : dr["NRECEIPT"].ToString();
                    proforma.FechaEmision = dr["FE_EMISION"] == DBNull.Value ? string.Empty : Convert.ToDateTime(dr["FE_EMISION"].ToString()).ToString("dd/MM/yyyy");
                    proforma.FechaVencimiento = dr["FE_VENCIMIENTO"] == DBNull.Value ? string.Empty : Convert.ToDateTime(dr["FE_VENCIMIENTO"].ToString()).ToString("dd/MM/yyyy");
                    proforma.Importe = dr["NPREMIUM"] == DBNull.Value ? "0.00" : Convert.ToDouble(dr["NPREMIUM"].ToString()).ToString("N2");
                  //  proforma.ImporteMora = dr["MORA"] == DBNull.Value ? "0.00" : Convert.ToDouble(dr["MORA"].ToString()).ToString("N2");
                  //  proforma.ImporteMontoMinimo = dr["MONTO_MINIMO"] == DBNull.Value ? "0.00" : Convert.ToDouble(dr["MONTO_MINIMO"].ToString()).ToString("N2");
                    
                    planilla.ListaProforma.Add(proforma);
                }
                planilla.Resultado = int.Parse(pValid.Value.ToString()) == 0 ? true : false;
                planilla.MensajeError = pMensaje.Value.ToString();
                planilla.RutaTrama = pRuta.Value.ToString();
            }
            return Task.FromResult<Planilla>(planilla);
        }
        public Task<bool> InsertarProceso(List<ListaConciliacion> listaConciliacions)
        {
            CultureInfo cultureInfo = new CultureInfo("es-PE");
            int count = 1;
            bool exito = true;
            try
            {

                foreach (var item in listaConciliacions)
                {
                    if (item.IsValido || item.TipoOperacion != "GP") {
                        List<OracleParameter> parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("P_SIDPROCESS", OracleDbType.Varchar2, item.IdProceso ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NIDBANK", OracleDbType.Int32, item.IdBanco, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_STYPEOPE", OracleDbType.Varchar2, item.TipoOperacion ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NPRODUCT", OracleDbType.Int32, item.IdProducto, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SPROFORMA", OracleDbType.Varchar2, item.NumeroRecibo ?? count.ToString(), ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NPREMIUM", OracleDbType.Decimal, item.Importe, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NIDCURRENCY", OracleDbType.Int32, item.IdMoneda, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NAMOUNT", OracleDbType.Decimal, item.MontoFormaPago, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NIDPAIDTYPE", OracleDbType.Int32, item.IdTipoPago, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NIDACCOUNTBANK", OracleDbType.Int64, item.IdCuentaBanco, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SOPERATION_NUMBER", OracleDbType.Varchar2, item.NumeroOperacion ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_DOPERATION_DATE", OracleDbType.Date, DateTime.Parse(item.FechaOperacion, cultureInfo), ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SREFERENCE", OracleDbType.Varchar2, item.Referencia ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SINDEXTORNO", OracleDbType.Varchar2, item.FlagExtorno, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NUSERCODE", OracleDbType.Int32, item.UserCode, ParameterDirection.Input));


                        var pCode = new OracleParameter("P_NCODE", OracleDbType.Int16, ParameterDirection.Output);
                        var p_Message = new OracleParameter("P_SMESSAGE", OracleDbType.NVarchar2, ParameterDirection.Output);
                        p_Message.Size = 2500;

                        parameters.Add(pCode);
                        parameters.Add(p_Message);
                        _connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_GET_TRAMA_INS_PAYROLLBILLS", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                        count++;
                        bool flag = pCode.Value.ToString() == "0" ? true : false;
                        if (!flag)
                        {
                            exito = false;
                            throw new Exception(p_Message.Value.ToString());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Task.FromResult<bool>(exito);
        }
        public Task<ResponseControl> GeneraPlanillaFactura(string idproceso, int idproducto, int idbanco, string tipooperacion, int usercode)
        {
            bool exito = true;
            List<OracleParameter> parameters = new List<OracleParameter>();
            ResponseControl Response = new ResponseControl(ResponseControl.Status.Ok);
            List<State_voucher> state_Vouchers = new List<State_voucher>();
            State_voucher voucher;
            try {

                parameters.Add(new OracleParameter("P_SIDPROCESS", OracleDbType.Varchar2, idproceso, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NPRODUCT", OracleDbType.Varchar2, idproducto, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NIDBANK", OracleDbType.Varchar2, idbanco, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_STYPEOPE", OracleDbType.Varchar2, tipooperacion, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NUSERCODE", OracleDbType.Varchar2, usercode, ParameterDirection.Input));

                var pCode = new OracleParameter("P_NCODE", OracleDbType.Int16, ParameterDirection.Output);
                var p_Message = new OracleParameter("P_SMESSAGE", OracleDbType.NVarchar2, ParameterDirection.Output);
                var table = new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output);
                p_Message.Size = 2500;
                table.Size = 4000;
                parameters.Add(pCode);
                parameters.Add(p_Message);
                parameters.Add(table);
                //_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_GENERA_PLANILLA_DOCUMENTO", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_GENERA_PLANILLA_DOCUMENTO", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
                {
                    while (dr.Read())
                    {
                        voucher = new State_voucher
                        {
                            SCAMPO = (dr["SCAMPO"] != null ? dr["SCAMPO"].ToString() : ""),
                            SGRUPO = (dr["SGRUPO"] != null ? dr["SGRUPO"].ToString() : ""),
                            SMENSAJE = (dr["SMENSAJE"] != null ? dr["SMENSAJE"].ToString() : ""),
                            SVALOR = (dr["SVALOR"] != null ? dr["SVALOR"].ToString() : ""),
                            BILLTYPE = (dr["SBILLTYPE"] != null ? dr["SBILLTYPE"].ToString() : ""),
                            SBILING = (dr["SBILLING"] != null ? dr["SBILLING"].ToString() : ""),
                            NINSUR_AREA = (dr["NINSUR_AREA"] != null ? dr["NINSUR_AREA"].ToString() : ""),
                            NBILLNUM = (dr["NBILLNUM"] != null ? dr["NBILLNUM"].ToString() : ""),
                            OPERADOR = (dr["SCORREO_OPE"] != null ? dr["SCORREO_OPE"].ToString() : "")
                        };
                        state_Vouchers.Add(voucher);
                    }
                    Response.Data = state_Vouchers;
                    Response.message = p_Message.Value.ToString();
                    if (!(pCode.Value.ToString() == "0"))
                    {
                        Response.Code = pCode.Value.ToString();
                        throw new Exception(p_Message.Value.ToString());

                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Task.FromResult<ResponseControl>(Response);
        }
        public Task<ResponseControl> Validar_Planilla_Factura(ListaConciliacion listaConciliacions)
        {
            ResponseControl response = new ResponseControl(ResponseControl.Status.Ok);
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("P_SIDPROCESS", OracleDbType.Varchar2, listaConciliacions.IdProceso ?? string.Empty, ParameterDirection.Input));
                var nnoBills = new OracleParameter("P_NNOBILLS", OracleDbType.Int16, ParameterDirection.Output);
                parameters.Add(nnoBills);
                _connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_VALIDA_PLANILLA", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                response.Data = nnoBills.Value.ToString() == "1" ? true : false;

            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.Code = "1";
            }
            return Task.FromResult<ResponseControl>(response);
        }

        public Task<ResponseControl> ObtenerLiquidacionManual(string idproceso, int idproducto, int idbanco, string StrProforma, string fechaInicio, string fechaFin, string usercode) {
            bool exito = true;
            CultureInfo cultureInfo = new CultureInfo("es-PE");
            List<OracleParameter> parameters = new List<OracleParameter>();
            ResponseControl Response = new ResponseControl(ResponseControl.Status.Ok);
            List<Conciliacion> ObjListaConciliacion = new List<Conciliacion>();
            State_voucher voucher;
            try
            {

                parameters.Add(new OracleParameter("P_SIDPROCESS", OracleDbType.Varchar2, idproceso, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NIDBANK", OracleDbType.Int32, idbanco, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NPRODUCT", OracleDbType.Int32, idproducto, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_SPROFORMA", OracleDbType.Varchar2, StrProforma, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_DFECHAINI", OracleDbType.Date, DateTime.Parse(fechaInicio, cultureInfo), ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_DFECHAFIN", OracleDbType.Date, DateTime.Parse(fechaFin, cultureInfo), ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NUSERCODE", OracleDbType.Varchar2, usercode, ParameterDirection.Input));

                var pCode = new OracleParameter("P_NCODE", OracleDbType.Int16, ParameterDirection.Output);
                var p_Message = new OracleParameter("P_SMESSAGE", OracleDbType.NVarchar2, ParameterDirection.Output);
                var table = new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output);
                p_Message.Size = 2500;
                table.Size = 4000;
                parameters.Add(pCode);
                parameters.Add(p_Message);
                parameters.Add(table);
                //_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_GENERA_PLANILLA_DOCUMENTO", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_LIQUIDACION_MANUAL", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
                {
                    while (dr.Read())
                    {
                        var _conciliacion = new Conciliacion();
                        _conciliacion.IsValido = true;
                        _conciliacion.IdProceso = idproceso;
                        _conciliacion.IdProducto = idproducto;
                        _conciliacion.NumeroRecibo = dr["SPROFORMA"] == DBNull.Value ? string.Empty : dr["SPROFORMA"].ToString();
                        _conciliacion.Importe = dr["NPREMIUM"] == DBNull.Value ? string.Empty : dr["NPREMIUM"].ToString();
                        _conciliacion.FechaVencimiento = dr["DEXPIRDAT"] == DBNull.Value ? string.Empty : dr["DEXPIRDAT"].ToString().Substring(0,10);
                        _conciliacion.FechaPago = dr["DCOMPDATE"] == DBNull.Value ? string.Empty : dr["DCOMPDATE"].ToString().Substring(0, 10);
                        _conciliacion.IdMoneda = dr["NCURRENCY"] == DBNull.Value ? string.Empty : dr["NCURRENCY"].ToString();
                        _conciliacion.NumeroDocuento = dr["SIDDOC"] == DBNull.Value ? string.Empty : dr["SIDDOC"].ToString();
                        ObjListaConciliacion.Add(_conciliacion);
                    }
                    Response.Data = ObjListaConciliacion;
                    Response.message = p_Message.Value.ToString();
                    if (!(pCode.Value.ToString() == "0"))
                    {
                        Response.Code = pCode.Value.ToString();
                        throw new Exception(p_Message.Value.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Task.FromResult<ResponseControl>(Response);
        }
        public Task<ResponseControl> ObtenerFormaPago(int idBanco, string idProceso)
        {
            
            List<OracleParameter> parameters = new List<OracleParameter>();
            ResponseControl Response = new ResponseControl(ResponseControl.Status.Ok);
            List<ListaConciliacion> ListFormaPago = new List<ListaConciliacion>();
            ListaConciliacion conciliacion;
            try
            {

                parameters.Add(new OracleParameter("P_SIDPROCESS", OracleDbType.Varchar2, idProceso, ParameterDirection.Input));
                parameters.Add(new OracleParameter("P_NIDBANK", OracleDbType.Int16, idBanco, ParameterDirection.Input));

                var pCode = new OracleParameter("P_NCODE", OracleDbType.Int16, ParameterDirection.Output);
                var p_Message = new OracleParameter("P_SMESSAGE", OracleDbType.NVarchar2, ParameterDirection.Output);
                var table = new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output);
                p_Message.Size = 2500;
                table.Size = 4000;
                parameters.Add(pCode);
                parameters.Add(p_Message);
                parameters.Add(table);
                //_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_GENERA_PLANILLA_DOCUMENTO", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_PLANILLA_AUTOMATICA", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
                {
                    while (dr.Read())
                    {
                        conciliacion = new ListaConciliacion
                        {
                            IdMoneda         =    (dr["NIDCURRENCY"] != null ? dr["NIDCURRENCY"].ToString() : string.Empty),
                            IdTipoPago       =    (dr["NIDPAIDTYPE"] != null ? dr["NIDPAIDTYPE"].ToString() : string.Empty),
                            IdCuentaBanco    =    (dr["NIDACCOUNTBANK"] != null ? dr["NIDACCOUNTBANK"].ToString() : string.Empty),
                            IdBanco          =    (dr["NIDBANK"] != null ? dr["NIDBANK"].ToString() : string.Empty),
                            NumeroOperacion  =    (dr["SOPERATION_NUMBER"] != null ? dr["SOPERATION_NUMBER"].ToString() : string.Empty),
                            FechaOperacion   =    (dr["DOPERATION_DATE"] != null ? dr["DOPERATION_DATE"].ToString() : string.Empty),
                            Referencia       =    (dr["SREFERENCE"] != null ? dr["SREFERENCE"].ToString() : string.Empty),
                            MontoFormaPago   =    (dr["NAMOUNT"] != null ? dr["NAMOUNT"].ToString() : string.Empty),
                        };
                        ListFormaPago.Add(conciliacion);
                    }
                    Response.Data = ListFormaPago;
                    Response.message = p_Message.Value.ToString();
                    if (!(pCode.Value.ToString() == "0"))
                    {
                        Response.Code = pCode.Value.ToString();
                        throw new Exception(p_Message.Value.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Task.FromResult<ResponseControl>(Response);
        }

        public Task<List<Cuenta>> ListarCuenta(int idBanco)
        {
            Cuenta entities = null;
            List<Cuenta> ListCuentas = new List<Cuenta>();
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("P_IDBANK", OracleDbType.Int16,idBanco, ParameterDirection.Input));
            parameters.Add(new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output));
            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.PA_SEL_ACCOUNTBANK", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
            {
                while (dr.Read())
                {
                    entities = new Cuenta();
                    entities.idCuenta = Convert.ToInt32(dr["NIDACCOUNTBANK"]);
                    entities.DescripcionCuenta = Convert.ToString(dr["SDESCRIPT"]);
                    ListCuentas.Add(entities);
                }
            }

            return Task.FromResult<List<Cuenta>>(ListCuentas);
        }



        public Task<bool> Insertar_Respuesta_FE(State_voucher _Voucher)
        {
            CultureInfo cultureInfo = new CultureInfo("es-PE");
            bool exito = true;
            try
            {  
                        List<OracleParameter> parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("P_NINSUR_AREA", OracleDbType.Int64, _Voucher.NINSUR_AREA ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SBILLTYPE", OracleDbType.Varchar2, _Voucher.BILLTYPE, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_SBILLING", OracleDbType.Varchar2, _Voucher.SBILING ?? string.Empty, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_NBILLNUM", OracleDbType.Int64, _Voucher.NBILLNUM, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_ADDRESS_SUBMIT", OracleDbType.Varchar2, _Voucher.OPERADOR , ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_RESPONSE", OracleDbType.Varchar2, _Voucher.Resultado, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_STATUS", OracleDbType.Varchar2, _Voucher.status, ParameterDirection.Input));
                        parameters.Add(new OracleParameter("P_APPLICATION", OracleDbType.Varchar2, _Voucher.Application, ParameterDirection.Input));


                        var pCode = new OracleParameter("P_NCODE", OracleDbType.Int16, ParameterDirection.Output);
                        var p_Message = new OracleParameter("P_SMESSAGE", OracleDbType.NVarchar2, ParameterDirection.Output);
                        p_Message.Size = 2500;

                        parameters.Add(pCode);
                        parameters.Add(p_Message);
                        _connectionBase.ExecuteByStoredProcedure("PKG_SCTR_COBRANZAS.SPS_INS_RESPONSE_FE", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion);
                        bool flag = pCode.Value.ToString() == "0" ? true : false;
                        if (!flag)
                        {
                            exito = false;
                            throw new Exception(p_Message.Value.ToString());

                        }
                    
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return Task.FromResult<bool>(exito);
        }

        public Task<List<Tipo_Pago>> ListarTipoPago()
        {
            Tipo_Pago entities = null;
            List<Tipo_Pago> ListarTipopago = new List<Tipo_Pago>();
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("C_TABLE", OracleDbType.RefCursor, ParameterDirection.Output));
            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("PKG_PAYROLL.PA_SEL_TYPE_PAY", parameters, ConnectionBase.enuTypeDataBase.OracleVTime))
            {
                while (dr.Read())
                {
                    entities = new Tipo_Pago();
                    entities.id = Convert.ToInt32(dr["NIDPAY"]);
                    entities.name = Convert.ToString(dr["SDESCRIPTION"]);
                    ListarTipopago.Add(entities);
                }
            }

            return Task.FromResult<List<Tipo_Pago>>(ListarTipopago);
        }
    }
}

