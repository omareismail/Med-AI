using DbManager;
using MedAI.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        static DbAccess con;
        static OracleConnection aOracleConnection;
        static Dictionary<string, string> whereValues = new Dictionary<string, string>();

        static void Open()
        {
            con = new DbAccess();
            aOracleConnection = con.get_con();
        }
        static void Close()
        {
            con.Close(aOracleConnection);
        }
        [HttpGet]
        public List<MEDICINE> GetAllMedicine()
        {
            string sql = $@"select * from MEDICINES";

            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                List<MEDICINE> lst = new List<MEDICINE>();
                var cmd = aOracleConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
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
                        string MEDICINENAME = dt.Rows[i]["MEDICINENAME"].ToString();
                        int MEDICINEID = Convert.ToInt32(dt.Rows[i]["MEDICINEID"].ToString());
                        string ILLNESSDESCRIPTION = dt.Rows[i]["ILLNESSDESCRIPTION"].ToString();
                        string ACTIVEINGREDIENT = dt.Rows[i]["ACTIVEINGREDIENT"].ToString();
                        string SIDEEFFECTS = dt.Rows[i]["SIDEEFFECTS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lst.Add(new MEDICINE() { MEDICINEID = MEDICINEID, MEDICINENAME = MEDICINENAME, ILLNESSDESCRIPTION = ILLNESSDESCRIPTION, ACTIVEINGREDIENT = ACTIVEINGREDIENT, SIDEEFFECTS = SIDEEFFECTS });
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
        public List<MEDICINE> GetMedicineById(int id)
        {
            string sql = $@"select * from MEDICINES where MEDICINEID = :id";

            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                List<MEDICINE> lst = new List<MEDICINE>();
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
                        string MEDICINENAME = dt.Rows[i]["MEDICINENAME"].ToString();
                        int MEDICINEID = Convert.ToInt32(dt.Rows[i]["MEDICINEID"].ToString());
                        string ILLNESSDESCRIPTION = dt.Rows[i]["ILLNESSDESCRIPTION"].ToString();
                        string ACTIVEINGREDIENT = dt.Rows[i]["ACTIVEINGREDIENT"].ToString();
                        string SIDEEFFECTS = dt.Rows[i]["SIDEEFFECTS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lst.Add(new MEDICINE() { MEDICINEID = MEDICINEID, MEDICINENAME = MEDICINENAME, ILLNESSDESCRIPTION = ILLNESSDESCRIPTION, ACTIVEINGREDIENT = ACTIVEINGREDIENT, SIDEEFFECTS = SIDEEFFECTS });
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


    }
}
