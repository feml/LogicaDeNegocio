using System;
using System.Data;
using Oracle.DataAccess.Client;
namespace logicaNegocio
{
    public class logicaNegocios
    {
        private string _cadena;
        private DataTable _dtConsulta;
        private string _planilla;
        private DateTime _fecha;
        private string _propietario;
        private string _ministerio;
        private string _deseguro;
        private string _osp;
        private string _oficina;

        public logicaNegocios()
        {
            _cadena = "User Id=soporte;Password=soporte;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.6)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MILEBOG1)(FAILOVER_MODE=(TYPE=select)(METHOD=basic)(RETRIES=20)(DELAY=15))));";
        }

        public DataTable dataAdapterRobots(int dias)
        {
            _dtConsulta = new DataTable("Consulta");
            _dtConsulta.Columns.Add("Planilla", typeof(string));
            _dtConsulta.Columns.Add("Fecha", typeof(DateTime));
            _dtConsulta.Columns.Add("Oficina", typeof(string));
            _dtConsulta.Columns.Add("Ministerio", typeof(string));
            _dtConsulta.Columns.Add("Deseguro", typeof(string));
            _dtConsulta.Columns.Add("Osp", typeof(string));
            string select = @"select VIAJ_NOPLANILLA_V2 planilla, 
                                VIAJ_FECVIAJE_DT fecha,
                                VEHI_PROPIETARIO_NB propietario,
                                OFIC_NOMBRE_V2 oficina
                                from viajes,vehiculos,oficinas
                                where VIAJ_PLACA_CH=VEHI_PLACA_CH
                                and VIAJ_OFDESPACHA_NB=OFIC_CODOFIC_NB
                                and VIAJ_ESTADO_V2 not in ('A') 
                                --and VIAJ_NOPLANILLA_V2='009555627'
                                and trunc(VIAJ_FECVIAJE_DT) between trunc(sysdate-" + dias + @") and trunc(sysdate)  
                                --and rownum <11
                                order by fecha desc";
            using (OracleConnection con = new OracleConnection(_cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                try
                {
                    DataTable dttmp = new DataTable();
                    da.Fill(dttmp);
                    foreach (DataRow dr in dttmp.Rows)
                    {
                        _planilla = dr["planilla"].ToString();
                        _fecha = DateTime.Parse(dr["fecha"].ToString());
                        _propietario = dr["propietario"].ToString();
                        _oficina = dr["oficina"].ToString();
                        ministerio(_planilla);
                        agregarColumna(_planilla, _fecha, _oficina, _ministerio, _deseguro, _osp, _propietario);
                    }
                }
                catch (OracleException ex)
                {
                    string miexce = ex.Message;
                }
                catch (Exception ex)
                {
                    string miexce = ex.Message;
                }
                con.Close();
            }
            return _dtConsulta;
        }
        public DataTable estadoRobots(int dias)
        {
            _dtConsulta = new DataTable("Consulta");
            _dtConsulta.Columns.Add("Planilla", typeof(string));
            _dtConsulta.Columns.Add("Fecha", typeof(DateTime));
            _dtConsulta.Columns.Add("Oficina", typeof(string));
            _dtConsulta.Columns.Add("Ministerio", typeof(string));
            _dtConsulta.Columns.Add("Deseguro", typeof(string));
            _dtConsulta.Columns.Add("Osp", typeof(string));
            string select = @"select VIAJ_NOPLANILLA_V2 planilla, 
                                VIAJ_FECVIAJE_DT fecha,
                                VEHI_PROPIETARIO_NB propietario,
                                OFIC_NOMBRE_V2 oficina
                                from viajes,vehiculos,oficinas
                                where VIAJ_PLACA_CH=VEHI_PLACA_CH
                                and VIAJ_OFDESPACHA_NB=OFIC_CODOFIC_NB
                                and VIAJ_ESTADO_V2 not in ('A') 
                                --and VIAJ_NOPLANILLA_V2='009555627'
                                and trunc(VIAJ_FECVIAJE_DT) between trunc(sysdate-" + dias + @") and trunc(sysdate)  
                                --and rownum <11
                                order by fecha desc";
            using (OracleConnection con = new OracleConnection(_cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    _planilla = dr["planilla"].ToString();
                    _fecha = DateTime.Parse(dr["fecha"].ToString());
                    _propietario = dr["propietario"].ToString();
                    _oficina = dr["oficina"].ToString();
                    ministerio(_planilla);
                    agregarColumna(_planilla, _fecha, _oficina, _ministerio, _deseguro, _osp, _propietario);
                }
                con.Close();
            }

            return _dtConsulta;
        }

        private void ministerio(string planilla)
        {
            string cadena = "User Id=soporte;Password=soporte;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.6)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MILEBOG1)(FAILOVER_MODE=(TYPE=select)(METHOD=basic)(RETRIES=20)(DELAY=15))));";
            string select = @"select decode(LRMI_ESTADO_V2,'E',1,'P',2,'R',3,'U',5,LRMI_ESTADO_V2,4) estado from log_reporte_ministerio where LRMI_TRANSACCION_NB=4 and LRMI_LLAVE_V2='" + planilla + "'";
            using (OracleConnection con = new OracleConnection(cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                try
                {
                    con.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        _ministerio = dr["estado"].ToString();
                        if (_propietario == "2")//vehiculo tercero
                        {
                            _osp = "6";
                            deseguro(planilla);
                        }
                        else
                        {
                            _deseguro = "6";//el vehiculo es tercero no se reporta a destino seguro
                            osp(planilla);
                        }
                    }
                    con.Close();
                }
                catch (OracleException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }

        private void deseguro(string planilla)
        {
            string cadena = "User Id=soporte;Password=soporte;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.6)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MILEBOG1)(FAILOVER_MODE=(TYPE=select)(METHOD=basic)(RETRIES=20)(DELAY=15))));";
            string select = @"select decode(REDE_ESTADO_V2,'E',1,'P',2,'R',3,'T',1,REDE_ESTADO_V2,4) estado from log_reporte_deseguro where REDE_TRANSACCION_NB=4 and REDE_LLAVE_V2='" + planilla + "'";
            using (OracleConnection con = new OracleConnection(cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                try
                {
                    con.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        _deseguro = dr["estado"].ToString();
                        //osp(planilla);
                    }
                    con.Close();

                }
                catch (OracleException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }

        private void osp(string planilla)
        {
            string cadena = "User Id=soporte;Password=soporte;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.30.6)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MILEBOG1)(FAILOVER_MODE=(TYPE=select)(METHOD=basic)(RETRIES=20)(DELAY=15))));";
            string select = @"select decode(LPAD_ESTADO_V2,'E',1,'P',2,'R',3,'T',4,LPAD_ESTADO_V2,4) estado from log_plan_adminsat where LPAD_TRANSACCION_NB=4 and LPAD_LLAVE_V2='" + planilla + "'";
            using (OracleConnection con = new OracleConnection(cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                try
                {
                    con.Open();
                    OracleDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        _osp = dr["estado"].ToString();
                    }
                    con.Close();
                }
                catch (OracleException ex)
                {
                    //MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }
        private void agregarColumna(string planilla, DateTime fecha, string oficina, string ministerio, string deseguro, string osp, string propietario)
        {
            _dtConsulta.Rows.Add(planilla, fecha, oficina, ministerio, deseguro, osp);
            _planilla = string.Empty;
            _ministerio = string.Empty;
            _oficina = string.Empty;
            _deseguro = string.Empty;
            _osp = string.Empty;
            _fecha = DateTime.Now;
            _propietario = string.Empty;
        }
        public DataTable respuestaMinisterio(string planilla)
        {
            DataTable dtrespuesta = new DataTable("respuesta");
            dtrespuesta.Columns.Add("oficina", typeof(string));
            dtrespuesta.Columns.Add("fecenvio", typeof(DateTime));
            dtrespuesta.Columns.Add("id", typeof(string));
            dtrespuesta.Columns.Add("respuesta", typeof(string));
            dtrespuesta.Columns.Add("estado", typeof(string));

            string oficina = string.Empty;
            DateTime fecenvio = DateTime.Now;
            string id = string.Empty;
            string estado = string.Empty;
            string respuesta = string.Empty;
            string select = @"select DELM_SECUENCIA_NB secuencia,
OFIC_NOMBRE_V2 oficina,
DELM_FECENVIO_DT fecenvio,
DELM_IDMINISTERIO_NB id,
DELM_ESTADO_V2 estado,
DELM_XMLRECIBIDO_XML recibido
from det_log_ministerio,oficinas
where DELM_OFICINA_NB=OFIC_CODOFIC_NB
and DELM_TRANSACCION_NB=4
and DELM_LLAVE_V2='009171757'
order by secuencia desc";
            using (OracleConnection con = new OracleConnection(_cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    oficina = dr["oficina"].ToString();
                    fecenvio = DateTime.Parse(dr["fecenvio"].ToString());
                    id = dr["id"].ToString();
                    estado = dr["estado"].ToString();
                    respuesta = dr["recibido"].ToString();
                    break;
                }
                con.Close();
            }
            dtrespuesta.Rows.Add(oficina, fecenvio, id, respuesta, estado);
            return dtrespuesta;
        }

        public DataTable respuestaDeseguro(string planilla)
        {
            DataTable dtrespuesta = new DataTable("respuesta");
            dtrespuesta.Columns.Add("oficina", typeof(string));
            dtrespuesta.Columns.Add("fecenvio", typeof(DateTime));
            dtrespuesta.Columns.Add("id", typeof(string));
            dtrespuesta.Columns.Add("respuesta", typeof(string));
            dtrespuesta.Columns.Add("estado", typeof(string));

            string oficina = string.Empty;
            DateTime fecenvio = DateTime.Now;
            string id = string.Empty;
            string estado = string.Empty;
            string respuesta = string.Empty;
            string select = @"";
            using (OracleConnection con = new OracleConnection(_cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    oficina = dr["oficina"].ToString();
                    fecenvio = DateTime.Parse(dr["fecenvio"].ToString());
                    id = dr["id"].ToString();
                    estado = dr["estado"].ToString();
                    respuesta = dr["recibido"].ToString();
                    break;
                }
                con.Close();
            }
            dtrespuesta.Rows.Add(oficina, fecenvio, id, respuesta, estado);
            return dtrespuesta;
        }
        public DataTable respuestaOsp(string planilla)
        {
            DataTable dtrespuesta = new DataTable("respuesta");
            dtrespuesta.Columns.Add("oficina", typeof(string));
            dtrespuesta.Columns.Add("fecenvio", typeof(DateTime));
            dtrespuesta.Columns.Add("id", typeof(string));
            dtrespuesta.Columns.Add("respuesta", typeof(string));
            dtrespuesta.Columns.Add("estado", typeof(string));

            string oficina = string.Empty;
            DateTime fecenvio = DateTime.Now;
            string id = string.Empty;
            string estado = string.Empty;
            string respuesta = string.Empty;
            string select = @"";
            using (OracleConnection con = new OracleConnection(_cadena))
            {
                OracleCommand cmd = new OracleCommand(select, con);
                cmd.CommandType = CommandType.Text;
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    oficina = dr["oficina"].ToString();
                    fecenvio = DateTime.Parse(dr["fecenvio"].ToString());
                    id = dr["id"].ToString();
                    estado = dr["estado"].ToString();
                    respuesta = dr["recibido"].ToString();
                    break;
                }
                con.Close();
            }
            dtrespuesta.Rows.Add(oficina, fecenvio, id, respuesta, estado);
            return dtrespuesta;
        }

    }
}
