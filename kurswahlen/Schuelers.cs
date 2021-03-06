﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;
using System.Text.RegularExpressions;

namespace kurswahlen
{
    public class Schuelers : List<Schueler>
    {
        public Schuelers()
        {
        }

        public Schuelers(Klasses klasses, string aktSj)
        {
            Schuelers atlantisschulers = new Schuelers();

            using (OdbcConnection connection = new OdbcConnection(Global.ConnectionStringAtlantis))
            {
                DataSet dataSet = new DataSet();

                OdbcDataAdapter schuelerAdapter = new OdbcDataAdapter(@"SELECT   
            schueler.pu_id AS AtlantisSchuelerId,
            schueler.gebname AS Geburtsname,
            schueler.gebort_lkrs AS Geburtsort,
            schueler.s_geburts_land AS Geburtsland,
            schue_sj.vorgang_akt_satz_jn AS AktuellJN,
schue_sj.s_schulabschluss_bos AS Versetzung1,
schue_sj.dat_versetzung AS VersetzungsDatumInDiesenBildungsgang,
schue_sj.s_austritts_grund_bdlg AS BBB,
schue_sc.dat_austritt AS Austrittsdatum,
schue_sc.s_ausb_ziel_erreicht AS BeruflicherAbschluss,
schue_sc.s_austritt_schulabschluss AS AllgemeinerAbschluss,
schue_sc.s_hoechst_schulabschluss AS LSHoechAllAb,
schueler.s_schwerstbehindert AS XX,
schue_sj.kl_id AS AtlantisKlasseId,			
schue_sj.fko_id AS AtlantisStundentafelId,			
schue_sj.durchschnitts_note_jz AS DurchschnittsnoteJahreszeugnis,			
			schueler.name_1 AS Nachname,
			schueler.name_2 AS Vorname,
			check_null (hole_schuljahr_rech ('', -1))												as schuljahr_vorher, 
			check_null (hole_schuljahr_rech ('', 0))												as Bezugsjahr,				/*  1 aktuelles SJ des Users */
			check_null (sv_km_kuerzel_wert ('s_typ_vorgang', schue_sj.s_typ_vorgang))	as Status,  				/*  2 */
			/* lfd_nr im cf_ realisiert */  																							/*  3 */ 
			(if check_null (klasse.klasse_statistik_name) <> ''	THEN
				 check_null (klasse.klasse_statistik_name)         ELSE
				 check_null (klasse.klasse)								ENDIF)				as Klasse,  					/*  4 */

         check_null (sv_km_kuerzel_wert ('s_berufs_nr_gliederung', 
                                 schue_sj.s_berufs_nr_gliederung))   				as Gliederung,  				/*  5 */   
         check_null (substr(schue_sj.s_berufs_nr,4,5))				  					as Fachklasse,  				/*  6 Stelle 4-8 */  
			''																								as Klassenart,					/*  7 nicht BK */
			check_null (sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art))	as OrgForm,  					/*  8 */
         check_null (sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang))			as AktJahrgang,  				/*  9 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf', schueler.s_art_foerderungsbedarf))
																											as Foerderschwerp,			/* 10 */ 
			(if check_null (schueler.s_schwerstbehindert) = ''	THEN
				 '0'                 									ELSE
				 '1'															ENDIF)					as Schwerstbeh,				/* 11 */ 
			''																								as Reformpdg,					/* 12 */ 
			(if sv_steuerung ('s_unter', schueler.s_unter) = '$JVA' THEN
				 '1'                 									ELSE
				 '0'															ENDIF)		 				as JVA,							/* 13 */ 
         check_null_10(adresse.plz )                                             	as Plz, 				 			/* 14 */ 
			hole_schueler_ort_bundesland (adresse.s_gem_kz, adresse.ort, adresse.lkz)	as Ort,							/* 15 */
         schueler.dat_geburt			                    										as Gebdat,   					/* 16 */
         check_null (sv_km_kuerzel_wert ('s_geschl' , schueler.s_geschl))				as Geschlecht,   				/* 17 */   
         check_null (sv_km_kuerzel_wert ('s_staat'  , schueler.s_staat))				as Staatsang,   				/* 18 */
         
		   check_null (sv_km_kuerzel_wert ('s_bekennt', schueler.s_bekennt))       as Religion,  					/* 19 */
			schue_sj.dat_rel_anmeld																	as Relianmeldung,				/* 20 */
			schue_sj.dat_rel_abmeld																	as Reliabmeldung,				/* 21 */

			(if Aufnahmedatum_Bildungsgang is null THEN     		
				 Aufnahmedatum_Schule					ELSE
				 Aufnahmedatum_Bildungsgang			ENDIF)        							as	Aufnahmedatum,   			/* 22 */  

			(select max (lehr_sc.ls_kuerzel)
            from lehr_sc, kl_ls, lehrer
        	  where lehr_sc.ls_id 		= kl_ls.ls_id                                                   
        		 and kl_ls.kl_id 			= klasse.kl_id
				 and kl_ls.s_typ_kl_ls 	= '0'
             and lehrer.le_id 		= lehr_sc.le_id) 										as Labk,							/* 23 */
						

			(select adresse.plz
			   from adresse, betrieb, pj_bt  
			  where adresse.ad_id 		= betrieb.id_hauptadresse  
				 and pj_bt.bt_id 			= betrieb.bt_id    
				 and pj_bt.s_typ_pj_bt 	= '0'       
				 and pj_bt.pj_id 			= schue_sj.pj_id)										as ausbildort,  				/* 24 */                       
				
			hole_schueler_betriebsort (schue_sj.pj_id, 'ort')								as betriebsort,  				/* 25 */                       
				
			/* Kapitel der zuletzt besuchten Schule */
         check_null (sv_km_kuerzel_wert ('s_herkunfts_schule', 
                                 schue_sj.s_herkunfts_schule))                   as	LSSchulform,/* 26 */  
			left (schue_sj.vo_s_letzte_schule, 6) 						      		  		as LSSchulnummer,				/* 27 */
			check_null (sv_km_kuerzel_wert ('s_berufl_vorbildung_glied', 
                                 schue_sj.s_berufl_vorbildung_glied))        		as	LSGliederung,   			/* 28 */  
			substr (schue_sj.s_berufl_vorbildung, 4, 5) 			   						as	LSFachklasse,   			/* 29 */  

			''																								as LSKlassenart,				/* 30 */
			''																								as LSReformpdg,				/* 31 */
			Null																							as LSSchulentl,				/* 32 */		

			check_null (sv_km_kuerzel_wert ('s_abgang_jg', schue_sj.vo_s_jahrgang))	
																        									as	LSJahrgang,   				/* 33 */  

			(if Gliederung || Fachklasse = VOGliederung || VOFachklasse then 
             (if AktJahrgang = vojahrgang and Schueler_le_schuljahr_da = 'J' then 'W' else 'V' endif)   else
		      right (check_null (sv_km_kuerzel_wert ('s_schulabschluss', schue_sc.s_hoechst_schulabschluss)), 1) ENDIF)
																             							as LSQual,    					/* 34 */

			'0'																							as LSVersetz,					/* 35 */ 
	 
			/* Kapitel für das abgelaufene Schuljahr */
			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT klasse.klasse									/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				klasse.klasse												ENDIF))				 	as VOKlasse,    				/* 36 */

			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select schue_sj.s_berufs_nr_gliederung			/* aus Vorjahr lesen */
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				schue_sj.s_berufs_nr_gliederung						ENDIF)) 					as VOGliederung,  			/* 37 */

			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select substr (schue_sj.s_berufs_nr, 4, 5)		/* aus Vorjahr lesen */ 
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				substr (schue_sj.s_berufs_nr, 4, 5)					ENDIF)) 					as VOFachklasse, 				/* 38 */

			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art)/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art) ENDIF))		as VOOrgForm,    				/* 39 */
			''																								as VOKlassenart,				/* 40 nicht BK*/

			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang)/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang) ENDIF))		   as VOJahrgang,    			/* 41 */

			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf_vj', 
                                 schueler.s_art_foerderungsbedarf_vj))   			as VOFoerderschwerp,			/* 42 */ 
			'0'																							as VOSchwerstbeh,				/* 43 */ 
			''																								as VOReformpdg,				/* 44 nicht BK*/
				
			hole_schueler_bildungsgang (schueler.pu_id, klasse.s_bildungsgang, schue_sj.dat_austritt, 'austritt')
																											as EntlDatum,					/* 45 */				

			check_null (sv_km_kuerzel_wert ('s_austritts_grund_bdlg', 
                                 schue_sj.s_austritts_grund_bdlg))               as Zeugnis_JZ, 				/* 46 */

			''																								as Schulpflichterf,			/* 47 nicht BK */ 			

			''																								as Schulwechselform,			/* 48 nicht BK */ 		

			''																								as Versetzung,					/* 49 */ 		

			(IF s_geburts_land is not null AND s_geburts_land <> '000' THEN
             string (year (dat_zuzug))
          ELSE
             ''
          ENDIF
			)																								as JahrZuzug_nicht_in_D_geboren,	/* 50 */

			string (year (dat_eintritt_gs))														as JahrEinschulung,					/* 51 */

			''																								as JahrWechselSekI,					/* 52 */

			(IF (s_geburts_land is not null AND s_geburts_land <> '000') OR (s_geburts_land is null AND dat_zuzug is not null) THEN
             '1'
          ELSE
             '0'
          ENDIF
			)																							   as Zugezogen,							/* 53 */


			/* Hilfsfelder */
			(SELECT max (adr_mutter.ad_id) 
            FROM adresse adr_mutter 
           WHERE adr_mutter.pu_id		= schueler.pu_id
             AND adr_mutter.s_typ_adr	= 'M'
			)																								as ad_id_mutter,


(select max (adresse.strasse)
			   from adresse  
			  where adresse.pu_id = schueler.pu_id)										as strasse, 

(select max (adresse.tel_1)
			   from adresse  
			  where adresse.pu_id = schueler.pu_id)										as telefon, 

			(SELECT adr_mutter.s_herkunftsland_adr 
            FROM adresse adr_mutter 
           WHERE adr_mutter.ad_id		= ad_id_mutter
			)																								as herkunftsland_mutter,			/* 54 */

			(SELECT max (adr_vater.ad_id) 
            FROM adresse adr_vater
           WHERE adr_vater.pu_id			= schueler.pu_id
             AND adr_vater.s_typ_adr	= 'V'
			)																								as ad_id_vater,

			(SELECT adr_vater.s_herkunftsland_adr 
            FROM adresse adr_vater
           WHERE adr_vater.ad_id			= ad_id_vater
			)																								as herkunftsland_vater,				/* 55 */

			(IF herkunftsland_mutter is not null AND herkunftsland_mutter <> '000'
             OR
				 herkunftsland_vater  is not null AND herkunftsland_vater  <> '000' THEN
             '1'
          ELSE
             '0'
          ENDIF
         )																								as Elternteilzugezogen,				/* 56 */

			(IF schueler.s_muttersprache is not null AND 
             schueler.s_muttersprache <> 'DE'     AND 
             schueler.s_muttersprache <> '000' THEN
             schueler.s_muttersprache
          ELSE
             ''
          ENDIF
         )																								as Verkehrssprache,					/* 57 */

			''																								as Einschulungsart					/* 58 */,
			''																								as Grundschulempfehlung				/* 59 */,

			schue_sj.pj_id																				as pj_id,
			check_null (sv_km_kuerzel_wert ('s_religions_unterricht', 
                                 schue_sj.s_religions_unterricht))					as s_religions_unterricht,
     		schue_sc.dat_eintritt	                   										as	Aufnahmedatum_schule,   			

			hole_schueler_bildungsgang (schueler.pu_id, klasse.s_bildungsgang, schue_sj.dat_eintritt, 'eintritt')
							     			                   										as	Aufnahmedatum_Bildungsgang,

			(SELECT max (pj_id) 
			   FROM schue_sj  
			  WHERE schue_sj.pu_id 					= schueler.pu_id 		and
					  schue_sj.s_typ_vorgang 		IN ('A', 'G', 'S')	and
					  schue_sj.vorgang_schuljahr 	= schuljahr_vorher) 						as VOpj_id,

			test_austritt (schue_sj.dat_austritt)												as ausgetreten,
		
			check_null ((SELECT noten_kopf.s_bestehen_absprf || noten_kopf.s_zusatzabschluss
			   FROM noten_kopf, schue_sj  
			  WHERE noten_kopf.s_typ_nok 			= 'HZ'
				 AND schue_sj.pj_id 					= noten_kopf.pj_id
				 AND schue_sj.pj_id 					= VOpj_id))            					as Zeugnis_HZ,


			check_null ((if ( schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('',  0)) /* '2005/06' */
	  				AND schue_sj.vorgang_akt_satz_jn = 'J'
	  				AND ausgetreten 						= 'N') 		THEN
				 '1'                 									ELSE
				 '0'															ENDIF))					as Fall_Bezugsjahr,			

			check_null ((if ( schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('', -1)) /* '2004/05' */
				   AND schue_sj.s_typ_vorgang 	IN ('A', 'G', 'S')
				   AND ausgetreten 					= 'J') 			THEN
				 '1'                 									ELSE
				 '0'															ENDIF))					as Fall_schuljahr_vorher,
			
			check_null ((if ( VOJahrgang <> '')							 			THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Schueler_le_schuljahr_da,	
			
			check_null ((if ( Fachklasse <> VOFachklasse
					OR Gliederung <> VOGliederung)		 			THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Schueler_Berufswechsel,	
			
			check_null ((if ( VoGliederung = 'C05' 
				  AND Gliederung = 'C06')		 						THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Vorjahr_C05_Aktjahr_C06,		
			
			check_null (sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang))				as schueler_jahrgang, 

			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select schue_sj.s_jahrgang			/* aus Vorjahr lesen */
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				schue_sj.s_jahrgang										ENDIF)) 					as VOSchueler_Jahrgang,

  			(IF EXISTS (SELECT 1 FROM schue_sj_info  
                      WHERE schue_sj_info.info_gruppe = 'MASSNA'
						      AND schue_sj_info.pj_id = schue_sj.pj_id) THEN '1' ELSE '0' ENDIF) as Massnahmetraeger,	/* 60 */

         check_null (sv_km_kuerzel_wert ('s_geschl', schueler.s_betreuungsart))	as Betreuung,   						/* 61 */   

  			(IF EXISTS (SELECT 1 FROM schueler_info  
                      WHERE schueler_info.info_gruppe = 'PUBEM'
                        AND schueler_info.s_typ_puin  = 'BKAZVO'
								AND schueler_info.betrag IN (0, 6, 12, 18)
						      AND schueler_info.pu_id = schueler.pu_id) THEN 
		   (SELECT konv_dec_string(schueler_info.betrag, 0)
			   FROM schueler_info  
			  WHERE schueler_info.puin_id = (SELECT max(puin.puin_id)  
														  FROM schueler_info puin 
														 WHERE puin.info_gruppe = 'PUBEM' 
														   AND puin.s_typ_puin = 'BKAZVO'
														   AND puin.betrag IN (0, 6, 12, 18)
														   AND puin.pu_id  = schueler.pu_id)) ELSE '0' ENDIF)	as BKAZVO,	/* 62 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf2', 
                                 schueler.s_art_foerderungsbedarf2))  	as Foerderschwerp2,						/* 63 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf_vj2',
                                 schueler.s_art_foerderungsbedarf_vj2)) as VOFoerderschwerp2,					/* 64 */   
			(IF schue_sc.berufsabschluss_jn = 'J' THEN 'Y' ELSE '' ENDIF)	as Berufsabschluss, 						/* 65 */   

			'Atlantis' 																		as Produktname, 						   /* 66 */   
			(SELECT 'V' || version.version_nr  
			   FROM version
           WHERE version.datum  = (SELECT max(v.datum) FROM version v)) as Produktversion,     			      /* 67 */   

         check_null (sv_km_kuerzel_wert ('s_bereich', klasse.s_bereich)) as Adressmerkmal,                  /* 68 */
			(if sv_steuerung ('s_unter', schueler.s_unter) = '$IN' THEN
				 '1'                 									ELSE
				 ''															ENDIF)		 as Internat,								/* 69 */ 
         '0'																			    as Koopklasse                      /* 70 */

  FROM   schueler,   
         schue_sc,  
         schue_sj,   
			schule,
         klasse,   
         adresse

 WHERE   schue_sj.kl_id 					= klasse.kl_id    			     
     AND schue_sc.sc_id 					= schule.sc_id  			    
     AND schue_sc.ps_id 					= schue_sj.ps_id  			     
     AND schue_sc.pu_id 					= schueler.pu_id  			     
	  AND adresse.ad_id 						= schueler.id_hauptadresse  
     AND (
			 (schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('',  0)) /* aktuelles Jahr */
	  
			 )
          OR
          (
			  schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('', -1)) /* letzte jahr */
	  AND   schue_sj.s_typ_vorgang 			IN ('G', 'S')
	  AND   ausgetreten = 'J'
			 )
         ) 
ORDER BY ausgetreten DESC, klasse, schueler.name_1, schueler.name_2", connection);

                Console.Write("Schüler*innen ".PadRight(75, '.') + " ");

                connection.Open();
                schuelerAdapter.Fill(dataSet, "DBA.schueler");

                foreach (DataRow theRow in dataSet.Tables["DBA.schueler"].Rows)
                {
                    string vorname = theRow["Vorname"] == null ? "" : theRow["Vorname"].ToString();
                    string nachname = theRow["Nachname"] == null ? "" : theRow["Nachname"].ToString();

                    if (vorname.Length > 1 && nachname.Length > 1)
                    {
                        var schueler = new Schueler();
                        schueler.Id = theRow["AtlantisSchuelerId"] == null ? -99 : Convert.ToInt32(theRow["AtlantisSchuelerId"]);
                        schueler.Nachname = theRow["Nachname"] == null ? "" : theRow["Nachname"].ToString();
                        if (schueler.Nachname == "Haupt")
                        {
                            string a = "";
                        }
                        schueler.Vorname = theRow["Vorname"] == null ? "" : theRow["Vorname"].ToString();
                        schueler.Klasse = theRow["Klasse"] == null ? "" : theRow["Klasse"].ToString();
                        schueler.Gebdat = theRow["Gebdat"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Gebdat"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Telefon = theRow["telefon"] == null ? "" : theRow["telefon"].ToString();
                        schueler.Mail = Bereinigen(schueler.Nachname) + Bereinigen(schueler.Vorname) + schueler.Id + "@students.berufskolleg-borken.de";
                        schueler.Eintrittsdatum = theRow["Aufnahmedatum"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Aufnahmedatum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        schueler.AktuellJN = theRow["AktuellJN"] == null ? "" : theRow["AktuellJN"].ToString();

                        schueler.Austrittsdatum = theRow["Austrittsdatum"].ToString().Length < 3 ? new DateTime((DateTime.Now.Month >= 8 ? DateTime.Now.Year + 1 : DateTime.Now.Year), 7, 31) : DateTime.ParseExact(theRow["Austrittsdatum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Volljährig = schueler.Gebdat.AddYears(18) > DateTime.Now ? false : true;
                        schueler.Geschlecht = theRow["Geschlecht"] == null ? "" : theRow["Geschlecht"].ToString();

                        schueler.Relianmeldung = theRow["Relianmeldung"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Relianmeldung"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Reliabmeldung = theRow["Reliabmeldung"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Reliabmeldung"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        schueler.Status = theRow["Status"] == null ? "" : theRow["Status"].ToString();
                        schueler.Bezugsjahr = Convert.ToInt32(theRow["Bezugsjahr"].ToString().Substring(0, 4)) - Convert.ToInt32(theRow["Fall_schuljahr_vorher"]);

                        if (schueler.Bezugsjahr == (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1) && schueler.Status != "VB" && schueler.Status != "8" && schueler.Status != "9" && schueler.Klasse != "Z" && schueler.AktuellJN == "J")
                        {
                            // Duplikate werden verhindert.

                            if (!(from s in this where s.Id == schueler.Id select s).Any())
                            {
                                atlantisschulers.Add(schueler);
                            }
                        }
                    }
                }
                
                using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
                {
                    string sch = "";
                    try
                    {
                        string queryString = @"SELECT
StudNumber,
Email,
BirthDate,
FirstName,
Longname,
Name,
Flags,
Student_ID,
CLASS_ID,
Deleted
FROM Student 
WHERE SCHOOLYEAR_ID =" + aktSj + ";";

                        OleDbCommand oleDbCommand = new OleDbCommand(queryString, oleDbConnection);
                        oleDbConnection.Open();
                        OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();

                        List<Schueler> schuelers = new List<Schueler>();

                        while (oleDbDataReader.Read())
                        {
                            Schueler schueler = new Schueler();
                            schueler.IdAtlantis = Convert.ToInt32(Global.SafeGetString(oleDbDataReader, 0));
                            

                            schueler.MailAtlantis = Global.SafeGetString(oleDbDataReader, 1);

                            try
                            {
                                schueler.Gebdat = DateTime.ParseExact((oleDbDataReader.GetInt32(2)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                            }
                            schueler.Vorname = Global.SafeGetString(oleDbDataReader, 3);
                            schueler.Nachname = Global.SafeGetString(oleDbDataReader, 4);

                            sch = schueler.Nachname + ", " + schueler.Vorname;

                            schueler.Anmeldename = Global.SafeGetString(oleDbDataReader, 5);

                            if (schueler.Anmeldename == "nm153778")
                            {
                                string a = "";
                            }
                            schueler.GeschlechtMw = Global.SafeGetString(oleDbDataReader, 6);
                            schueler.IdUntis = oleDbDataReader.GetInt32(7);
                            schueler.UntisClass_Id = oleDbDataReader.GetInt32(8);
                            schueler.Deleted = oleDbDataReader.GetBoolean(9);

                            // Die idUntis wird vorhandenen Schülern zugewiesen
                            var schü = (from s in this where s.Anmeldename == schueler.Anmeldename select s).FirstOrDefault();
                            if (schü != null)
                            {
                                schü.IdUntis = schueler.IdUntis;
                            }

                            var untisKlasseName = (from k in klasses where k.IdUntis == schueler.UntisClass_Id select k.NameUntis).FirstOrDefault();

                            var atlantisKlasseName = (from a in atlantisschulers
                                                      where a.Mail == schueler.MailAtlantis
                                                      select a.Klasse).FirstOrDefault();

                            schueler.Klasse = atlantisKlasseName;

                            if (atlantisKlasseName != null && untisKlasseName != atlantisKlasseName)
                            {                                
                                Console.WriteLine("[!] Der Schüler " + schueler.Nachname + " " + schueler.Vorname + " ist in Untis in der falschen Klasse (" + untisKlasseName + "). Er wird nach " + atlantisKlasseName + " umgebucht.");
                                

                                var untisZielKlasseId = (from k in klasses where k.NameUntis == atlantisKlasseName select k.IdUntis).FirstOrDefault();
                                
                                KlasseWechseln(schueler, untisKlasseName, untisZielKlasseId, aktSj);

                                


                            }

                            schueler.Klasse = atlantisKlasseName;


                            schueler.Reliabmeldung = (from a in atlantisschulers
                                                      where a.Mail == schueler.MailAtlantis
                                                      select a.Reliabmeldung).FirstOrDefault();

                            schueler.Relianmeldung = (from a in atlantisschulers
                                                      where a.Mail == schueler.MailAtlantis
                                                      select a.Relianmeldung).FirstOrDefault();


                            if ((from a in atlantisschulers where a.Mail == schueler.MailAtlantis where !schueler.Deleted select a).Any())
                            {
                                schuelers.Add(schueler);
                            }
                            else
                            {
                                if (!schueler.Deleted)
                                {    
                                    DeleteSchuelerAusUntis(schueler, aktSj);
                                }                                
                            }
                        };
                        oleDbDataReader.Close();

                        schuelers = schuelers.OrderBy(o => o.Klasse).ToList();

                        foreach (var schueler in schuelers)
                        {
                            this.Add(schueler);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    finally
                    {
                        oleDbConnection.Close();

                        Console.WriteLine(this.Count);
                    }
                }
            }
        }

        private void DeleteSchuelerAusUntis(Schueler schueler, string aktSj)
        {
            using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    Console.WriteLine(("[-] " + (schueler.Nachname + ", " + schueler.Vorname).PadRight(40, '.') + " (" + schueler.Klasse + ") wird gelöscht.").PadRight(75, '.'));
                    Console.ReadKey();
                    oleDbConnection.Open();

                    String my_querry = @"UPDATE Student SET 
Student.Deleted = True
WHERE(((Student.SCHOOL_ID) = 177659) AND((Student.SCHOOLYEAR_ID) = " + aktSj + ") AND((Student.VERSION_ID) = 1) AND((Student.STUDENT_ID) = " + schueler.IdUntis + "));";

                    OleDbCommand cmd = new OleDbCommand(my_querry, oleDbConnection);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    oleDbConnection.Close();
                }
            }
        }

        private void KlasseWechseln(Schueler schueler, string quellKlasse, int untisZielKlasseId, string aktSj)
        {
            using (OleDbConnection oleDbConnection = new OleDbConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    Console.WriteLine(("[U] " + (schueler.Nachname + ", " + schueler.Vorname).PadRight(40, '.') + " (" + quellKlasse + ") -> " + schueler.Klasse + "").PadRight(75, '.'));
                    Console.ReadKey();

                    oleDbConnection.Open();

                    String my_querry = @"UPDATE Student SET 
Student.CLASS_ID = " + untisZielKlasseId + @" 
WHERE(((Student.SCHOOL_ID) = 177659) AND((Student.SCHOOLYEAR_ID) = " + aktSj + ") AND((Student.VERSION_ID) = 1) AND((Student.STUDENT_ID) = " + schueler.IdUntis + "));";

                    OleDbCommand cmd = new OleDbCommand(my_querry, oleDbConnection);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    oleDbConnection.Close();
                }
            }
        }

        private string Bereinigen(string Textinput)
        {
            string Text = Textinput;

            Text = Text.ToLower();                          // Nur Kleinbuchstaben
            Text = UmlauteBehandeln(Text);                 // Umlaute ersetzen


            Text = Regex.Replace(Text, "-", "_");           //  kein Minus-Zeichen
            Text = Regex.Replace(Text, ",", "_");           //  kein Komma            
            Text = Regex.Replace(Text, " ", "_");           //  kein Leerzeichen
            // Text = Regex.Replace(Text, @"[^\w]", string.Empty);   // nur Buchstaben

            Text = Regex.Replace(Text, "[^a-z]", string.Empty);   // nur Buchstaben

            Text = Text.Substring(0, 1);  // Auf maximal 1 Zeichen begrenzen
            return Text;
        }

        public string UmlauteBehandeln(string Textinput)
        {
            string Text = Textinput;

            // deutsche Sonderzeichen
            Text = Regex.Replace(Text, "[æ|ä]", "ae");
            Text = Regex.Replace(Text, "[Æ|Ä]", "Ae");
            Text = Regex.Replace(Text, "[œ|ö]", "oe");
            Text = Regex.Replace(Text, "[Œ|Ö]", "Oe");
            Text = Regex.Replace(Text, "[ü]", "ue");
            Text = Regex.Replace(Text, "[Ü]", "Ue");
            Text = Regex.Replace(Text, "ß", "ss");

            // Sonderzeichen aus anderen Sprachen
            Text = Regex.Replace(Text, "[ã|à|â|á|å]", "a");
            Text = Regex.Replace(Text, "[Ã|À|Â|Á|Å]", "A");
            Text = Regex.Replace(Text, "[é|è|ê|ë]", "e");
            Text = Regex.Replace(Text, "[É|È|Ê|Ë]", "E");
            Text = Regex.Replace(Text, "[í|ì|î|ï]", "i");
            Text = Regex.Replace(Text, "[Í|Ì|Î|Ï]", "I");
            Text = Regex.Replace(Text, "[õ|ò|ó|ô]", "o");
            Text = Regex.Replace(Text, "[Õ|Ó|Ò|Ô]", "O");
            Text = Regex.Replace(Text, "[ù|ú|û|µ]", "u");
            Text = Regex.Replace(Text, "[Ú|Ù|Û]", "U");
            Text = Regex.Replace(Text, "[ý|ÿ]", "y");
            Text = Regex.Replace(Text, "[Ý]", "Y");
            Text = Regex.Replace(Text, "[ç|č]", "c");
            Text = Regex.Replace(Text, "[Ç|Č]", "C");
            Text = Regex.Replace(Text, "[Ð]", "D");
            Text = Regex.Replace(Text, "[ñ]", "n");
            Text = Regex.Replace(Text, "[Ñ]", "N");
            Text = Regex.Replace(Text, "[š]", "s");
            Text = Regex.Replace(Text, "[Š]", "S");

            return Text;

        }

        internal void ReliKurswahlenHinzufügenOderLöschen(Unterrichts unterrichts, Kurswahlen kurswahlenIst, string aktSJ, int periode)
        {
            foreach (var schueler in this)
            {
                if (true/*schueler.Anmeldename == "bm153842"*/)
                {
                    // Für jeden Schüler au0erhalb der Gym ...

                    if (schueler.Klasse != null)
                    {
                        if (!schueler.Klasse.StartsWith("G"))
                        {
                            // ... sofern in seiner Klasse Religionsunterricht angeboten wird ...

                            var reliunterrichtDerKlasseDesSchülers = (from u in unterrichts
                                                                      where u.Klasse.NameUntis == schueler.Klasse
                                                                      where (u.Fach.KürzelUntis.StartsWith("KR ") ||
                                                                      u.Fach.KürzelUntis.StartsWith("ER ") ||
                                                                      u.Fach.KürzelUntis == "KR" ||
                                                                      u.Fach.KürzelUntis == "ER")
                                                                      select u).FirstOrDefault();
                            if (reliunterrichtDerKlasseDesSchülers != null)
                            {
                                // ... und der Schüler sich nicht abgemeldet oder ab- und später wieder angemeldet hat ...

                                Kurswahl kurswahl = new Kurswahl();

                                if ((schueler.Reliabmeldung.Year == 1 || schueler.Relianmeldung > schueler.Reliabmeldung))
                                {
                                    // ... wird die Kurswahl hinzugefügt.

                                    kurswahl.StudentKurzname = schueler.Anmeldename;
                                    kurswahl.Geburtsdatum = schueler.Gebdat;
                                    kurswahl.Nachname = schueler.Nachname;
                                    kurswahl.Vorname = schueler.Vorname;
                                    kurswahl.StudentId = schueler.IdUntis;
                                    kurswahl.AlternativeCourses.Add(reliunterrichtDerKlasseDesSchülers.IdUntis.ToString() + "/" + reliunterrichtDerKlasseDesSchülers.Fach.IdUntis + "/1");
                                    kurswahl.Fach = reliunterrichtDerKlasseDesSchülers.Fach.KürzelUntis;
                                    kurswahl.Klasse = schueler.Klasse;
                                    
                                    try
                                    {
                                        if ((from k in kurswahlenIst where k.StudentId == schueler.IdUntis select k).Count() == 0)
                                        {
                                            kurswahl.Number = 1;
                                        }
                                        else
                                        {
                                            kurswahl.Number = (from k in kurswahlenIst where k.StudentId == schueler.IdUntis select k.Number).Max() + 1;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                        Console.ReadKey();
                                    }
                                    
                                    if (schueler.Kurse == null) { schueler.Kurse = new List<Kurswahl>(); }
                                    schueler.Kurse.Add(kurswahl);

                                    // Wenn die Reli-Kurswahl in Untis noch nicht existiert, wird sie ergänzt:

                                    if (!(from k in kurswahlenIst
                                          where k.Fach == kurswahl.Fach
                                          where k.Klasse == kurswahl.Klasse
                                          where k.StudentId == kurswahl.StudentId
                                          select k).Any())
                                    {
                                        kurswahl.AktSj = aktSJ;
                                        kurswahl.InsertIntoStudentChoice(periode);                                        
                                    }

                                    // Wenn die Relikurswahl existert, aber Deleted ist, wird sie wieder aktiviert:

                                    if ((from k in kurswahlenIst
                                         where k.Fach == kurswahl.Fach
                                         where k.Klasse == kurswahl.Klasse
                                         where k.StudentId == kurswahl.StudentId
                                         where k.Deleted == true
                                         select k).Any())
                                    {
                                        kurswahl.AktSj = aktSJ;
                                        schueler.UpdateStudentChoice(periode);
                                        
                                    }
                                }

                                // Wenn der Schüler den existierenden Religionskurs abgewählt hat, ...

                                else
                                {
                                    // ... und bereits eine Kurswahl besteht ...

                                    var kk = (from k in kurswahlenIst
                                              where k.Fach == reliunterrichtDerKlasseDesSchülers.Fach.KürzelUntis
                                              where k.Klasse == schueler.Klasse
                                              where k.StudentId == schueler.IdUntis
                                              select k).FirstOrDefault();

                                    if (kk != null)
                                    {
                                        // ... und die Kurswahl nicht bereits delted wurde ...

                                        if (!kk.Deleted)
                                        {
                                            // ... wird die Kurswahl gelöscht.

                                            schueler.DeleteStudentChoice(periode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Der Schüler " + schueler.Nachname + ", " + schueler.Vorname + "(" + schueler.IdAtlantis +"; " + schueler.IdUntis  + ") hat keine Klasse.");
                        //Console.ReadKey();
                    }
                }
            }
        }

        internal void KurswahlenGymIst(Kurswahlen kurswahlenIst)
        {
            foreach (var schueler in this)
            {
                schueler.Kurse = new List<Kurswahl>();

                schueler.Kurse.AddRange((from k in kurswahlenIst
                                  where k.Nachname == schueler.Nachname
                                  where k.Vorname == schueler.Vorname
                                  where k.Geburtsdatum == schueler.Gebdat
                                  //where k.Klasse.StartsWith("G")
                                  select k).ToList());
            }
        }
    }
}