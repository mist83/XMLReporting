using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public enum PageSize
    {
        /// <summary>
        /// 841mm × 1189mm
        /// </summary>
        A0 = 0,

        /// <summary>
        /// 594mm × 841mm
        /// </summary>
        A1, // 

        /// <summary>
        /// 420mm × 594mm
        /// </summary>
        A2, // 

        /// <summary>
        /// 297mm × 420mm
        /// </summary>
        A3, // 


        /// <summary>
        /// 210mm × 297mm
        /// </summary>
        A4, // 

        /// <summary>
        /// 148mm × 210mm
        /// </summary>
        A5, // 

        /// <summary>
        /// 105mm × 148mm
        /// </summary>
        A6, // 

        /// <summary>
        /// 74mm × 105mm
        /// </summary>
        A7, // 

        /// <summary>
        /// 52mm × 74mm
        /// </summary>
        A8, // 

        /// <summary>
        /// 37mm × 52mm
        /// </summary>
        A9, // 

        /// <summary>
        /// 26mm × 37mm
        /// </summary>
        A10, // 

        /// <summary>
        /// 1000mm × 1414mm
        /// </summary>
        B0, // 

        /// <summary>
        /// 707mm × 1000mm
        /// </summary>
        B1, // 

        /// <summary>
        /// 
        /// </summary>
        B2, // 500mm × 707mm

        /// <summary>
        /// 
        /// </summary>
        B3, // 353mm × 500mm

        /// <summary>
        /// 250mm × 353mm
        /// </summary>
        B4, // 

        /// <summary>
        /// 176mm × 250mm
        /// </summary>
        B5, // 

        /// <summary>
        /// 125mm × 176mm
        /// </summary>
        B6, // 

        /// <summary>
        /// 88mm × 125mm
        /// </summary>
        B7, // 

        /// <summary>
        /// 62mm × 88mm
        /// </summary>
        B8, // 

        /// <summary>
        /// 44mm × 62mm
        /// </summary>
        B9, // 

        /// <summary>
        /// 31mm × 44mm
        /// </summary>
        B10, // 

        /// <summary>
        /// 917mm × 1297mm
        /// </summary>
        C0, // 

        /// <summary>
        /// 648mm × 917mm
        /// </summary>
        C1, // 

        /// <summary>
        /// 458mm × 648mm
        /// </summary>
        C2, // 

        /// <summary>
        /// 324mm × 458mm
        /// </summary>
        C3, // 

        /// <summary>
        /// 229mm × 324mm
        /// </summary>
        C4, // 

        /// <summary>
        /// 162mm × 229mm
        /// </summary>
        C5, // 

        /// <summary>
        /// 114mm × 162mm
        /// </summary>
        C6, // 

        /// <summary>
        /// 81mm × 114mm
        /// </summary>
        C7, // 

        /// <summary>
        /// 57mm × 81mm
        /// </summary>
        C8, // 

        /// <summary>
        /// 40mm × 57mm
        /// </summary>
        C9, // 

        /// <summary>
        /// 28mm × 40mm
        /// </summary>
        C10, // 

        /// <summary>
        /// 85.60mm × 53.98mm
        /// </summary>
        id_1, // 

        /// <summary>
        /// 105.0mm × 74.0mm
        /// </summary>
        id_2, // 

        /// <summary>
        /// 125.0mm × 88.0mm
        /// </summary>
        id_3, // 

        /// <summary>
        /// 8.5in × 11.0in
        /// </summary>
        US_Letter, // 

        /// <summary>
        /// 8.5in × 14.0in
        /// </summary>
        US_Legal, // 

        /// <summary>
        /// 7.25in × 10.5in
        /// </summary>
        US_Executive, // 

        /// <summary>
        /// 17.0in × 11.0in
        /// </summary>
        US_Ledger, // 

        /// <summary>
        /// 11.0in × 17.0in
        /// </summary>
        US_Tabloid, // 

        /// <summary>
        /// 8.0in × 11.0in
        /// </summary>
        US_Government, // 

        /// <summary>
        /// 5.5in × 8.5in
        /// </summary>
        US_Statement, // 

        /// <summary>
        /// 8.5in × 13.0in
        /// </summary>
        US_Folio, 

        /// <summary>
        /// 8.5in × 11.0in
        /// </summary>
        ansi_a, // 

        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// 11.0in × 17.0in
        /// </summary>
        ansi_b, // 

        /// <summary>
        /// 17.0in × 22.0in
        /// </summary>
        ansi_c, // 

        /// <summary>
        /// 22.0in × 34.0in
        /// </summary>
        ansi_d, // 

        /// <summary>
        /// 34.0in × 44.0in
        /// </summary>
        ansi_e, // 

        /// <summary>
        /// 9.0in × 12.0in
        /// </summary>
        arch_a, // 

        /// <summary>
        /// 12.0in × 18.0in
        /// </summary>
        arch_b, // 

        /// <summary>
        /// 18.0in × 24.0in
        /// </summary>
        arch_c, // 

        /// <summary>
        /// 24.0in × 36.0in
        /// </summary>
        arch_d, // 

        /// <summary>
        /// 30.0in × 42.0in
        /// </summary>
        arch_e1, // 

        /// <summary>
        /// 36.0in × 48.0in
        /// </summary>
        arch_e, // 

        /// <summary>
        /// 15.0in × 22.0in
        /// </summary>
        imperial_folio, // 

        /// <summary>
        /// 11.0in × 15.0in
        /// </summary>
        imperial_quarto, // 

        /// <summary>
        /// 7.5in × 11.0in
        /// </summary>
        imperial_octavo, // 

        /// <summary>
        /// 12.5in × 20.0in
        /// </summary>
        royal_folio, // 

        /// <summary>
        /// 10.0in × 12.5in
        /// </summary>
        royal_quarto, // 

        /// <summary>
        /// 6.25in × 10.0in
        /// </summary>
        royal_octavo, // 

        /// <summary>
        /// 10.0in × 15.0in
        /// </summary>
        crown_folio, // 

        /// <summary>
        /// 7.5in × 10.0in
        /// </summary>
        crown_quarto, // 

        /// <summary>
        /// 5.0in × 7.5in
        /// </summary>
        crown_octavo, // 

        /// <summary>
        /// 8.5in × 13.5in
        /// </summary>
        foolscap_folio, // 

        /// <summary>
        /// 6.75in × 8.5in
        /// </summary>
        foolscap_quarto, // 

        /// <summary>
        /// 4.25in × 6.75in
        /// </summary>
        foolscap_octavo, // 

        /// <summary>
        /// 9.0in × 11.5in
        /// </summary>
        medium_quarto, // 

        /// <summary>
        /// 8.75in × 11.25in
        /// </summary>
        demy_quarto, // 

        /// <summary>
        /// 5.625in × 8.75in
        /// </summary>
        demy_octavo, // 
    }
}
