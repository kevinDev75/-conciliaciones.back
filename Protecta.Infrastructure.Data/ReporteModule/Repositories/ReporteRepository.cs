using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Protecta.CrossCuting.Utilities.Configuration;
using Protecta.Domain.Service.ConsultaModule.Aggregates.ConsultaAgg;
using Protecta.Domain.Service.ReporteModule.Aggregates.ReporteAgg;
using Protecta.Infrastructure.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Protecta.Infrastructure.Data.ReporteModule.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly IConnectionBase _connectionBase;

        public ReporteRepository(IOptions<AppSettings> appSettings, IConnectionBase ConnectionBase)
        {
            this.appSettings = appSettings;
            _connectionBase = ConnectionBase;
        }

        public Task<List<ReporteDepositoPendiente>> ReporteDepositosPendientes(DatosReporteConciliacionPendiente datosConsulta)
        {
            ReporteDepositoPendiente entities = null;
            List<ReporteDepositoPendiente> listaEntidades = new List<ReporteDepositoPendiente>();
            List<OracleParameter> parameters = new List<OracleParameter>();

            long _idProducto = datosConsulta.IdProducto;          
            parameters.Add(new OracleParameter("P_CU_SALIDA", OracleDbType.RefCursor, ParameterDirection.Output));

            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("RAC_CONCILIACION_REPORTES.REP_DEPOSITOS_PENDIENTES", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
            {
                while (dr.Read())
                {
                    entities = new ReporteDepositoPendiente();
                    entities.IdDeposito = Convert.ToInt64(dr["ID_DEPOSITO"]);
                    entities.NumeroOperacion = Convert.ToString(dr["VC_NUMERO_OPERACION"]);
                    entities.Monto = Convert.ToDecimal(dr["DC_MONTO"]);
                    entities.Saldo = Convert.ToDecimal(dr["DC_SALDO"]);
                    entities.FechaDeposito = Convert.ToDateTime(dr["dt_fecha_deposito"]);
                    entities.NombreArchivo = Convert.ToString(dr["VC_NOMBRE_ARCHIVO"]);
                    entities.Cuenta = Convert.ToString(dr["NUMERO_CUENTA"]);
                    entities.Banco = Convert.ToString(dr["VC_NOMBRE"]);
                    listaEntidades.Add(entities);
                }
            }

            return Task.FromResult<List<ReporteDepositoPendiente>>(listaEntidades);
        }

        public Task<List<ReportePlanillaPendiente>> ReportePlanillasPendientes(DatosReporteConciliacionPendiente datosConsulta)
        {
            ReportePlanillaPendiente entities = null;
            List<ReportePlanillaPendiente> listaPlanillas = new List<ReportePlanillaPendiente>();
            List<OracleParameter> parameters = new List<OracleParameter>();

            long _idProducto = datosConsulta.IdProducto;          
          
            parameters.Add(new OracleParameter("P_ID_PRODUCTO", OracleDbType.Long, _idProducto, ParameterDirection.Input));
            parameters.Add(new OracleParameter("P_CU_SALIDA", OracleDbType.RefCursor, ParameterDirection.Output));

            using (OracleDataReader dr = (OracleDataReader)_connectionBase.ExecuteByStoredProcedure("RAC_CONCILIACION_REPORTES.REP_PLANILLAS_PENDIENTES", parameters, ConnectionBase.enuTypeDataBase.OracleConciliacion))
            {
                while (dr.Read())
                {
                    entities = new ReportePlanillaPendiente();
                    entities.IdPlanilla = Convert.ToInt32(dr["ID_PLANILLA"].ToString());
                    entities.FechaPlanilla = Convert.ToDateTime(dr["DT_FECHA_PLANILLA"]);
                    entities.Monto = Convert.ToDecimal(dr["DC_MONTO"]).ToString("00.00");                    
                    entities.IdTipoMedioPago = Convert.ToInt32(dr["ID_TIPO_MEDIO_PAGO"].ToString());
                    entities.DescripcionMedioPago = Convert.ToString(dr["DESC_MEDIO_PAGO"]);
                    entities.NumeroOperacion = Convert.ToString(dr["VC_NUMERO_OPERACION"]);
                    entities.FechaProceso = Convert.ToDateTime(dr["dt_fecha_proceso"]);
                  
                    //entities.DtFechaPlanilla = Convert.ToDateTime(dr["DT_FECHA_PLANILLA"].ToString()).ToShortDateString();
                    //entities.IdMoneda = Convert.ToInt64(dr["ID_MONEDA"].ToString());                   
                    listaPlanillas.Add(entities);
                }
            }

            return Task.FromResult<List<ReportePlanillaPendiente>>(listaPlanillas);
        }
    }
}
