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
                Console.WriteLine("Das Programm liest die Religionskurswahlen aus Atlantis aus und trägt sie in Untis als Kurswahl ein.");
                
                aktSj = new List<string>();
                
                aktSj.Add((DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString());
                aktSj.Add((DateTime.Now.Month >= 8 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString());

                Periodes periodes = new Periodes(aktSj[0] + aktSj[1]);
                var periode = periodes.Count;

                Klasses klasses = new Klasses(aktSj[0] + aktSj[1], periode);
                Schuelers schuelers = new Schuelers(klasses, aktSj[0] + aktSj[1]);
                Fachs fachs = new Fachs(aktSj[0] + aktSj[1]);
                
                Unterrichts unterrichtsImKurssystem = new Unterrichts(schuelers, klasses, fachs, aktSj[0] + aktSj[1], periode);

                Kurswahlen kurswahlenIst = new Kurswahlen(aktSj[0] + aktSj[1], klasses, fachs, unterrichtsImKurssystem, periode);

                // Allen Schülern werden Gym-Wahlen zugewiesen

                //schuelers.KurswahlenGymIst(kurswahlenIst);
                
                // Für alle Nicht-Gym-Schüler werden die Religionskurswahlen hinzugefügt

                schuelers.ReliKurswahlenHinzufügenOderLöschen(unterrichtsImKurssystem, kurswahlenIst, aktSj[0] + aktSj[1], periode);

                Console.WriteLine("");

                Console.WriteLine("ENTER beendet das Programm");
                Console.ReadKey();
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}

        