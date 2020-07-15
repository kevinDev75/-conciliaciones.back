using Microsoft.Reporting.WinForms;
using Protecta.Domain.Service.CuponeraModule.Aggregates.CuponeraAgg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Protecta.Domain.Service.CuponeraModule.Aggregates
{
    public class GenerateReports
    {
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


        public string GeneratePDF(TemplateCupon1 cupon)
        {
            string base64String = string.Empty;
            try
            {
                ReportDataSource dsOBJ = new ReportDataSource();
                dsOBJ.Name = "TblCuponPago";
                dsOBJ.Value = CuponPagoDatatable(cupon);
                ReportDataSource dsOBJ2 = new ReportDataSource();
                dsOBJ.Name = "TblLugaresPago";
                dsOBJ.Value = CuponPagoLugares();

                IEnumerable<ReportDataSource> datasets = new List<ReportDataSource> { dsOBJ, dsOBJ2 };
                LocalReport localReport = new LocalReport();
                string startupPath = System.AppDomain.CurrentDomain.BaseDirectory;
                //localReport.ReportPath = startupPath + @"Reports\\ReportError.rdlc";
                localReport.ReportPath = @"C:\Report\CuponPago.rdlc";

                ReportParameter[] reportParameter = new ReportParameter[9];
                reportParameter[0] = new ReportParameter("RAMO", cupon.DesRamo);
                reportParameter[1] = new ReportParameter("POLIZA", cupon.Policy);
                reportParameter[2] = new ReportParameter("MONEDA", cupon.DesMoneda);
                reportParameter[3] = new ReportParameter("VIGENCIA_DESDE", cupon.VigenciaDesde);
                reportParameter[4] = new ReportParameter("VIGENCIA_HASTA", cupon.VigenciaHasta);
                reportParameter[5] = new ReportParameter("ASEGURADO", cupon.Asegurado);
                reportParameter[6] = new ReportParameter("DOCUMENTO", cupon.Documento);
                reportParameter[7] = new ReportParameter("DIRECCION", cupon.Direccion);
                reportParameter[8] = new ReportParameter("INTERMEDIARIO", cupon.Intermediario);
                reportParameter[9] = new ReportParameter("CUPON_CABECERA", cupon.Cupon);

                localReport.SetParameters(reportParameter);
                localReport.Refresh();
                foreach (ReportDataSource datasource in datasets)
                {
                    localReport.DataSources.Add(datasource);
                }
                

                //Renderizado
                string deviceInfo = "<DeviceInfo><OutputFormat>PDF</OutputFormat></DeviceInfo>";
                Warning[] warnings;
                string[] streams;
                string mimeType;
                byte[] renderedBytes;
                string encoding;
                string fileNameExtension;


                localReport.Refresh();
                renderedBytes =  localReport.Render("PDF", deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                base64String = Convert.ToBase64String(renderedBytes, 0, renderedBytes.Length);
            }
            catch (Exception ex)
            {
                base64String = null;
            }
            return base64String;
        }
    }
}
