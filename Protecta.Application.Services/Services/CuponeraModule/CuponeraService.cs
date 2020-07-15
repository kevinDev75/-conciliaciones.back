using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Protecta.Application.Service.Dtos.Cuponera;
using Protecta.CrossCuting.Log.Contracts;
using Protecta.CrossCuting.Reports;
using Protecta.CrossCuting.Utilities.Configuration;
using Protecta.Domain.Service.CuponeraModule.Aggregates;
using Protecta.Domain.Service.CuponeraModule.Aggregates.CuponeraAgg;

namespace Protecta.Application.Service.Services.CuponeraModule
{
    public class CuponeraService : ICuponeraService
    {

        private readonly ICuponeraRepository _cuponeraRepository;
        private ILoggerManager _logger;
        private IMapper _mapper;
        private readonly Ldap _ldapSettings;


        public CuponeraService(ICuponeraRepository cuponeraRepository, ILoggerManager logger, IMapper mapper, IOptions<Ldap> ldapSettings)
        {

            this._cuponeraRepository = cuponeraRepository;
            _logger = logger;
            _mapper = new MapperConfiguration(Config => { Config.CreateMissingTypeMaps = true; }).CreateMapper();
            _ldapSettings = ldapSettings.Value;
        }


        public async Task<IEnumerable<TransacionDto>> ListarTransaciones()
        {
            IEnumerable<TransacionDto> transacionDtos = null;

            try
            {
                var TransacionResult = await _cuponeraRepository.ListarTransaciones();
                if (TransacionResult == null) return null;
                transacionDtos = _mapper.Map<IEnumerable<TransacionDto>>(TransacionResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }

            return transacionDtos;
        }

        public async Task<ReciboDto> GetInfoRecibo(ParametersReciboDto parameters)
        {
            ReciboDto recibo = new ReciboDto();
            try
            {
                GenerateResponse response = new GenerateResponse();
                response = await _cuponeraRepository.ValidateRecibo(_mapper.Map<ParametersRecibo>(parameters));

                if (response.P_NCODE == 0) {

                    var ReciboResult = await _cuponeraRepository.GetInfoRecibo(_mapper.Map<ParametersRecibo>(parameters));
                    if (ReciboResult == null) return null;
                    recibo = _mapper.Map<ReciboDto>(ReciboResult);
                }
                else
                {
                    recibo.P_NCODE = response.P_NCODE;
                    recibo.P_SMESSAGE = response.P_SMESSAGE;
                }


            } catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return recibo;
        }

        public async Task<List<CuponDto>> GetInfoCuponPreview(ParametersReciboDto parameters)
        {
            List<CuponDto> ListCupon = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.GetInfoCuponPreview(_mapper.Map<ParametersRecibo>(parameters));
                if (ReciboResult == null) return null;
                ListCupon = _mapper.Map<List<CuponDto>>(ReciboResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return ListCupon;
        }

        public async Task<List<CuponDto>> GetInfoCuponeraDetail(ParametersReciboDto parameters)
        {
            List<CuponDto> ListCupon = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.GetInfoCuponeraDetail(_mapper.Map<ParametersRecibo>(parameters));
                if (ReciboResult == null) return null;
                ListCupon = _mapper.Map<List<CuponDto>>(ReciboResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return ListCupon;
        }

        public async Task<GenerateResponse> GenerateCupon(ParametersReciboDto parametersRecibo)
        {
            GenerateResponse reponse = null;
            try
            {
                var ResponseResult = await _cuponeraRepository.GenerateCupon(_mapper.Map<ParametersRecibo>(parametersRecibo));
                if (ResponseResult == null) return null;
                reponse = _mapper.Map<GenerateResponse>(ResponseResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return reponse;
        }




        public async Task<ReciboDto> GetInfoCuponera(ParametersReciboDto parametersRecibo)
        {
            ReciboDto Cupon = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.GetInfoCuponera(_mapper.Map<ParametersRecibo>(parametersRecibo));
                if (ReciboResult == null) return null;
                Cupon = _mapper.Map<ReciboDto>(ReciboResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return Cupon;
        }

        public async Task<DetalleReciboDto> GetInfoMovimiento(ParametersReciboDto parametersRecibo)
        {
            DetalleReciboDto Cupon = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.GetInfoMovimiento(_mapper.Map<ParametersRecibo>(parametersRecibo));
                if (ReciboResult == null) return null;
                Cupon = _mapper.Map<DetalleReciboDto>(ReciboResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return Cupon;
        }

        public async Task<GenerateResponse> AnnulmentCupon(ParametersReciboDto parametersRecibo)
        {
            GenerateResponse Cupon = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.AnnulmentCupon(_mapper.Map<ParametersRecibo>(parametersRecibo));
                if (ReciboResult == null) return null;
                Cupon = _mapper.Map<GenerateResponse>(ReciboResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return Cupon;
        }

        public async Task<GenerateResponse> PrintCupon(PrintCupon paramPrint)
        {
            GenerateResponse Cupon = null;
            List<TemplateCupon1> template = null;
            try
            {
                var ReciboResult = await _cuponeraRepository.PrintCupon(_mapper.Map<PrintCupon>(paramPrint));
                if (ReciboResult == null) return null;
                template = _mapper.Map<List<TemplateCupon1>>(ReciboResult);

                Generate generate = new Generate();
                string base64 = generate.GeneratePDF(CuponPagoDatatable(template[0]), CuponPagoLugares()) ;

                Cupon = new GenerateResponse() { data = base64 };
                if (template.Count > 0)
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
            return Cupon;
        }
        private DataTable CuponPagoDatatable(TemplateCupon1 cupon)
        {
            DataTable dt = new DataTable();
            dt.TableName = "CuponPago";
            dt.Columns.Add("CONVENIO");
            dt.Columns.Add("PAGO_NUMERO");
            dt.Columns.Add("VENCIMIENTO_PAGO");
            dt.Columns.Add("IMPORTE");
            DataRow row = dt.NewRow();
            row["CONVENIO"] = cupon.Convenio;
            row["PAGO_NUMERO"] = cupon.Cupon;
            row["VENCIMIENTO_PAGO"] = cupon.FechaVencimiento;
            row["IMPORTE"] = cupon.Importe.ToString();
            dt.Rows.Add(row);
            return dt;
        }
        private DataTable CuponPagoLugares()
        {
            DataTable dt = new DataTable();
            dt.TableName = "LugaresPago";
            dt.Columns.Add("ENTIDAD");
            dt.Columns.Add("IND_PAG_WEB");
            dt.Columns.Add("IND_AGE_EXP");
            dt.Columns.Add("CUENTA_RECAUDA");
            DataRow row = dt.NewRow();
            row["ENTIDAD"] = "BBVA CONTINENTAL";
            row["IND_PAG_WEB"] = "SI";
            row["IND_AGE_EXP"] = "SI";
            row["CUENTA_RECAUDA"] = "1213213123";
            dt.Rows.Add(row);
            return dt;
        }




    }
}
