using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace kurswahlen
{
    class Kurswahlen : List<Kurswahl>
    {
        public Kurswahlen()
        {
        }

        public Kurswahlen(string aktSj, Klasses klasses, Fachs fachs, Unterrichts unterrichts, int periode)
        {
            using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT 
StudentChoice.STUDENT_ID, 
Student.Longname, 
Student.FirstName, 
Student.BirthDate, 
StudentChoice.Number, 
StudentChoice.AlternativeCourses,
Student.Name,
Student.StudNumber
FROM Student LEFT JOIN StudentChoice ON Student.STUDENT_ID = StudentChoice.STUDENT_ID
WHERE (((StudentChoice.SCHOOLYEAR_ID)= " + aktSj + ") AND ((StudentChoice.TERM_ID)=" + periode + ") AND (((StudentChoice.Deleted)=No))) ORDER BY StudentChoice.STUDENT_ID;";

                    OleDbCommand oleDbCommand = new OleDbCommand(queryString, oleDbConnection);
                    oleDbConnection.Open();
                    OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();

                    while (oleDbDataReader.Read())
                    {
                        Kurswahl kurswahl = new Kurswahl(aktSj, periode);

                        kurswahl.StudentId = oleDbDataReader.GetInt32(0);
                        kurswahl.Nachname = Global.SafeGetString(oleDbDataReader, 1);
                        kurswahl.Vorname = Global.SafeGetString(oleDbDataReader, 2);
                        kurswahl.Geburtsdatum = DateTime.ParseExact((oleDbDataReader.GetInt32(3)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        kurswahl.Number = Convert.ToInt32(oleDbDataReader.GetValue(4));
                        kurswahl.AlternativeCourses.Add(Global.SafeGetString(oleDbDataReader, 5));
                        kurswahl.StudentKurzname = Global.SafeGetString(oleDbDataReader, 6);
                        kurswahl.Fach = (from f in fachs where f.IdUntis.ToString() == kurswahl.AlternativeCourses[0].Split('/')[1] select f.KürzelUntis).FirstOrDefault();
                        kurswahl.Klasse = (from u in unterrichts
                                           where u.IdUntis.ToString() == kurswahl.AlternativeCourses[0].Split('/')[0]
                                           select u.Klasse.NameUntis).FirstOrDefault();
                        kurswahl.AtlantisId = Global.SafeGetString(oleDbDataReader, 7);
                        this.Add(kurswahl);
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
                }
            }
        }
    }
}