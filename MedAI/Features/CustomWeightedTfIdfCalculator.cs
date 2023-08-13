namespace MedAI.Features
{
    public class CustomWeightedTfIdfCalculator
    {
        public Dictionary<string, double> customWeights = new Dictionary<string, double>
        {

                {"Insomnia",-2.2 },
                {"Vomiting",-1.4 },
                {"Bacterial Infections",-1.2 },
                {"Fever",-2.5 },
                {"Headache",-1.1 },
                {"Pain",-1 },
                {"Inflammation",-2.3 },
                {"Allergic Reactions",-2.6 },
                {"High Blood Pressure",-3.5 },
                {"Hypertension",-3.5 },
                {"Gastroesophageal Reflux Disease (GERD)",-4 },
                {"Peptic Ulcers",-2.3 },
                {"Allergic Rhinitis",-2.7 },
                {"Hives",-4.5 },
                {"Cardiovascular Disease",-3.6 },
                {"Depression",-3.8 },
                {"Edema",-1.5 },
                {"Muscle Spasms",-2.8 },
                {"Skin Inflammation",-2.8 },
                {"Itching",-2.5 },
                {"Blood Clots",-5 },
                {"Anticoagulation",-3.6 },
                {"Hypothyroidism",-4.8 },
                {"Erectile Dysfunction",-3.9 },
                {"Heartburn",-4.9 },
                {"Herpes Infections",-5 },
                {"Schizophrenia",-5 },
                {"Bipolar Disorder",-4.2 },
                {"Anemia",3.2 },
                {"Neural Tube Defects",3.7 },
                {"Angina",-1.7 },
                {"Chronic Obstructive Pulmonary Disease (COPD)",-2.5 },
                {"Smoking Cessation",-3.6 },
                {"Insulin Resistance",-4.5 },
                {"Glaucoma",-2.1 },
                {"Constipation",-2.2 },
                {"Fungal Infections",-3.3 },
                {"Kidney Stones",-4.4 },
                {"Epilepsy",-2.0 },
                {"Obsessive-Compulsive Disorder",-3.0 },
                {"Nausea",-2.4 },
                {"Gout",-2.3 },
                {"Asthma",-3.7 },
                {"Inflammatory Bowel Disease",-2.8 },
                {"Panic Disorder",-1.4 },
                {"Hypotrichosis",-2.4 },
                {"Inflammatory Conditions",-2.7 },
                {"Type 2 Diabetes",-1.3 },
                {"Anxiety Disorders",-1.5 },
                {"Benign Prostatic Hyperplasia (BPH)",-1.6 },
                {"Urinary Tract Infections",-1.7 },
                {"Allergies",-1.8 },
                { "Neuropathic Pain",-2.8}
            };
        public Dictionary<string, Dictionary<string, double>> CalculateCustomWeightedTfIdf(Dictionary<string, List<string>> documents)
        {

            Dictionary<string, Dictionary<string, double>> tfidfScores = new Dictionary<string, Dictionary<string, double>>();

            // Step 1: Calculate the term frequency (TF) for each term in each document
            Dictionary<string, Dictionary<string, int>> termFrequency = new Dictionary<string, Dictionary<string, int>>();
            foreach (var document in documents)
            {
                string docId = document.Key;
                termFrequency[docId] = new Dictionary<string, int>();
                foreach (var term in document.Value)
                {
                    if (!termFrequency[docId].ContainsKey(term))
                    {
                        termFrequency[docId][term] = 1;
                    }
                    else
                    {
                        termFrequency[docId][term]++;
                    }
                }
            }

            // Step 2: Calculate the inverse document frequency (IDF) for each term
            Dictionary<string, double> inverseDocumentFrequency = new Dictionary<string, double>();
            int totalDocuments = documents.Values.Count;
            foreach (var document in documents.Values)
            {
                foreach (var term in document.Distinct())
                {
                    if (!inverseDocumentFrequency.ContainsKey(term))
                    {
                        int docsWithTerm = documents.Values.Count(d => d.Contains(term));
                        inverseDocumentFrequency[term] = Math.Log((double)totalDocuments / (double)(docsWithTerm + 1));
                    }
                }
            }

            // Step 3: Calculate the TF-IDF score for each term in each document with custom weights
            foreach (var docId in termFrequency.Keys)
            {
                tfidfScores[docId] = new Dictionary<string, double>();
                foreach (var term in termFrequency[docId].Keys)
                {
                    double tf = (double)termFrequency[docId][term] / (double)termFrequency[docId].Values.Sum();
                    double idf = inverseDocumentFrequency[term];
                    double customWeight = customWeights.ContainsKey(term) ? customWeights[term] : 1.0; // Default weight if not specified
                    tfidfScores[docId][term] = tf * idf;// * customWeight;
                }
            }

            return tfidfScores;
        }

    }
}
