using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace CreditCard_Statement_Generator
{
    /// <summary>Class <c>Program</c> Models A Generic Statement Generator. </summary>
    class Program
    {
        /*
         Creating global variables.
         */
        static Dictionary<String, int> columns = new Dictionary<String, int>();
        static List<string> Amount = new List<string>();
        static List<String> transactionDetails_currency_Location = new List<String>();
        static string CardName = " ";
        static string Transaction = " ";
        static string row;

        // Main Function
        static void Main(string[] args)
        {
            /*
                Reading all the Input Files path.
                Creating the Output Files path List based on Input Files.
             */
            string[] InputDocuments = Directory.GetFiles("../../../InputFiles/", "*.csv");
            List<String> OutputDocuments = new List<String>();

            foreach (string file in InputDocuments)
                OutputDocuments.Add(file.Replace("In", "Out"));

            //Creating Local Variable to store columns values.
            string Date;
            string Transaction_Description;
            string Debit;
            string Credit;
            string Currency;
            string Location;

            //Running for loop for every Input Files and Generating Respective Output Files  
            for (int i = 0; i<InputDocuments.Length; i++) { 
                
                // Storing every lines of ith file into list of strings.
                string[] lines = System.IO.File.ReadAllLines(InputDocuments[i]);

                // Creating and adding Meta Data i.e., Column names for the Generic Output File.
                System.IO.File.AppendAllText(OutputDocuments[i], ("Date," + "Transaction Description," + "Debit," + "Credit," + "Currency," + "CardName," + "Transaction," + "Location\n"));

                // Reading each line of file and parsing and storing respective data into columns of Output File.
                foreach (string line in lines)
                {
                    // Checking all lines which contains data. Removing cases for all the empty rows.
                    if (Regex.IsMatch(line, "[^,   ]"))
                    {
                        Transaction = transaction_parse(line, Transaction);
                        CardName = cardName_parse(line, CardName);
                        
                        /* Generating Dictionary based on Key->ColumnName and Value->Index so as parse data based on columns.
                        --> Helps in to make code generic for every file.
                        */
                        if (line.Contains("Date"))
                        {
                            int count = 0;
                            columns = new Dictionary<string, int>();
                            foreach (string column in line.Split(','))
                            {
                                columns.Add(column.Trim().Split(' ')[0], count);
                                count++;
                            }
                        }

                        /* Running for every line that contains data.*/
                        if (Regex.IsMatch(line, "[0-9]"))
                        {
                            Date = line.Split(',')[columns["Date"]];
                            if (columns.ContainsKey("Debit") && columns.ContainsKey("Credit"))
                            {
                                if (line.Split(',')[columns["Debit"]].Trim().Length >= 1) {
                                    Debit = line.Split(',')[columns["Debit"]].Trim();
                                    if (Regex.IsMatch(Debit, "[a-zA-Z]")) { 
                                        Debit = "0";
                                    }
                                    Credit = "0";
                                }

                                else {
                                    Debit = "0";
                                    Credit = line.Split(',')[columns["Credit"]].Trim();
                                    if (Regex.IsMatch(Credit, "[a-zA-Z]"))
                                    {
                                        Credit = "0";
                                    }
                                }

                            }
                            else {
                                amount_parse(line.Split(',')[columns["Amount"]]);
                                Debit = Amount[0];
                                Credit = Amount[1];
                            }
                            transaction_details_parse(line.Split(',')[columns["Transaction"]]);
                            Currency = transactionDetails_currency_Location[0];
                            Location = transactionDetails_currency_Location[1];
                            if (Regex.IsMatch(Location, "[0-9]") && Currency == "INR") {
                                Location = "India";
                            }
                            Transaction_Description = transactionDetails_currency_Location[2];
                            row = Date + "," + Transaction_Description + "," + Debit + "," + Credit + "," + Currency + "," + CardName + "," + Transaction + "," + Location + "\n";
                            
                            // Appending parsed line into the output file.
                            System.IO.File.AppendAllText(OutputDocuments[i], row);
                        }

                    }
                }
                System.IO.File.AppendAllText(OutputDocuments[i], "\n");
                System.IO.File.AppendAllText(OutputDocuments[i], "\n");
                System.IO.File.AppendAllText(OutputDocuments[i], "Vishal Lala 1704484@kiit.ac.in \n");
                Console.WriteLine("Output File for \"" + InputDocuments[i] + "\" generated.");
            }
        }

        /// <summary>Method <c>transaction_details_parse</c> Gets the Transaction Details data and parse it into a list of Strings. </summary>
        /// <return_type>Void</return_type>
        /// <param name="details"> String->: The Transaction Details Column Value. </param>>
        /// <returns>List<Strings>: Appends the parsed data i.e., Currency, Location and Transaction Description. </returns>
        static void transaction_details_parse(string details)
        {
            transactionDetails_currency_Location = new List<String>();
            char[] charsToTrim = { ' ', '\t' };
            details = details.Trim(charsToTrim);

            if (details.Split(' ').Last().Length == 3)
            {
                transactionDetails_currency_Location.Add(details.Split(' ').Last());
                details = details.Substring(0, details.LastIndexOf(' ')).TrimEnd();
            }
            else
            {
                transactionDetails_currency_Location.Add("INR");
            }
            transactionDetails_currency_Location.Add(details.Split(' ').Last());
            transactionDetails_currency_Location.Add(details);
        }

        /// <summary>Method <c>transaction_parse</c> Gets the Transaction Type and parse it into respective Transaction type. </summary>
        /// <return_type>String</return_type>
        /// <param name="transaction_type"> String->: A line from the CSV File. </param>>
        /// <param name="previousStateTransaction"> String->: Previous Value of the Transaction Type. </param>>
        /// <returns>Strings->: Returns the Transaction Type if the line contains the type. Else returns previous value of transaction type. </returns>

        static string transaction_parse(string transaction_type, string previousStateTransaction)
        {
            if (transaction_type.Contains("Domestic"))
            {
                return "Domestic";
            }
            else if (transaction_type.Contains("International"))
            {
                return "International";
            }
            else
                return previousStateTransaction;
        }

        /// <summary>Method <c>cardName_parse</c> Gets the line and parse it into respective Card Holder. </summary>
        /// <return_type>String</return_type>
        /// <param name="cardName"> String->: A line from the CSV File. </param>>
        /// <param name="previousStateName"> String->: Previous Value of the Transaction Type. </param>>
        /// <returns>Strings->: Returns the Card Holder name if the line contains the name. Else returns previous value of Card Holder. </returns>
        static string cardName_parse(string cardName, string previousStateName)
        {
            if (!Regex.IsMatch(cardName, "[0-9]") && !cardName.Contains("Transaction"))
            {
                cardName = cardName.Replace(',', ' ').Trim();
                return cardName;
            }
            else
                return previousStateName;
        }

        /// <summary>Method <c>amount_parse</c> Gets the Amount and parse it into a list of Strings containing Debit and Credit value. </summary>
        /// <return_type>Void</return_type>
        /// <param name="amount"> String->: The Amount Column Value. </param>>
        /// <returns>List<Strings>: Appends the parsed data into a list i.e., of Debit and Credit. </returns>
        static void amount_parse(string amount)
        {
            Amount = new List<string>();
            amount = amount.ToLower().Trim();
            if (amount.Contains("cr"))
            {
                Amount.Add("0");
                Amount.Add(amount.Split(' ')[0]);
            }
            else {
                Amount.Add(amount);
                Amount.Add("0");
            }
            
        }
    }
}
