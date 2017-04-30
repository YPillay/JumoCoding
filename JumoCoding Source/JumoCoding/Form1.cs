using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JumoCoding
{
    public partial class Form1 : Form
    {
        Dictionary<string,int> networks ;
        Dictionary<string, int> products;
        List<string> months;
        public Form1()
        {
            InitializeComponent();
        }
        #region Onclick event, Main event
        private void btn_importCSV_Click(object sender, EventArgs e)
        {
            //initialize global variables
            networks = new Dictionary<string,int>();
            products = new Dictionary<string, int>();
            months = new List<string>();
            // Import csv files using browser
            // Filter out other file types
            //openFileDialog1.Title = "Select .csv to import";
            openFileDialog1.Filter = "CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml";
            try
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //run fuction to process values in CSV
                    List<Loan> loans = ImportCSV(openFileDialog1.FileName);
                    MessageBox.Show("Import Successful");
                    Aggregate(loans);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error importing CSV \n" + exception.ToString());
            }


        }
        #endregion

        #region Functions

        //Import Loans from CSV, Returns list of loans
        public List<Loan> ImportCSV(string filename)
        {
            List<Loan> loans = new List<Loan>();
            StreamReader sr = new StreamReader(filename);
            //remove 1st line as we know the contents of the file 
            sr.ReadLine();
            //initialize counts
            int c_net = 0;
            int c_prod = 0;
            while (!sr.EndOfStream)
            {
                //add loan to list of loans

                string line = sr.ReadLine();
                string[] vals = line.Split(',');
                Loan tmp = new Loan(vals);
                loans.Add(tmp);
                //populate lists to find number of unique products,networks and months
                if (!networks.ContainsKey(tmp.network))
                {
                    networks.Add(tmp.network,c_net);
                    c_net++;
                }
                if (!products.ContainsKey(tmp.product))
                {
                    products.Add(tmp.product,c_prod);
                    c_prod++;
                }
                if (!months.Contains(tmp.date.Month.ToString()))
                {
                    months.Add(tmp.date.Month.ToString());
                }
            }
            sr.Close();
            return loans;
        }

        //sort loans into arrays and into dictionaries
        public Dictionary<string,decimal[,,]> Aggregate(List<Loan> loans)
        {
            
            //TODO : make sure arrays are not empty
            //create an dictionary which will contain the arrays for the different months
            Dictionary<string,decimal[,,]> d_Agg = new Dictionary<string,decimal[,,]>();
            for (int i = 0; i < months.Count; i++)
            {
                //create arrays for the networks and products per month
                
                decimal[,,] arr = new decimal[networks.Count, products.Count,2];

                foreach (Loan l in loans)
                {
                    if (l.date.Month.ToString()==months[i]){
                        //get the network from the current loan
                        string network = l.network;
                        int pos_net = networks[network];
                        //get the product for the curent loan
                        string product = l.product;
                        int pos_prod = products[product];
                        arr[pos_net,pos_prod,0]=l.amount;
                        //increment count
                        decimal count = arr[pos_net,pos_prod,1];
                        arr[pos_net, pos_prod, 1]=count+1;
                    }
                    
                }

                d_Agg.Add(months.ElementAt(i),arr);
            }
            Output(d_Agg);
            return d_Agg;
        }

        //output to csv
        public void Output(Dictionary<string,decimal[,,]> d_agg)
        {
            //get save location
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Comma Seperated values|*.csv";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            //write to csv file
            var csv = new StringBuilder();
            string temp = "";
            foreach (KeyValuePair<string, decimal[,,]> entry in d_agg)
            {
                //go through the dictionary of the different months and pull the arrays of loans
                decimal[,,] arr = entry.Value;
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    for (int j = 0; j < arr.GetLength(1); j++)
                    {
                        //loop through the array 
                            temp += getNetworkName(i) +"," ;
                            temp += getProductName(j) + ",";
                            temp += CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(int.Parse(entry.Key))+",";
                            temp += arr[i, j, 0] +",";
                            temp += arr[i, j, 1];
                            csv.AppendLine(temp);
                            temp = "";

                    }
                }

                
                
            }
            //save file

            if (saveFileDialog1.FileName != "")
            {
                File.WriteAllText(saveFileDialog1.FileName, csv.ToString());
                MessageBox.Show("Output csv created successfully");
            }
            else
            {
                MessageBox.Show("Output csv could not be created");
            }
        }

        //return product name from index
        public string getProductName(int c)
        {
            foreach (KeyValuePair<string, int> entry in products)
            {
                if (entry.Value == c)
                {
                    return entry.Key;
                }
            }
            return ("error");

        }
        //return network name from index
        public string getNetworkName(int c){
            foreach (KeyValuePair<string, int> entry in networks)
            {
                if (entry.Value == c) {
                    return entry.Key;
                }
            }
            return ("error");

        }
        #endregion
    }


    #region Loan Class
    //Loan class contains details of a specific loan
    public class Loan
    {
        #region  variables
        public string network { get; set; }
        public DateTime date { get; set; }
        public string product { get; set; }
        public decimal amount { get; set; }
        #endregion

        #region Constructor
        //construct loan object, Remove ' character when necessary
        public Loan(string[] vals)
        {
            network = vals[1].Trim('\'');
            string format = "dd-MMM-yyyy";
            //convert from string to datetime for ease of use
            date = DateTime.ParseExact(vals[2].Trim('\''), format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            product = vals[3].Trim('\'');
            // "." is culture specific. Invariant culture avoids errors
            amount = decimal.Parse(vals[4], CultureInfo.InvariantCulture);
        }
        #endregion

    #endregion
    }
}
