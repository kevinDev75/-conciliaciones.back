using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Protecta.Application.Service.Dtos.Cobranzas;
using Protecta.Application.Service.Models;
using Protecta.CrossCuting.Log.Contracts;
using Protecta.Domain.Service.CobranzaModule.Aggregates.CobranzaAgg;
using Microsoft.Extensions.Configuration;
using Protecta.CrossCuting.Utilities.Configuration;
using System.Text;
using System.Net;
using Microsoft.Extensions.Options;
using System.Security.Principal;

namespace Protecta.Application.Service.Services.CobranzasModule
{
    public class CobranzasService : ICobranzasService
    {
        private readonly ICobranzaRepository _cobranzasRepository;
        private ILoggerManager _logger;
        private IMapper _mapper;
        private readonly Ldap _ldapSettings;
        private string NumeroCuenta { get; set; }
        private string IdMoneda { get; set; }
        private int FlagExtorno { get; set; }

        public CobranzasService(ICobranzaRepository cobranzaRepository, ILoggerManager logger, IMapper mapper, IOptions<Ldap> ldapSettings)
        {

            this._cobranzasRepository = cobranzaRepository;
            _logger = logger;
            //_mapper = mapper;
            _mapper = new MapperConfiguration(Config => { Config.CreateMissingTypeMaps = true; }).CreateMapper();
            _ldapSettings = ldapSettings.Value;
        }
        public async Task<IEnumerable<BancoDto>> ListarBancos()
        {
            IEnumerable<BancoDto> BancoDtos = null;

            try
            {
                var bancoResult = await _cobranzasRepository.ListarBancos();
                if (bancoResult == null) return null;
                BancoDtos = _mapper.Map<IEnumerable<BancoDto>>(bancoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }

            return BancoDtos;
        }

        public void ValidateProforma(ref ConciliacionDto conciliacionDto, ref List<string> ListProforma,string Line,int segmento,int idbanco)
        {
            if (conciliacionDto.IsValido && (((idbanco == 1 || idbanco == 2) && segmento == 2) || (idbanco == 3 && segmento == 1)) )
            {
                if (ListProforma.Contains(conciliacionDto.NumeroRecibo.Trim()))
                {
                    conciliacionDto.IsValido = false;
                    conciliacionDto.Mensaje = "Excepción en la Ln " + Line + " : El código de proforma ya existe en la trama.";
                }
                else
                {
                    ListProforma.Add(conciliacionDto.NumeroRecibo.Trim());
                }
            }
        }



        public async Task<TramaDto> ValidarTrama(string usercode, string base64String, int idbanco, int idproducto, string idproceso, string fechaInicio, string fechaFinal, string CodProforma)
        {


            int CantidadCab = 0;
            string MontoCab = string.Empty;
            decimal MontoDet = 0;
            int CantidadDet = 0;


            string MontoOrigen = string.Empty;
            decimal MontoDetOrigen = 0;
            

            Boolean ExistCab = true, ExistPie = true;
            Boolean ExistDet = false;
            List<string> ListProforma = new List<string>();


            TramaDto tramaDto = new TramaDto();
            ConciliacionDto conciliacionDto = new ConciliacionDto();
            DateTime tinicial = DateTime.Now;
            tramaDto.listado = new List<ListadoConciliacionDto>();
            int totalfilas = 1;
            int indice = 1;
            bool ProcesoValido = true;
            idproceso = string.IsNullOrEmpty(idproceso) ? ObtenerIdProceso(idbanco, idproducto, usercode) : idproceso;


            if (idbanco != 0)
            {
                Stream stream = new MemoryStream(Convert.FromBase64String(base64String));
                StreamReader file = new StreamReader(stream);

                string line;
                int numeroFilas = 1;
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        totalfilas++;
                    }
                }
                stream = new MemoryStream(Convert.FromBase64String(base64String));
                file = new StreamReader(stream);

                while ((line = file.ReadLine()) != null)
                {
                   
                        if (!string.IsNullOrEmpty(line.Trim()))
                        {
                            tramaDto.IdProducto = idproducto;
                            tramaDto.StringTrama = line.Trim();
                            tramaDto.IdBanco = idbanco;
                            tramaDto.Fila = numeroFilas.ToString();
                            tramaDto.Segmento = ObtenerSegmento(numeroFilas, totalfilas, idbanco);
                            tramaDto.TipoIngreso = "R";
                            
                            var tramaResult = await _cobranzasRepository.ValidarTrama(_mapper.Map<Trama>(tramaDto));
                            conciliacionDto = _mapper.Map<ConciliacionDto>(tramaResult);



                           if (tramaDto.Segmento == 1 && conciliacionDto.IsValido == false) { ExistCab = false; }
                           if (tramaDto.Segmento == 3 && conciliacionDto.IsValido == false) { ExistPie = false; }
                           ValidateProforma(ref conciliacionDto, ref ListProforma,tramaDto.Fila,tramaDto.Segmento,idbanco);
                           if (((idbanco == 1 || idbanco == 2) && tramaDto.Segmento == 2) || (idbanco == 3 && tramaDto.Segmento == 1)){ExistDet = true;}

                        if (conciliacionDto.IsValido)
                            {
                            if (((idbanco == 1) && tramaDto.Segmento == 1) || ( idbanco == 2 && tramaDto.Segmento == 3))
                                {
                                    CantidadCab = Int32.Parse(tramaResult.CantTotal);
                                    MontoCab = FormatoImporte(tramaResult.MontoTotal);
                                    if(idbanco == 2) { 
                                    MontoOrigen = FormatoImporte(tramaResult.MontoTotalOrigen);
                                    }
                            }

                                conciliacionDto.IdBanco = idbanco;
                                conciliacionDto.IdProducto = idproducto;
                                conciliacionDto.IdProceso = idproceso;
                                conciliacionDto.UserCode = usercode;
                                
                                if (((idbanco == 1 || idbanco == 2) && tramaDto.Segmento == 2) || (idbanco == 3 && tramaDto.Segmento == 1))
                                {
                                    CantidadDet++;
                                    MontoDet = MontoDet + Convert.ToDecimal(FormatoImporte(conciliacionDto.Importe));
                                    if (idbanco == 2)
                                    {
                                    MontoDetOrigen = MontoDetOrigen + Convert.ToDecimal(FormatoImporte(conciliacionDto.ImporteOrigen));
                                }
                                tramaDto.listado.Add(ObtenerListadoConciliacion(conciliacionDto));
                                }
                                else if (idbanco == 2 && tramaDto.Segmento == 1)
                                {
                                    this.NumeroCuenta = conciliacionDto.NumeroCuenta;
                                    this.IdMoneda = conciliacionDto.IdMoneda;
                                }

                                tramaDto.EsValido = conciliacionDto.IsValido;
                                tramaDto.Mensaje = conciliacionDto.Mensaje;

                                indice++;
                            }
                            else
                            {
                                tramaDto.listado.Add(new ListadoConciliacionDto { IsValido = conciliacionDto.IsValido, Mensaje = conciliacionDto.Mensaje, TipoOperacion = "GP" });
                                ProcesoValido = false;
                                tramaDto.EsValido = conciliacionDto.IsValido;
                                //break;
                            }
                            numeroFilas++;
                            //if (!conciliacionDto.IsValido)
                            //    {
                            //        ProcesoValido = false;
                            //        tramaDto.EsValido = conciliacionDto.IsValido;
                            //        break;
                            //    }
                        }
                        else
                        {
                            ProcesoValido = false;
                            break;
                        }
                     
                }
                try {
                    if ((idbanco == 1 && ExistCab && ExistDet) || (idbanco == 2 && tramaDto.Segmento == 3 && ExistPie && ExistDet)) {
                            if (CantidadCab != CantidadDet)
                            {
                                tramaDto.listado.Add(new ListadoConciliacionDto { IsValido = false, Mensaje = "Excepción " + ((idbanco == 2) ? "Pie" : "Cabecera") + " : La cantidad total de registros enviados no corresponde con el detalle.", TipoOperacion = "GP" });
                            }
                            if (MontoDet.ToString("N2") != Convert.ToDecimal(MontoCab).ToString("N2"))
                            {
                                tramaDto.listado.Add(new ListadoConciliacionDto { IsValido = false, Mensaje = "Excepción " + ((idbanco == 2) ? "Pie" : "Cabecera") + " : El monto total de los importes pagados no corresponde con el detalle.", TipoOperacion = "GP" });
                            }
                        if (idbanco == 2)
                        {
                            if (MontoDetOrigen.ToString("N2") != Convert.ToDecimal(MontoOrigen).ToString("N2"))
                            {
                                tramaDto.listado.Add(new ListadoConciliacionDto { IsValido = false, Mensaje = "Excepción " + ((idbanco == 2) ? "Pie" : "Cabecera") + " : El monto total de pagos no corresponde con la suma de importes de origen.", TipoOperacion = "GP" });
                            }
                        }
                    }
                    if (!ExistDet)
                    {
                        tramaDto.listado.Add(new ListadoConciliacionDto { IsValido = false, Mensaje = "Excepción Detalle : No se encontró ningún detalle en la trama.", TipoOperacion = "GP" });
                    }
                }catch(Exception ex)
                {

                }

            }
            else
            {
                try
                {
                    var LiqManual = _mapper.Map<Models.ResponseControl>( await _cobranzasRepository.ObtenerLiquidacionManual(idproceso, idproducto, idbanco, CodProforma, fechaInicio, fechaFinal, usercode));
                    if (LiqManual.Code != "1")
                    {
                        var LstConciliacion = _mapper.Map<List<ConciliacionDto>>(LiqManual.Data);
                        foreach (ConciliacionDto conciliacion in LstConciliacion)
                        {
                            conciliacion.UserCode = usercode;
                            var ListaConciliacion = ObtenerListadoConciliacionManual(_mapper.Map<ConciliacionDto>(conciliacion));
                            tramaDto.listado.Add(ListaConciliacion);
                        }
                        tramaDto.EsValido = true;
                        return tramaDto;
                    }
                } catch (Exception ex)
                {
                    tramaDto.EsValido = false;
                    tramaDto.Mensaje = ex.Message;
                }
                                
            }
            //if (ProcesoValido)
            if (tramaDto.listado.Where(x => x.IsValido == true).Count() > 0)
            {
                var result = await _cobranzasRepository.InsertarProceso(_mapper.Map<List<ListaConciliacion>>(tramaDto.listado));
                if (result)
                {
                    tramaDto.EsValido = true;
                    tramaDto.Mensaje = "Procesado con éxito";
                }
                else
                {
                    tramaDto.EsValido = false;
                    tramaDto.Mensaje = "No se pudo realizar la insercion de los documentos";
                }
            }
            else
            {
                tramaDto.EsValido = true;
                tramaDto.Mensaje = "No se pudo Encontraron documentos a insertar";
            }
            DateTime tfinal = DateTime.Now;
            TimeSpan totaltiempo = new TimeSpan(tfinal.Ticks - tinicial.Ticks);
            tramaDto.TiempoTranscurrido = totaltiempo.TotalSeconds.ToString();


            return tramaDto;
        }


        public async Task<PlanillaDto> ObtenerTrama(TramaDto trama)
        {
            PlanillaDto planillaDto = new PlanillaDto();
            var tramaResult = await _cobranzasRepository.ObtenerTrama(_mapper.Map<Trama>(trama));
            if (tramaResult == null) return null;
            planillaDto = _mapper.Map<PlanillaDto>(tramaResult);
            //   var nombreRuta = planillaDto.RutaTrama.Replace('','*').
            planillaDto.RutaTrama = Base64StringEncode(planillaDto.RutaTrama);
            //planillaDto.RutaTrama = Base64StringEncode(@"D:\Kevin\Develop\APPCONCILACIONES\Doc. Protecta\CARGA DE PLANOS AL BANCO\BCP\19-02\crep.txt");
            return planillaDto;
        }
        public async Task<Models.ResponseControl> InsertarFacturaFormaPago(List<ListadoConciliacionDto> listadoConciliacionDtos)
        {
            //var mapper = new MapperConfiguration(config => { config.CreateMissingTypeMaps = true; }).CreateMapper();
            Models.ResponseControl Rpt = new Models.ResponseControl();
            string idproceso = string.Empty, tipooperacion = string.Empty;
            int idproducto, idbanco, usercode;
            var response = await _cobranzasRepository.InsertarProceso(_mapper.Map<List<ListaConciliacion>>(listadoConciliacionDtos));
            if (listadoConciliacionDtos.Count > 0 && response)
            {

                idproceso = listadoConciliacionDtos[0].IdProceso;
                idproducto = int.Parse(listadoConciliacionDtos[0].IdProducto);
                idbanco = int.Parse(listadoConciliacionDtos[0].IdBanco);
                //tipooperacion = listadoConciliacionDtos[0].TipoOperacion;
                tipooperacion = (listadoConciliacionDtos[0].TipoOperacion == "FP") ? "GP" : listadoConciliacionDtos[0].TipoOperacion;
                usercode = int.Parse(listadoConciliacionDtos[0].UserCode);
                Rpt = _mapper.Map<Models.ResponseControl>((await _cobranzasRepository.GeneraPlanillaFactura(idproceso, idproducto, idbanco, tipooperacion, usercode)));

                if (Rpt.Data != null)
                {
                     SendVouchers((List<State_voucher>)Rpt.Data);
                }

                //Rpt = mapper.Map(Obj, Rpt);
                // =  (Obj);

                //cobranzasRepository.GeneraPlanillaFactura(idproceso, idproducto, idbanco, tipooperacion, usercode));
            }
            return Rpt;
        }

        private void SendVouchers(List<State_voucher> state_Voucher)
        {

            foreach (State_voucher voucher in state_Voucher)
            {
                if (voucher.SVALOR == "1") //Se genero la factura
                {
                        State_voucher Response = new State_voucher();
                        var ObjVoucher = _mapper.Map<CobranzaVoucher>(voucher);
                        string JsonRequest = JsonConvert.SerializeObject(ObjVoucher);
                        for(int i = 1; i <= 3; i++)
                        {
                            try
                            {
                                Response = JsonConvert.DeserializeObject<State_voucher>(SendFE(JsonRequest));
                                voucher.Resultado = Response.Resultado;
                                InsertarLogFE(voucher);
                                if (Response.Resultado == "ok") { break; }
                            }
                            catch (Exception ex)
                            {
                                voucher.Resultado = ex.Message;
                                InsertarLogFE(voucher);
                            }
                        }
                    
                }
            }
        }


        public async void InsertarLogFE(State_voucher voucher)
        {
            voucher.status = (voucher.Resultado == "ok" ? "1" : "0");
            voucher.Application = "CONCILIACIONES";
            Boolean RptLog = await _cobranzasRepository.Insertar_Respuesta_FE(voucher);
        }

        private string SendFE(string Json_request)
        {
            string response;


            var Url = _ldapSettings.UrlFE;
            byte[] data = UTF8Encoding.UTF8.GetBytes(Json_request);
           
                HttpWebRequest request;
                request = WebRequest.Create(Url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json; charset=utf-8";
                //request.Headers.Add("Usuario", user);
                //request.Headers.Add("Clave", password);
                request.Proxy = new WebProxy() { UseDefaultCredentials = true };

                Stream postTorrente = request.GetRequestStream();
                postTorrente.Write(data, 0, data.Length);
                postTorrente.Close();


                HttpWebResponse respuesta = request.GetResponse() as HttpWebResponse;
                StreamReader read = new StreamReader(respuesta.GetResponseStream(), Encoding.UTF8);
                response = read.ReadToEnd();
                return response;
            
          
        }


        private int ObtenerSegmento(int lineaactual, int totallineas, int idbanco)
        {
            int segmento = 0;
            switch (idbanco)
            {
                case 1:
                    segmento = lineaactual == 1 ? 1 : 2;
                    break;
                case 2:
                    segmento = lineaactual == 1 ? 1 : ((lineaactual < totallineas - 1) ? 2 : 3);
                    break;
                case 3:
                    segmento = 1;
                    break;
            }
            return segmento;
        }
        private string ObtenerIdProceso(int idbanco, int idproducto, string usercode)//ConciliacionDto conciliacionDto) //ListadoConciliacionDto
        {
            string fecha = DateTime.Now.ToString("ddMMyyyy");
            string hora = DateTime.Now.ToString("HHmmssms");
            return string.Format("{0}{1}{2}{3}{4}", usercode, idbanco.ToString(), idproducto.ToString(), fecha, hora);

        }
        private ListadoConciliacionDto ObtenerListadoConciliacion(ConciliacionDto conciliacionDto)
        {

            return new ListadoConciliacionDto()
            {
                IsValido = conciliacionDto.IsValido,
                Mensaje = conciliacionDto.Mensaje,
                IdProceso = conciliacionDto.IdProceso,
                IdBanco = conciliacionDto.IdBanco.ToString(),
                TipoOperacion = "GP",
                IdProducto = conciliacionDto.IdProducto.ToString(),
                NumeroRecibo = FormatoNumeroRecibo(conciliacionDto.NumeroRecibo, conciliacionDto.IdBanco),
                Importe = FormatoImporte(conciliacionDto.Importe),
                IdCuentaBanco = conciliacionDto.NumeroCuenta == string.Empty ? this.NumeroCuenta : conciliacionDto.NumeroCuenta,
                NombreCliente = conciliacionDto.NombreCliente,
                DocumentoCliente = conciliacionDto.NumeroDocuento.ToString(),
                FechaVencimiento = FormatoFecha(conciliacionDto.FechaVencimiento),
                FechaCargaArchivo = DateTime.Now.ToString("dd/MM/yyyy"),
                FechaOperacion = FormatoFecha(conciliacionDto.FechaPago),
                NumeroOperacion = conciliacionDto.NumeroOperacion,
                Referencia = conciliacionDto.Referencia,
                IdMoneda = conciliacionDto.IdMoneda == string.Empty ? this.IdMoneda : conciliacionDto.IdMoneda,
                //FlagExtorno = int.Parse(conciliacionDto.FlagExtorno)==2 || this.FlagExtorno == 2 ? 2 :  1
                FlagExtorno = int.Parse(conciliacionDto.FlagExtorno) == 2 ? 2 : 1,
                UserCode = conciliacionDto.UserCode
            };


        }
        private ListadoConciliacionDto ObtenerListadoConciliacionManual(ConciliacionDto conciliacionDto)
        {
            var retornar = new ListadoConciliacionDto();


            retornar.IsValido = conciliacionDto.IsValido;
                retornar.Mensaje = conciliacionDto.Mensaje;
            retornar.IdProceso = conciliacionDto.IdProceso;
            retornar.IdBanco = conciliacionDto.IdBanco.ToString();
            retornar.TipoOperacion = "GP";
            retornar.IdProducto = conciliacionDto.IdProducto.ToString();
            retornar.NumeroRecibo = FormatoNumeroRecibo(conciliacionDto.NumeroRecibo, conciliacionDto.IdBanco);
            retornar.Importe = conciliacionDto.Importe;
            retornar.IdCuentaBanco = conciliacionDto.NumeroCuenta == string.Empty ? this.NumeroCuenta : conciliacionDto.NumeroCuenta;
            retornar.NombreCliente = conciliacionDto.NombreCliente;
            retornar.DocumentoCliente = conciliacionDto.NumeroDocuento.ToString();
            retornar.FechaVencimiento = (conciliacionDto.FechaVencimiento);
            retornar.FechaCargaArchivo = DateTime.Now.ToString("dd/MM/yyyy");
            retornar.FechaOperacion = conciliacionDto.FechaPago;
            retornar.IdMoneda = conciliacionDto.IdMoneda == string.Empty ? this.IdMoneda : conciliacionDto.IdMoneda;
            //FlagExtorno = int.Parse(conciliacionDto.FlagExtorno)==2 || this.FlagExtorno == 2 ? 2 :  1
            retornar.FlagExtorno =2;
            retornar.UserCode = conciliacionDto.UserCode;
            return retornar;
        }
        private string FormatoNumeroRecibo(string numero, int idBanco)
        {
            string numeroRecibo = string.Empty;
            if (!string.IsNullOrEmpty(numero))
            {
                numero = numero.Trim();
                switch (idBanco)
                {
                    case 1:
                        numeroRecibo = numero.Substring(numero.IndexOf('0'), 9);
                        break;
                    case 2:
                        numeroRecibo = numero.Substring(numero.Length - 18, 18);
                        break;
                    case 3:
                        numeroRecibo = numero;
                        break;
                }
            }
            return (idBanco == 2) ? numeroRecibo : numero;
        }
        private string FormatoImporte(string numero)
        {
            string importe = string.Empty;
            int entero = 0;
            string parteEntera, parteDecimal;
            if (!string.IsNullOrEmpty(numero))
            {
                entero = int.Parse(numero.Trim());
                //this.FlagExtorno = entero > 0 ? 2 : 1;
                parteEntera = entero.ToString().Substring(0, entero.ToString().Length - 2);
                parteDecimal = entero.ToString().Substring(entero.ToString().Length - 2, 2);
                importe = string.Format("{0}.{1}", parteEntera, parteDecimal);
            }
            return importe;
        }
        private string FormatoFecha(string fecha)
        {
            if (!string.IsNullOrEmpty(fecha))
            {
                fecha = DateTime.ParseExact(fecha, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
            }
            return fecha;
        }
        private string Base64StringEncode(string ruta)
        {

            var rt = "";
            using (var impersonation = new ImpersonateUser(_ldapSettings.usernameServidor, _ldapSettings.Dominio, _ldapSettings.PasswordServidor))
            {
                WindowsIdentity.RunImpersonated(impersonation.Identity.AccessToken, () =>
                {
                    WindowsIdentity useri = WindowsIdentity.GetCurrent();
                    byte[] AsBytes = File.ReadAllBytes(ruta);
                    rt = Convert.ToBase64String(AsBytes);
                });
            }

            return rt;
        }



        public async Task<Models.ResponseControl> Validar_Planilla_FacturaAsync(ListadoConciliacionDto Listado)
        {
                return  _mapper.Map<Models.ResponseControl>(await _cobranzasRepository.Validar_Planilla_Factura(_mapper.Map<ListaConciliacion>(Listado)));  
        }

        public  async Task<Models.ResponseControl> ObtenerFormaPago(int idbanco, string idproceso)
        {
            
            try
            {
                    var  response = _mapper.Map<Models.ResponseControl>((await _cobranzasRepository.ObtenerFormaPago(idbanco,idproceso)));
                    if (response.Code == "0")
                    {
                        List<ListaConciliacion> ListFormaPago = (List<ListaConciliacion>)response.Data;
                    }
                    return response;
             }
             catch(Exception ex)
             {
                return new Models.ResponseControl
                {
                    Code = "1",
                    message = ex.Message
                };
             }
          }

        public async Task<IEnumerable<CuentaDto>> ListarCuenta(int Idbanco)
        {
            IEnumerable<CuentaDto> CuentaDtos = null;

            try
            {
                var CuentaResult = await _cobranzasRepository.ListarCuenta(Idbanco);
                if (CuentaResult == null) return null;
                CuentaDtos = _mapper.Map<IEnumerable<CuentaDto>>(CuentaResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }

            return CuentaDtos;
        }

        public  async Task<IEnumerable<Tipo_PagoDto>> ListarTipoPago()
        {
            IEnumerable<Tipo_PagoDto> TipoPagoDto = null;

            try
            {
                var TipoPagoResult = await _cobranzasRepository.ListarTipoPago();
                if (TipoPagoResult == null) return null;
                TipoPagoDto = _mapper.Map<IEnumerable<Tipo_PagoDto>>(TipoPagoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }

            return TipoPagoDto;
        }
    }

}
