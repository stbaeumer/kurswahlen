using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace kurswahlen
{
    public class Unterrichts: List<Unterricht>
    {
        public Unterrichts(Schuelers schuelers, Klasses klasses, Fachs fachs, string aktSj, int periode)
        {

            Console.Write("Unterrichte (nur Verkursungen) ".PadRight(75, '.') + " " );

            int id = 0;
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
                {
                    string queryString = @"SELECT DISTINCT 
                                            Lesson_ID,
                                            LessonElement1,
                                            Periods,
                                            Lesson.LESSON_GROUP_ID,
                                            Lesson_TT,
                                            Flags
FROM LESSON
WHERE (((SCHOOLYEAR_ID)= " + aktSj + ") AND ((TERM_ID)=" + periode + ") AND (((Lesson.Deleted)=No))) ORDER BY LESSON_ID;";

                    OleDbCommand oleDbCommand = new OleDbCommand(queryString, oleDbConnection);
                    oleDbConnection.Open();
                    OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                    int z = 0;

                    while (oleDbDataReader.Read())
                    {
                        id = oleDbDataReader.GetInt32(0);

                        string lessonElement = Global.SafeGetString(oleDbDataReader, 1);
                        z++;
                        int anzahlGekoppelterLehrer = lessonElement.Count(x => x == '~') / 21;

                        for (int i = 0; i < anzahlGekoppelterLehrer; i++)
                        {
                            var lesson = lessonElement.Split(',');

                            var les = lesson[i].Split('~');
                            
                            int anzahlStunden = oleDbDataReader.GetInt32(2);

                            try
                            {
                                foreach (var kla in les[17].Split(';'))
                                {
                                    Klasse klasse = new Klasse();

                                    if (kla != "")
                                    {
                                        var istKurs = Global.SafeGetString(oleDbDataReader, 5).Contains("c") ? true : false;

                                        Unterricht unterricht = new Unterricht();
                                        unterricht.IdUntis = id;
                                        unterricht.Fach = les[2] == "" ? null : (from f in fachs where f.IdUntis.ToString() == les[2] select f).FirstOrDefault();
                                        unterricht.Klasse = (from k in klasses where k.IdUntis.ToString() == kla select k).FirstOrDefault();

                                        // nur Kurse mit mehr als Null Stunden

                                        if (istKurs && oleDbDataReader.GetInt32(2) > 0 && unterricht.Fach != null)
                                        {
                                            this.Add(unterricht);
                                        }
                                        if (!istKurs && oleDbDataReader.GetInt32(2) > 0 && unterricht.Fach != null)
                                        {
                                            // Reliunterricht muss verkurst sein!

                                            if (unterricht.Klasse.NameUntis != "BTeam")
                                            {
                                                if (
                                                unterricht.Fach.KürzelUntis.StartsWith("KR ") ||
                                                unterricht.Fach.KürzelUntis.StartsWith("ER ") ||
                                                unterricht.Fach.KürzelUntis == "KR" ||
                                                unterricht.Fach.KürzelUntis == "ER"
                                                )
                                                {

                                                    Console.WriteLine("Achtung: der Unterricht " + unterricht.Fach.KürzelUntis + " mit der ID " + id + " ist Relgionsunterricht, aber nicht als Kurs angelegt!");
                                                    Console.ReadKey();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                
                            }                                            
                        }
                    };
                    oleDbDataReader.Close();
                    oleDbConnection.Close();
                }                
            }
            catch (Exception ex)
            {
               
            }
            Console.WriteLine(this.Count);
        }
    }
}