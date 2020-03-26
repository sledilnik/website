
[<RequireQualifiedAccess>]
module Utils

open Feliz

open Recharts

let zeroToNone value =
    match value with
    | Some 0 -> None
    | _ -> value

let formatChartAxixDate (date : System.DateTime) =
    sprintf "%d.%d." date.Date.Day date.Date.Month

let renderScaleSelector scaleType dispatch =
    let renderSelector (scaleType : ScaleType) (currentScaleType : ScaleType) (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "scale-type-selector__item"
                  scaleType = currentScaleType, "selected" ] ]
        if scaleType = currentScaleType
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "scale-type-selector"
        prop.children [
            Html.text "Skala na Y osi: "
            renderSelector Linear scaleType "linearna"
            renderSelector Log scaleType "logaritmična"
        ]
    ]

let monthNameOfdate (date : System.DateTime) =
    match date.Month with
    | 1 -> "januar"
    | 2 -> "februar"
    | 3 -> "marec"
    | 4 -> "april"
    | 5 -> "maj"
    | 6 -> "junij"
    | 7 -> "julij"
    | 8 -> "avgust"
    | 9 -> "september"
    | 10 -> "oktober"
    | 11 -> "november"
    | 12 -> "december"
    | _ -> failwith "Invalid month"

module Dictionaries =

    let regions =
        [ "si", "SLOVENIJA", Some 2089310
          "ms", "Pomurska", Some 114396
          "mb", "Podravska", Some 324875
          "sg", "Koroška", Some 70683
          "ce", "Savinjska", Some 257425
          "za", "Zasavska", Some 57059
          "kk", "Posavska", Some 75807
          "nm", "Jugovzhodna Slovenija", Some 144688
          "lj", "Osrednjeslovenska", Some 552221
          "kr", "Gorenjska", Some 205717
          "po", "Primorsko-notranjska", Some 52818
          "ng", "Goriška", Some 118008
          "kp", "Obalno-kraška", Some 115613
          "t", "Tujci", None
          "n", "Neznano", None ]
        |> List.map (fun (key, name, population) -> key, {| Name = name ; Population = population |})
        |> Map.ofList

    let municipalities =
        [ ("slovenija", "SLOVENIJA", 2089310)
          ("ajdovščina", "Ajdovščina", 19364)
          ("ankaran", "Ankaran/Ancarano", 3215)
          ("apače", "Apače", 3532)
          ("beltinci", "Beltinci", 8098)
          ("benedikt", "Benedikt", 2584)
          ("bistrica_ob_sotli", "Bistrica ob Sotli", 1333)
          ("bled", "Bled", 7846)
          ("bloke", "Bloke", 1567)
          ("bohinj", "Bohinj", 5118)
          ("borovnica", "Borovnica", 4547)
          ("bovec", "Bovec", 3074)
          ("braslovče", "Braslovče", 5584)
          ("brda", "Brda", 5632)
          ("brezovica", "Brezovica", 12527)
          ("brežice", "Brežice", 24157)
          ("cankova", "Cankova", 1724)
          ("celje", "Celje", 49602)
          ("cerklje_na_gorenjskem", "Cerklje na Gorenjskem", 7712)
          ("cerknica", "Cerknica", 11625)
          ("cerkno", "Cerkno", 4577)
          ("cerkvenjak", "Cerkvenjak", 2080)
          ("cirkulane", "Cirkulane", 2362)
          ("črenšovci", "Črenšovci", 3958)
          ("črna_na_koroškem", "Črna na Koroškem", 3281)
          ("črnomelj", "Črnomelj", 14307)
          ("destrnik", "Destrnik", 2617)
          ("divača", "Divača", 4195)
          ("dobje", "Dobje", 965)
          ("dobrepolje", "Dobrepolje", 3838)
          ("dobrna", "Dobrna", 2240)
          ("dobrova-polhov_gradec", "Dobrova - Polhov Gradec", 7752)
          ("dobrovnik", "Dobrovnik/Dobronak", 1287)
          ("dol_pri_ljubljani", "Dol pri Ljubljani", 6295)
          ("dolenjske_toplice", "Dolenjske Toplice", 3525)
          ("domžale", "Domžale", 36429)
          ("dornava", "Dornava", 2894)
          ("dravograd", "Dravograd", 8849)
          ("duplek", "Duplek", 6955)
          ("gorenja_vas-poljane", "Gorenja vas - Poljane", 7560)
          ("gorišnica", "Gorišnica", 4097)
          ("gorje", "Gorje", 2751)
          ("gornja_radgona", "Gornja Radgona", 8401)
          ("gornji_grad", "Gornji Grad", 2506)
          ("gornji_petrovci", "Gornji Petrovci", 1992)
          ("grad", "Grad", 2063)
          ("grosuplje", "Grosuplje", 21265)
          ("hajdina", "Hajdina", 3850)
          ("hoče-slivnica", "Hoče - Slivnica", 11848)
          ("hodoš", "Hodoš/Hodos", 358)
          ("horjul", "Horjul", 3032)
          ("hrastnik", "Hrastnik", 9140)
          ("hrpelje-kozina", "Hrpelje - Kozina", 4534)
          ("idrija", "Idrija", 11797)
          ("ig", "Ig", 7577)
          ("ilirska_bistrica", "Ilirska Bistrica", 13290)
          ("ivančna_gorica", "Ivančna Gorica", 16880)
          ("izola", "Izola/Isola", 16367)
          ("jesenice", "Jesenice", 21168)
          ("jezersko", "Jezersko", 635)
          ("juršinci", "Juršinci", 2367)
          ("kamnik", "Kamnik", 29847)
          ("kanal", "Kanal", 5304)
          ("kidričevo", "Kidričevo", 6497)
          ("kobarid", "Kobarid", 4078)
          ("kobilje", "Kobilje", 545)
          ("kočevje", "Kočevje", 15688)
          ("komen", "Komen", 3546)
          ("komenda", "Komenda", 6345)
          ("koper", "Koper/Capodistria", 52540)
          ("kostanjevica_na_krki", "Kostanjevica na Krki", 2445)
          ("kostel", "Kostel", 643)
          ("kozje", "Kozje", 3053)
          ("kranj", "Kranj", 56715)
          ("kranjska_gora", "Kranjska Gora", 5206)
          ("križevci", "Križevci", 3545)
          ("krško", "Krško", 26117)
          ("kungota", "Kungota", 4734)
          ("kuzma", "Kuzma", 1574)
          ("laško", "Laško", 13023)
          ("lenart", "Lenart", 8443)
          ("lendava", "Lendava/Lendva", 10464)
          ("litija", "Litija", 15429)
          ("ljubljana", "Ljubljana", 294113)
          ("ljubno", "Ljubno", 2547)
          ("ljutomer", "Ljutomer", 11265)
          ("log-dragomer", "Log - Dragomer", 3654)
          ("logatec", "Logatec", 14232)
          ("loška_dolina", "Loška dolina", 3797)
          ("loški_potok", "Loški Potok", 1810)
          ("lovrenc_na_pohorju", "Lovrenc na Pohorju", 2974)
          ("luče", "Luče", 1452)
          ("lukovica", "Lukovica", 5882)
          ("majšperk", "Majšperk", 4067)
          ("makole", "Makole", 2025)
          ("maribor", "Maribor", 112095)
          ("markovci", "Markovci", 4000)
          ("medvode", "Medvode", 16651)
          ("mengeš", "Mengeš", 8150)
          ("metlika", "Metlika", 8397)
          ("mežica", "Mežica", 3559)
          ("miklavž_na_dravskem_polju", "Miklavž na Dravskem polju", 6828)
          ("miren-kostanjevica", "Miren - Kostanjevica", 4976)
          ("mirna", "Mirna", 2622)
          ("mirna_peč", "Mirna Peč", 2987)
          ("mislinja", "Mislinja", 4561)
          ("mokronog-trebelno", "Mokronog - Trebelno", 3080)
          ("moravče", "Moravče", 5435)
          ("moravske_toplice", "Moravske Toplice", 5853)
          ("mozirje", "Mozirje", 4133)
          ("murska_sobota", "Murska Sobota", 18742)
          ("muta", "Muta", 3407)
          ("naklo", "Naklo", 5402)
          ("nazarje", "Nazarje", 2608)
          ("nova_gorica", "Nova Gorica", 31932)
          ("novo_mesto", "Novo mesto", 37280)
          ("odranci", "Odranci", 1644)
          ("oplotnica", "Oplotnica", 4130)
          ("ormož", "Ormož", 11968)
          ("osilnica", "Osilnica", 366)
          ("pesnica", "Pesnica", 7360)
          ("piran", "Piran/Pirano", 17692)
          ("pivka", "Pivka", 6176)
          ("podčetrtek", "Podčetrtek", 3434)
          ("podlehnik", "Podlehnik", 1790)
          ("podvelka", "Podvelka", 2344)
          ("poljčane", "Poljčane", 4459)
          ("polzela", "Polzela", 6224)
          ("postojna", "Postojna", 16363)
          ("prebold", "Prebold", 5125)
          ("preddvor", "Preddvor", 3697)
          ("prevalje", "Prevalje", 6812)
          ("ptuj", "Ptuj", 23443)
          ("puconci", "Puconci", 5892)
          ("rače-fram", "Rače - Fram", 7433)
          ("radeče", "Radeče", 4169)
          ("radenci", "Radenci", 5107)
          ("radlje_ob_dravi", "Radlje ob Dravi", 6183)
          ("radovljica", "Radovljica", 19053)
          ("ravne_na_koroškem", "Ravne na Koroškem", 11315)
          ("razkrižje", "Razkrižje", 1269)
          ("rečica_ob_savinji", "Rečica ob Savinji", 2333)
          ("renče-vogrsko", "Renče - Vogrsko", 4366)
          ("ribnica", "Ribnica", 9513)
          ("ribnica_na_pohorju", "Ribnica na Pohorju", 1125)
          ("rogaška_slatina", "Rogaška Slatina", 11129)
          ("rogašovci", "Rogašovci", 3070)
          ("rogatec", "Rogatec", 3094)
          ("ruše", "Ruše", 7038)
          ("selnica_ob_dravi", "Selnica ob Dravi", 4479)
          ("semič", "Semič", 3838)
          ("sevnica", "Sevnica", 17586)
          ("sežana", "Sežana", 13524)
          ("slovenj_gradec", "Slovenj Gradec", 16599)
          ("slovenska_bistrica", "Slovenska Bistrica", 25690)
          ("slovenske_konjice", "Slovenske Konjice", 14993)
          ("sodražica", "Sodražica", 2191)
          ("solčava", "Solčava", 517)
          ("središče_ob_dravi", "Središče ob Dravi", 1951)
          ("starše", "Starše", 4011)
          ("straža", "Straža", 3912)
          ("sveta_ana", "Sveta Ana", 2297)
          ("sveta_trojica_v_slov._goricah", "Sveta Trojica v Slov. goricah", 2083)
          ("sveti_andraž_v_slov._goricah", "Sveti Andraž v Slov. goricah", 1187)
          ("sveti_jurij_ob_ščavnici", "Sveti Jurij ob Ščavnici", 2788)
          ("sveti_jurij_v_slov._goricah", "Sveti Jurij v Slov. goricah", 2083)
          ("sveti_tomaž", "Sveti Tomaž", 1998)
          ("šalovci", "Šalovci", 1383)
          ("šempeter-vrtojba", "Šempeter - Vrtojba", 6243)
          ("šenčur", "Šenčur", 8756)
          ("šentilj", "Šentilj", 8391)
          ("šentjernej", "Šentjernej", 7157)
          ("šentjur", "Šentjur", 19186)
          ("šentrupert", "Šentrupert", 2978)
          ("škocjan", "Škocjan", 3328)
          ("škofja_loka", "Škofja Loka", 23216)
          ("škofljica", "Škofljica", 11419)
          ("šmarje_pri_jelšah", "Šmarje pri Jelšah", 10278)
          ("šmarješke_toplice", "Šmarješke Toplice", 3400)
          ("šmartno_ob_paki", "Šmartno ob Paki", 3271)
          ("šmartno_pri_litiji", "Šmartno pri Litiji", 5629)
          ("šoštanj", "Šoštanj", 8735)
          ("štore", "Štore", 4576)
          ("tabor", "Tabor", 1659)
          ("tišina", "Tišina", 3970)
          ("tolmin", "Tolmin", 11031)
          ("trbovlje", "Trbovlje", 16037)
          ("trebnje", "Trebnje", 13018)
          ("trnovska_vas", "Trnovska vas", 1350)
          ("trzin", "Trzin", 3923)
          ("tržič", "Tržič", 14884)
          ("turnišče", "Turnišče", 3170)
          ("velenje", "Velenje", 33506)
          ("velika_polana", "Velika Polana", 1412)
          ("velike_lašče", "Velike Lašče", 4383)
          ("veržej", "Veržej", 1290)
          ("videm", "Videm", 5587)
          ("vipava", "Vipava", 5634)
          ("vitanje", "Vitanje", 2256)
          ("vodice", "Vodice", 4964)
          ("vojnik", "Vojnik", 8846)
          ("vransko", "Vransko", 2620)
          ("vrhnika", "Vrhnika", 17452)
          ("vuzenica", "Vuzenica", 2648)
          ("zagorje_ob_savi", "Zagorje ob Savi", 16453)
          ("zavrč", "Zavrč", 1533)
          ("zreče", "Zreče", 6505)
          ("žalec", "Žalec", 21425)
          ("železniki", "Železniki", 6699)
          ("žetale", "Žetale", 1297)
          ("žiri", "Žiri", 4915)
          ("žirovnica", "Žirovnica", 4384)
          ("žužemberk", "Žužemberk", 4648) ]
        |> List.map (fun (key, name, population) -> key, {| Name = name ; Population = population |})
        |> Map.ofList
