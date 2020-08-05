namespace kurswahlen
{
    public class Unterricht
    {
        public int IdUntis { get; set; }
        
        public Fach Fach { get; set; }
        
        public Klasse Klasse { get; set; }

        /// <summary>
        /// Es geht um die Anzahl pro Woche. Der Wert entspricht der Anzahl / Unterrichtsgruppe
        /// </summary>
        public int AnzahlStunden { get; private set; }

        
        public double Wert { get; set; }

        /// <summary>
        /// Die Kursart außerhalb des beruflichen Gymnasiums ist immer PUK.
        /// </summary>
        public string Kursart { get; private set; }

        /// <summary>
        /// Wenn die Untis-Flag ein kleines 'c' enthält, handelt es sich um einen Kurs.
        /// </summary>
        public bool IstKurs { get; private set; }

        public string Jahrgang { get; private set; }
        /// <summary>
        /// Der Kursname wird in Untis gesetzt und nach Schild übernommen.
        /// </summary>
        public string Kursname { get; private set; }
    }
}