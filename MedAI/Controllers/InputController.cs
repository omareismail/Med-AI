using DbManager;
using MedAI.Features;
using MedAI.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace MedAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InputController : ControllerBase
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
        string CapitalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Convert the first character to uppercase and concatenate the rest of the string
            return char.ToUpper(input[0]) + input.Substring(1);
        }
        [HttpPost]
        [Route("Evaluate")]
        public IActionResult MakeDecision(Input data)
        {

            /*string medName = CapitalizeString(data.Medicines.Trim().ToLower());
            string DiaName = CapitalizeString(data.Diagnosis.Trim().ToLower());*/
            double holdMed = new DBConnection().calculateForMed(data.Medicines);
            double holdDia = new DBConnection().calculateForDia(data.Diagnosis);
            double mean = new DBConnection().calculateMean();
            Dictionary<string, List<string>> newDoc = new DBConnection().MakeList();
            Dictionary<string, Dictionary<string, double>> newDocTFidf = new CustomWeightedTfIdfCalculator().CalculateCustomWeightedTfIdf(newDoc);


            if (newDoc.ContainsKey(data.Medicines))
            {
                if (newDoc[data.Medicines].Contains(data.Diagnosis))
                {

                    return Ok(newDocTFidf[data.Medicines][data.Diagnosis] - holdDia < 0.3 | newDocTFidf[data.Medicines][data.Diagnosis] - holdMed < 0.3);
                }
                else
                {
                    newDoc[data.Medicines].Add(data.Diagnosis);
                    newDocTFidf = new CustomWeightedTfIdfCalculator().CalculateCustomWeightedTfIdf(newDoc);
                    new DBConnection().makeModel2(newDocTFidf);
                }
                ///////
            }
            else
            {
                newDoc.Add(data.Medicines, new List<string>() { data.Diagnosis });
                newDocTFidf = new CustomWeightedTfIdfCalculator().CalculateCustomWeightedTfIdf(newDoc);
                new DBConnection().makeModel2(newDocTFidf);

            }
            return Ok(newDocTFidf[data.Medicines][data.Diagnosis] - holdDia < 0.3 | newDocTFidf[data.Medicines][data.Diagnosis] - holdMed < 0.3);


            /*whereValues.Add("medName", medName);
            DataTable dtM = new CustomHelper().QueryReaderParmeters(CmdTrans, aOracleConnection, 0, 0, sql, whereValues, true);*/
            /*string json=JsonConvert.SerializeObject(dtM, Formatting.None);*/


            /*if (newDoc.ContainsKey(data.Medicines))
            {

                newDoc.
                newDoc[data.Medicines].Add(data.Diagnosis);
            }
            else
            {
                newDoc.Add(data.Medicines, new List<string> { data.Diagnosis });
            }

            var n = new CustomWeightedTfIdfCalculator().CalculateNewTfIdf(newDoc);*/

            /*new CustomWeightedTfIdfCalculator().CalculateCustomWeightedTfIdf({ dtM.Rows[0]["MED_NAME"].ToString() ,new List { new List<string>[] ,new List<string>[] } })*/

        }
    }
}
