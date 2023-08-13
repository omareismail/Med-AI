using DbManager;
using MedAI.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MedAI.Features
{
    public class DBConnection
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

        public void InsertDataToDaata()
        {
            string sql = $@"select * from DIAGNOSTICS";
            string sql2 = $@"select * from MEDICINES";
            // TODO: Use insert as select insted.
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                DataTable dtD = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from DIAGNOSTICS", whereValues, true);
                List<DIAGNOSTIC> lstDia = new List<DIAGNOSTIC>();
                if (dtD.Rows.Count > 0)
                {
                    for (int i = 0; i < dtD.Rows.Count; i++)
                    {
                        string DIAGNOSTICNAME = dtD.Rows[i]["DIAGNOSTICNAME"].ToString();
                        int DIAGNOSTICID = Convert.ToInt32(dtD.Rows[i]["DIAGNOSTICID"].ToString());
                        string DESCRIPTION = dtD.Rows[i]["DESCRIPTION"].ToString();
                        string TESTTYPE = dtD.Rows[i]["TESTTYPE"].ToString();
                        string SYMPTOMS = dtD.Rows[i]["SYMPTOMS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lstDia.Add(new DIAGNOSTIC() { SYMPTOMS = SYMPTOMS, TESTTYPE = TESTTYPE, DESCRIPTION = DESCRIPTION, DIAGNOSTICID = DIAGNOSTICID, DIAGNOSTICNAME = DIAGNOSTICNAME });
                    }
                }
                List<MEDICINE> lstMed = new List<MEDICINE>();
                DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from MEDICINES", whereValues, true);
                if (dtM.Rows.Count > 0)
                {
                    for (int i = 0; i < dtM.Rows.Count; i++)
                    {
                        string MEDICINENAME = dtM.Rows[i]["MEDICINENAME"].ToString();
                        int MEDICINEID = Convert.ToInt32(dtM.Rows[i]["MEDICINEID"].ToString());
                        string ILLNESSDESCRIPTION = dtM.Rows[i]["ILLNESSDESCRIPTION"].ToString();
                        string ACTIVEINGREDIENT = dtM.Rows[i]["ACTIVEINGREDIENT"].ToString();
                        string SIDEEFFECTS = dtM.Rows[i]["SIDEEFFECTS"].ToString();
                        /* char isAdmin = Convert.ToChar(dt.Rows[i]["ISADMIN"]);*/

                        lstMed.Add(new MEDICINE() { MEDICINEID = MEDICINEID, MEDICINENAME = MEDICINENAME, ILLNESSDESCRIPTION = ILLNESSDESCRIPTION, ACTIVEINGREDIENT = ACTIVEINGREDIENT, SIDEEFFECTS = SIDEEFFECTS });
                    }
                }
                lstDia.ForEach(dia =>
                {
                    string[] systoms = dia.SYMPTOMS.Split(",");
                    systoms.ToList().ForEach(s =>
                    {
                        lstMed.ForEach(med =>
                        {
                            if (med.ILLNESSDESCRIPTION.Contains(s) == true)
                            {
                                string medID = med.MEDICINEID.ToString();
                                string diaID = dia.DIAGNOSTICID.ToString();
                                string medName = med.MEDICINENAME.ToString();
                                string diaName = dia.DIAGNOSTICNAME.ToString();
                                string sysp = dia.SYMPTOMS;
                                string illDes = med.ILLNESSDESCRIPTION;
                                whereValues.Add("medID", medID);
                                whereValues.Add("diaID", diaID);
                                whereValues.Add("medName", medName);
                                whereValues.Add("diaName", diaName);
                                whereValues.Add("sysp", sysp);
                                whereValues.Add("illDes", illDes);
                                string cmdText = $@"INSERT into Tratment (MEDICINE_ID,MEDICINENAME,DIAGNOSTIC_ID,DIAGNOSTICNAME,ILLNESSDESCRIPTION,SYMPTOMS) VALUES (:medID,:medName,:diaID,:diaName,:illDes,:sysp)";
                                new CustomHelper().ExecuteQueryParmeters(CmdTrans, aOracleConnection, cmdText, whereValues, true);

                            }


                        });

                    });
                });
                CmdTrans.Commit();

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

        public Dictionary<string, Dictionary<string, double>> makeModel1()
        {

            string sql2 = $@"select * from MEDICINES";

            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {

                List<MEDICINE> lstMed = new List<MEDICINE>();
                DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from MEDICINES", whereValues, true);
                if (dtM.Rows.Count > 0)
                {
                    for (int i = 0; i < dtM.Rows.Count; i++)
                    {
                        string MEDICINENAME = dtM.Rows[i]["MEDICINENAME"].ToString();
                        int MEDICINEID = Convert.ToInt32(dtM.Rows[i]["MEDICINEID"].ToString());
                        string ILLNESSDESCRIPTION = dtM.Rows[i]["ILLNESSDESCRIPTION"].ToString();

                        lstMed.Add(new MEDICINE() { MEDICINEID = MEDICINEID, MEDICINENAME = MEDICINENAME, ILLNESSDESCRIPTION = ILLNESSDESCRIPTION });
                    }
                }

                Dictionary<string, List<string>> Dic = new Dictionary<string, List<string>>();
                lstMed.ForEach(m =>
                {
                    List<string> ls = new List<string>();
                    m.ILLNESSDESCRIPTION.Split(",").ToList().ForEach(word =>
                    {
                        string MED_ID = m.MEDICINEID.ToString();
                        string MED_NAME = m.MEDICINENAME.ToString();
                        string MED_ILL = word.Trim();
                        ls.Add(word.Trim());
                    });
                    Dic.Add(m.MEDICINENAME, ls);
                });


                Dictionary<string, Dictionary<string, double>> tfidfScores = new CustomWeightedTfIdfCalculator().CalculateCustomWeightedTfIdf(Dic);
                int num = 1;
                tfidfScores.ToList().ForEach(Med =>
                {

                    string MED_NAME = Med.Key.Trim();

                    Med.Value.ToList().ForEach(ILL =>
                    {
                        string MED_ILL = ILL.Key.Trim();
                        string AI_NUM = ILL.Value.ToString();
                        string sql = $@"Insert into model (MED_ID,MED_NAME,MED_ILL,AI_NUM) VALUES (:MED_ID,:MED_NAME,:MED_ILL,:AI_NUM)";
                        whereValues.Clear();
                        whereValues.Add("MED_ID", num.ToString());
                        whereValues.Add("MED_NAME", MED_NAME.ToString());
                        whereValues.Add("MED_ILL", MED_ILL.ToString());
                        whereValues.Add("AI_NUM", AI_NUM.ToString());
                        new CustomHelper().ExecuteQueryParmeters(CmdTrans, aOracleConnection, sql, whereValues, true);
                        num++;
                    });



                });

                CmdTrans.Commit();
                return tfidfScores;

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
        public void deleteDataFromModel()
        {
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                string sql1 = $@"DELETE FROM model";
                new CustomHelper().ExecuteQueryParmeters(CmdTrans, aOracleConnection, sql1, whereValues, true);

                CmdTrans.Commit();


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

        public Dictionary<string, Dictionary<string, double>> makeModel2(Dictionary<string, Dictionary<string, double>> tfidfScores)
        {


            deleteDataFromModel();
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {

                int num = 1;
                tfidfScores.ToList().ForEach(Med =>
                {
                    /*string sql1 = $@"DELETE FROM model";
                    new CustomHelper().ExecuteQueryParmeters(CmdTrans, aOracleConnection, sql1, whereValues, true);*/

                    string MED_NAME = Med.Key.Trim();

                    Med.Value.ToList().ForEach(ILL =>
                    {
                        string MED_ILL = ILL.Key.Trim();
                        string AI_NUM = ILL.Value.ToString();
                        string sql = $@"Insert into model (MED_ID,MED_NAME,MED_ILL,AI_NUM) VALUES (:MED_ID,:MED_NAME,:MED_ILL,:AI_NUM)";
                        whereValues.Clear();
                        whereValues.Add("MED_ID", num.ToString());
                        whereValues.Add("MED_NAME", MED_NAME.ToString());
                        whereValues.Add("MED_ILL", MED_ILL.ToString());
                        whereValues.Add("AI_NUM", AI_NUM.ToString());
                        new CustomHelper().ExecuteQueryParmeters(CmdTrans, aOracleConnection, sql, whereValues, true);
                        num++;
                    });

                });

                CmdTrans.Commit();
                return tfidfScores;

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
        public Dictionary<string, List<string>> MakeList()
        {
            Dictionary<string, List<string>> doc = new Dictionary<string, List<string>>();
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                string sql = $@"select * from model";
                DataTable dtm = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from model", whereValues, true);
                if (dtm.Rows.Count > 0)
                {
                    for (int i = 0; i < dtm.Rows.Count; i++)
                    {
                        if (doc.ContainsKey(dtm.Rows[i]["MED_NAME"].ToString()))
                        {
                            doc[dtm.Rows[i]["MED_NAME"].ToString()].Add(dtm.Rows[i]["MED_ILL"].ToString());
                        }
                        else
                        {
                            doc.Add(dtm.Rows[i]["MED_NAME"].ToString(), new List<string>() { dtm.Rows[i]["MED_ILL"].ToString() });
                        }

                    }
                }
                return doc;

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
        public double calculateMean()
        {
            double sum = 0;
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from model", whereValues, true);
                if (dtM.Rows.Count > 0)
                {
                    for (int i = 0; i < dtM.Rows.Count; i++)
                    {
                        sum += double.Parse(dtM.Rows[i]["AI_NUM"].ToString());
                    }
                    sum /= dtM.Rows.Count;
                }
                return sum;
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
        public double calculateForMed(string med)
        {
            double sum = 0;
            int num = 0;
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from model", whereValues, true);
                if (dtM.Rows.Count > 0)
                    for (int i = 0; i < dtM.Rows.Count; i++)
                    {
                        if (dtM.Rows[i]["MED_NAME"].ToString() == med)
                        {
                            sum += double.Parse(dtM.Rows[i]["AI_NUM"].ToString());
                            num++;
                        }
                    }
                sum = sum / num;
                /*{
                    sum=  double.Parse( dtM.Rows[dtM.Rows.Count / 2 + 1]["AI_NUM"].ToString());
                }*/
                return sum;
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
        public double calculateForDia(string Dia)
        {
            double sum = 0;
            int num = 0;
            Open();
            OracleTransaction CmdTrans = aOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, "select * from model", whereValues, true);
                if (dtM.Rows.Count > 0)
                    for (int i = 0; i < dtM.Rows.Count; i++)
                    {
                        if (dtM.Rows[i]["MED_ILL"].ToString() == Dia)
                        {
                            sum += double.Parse(dtM.Rows[i]["AI_NUM"].ToString());
                            num++;
                        }
                    }
                sum = sum / num;
                /*{
                    sum=  double.Parse( dtM.Rows[dtM.Rows.Count / 2 + 1]["AI_NUM"].ToString());
                }*/
                return sum;
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



