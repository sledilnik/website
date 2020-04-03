
[<RequireQualifiedAccess>]
module Utils

open Feliz

open Types

let zeroToNone value =
    match value with
    | Some 0 -> None
    | _ -> value

let calculateDoublingTime (v1 : {| Day : int ; PositiveTests : int |}) (v2 : {| Day : int ; PositiveTests : int |}) =
    let v1, v2, dt = float v1.PositiveTests, float v2.PositiveTests, float (v2.Day - v1.Day)
    if v1 = v2 then None
    else
        let value = log10 2.0 / log10 ((v2 / v1) ** (1.0 / dt))
        if value < 0.0 then None
        else Some value

let renderScaleSelector scaleType dispatch =
    let renderSelector (scaleType : ScaleType) (currentScaleType : ScaleType) (label : string) =
        let defaultProps =
            [ prop.text label
              prop.className [
                  true, "chart-display-property-selector__item"
                  scaleType = currentScaleType, "selected" ] ]
        if scaleType = currentScaleType
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.text "Skala na Y osi: "
            renderSelector Linear scaleType "Linearna"
            renderSelector Logarithmic scaleType "Logaritemska"
        ]
    ]

let renderLoading =
    Html.div [
        prop.className "loader"
        prop.children [ Html.text "Nalagam podatke..." ]
    ]


let renderErrorLoading (error : string) =
    Html.text error

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

let daysMestnik days =
    match days % 100 with
    | 1 -> "dnevu"
    | _ -> "dneh"

let transliterateCSZ (str : string) =
    str
        .Replace("Č", "C")
        .Replace("Š", "S")
        .Replace("Ž", "Z")
        .Replace("č", "c")
        .Replace("š", "s")
        .Replace("ž", "z")

module Dictionaries =

    type Region = {
        Key : string
        Name : string
        Population : int option }

    let excludedRegions = Set.ofList ["t" ; "n"]

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
        |> List.map (fun (key, name, population) -> key, { Key = key ; Name = name ; Population = population })
        |> Map.ofList

    type Municipality = {
        Key : string
        Name : string
        Population : int
        Code : string }

    let municipalities =
        [ "ajdovščina", "Ajdovščina", 19364, "SI-001"
          "ankaran", "Ankaran/Ancarano", 3215, "SI-213"
          "apače", "Apače", 3532, "SI-195"
          "beltinci", "Beltinci", 8098, "SI-002"
          "benedikt", "Benedikt", 2584, "SI-148"
          "bistrica_ob_sotli", "Bistrica ob Sotli", 1333, "SI-149"
          "bled", "Bled", 7846, "SI-003"
          "bloke", "Bloke", 1567, "SI-150"
          "bohinj", "Bohinj", 5118, "SI-004"
          "borovnica", "Borovnica", 4547, "SI-005"
          "bovec", "Bovec", 3074, "SI-006"
          "braslovče", "Braslovče", 5584, "SI-151"
          "brda", "Brda", 5632, "SI-007"
          "brezovica", "Brezovica", 12527, "SI-008"
          "brežice", "Brežice", 24157, "SI-009"
          "cankova", "Cankova", 1724, "SI-152"
          "celje", "Celje", 49602, "SI-011"
          "cerklje_na_gorenjskem", "Cerklje na Gorenjskem", 7712, "SI-012"
          "cerknica", "Cerknica", 11625, "SI-013"
          "cerkno", "Cerkno", 4577, "SI-014"
          "cerkvenjak", "Cerkvenjak", 2080, "SI-153"
          "cirkulane", "Cirkulane", 2362, "SI-196"
          "črenšovci", "Črenšovci", 3958, "SI-015"
          "črna_na_koroškem", "Črna na Koroškem", 3281, "SI-016"
          "črnomelj", "Črnomelj", 14307, "SI-017"
          "destrnik", "Destrnik", 2617, "SI-018"
          "divača", "Divača", 4195, "SI-019"
          "dobje", "Dobje", 965, "SI-154"
          "dobrepolje", "Dobrepolje", 3838, "SI-020"
          "dobrna", "Dobrna", 2240, "SI-155"
          "dobrova-polhov_gradec", "Dobrova - Polhov Gradec", 7752, "SI-021"
          "dobrovnik", "Dobrovnik/Dobronak", 1287, "SI-156"
          "dol_pri_ljubljani", "Dol pri Ljubljani", 6295, "SI-022"
          "dolenjske_toplice", "Dolenjske Toplice", 3525, "SI-157"
          "domžale", "Domžale", 36429, "SI-023"
          "dornava", "Dornava", 2894, "SI-024"
          "dravograd", "Dravograd", 8849, "SI-025"
          "duplek", "Duplek", 6955, "SI-026"
          "gorenja_vas-poljane", "Gorenja vas - Poljane", 7560, "SI-027"
          "gorišnica", "Gorišnica", 4097, "SI-028"
          "gorje", "Gorje", 2751, "SI-207"
          "gornja_radgona", "Gornja Radgona", 8401, "SI-029"
          "gornji_grad", "Gornji Grad", 2506, "SI-030"
          "gornji_petrovci", "Gornji Petrovci", 1992, "SI-031"
          "grad", "Grad", 2063, "SI-158"
          "grosuplje", "Grosuplje", 21265, "SI-032"
          "hajdina", "Hajdina", 3850, "SI-159"
          "hoče-slivnica", "Hoče - Slivnica", 11848, "SI-160"
          "hodoš", "Hodoš/Hodos", 358, "SI-161"
          "horjul", "Horjul", 3032, "SI-162"
          "hrastnik", "Hrastnik", 9140, "SI-034"
          "hrpelje-kozina", "Hrpelje - Kozina", 4534, "SI-035"
          "idrija", "Idrija", 11797, "SI-036"
          "ig", "Ig", 7577, "SI-037"
          "ilirska_bistrica", "Ilirska Bistrica", 13290, "SI-038"
          "ivančna_gorica", "Ivančna Gorica", 16880, "SI-039"
          "izola", "Izola/Isola", 16367, "SI-040"
          "jesenice", "Jesenice", 21168, "SI-041"
          "jezersko", "Jezersko", 635, "SI-163"
          "juršinci", "Juršinci", 2367, "SI-042"
          "kamnik", "Kamnik", 29847, "SI-043"
          "kanal", "Kanal", 5304, "SI-044"
          "kidričevo", "Kidričevo", 6497, "SI-045"
          "kobarid", "Kobarid", 4078, "SI-046"
          "kobilje", "Kobilje", 545, "SI-047"
          "kočevje", "Kočevje", 15688, "SI-048"
          "komen", "Komen", 3546, "SI-049"
          "komenda", "Komenda", 6345, "SI-164"
          "koper", "Koper/Capodistria", 52540, "SI-050"
          "kostanjevica_na_krki", "Kostanjevica na Krki", 2445, "SI-197"
          "kostel", "Kostel", 643, "SI-165"
          "kozje", "Kozje", 3053, "SI-051"
          "kranj", "Kranj", 56715, "SI-052"
          "kranjska_gora", "Kranjska Gora", 5206, "SI-053"
          "križevci", "Križevci", 3545, "SI-166"
          "krško", "Krško", 26117, "SI-054"
          "kungota", "Kungota", 4734, "SI-055"
          "kuzma", "Kuzma", 1574, "SI-056"
          "laško", "Laško", 13023, "SI-057"
          "lenart", "Lenart", 8443, "SI-058"
          "lendava", "Lendava/Lendva", 10464, "SI-059"
          "litija", "Litija", 15429, "SI-060"
          "ljubljana", "Ljubljana", 294113, "SI-061"
          "ljubno", "Ljubno", 2547, "SI-062"
          "ljutomer", "Ljutomer", 11265, "SI-063"
          "log-dragomer", "Log - Dragomer", 3654, "SI-208"
          "logatec", "Logatec", 14232, "SI-064"
          "loška_dolina", "Loška dolina", 3797, "SI-065"
          "loški_potok", "Loški Potok", 1810, "SI-066"
          "lovrenc_na_pohorju", "Lovrenc na Pohorju", 2974, "SI-167"
          "luče", "Luče", 1452, "SI-067"
          "lukovica", "Lukovica", 5882, "SI-068"
          "majšperk", "Majšperk", 4067, "SI-069"
          "makole", "Makole", 2025, "SI-198"
          "maribor", "Maribor", 112095, "SI-070"
          "markovci", "Markovci", 4000, "SI-168"
          "medvode", "Medvode", 16651, "SI-071"
          "mengeš", "Mengeš", 8150, "SI-072"
          "metlika", "Metlika", 8397, "SI-073"
          "mežica", "Mežica", 3559, "SI-074"
          "miklavž_na_dravskem_polju", "Miklavž na Dravskem polju", 6828, "SI-169"
          "miren-kostanjevica", "Miren - Kostanjevica", 4976, "SI-075"
          "mirna", "Mirna", 2622, "SI-212"
          "mirna_peč", "Mirna Peč", 2987, "SI-170"
          "mislinja", "Mislinja", 4561, "SI-076"
          "mokronog-trebelno", "Mokronog - Trebelno", 3080, "SI-199"
          "moravče", "Moravče", 5435, "SI-077"
          "moravske_toplice", "Moravske Toplice", 5853, "SI-078"
          "mozirje", "Mozirje", 4133, "SI-079"
          "murska_sobota", "Murska Sobota", 18742, "SI-080"
          "muta", "Muta", 3407, "SI-081"
          "naklo", "Naklo", 5402, "SI-082"
          "nazarje", "Nazarje", 2608, "SI-083"
          "nova_gorica", "Nova Gorica", 31932, "SI-084"
          "novo_mesto", "Novo mesto", 37280, "SI-085"
          "odranci", "Odranci", 1644, "SI-086"
          "oplotnica", "Oplotnica", 4130, "SI-171"
          "ormož", "Ormož", 11968, "SI-087"
          "osilnica", "Osilnica", 366, "SI-088"
          "pesnica", "Pesnica", 7360, "SI-089"
          "piran", "Piran/Pirano", 17692, "SI-090"
          "pivka", "Pivka", 6176, "SI-091"
          "podčetrtek", "Podčetrtek", 3434, "SI-092"
          "podlehnik", "Podlehnik", 1790, "SI-172"
          "podvelka", "Podvelka", 2344, "SI-093"
          "poljčane", "Poljčane", 4459, "SI-200"
          "polzela", "Polzela", 6224, "SI-173"
          "postojna", "Postojna", 16363, "SI-094"
          "prebold", "Prebold", 5125, "SI-174"
          "preddvor", "Preddvor", 3697, "SI-095"
          "prevalje", "Prevalje", 6812, "SI-175"
          "ptuj", "Ptuj", 23443, "SI-096"
          "puconci", "Puconci", 5892, "SI-097"
          "rače-fram", "Rače - Fram", 7433, "SI-098"
          "radeče", "Radeče", 4169, "SI-099"
          "radenci", "Radenci", 5107, "SI-100"
          "radlje_ob_dravi", "Radlje ob Dravi", 6183, "SI-101"
          "radovljica", "Radovljica", 19053, "SI-102"
          "ravne_na_koroškem", "Ravne na Koroškem", 11315, "SI-103"
          "razkrižje", "Razkrižje", 1269, "SI-176"
          "rečica_ob_savinji", "Rečica ob Savinji", 2333, "SI-209"
          "renče-vogrsko", "Renče - Vogrsko", 4366, "SI-201"
          "ribnica", "Ribnica", 9513, "SI-104"
          "ribnica_na_pohorju", "Ribnica na Pohorju", 1125, "SI-177"
          "rogaška_slatina", "Rogaška Slatina", 11129, "SI-106"
          "rogašovci", "Rogašovci", 3070, "SI-105"
          "rogatec", "Rogatec", 3094, "SI-107"
          "ruše", "Ruše", 7038, "SI-108"
          "selnica_ob_dravi", "Selnica ob Dravi", 4479, "SI-178"
          "semič", "Semič", 3838, "SI-109"
          "sevnica", "Sevnica", 17586, "SI-110"
          "sežana", "Sežana", 13524, "SI-111"
          "slovenj_gradec", "Slovenj Gradec", 16599, "SI-112"
          "slovenska_bistrica", "Slovenska Bistrica", 25690, "SI-113"
          "slovenske_konjice", "Slovenske Konjice", 14993, "SI-114"
          "sodražica", "Sodražica", 2191, "SI-179"
          "solčava", "Solčava", 517, "SI-180"
          "središče_ob_dravi", "Središče ob Dravi", 1951, "SI-202"
          "starše", "Starše", 4011, "SI-115"
          "straža", "Straža", 3912, "SI-203"
          "sveta_ana", "Sveta Ana", 2297, "SI-181"
          "sveta_trojica_v_slov._goricah", "Sveta Trojica v Slov. goricah", 2083, "SI-204"
          "sveti_andraž_v_slov._goricah", "Sveti Andraž v Slov. goricah", 1187, "SI-182"
          "sveti_jurij_ob_ščavnici", "Sveti Jurij ob Ščavnici", 2788, "SI-116"
          "sveti_jurij_v_slov._goricah", "Sveti Jurij v Slov. goricah", 2083, "SI-210"
          "sveti_tomaž", "Sveti Tomaž", 1998, "SI-205"
          "šalovci", "Šalovci", 1383, "SI-033"
          "šempeter-vrtojba", "Šempeter - Vrtojba", 6243, "SI-183"
          "šenčur", "Šenčur", 8756, "SI-117"
          "šentilj", "Šentilj", 8391, "SI-118"
          "šentjernej", "Šentjernej", 7157, "SI-119"
          "šentjur", "Šentjur", 19186, "SI-120"
          "šentrupert", "Šentrupert", 2978, "SI-211"
          "škocjan", "Škocjan", 3328, "SI-121"
          "škofja_loka", "Škofja Loka", 23216, "SI-122"
          "škofljica", "Škofljica", 11419, "SI-123"
          "šmarje_pri_jelšah", "Šmarje pri Jelšah", 10278, "SI-124"
          "šmarješke_toplice", "Šmarješke Toplice", 3400, "SI-206"
          "šmartno_ob_paki", "Šmartno ob Paki", 3271, "SI-125"
          "šmartno_pri_litiji", "Šmartno pri Litiji", 5629, "SI-194"
          "šoštanj", "Šoštanj", 8735, "SI-126"
          "štore", "Štore", 4576, "SI-127"
          "tabor", "Tabor", 1659, "SI-184"
          "tišina", "Tišina", 3970, "SI-010"
          "tolmin", "Tolmin", 11031, "SI-128"
          "trbovlje", "Trbovlje", 16037, "SI-129"
          "trebnje", "Trebnje", 13018, "SI-130"
          "trnovska_vas", "Trnovska vas", 1350, "SI-185"
          "trzin", "Trzin", 3923, "SI-186"
          "tržič", "Tržič", 14884, "SI-131"
          "turnišče", "Turnišče", 3170, "SI-132"
          "velenje", "Velenje", 33506, "SI-133"
          "velika_polana", "Velika Polana", 1412, "SI-187"
          "velike_lašče", "Velike Lašče", 4383, "SI-134"
          "veržej", "Veržej", 1290, "SI-188"
          "videm", "Videm", 5587, "SI-135"
          "vipava", "Vipava", 5634, "SI-136"
          "vitanje", "Vitanje", 2256, "SI-137"
          "vodice", "Vodice", 4964, "SI-138"
          "vojnik", "Vojnik", 8846, "SI-139"
          "vransko", "Vransko", 2620, "SI-189"
          "vrhnika", "Vrhnika", 17452, "SI-140"
          "vuzenica", "Vuzenica", 2648, "SI-141"
          "zagorje_ob_savi", "Zagorje ob Savi", 16453, "SI-142"
          "zavrč", "Zavrč", 1533, "SI-143"
          "zreče", "Zreče", 6505, "SI-144"
          "žalec", "Žalec", 21425, "SI-190"
          "železniki", "Železniki", 6699, "SI-146"
          "žetale", "Žetale", 1297, "SI-191"
          "žiri", "Žiri", 4915, "SI-147"
          "žirovnica", "Žirovnica", 4384, "SI-192"
          "žužemberk", "Žužemberk", 4648, "SI-193" ]
        |> List.map (fun (key, name, population, code) -> key, { Key = key ; Name = name ; Population = population ; Code = code })
        |> Map.ofList
