using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurswahlen
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> aktSj = new List<string>();

            try
            {
                Console.WriteLine("Kurswahlen (Version 20200803)");
                Console.WriteLine("=============================");
                Console.WriteLine("");

                aktSj = new List<string>();
                int periode = 0;

                aktSj.Add((DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString());
                aktSj.Add((DateTime.Now.Month >= 8 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString());

                Klasses klasses = new Klasses(aktSj[0] + aktSj[1], periode);
                Schuelers schuelers = new Schuelers(klasses, aktSj[0] + aktSj[1]);
                Fachs fachs = new Fachs(aktSj[0] + aktSj[1]);
                
                Unterrichts unterrichtsImKurssystem = new Unterrichts(schuelers, klasses, fachs, aktSj[0] + aktSj[1], periode);

                Kurswahlen kurswahlenIst = new Kurswahlen(aktSj[0] + aktSj[1], klasses, fachs, unterrichtsImKurssystem, periode);

                // Allen Schülern werden Gym-Wahlen zugewiesen

                schuelers.KurswahlenGymIst(kurswahlenIst);
                
                // Für alle Nicht-Gym-Schüler werden die Religionskurswahlen hinzugefügt

                schuelers.ReliKurswahlenHinzufügenOderLöschen(unterrichtsImKurssystem, kurswahlenIst, periode);

                Console.WriteLine("");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}

        