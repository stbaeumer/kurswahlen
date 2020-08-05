using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace kurswahlen
{
    public class Fachs : List<Fach>
    {
        public Fachs(string aktSj)
        {
            using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
            {
                Console.Write("Fächer ".PadRight(75, '.'));

                try
                {
                    string queryString = @"SELECT DISTINCT 
Subjects.Subject_ID,
Subjects.Name,
Subjects.Longname,
Subjects.Text,
Description.Name
FROM Description RIGHT JOIN Subjects ON Description.DESCRIPTION_ID = Subjects.DESCRIPTION_ID
WHERE Subjects.Schoolyear_id = " + aktSj + " AND Subjects.Deleted=No  AND ((Subjects.SCHOOL_ID)=177659) ORDER BY Subjects.Name;";

                    OleDbCommand oleDbCommand = new OleDbCommand(queryString, oleDbConnection);
                    oleDbConnection.Open();
                    OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();

                    while (oleDbDataReader.Read())
                    {
                        Fach fach = new Fach()
                        {
                            IdUntis = oleDbDataReader.GetInt32(0),
                            KürzelUntis = Global.SafeGetString(oleDbDataReader, 1),
                            Langname = Global.SafeGetString(oleDbDataReader, 2)
                        };
                                                
                        this.Add(fach);
                    };
                    
                    oleDbDataReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    oleDbConnection.Close();
                    Console.WriteLine(this.Count);
                }
            }
        }
    }
}