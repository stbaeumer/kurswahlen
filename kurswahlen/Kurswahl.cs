﻿using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace kurswahlen
{
    internal class Kurswahl
    {
        //https://www.untis.at/HTML/WebHelp/de/untis/hid_export_kurswahl.htm

        public int StudentId { get; internal set; }
        public string Nachname { get; internal set; }
        public string Vorname { get; internal set; }
        public DateTime Geburtsdatum { get; internal set; }
        public List<string> AlternativeCourses { get; internal set; }
        public string Fach { get; internal set; }
        public string Klasse { get; internal set; }
        public string StudentKurzname { get; internal set; }
        /// <summary>
        /// 123456 Die Atantis-ID des Schülers
        /// </summary>
        public string AtlantisId { get; internal set; }
        public string Unterrichtsnummer { get; internal set; }

        /// <summary>
        /// Die Number ist Teil des PS bei der Kurswahl. Sie muss je Schüler einmalig sein.
        /// </summary>
        public int Number { get; internal set; }
        public string AktSj { get; private set; }
        public int Periode { get; private set; }

        public Kurswahl(string aktSj, int periode)
        {
            AlternativeCourses = new List<string>();
            AktSj = aktSj;
            Periode = periode;
        }

        public Kurswahl()
        {
            AlternativeCourses = new List<string>();
        }

        public void InsertIntoStudentChoice()
        {
            using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    oleDbConnection.Open();

                    String my_querry = "INSERT INTO StudentChoice(" +
                        "SCHOOL_ID, " +
                        "SCHOOLYEAR_ID, " +
                        "VERSION_ID, " +
                        "STUDENT_ID, " +
                        "TERM_ID, " +
                        "[Number], " +
                        "AlternativeCourses" +
                         ")VALUES('" +
                         "177659" + "','" +
                         AktSj + "','" +
                         "1" + "','" +
                         this.StudentId + "','" +
                         Periode + "','" +
                         this.Number + "','" +
                         this.AlternativeCourses[0] + "')";

                    OleDbCommand cmd = new OleDbCommand(my_querry, oleDbConnection);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    oleDbConnection.Close();
                }
            }
        }        
    }
}