using DbManager;
using MedAI.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedAI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        DbAccess con;
        OracleConnection aOracleConnection;
        Dictionary<string, string> whereValues = new Dictionary<string, string>();

        void Open()
        {
            con = new DbAccess();
            aOracleConnection = con.get_con();
        }
        void Close()
        {
            con.Close(aOracleConnection);
        }
        [HttpGet]
        public List<DIAGNOSTIC> GetAllDiagnostics()
        {
            string sql = $@"select * from DIAGNOSTICS";

            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                DataTable dt = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from DIAGNOSTICS", whereValues, true);
                List<DIAGNOSTIC> lst = new List<DIAGNOSTIC>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string DIAGNOSTICNAME = dt.Rows[i]["DIAGNOSTICNAME"].ToString();
                        int DIAGNOSTICID = Convert.ToInt32(dt.Rows[i]["DIAGNOSTICID"].ToString());
                        string DESCRIPTION = dt.Rows[i]["DESCRIPTION"].ToString();
                        string TESTTYPE = dt.Rows[i]["TESTTYPE"].ToString();
                        string SYMPTOMS = dt.Rows[i]["SYMPTOMS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lst.Add(new DIAGNOSTIC() { SYMPTOMS = SYMPTOMS, TESTTYPE = TESTTYPE, DESCRIPTION = DESCRIPTION, DIAGNOSTICID = DIAGNOSTICID, DIAGNOSTICNAME = DIAGNOSTICNAME });
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                CmdTrans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        [HttpGet]
        [Route("{id:int}")]
        public List<DIAGNOSTIC> GetDiagnosticById(int id)
        {
            string sql = $@"select * from DIAGNOSTICS where DIAGNOSTICID = :id";

            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                List<DIAGNOSTIC> lst = new List<DIAGNOSTIC>();
                var cmd = aOracleConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.Add("id", id);
                cmd.Transaction = CmdTrans;
                cmd.ExecuteNonQuery();
                CmdTrans.Commit();
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string DIAGNOSTICNAME = dt.Rows[i]["DIAGNOSTICNAME"].ToString();
                        int DIAGNOSTICID = Convert.ToInt32(dt.Rows[i]["DIAGNOSTICID"].ToString());
                        string DESCRIPTION = dt.Rows[i]["DESCRIPTION"].ToString();
                        string TESTTYPE = dt.Rows[i]["TESTTYPE"].ToString();
                        string SYMPTOMS = dt.Rows[i]["SYMPTOMS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lst.Add(new DIAGNOSTIC() { SYMPTOMS = SYMPTOMS, TESTTYPE = TESTTYPE, DESCRIPTION = DESCRIPTION, DIAGNOSTICID = DIAGNOSTICID, DIAGNOSTICNAME = DIAGNOSTICNAME });
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                CmdTrans.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        /*[HttpGet]
        [Route("CreateTable")]*/
        /*public void CreateTable()
        {
            DBConnection db = new DBConnection();
            db.createTreatmentTable();
        }*/
        /*[HttpGet]
        [Route("makeModel1")]
        public IActionResult makeModel1()
        {
            DBConnection db = new DBConnection();
            return Ok(db.makeModel1());
        }*/


    }
}
