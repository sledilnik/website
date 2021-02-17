[<RequireQualifiedAccess>]
module Utils

open Fable.Core
open Feliz
open Fable.Core.JsInterop

open Types
open System

let memoize f =
    let cache = System.Collections.Generic.Dictionary<_,_>()
    (fun x ->
        match cache.TryGetValue x with
        | true, value ->
            value
        | false, _ ->
            let value = f x
            cache.Add(x, value)
            value)

// Converts Some 0 value to None
let zeroToNone value =
    match value with
    | Some 0 -> None
    | _ -> value

let optionToInt (value: int option) =
    match value with
    | Some x -> x
    | None -> 0

let noneToZeroFloat (value: float option) =
    match value with
    | Some x -> x
    | None -> 0.

[<Emit("(x => isNaN(x) ? null : x)(+$0)")>]
let nativeParseInt (input : string) : int option = jsNative

[<Emit("(x => isNaN(x) ? null : x)(+$0)")>]
let nativeParseFloat (input : string) : float option = jsNative

let getISOWeekYear (date : System.DateTime) : int =
    importDefault "date-fns/getISOWeekYear"

let parseDate (str : string) =
    try
        DateTime.Parse(str) |> Ok
    with _ ->
        sprintf "Invalid date representation: %s" str |> Error

let roundDecimals (decimals:int) (value: float) = Math.Round(value, decimals)

let roundToInt = roundDecimals 0

let roundTo1Decimal = roundDecimals 1

let roundTo3Decimals = roundDecimals 3

let formatToInt (value: float) =
    I18N.NumberFormat.formatNumber(value)

let formatTo1DecimalWithTrailingZero (value: float) =
    I18N.NumberFormat.formatNumber(
        value, {| minimumFractionDigits=1; maximumFractionDigits=1 |})

let formatTo2DecimalWithTrailingZero (value: float) =
    I18N.NumberFormat.formatNumber(
        value, {| minimumFractionDigits=2; maximumFractionDigits=2 |})

let formatTo3DecimalWithTrailingZero (value: float) =
    I18N.NumberFormat.formatNumber(
        value, {| minimumFractionDigits=3; maximumFractionDigits=3 |})

let percentWith0DecimalFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=0; maximumFractionDigits=0 |})

let percentWith1DecimalFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=1; maximumFractionDigits=1 |})

let percentWith2DecimalFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=1; maximumFractionDigits=2 |})

let percentWith3DecimalFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=1; maximumFractionDigits=3 |})

let percentWith1DecimalSignFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=1
           maximumFractionDigits=1; signDisplay="always" |})

let percentWith2DecimalSignFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=1
           maximumFractionDigits=2; signDisplay="always" |})

let percentWith3DecimalSignFormatter (value: float) =
    I18N.NumberFormat.formatNumber(
        (abs (value / 100.)),
        {| style="percent"; minimumFractionDigits=0
           maximumFractionDigits=3; signDisplay="always" |})

let calculateDoublingTime (v1 : {| Day : int ; PositiveTests : int |}) (v2 : {| Day : int ; PositiveTests : int |}) =
    let v1,  v2,  dt = float v1.PositiveTests,  float v2.PositiveTests,  float (v2.Day - v1.Day)
    if v1 = v2 then None
    else
        let value = log10 2.0 / log10 ((v2 / v1) ** (1.0 / dt))
        if value < 0.0 then None
        else Some value

let findDoublingTime (values : {| Date : DateTime ; Value : int option |} list) =
    let reversedValues =
        values
        |> List.choose (fun dp ->
            match dp.Value with
            | None -> None
            | Some value -> Some {| Date = dp.Date ; Value = value |}
        )
        |> List.rev

    match reversedValues with
    | head :: tail ->
        match tail |> List.tryFind (fun dp ->
            float head.Value / 2. >= float dp.Value) with
        | None -> None
        | Some halfValue -> (head.Date - halfValue.Date).TotalDays |> Some
    | _ -> None

let classes (classTuples: seq<bool * string>) =
    classTuples
    |> Seq.filter (fun (visible, _) -> visible)
    |> Seq.map (fun (_, className) -> className)
    |> Seq.toList
    |> prop.className

let renderScaleSelector scaleType dispatch =
    let renderSelector (scaleType : ScaleType) (currentScaleType : ScaleType) (label : string) =
        let defaultProps =
            [ prop.text label
              classes [
                  (true, "chart-display-property-selector__item")
                  (scaleType = currentScaleType, "selected") ] ]
        if scaleType = currentScaleType
        then Html.div defaultProps
        else Html.div ((prop.onClick (fun _ -> dispatch scaleType)) :: defaultProps)

    let yLabel = I18N.t "charts.common.yAxis"
    let linearLabel = I18N.t "charts.common.linear"
    let logLabel = I18N.t "charts.common.log"
    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            Html.div [
                prop.text yLabel
                prop.className "chart-display-property-selector__item"
            ]
            renderSelector Linear scaleType linearLabel
            renderSelector Logarithmic scaleType logLabel
        ]
    ]

let renderBarChartTypeSelector (activeChartType: BarChartType) dispatch =
    let renderSelector (chartType : BarChartType) (label : string) =
        let active = chartType = activeChartType
        Html.div [
            prop.text label
            prop.onClick (fun _ -> dispatch chartType)
            classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected") ]
        ]

    Html.div [
        prop.className "chart-display-property-selector"
        prop.children [
            renderSelector AbsoluteChart (I18N.t "charts.common.absolute")
            renderSelector RelativeChart (I18N.t "charts.common.relative")
        ]
    ]

let renderChartTopControls (children: ReactElement seq) =
    Html.div [
        prop.className "chart-display-properties"
        prop.children children
    ]

let renderChartTopControlRight (topControl: ReactElement) =
    Html.div [
        prop.className "chart-display-properties"
        prop.style [ style.justifyContent.flexEnd ]
        prop.children [ topControl ]
    ]

let renderLoading =
    let loadingLabel = I18N.t "charts.common.loading"
    Html.div [
        prop.className "loader"
        prop.text loadingLabel
    ]

let renderErrorLoading (error : string) =
    Html.text error

let renderMaybeVisible (visible: bool) (children: ReactElement seq) =
    Html.div [
        prop.className (match visible with
                        | true -> ""
                        | false -> "invisible" )
        prop.children children
    ]

let monthNameOfIndex (index : int) =
    match index with
    | 1 -> I18N.t "month.0"
    | 2 -> I18N.t "month.1"
    | 3 -> I18N.t "month.2"
    | 4 -> I18N.t "month.3"
    | 5 -> I18N.t "month.4"
    | 6 -> I18N.t "month.5"
    | 7 -> I18N.t "month.6"
    | 8 -> I18N.t "month.7"
    | 9 -> I18N.t "month.8"
    | 10 -> I18N.t "month.9"
    | 11 -> I18N.t "month.10"
    | 12 -> I18N.t "month.11"
    | _ -> failwith "Invalid month"

let monthNameOfDate (date : DateTime) =
    monthNameOfIndex date.Month

let transliterateCSZ (str : string) =
    str
        .Replace("Č",  "C")
        .Replace("Š",  "S")
        .Replace("Ž",  "Z")
        .Replace("č",  "c")
        .Replace("š",  "s")
        .Replace("ž",  "z")

let mixColors
    (minColorR, minColorG, minColorB)
    (maxColorR, maxColorG, maxColorB)
    mixRatio =

    let colorR =
        ((maxColorR - minColorR) |> float)
        * mixRatio + (float minColorR)
        |> round |> int
    let colorG =
        ((maxColorG - minColorG) |> float)
        * mixRatio + (float minColorG)
        |> round |> int
    let colorB =
        ((maxColorB - minColorB) |> float)
        * mixRatio + (float minColorB)
        |> round |> int

    "#" + colorR.ToString("X2")
        + colorG.ToString("X2")
        + colorB.ToString("X2")

module Dictionaries =

    type Facility = {
        Key : string
        Name : string
        Color : string option
    }

    let facilities =
        [
            "bse",      "B Sežana",             None
            "bto",      "B Topolšica",          None
            "sbbr",     "SB Brežice",           None
            "sbce",     "SB Celje",             Some "#70a471"
            "sbje",     "SB Jesenice",          None
            "sbiz",     "SB Izola",             None
            "sbms",     "SB Murska Sobota",     None
            "sbng",     "SB Nova Gorica",       None
            "sbnm",     "SB Novo mesto",        None
            "sbpt",     "SB Ptuj",              None
            "sbsg",     "SB Slovenj Gradec",    None
            "sbtr",     "SB Trbovlje",          None
            "ukclj",    "UKC Ljubljana",        Some "#10829a"
            "ukcmb",    "UKC Maribor",          Some "#003f5c"
            "ukg",      "UK Golnik",            Some "#7B7226"
            "upklj",    "UPK Ljubljana",        None
            "pbbe",     "PB Begunje",           None
            "pbvo",     "PB Vojnik",            None
            "pbor",     "PB Ormož",             None
            "pbid",     "PB Idrija",            None
            "imi",      "IMI Ljubljana",        None
            "nlzohce",  "NLZOH Celje",          None
            "nlzohkp",  "NLZOH Koper",          None
            "nlzohkr",  "NLZOH Kranj",          None
            "nlzohlj",  "NLZOH Ljubljana",      None
            "nlzohmb",  "NLZOH Maribor",        None
            "nlzohms",  "NLZOH Murska Sobota",  None
            "nlzohnm",  "NLZOH Novo mesto",     None
        ]
        |> List.map (fun (key, name, color) -> key, { Key = key ; Name = name ; Color = color })
        |> Map.ofList

    let GetFacilityName key =
        match facilities.TryFind(key) with
        | Some facility -> facility.Name
        | None -> key

    let GetFacilityColor key =
        match facilities.TryFind(key) with
        | Some facility -> facility.Color
        | None -> None

    type Region = {
        Key : string
        Name : string
        Population : int option }

    let excludedRegions = Set.ofList ["si" ; "unknown" ; "foreign"; "t" ; "n"]

    let regions =
        [ "si",  "SLOVENIJA",  Some 2100126
          "ms",  "Pomurska",  Some 114397
          "mb",  "Podravska",  Some 326510
          "sg",  "Koroška",  Some 70835
          "ce",  "Savinjska",  Some 258345
          "za",  "Zasavska",  Some 57148
          "kk",  "Posavska",  Some 75983
          "nm",  "Jugovzhodna Slovenija",  Some 145859
          "lj",  "Osrednjeslovenska",  Some 554823
          "kr",  "Gorenjska",  Some 207842
          "po",  "Primorsko-notranjska",  Some 53092
          "ng",  "Goriška",  Some 118421
          "kp",  "Obalno-kraška",  Some 116871
          "unknown", "TUJINA", None
          "foreign", "NEZNANO", None ]
        |> List.map
               (fun (key,  name,  population) ->
                    key, { Key = key ; Name = name ; Population = population })
        |> Map.ofList

    type Municipality = {
        Key : string
        Name : string
        Code : string
        Population : int
    }

    let municipalities =
        [
            "ajdovščina", "Ajdovščina", "SI-001", 19671
            "ankaran", "Ankaran/Ancarano", "SI-213", 3253
            "apače", "Apače", "SI-195", 3541
            "beltinci", "Beltinci", "SI-002", 8044
            "benedikt", "Benedikt", "SI-148", 2611
            "bistrica_ob_sotli", "Bistrica ob Sotli", "SI-149", 1341
            "bled", "Bled", "SI-003", 7983
            "bloke", "Bloke", "SI-150", 1603
            "bohinj", "Bohinj", "SI-004", 5360
            "borovnica", "Borovnica", "SI-005", 4625
            "bovec", "Bovec", "SI-006", 3193
            "braslovče", "Braslovče", "SI-151", 5640
            "brda", "Brda", "SI-007", 5613
            "brezovica", "Brezovica", "SI-008", 12771
            "brežice", "Brežice", "SI-009", 24250
            "cankova", "Cankova", "SI-152", 1746
            "celje", "Celje", "SI-011", 49069
            "cerklje_na_gorenjskem", "Cerklje na Gorenjskem", "SI-012", 7796
            "cerknica", "Cerknica", "SI-013", 11668
            "cerkno", "Cerkno", "SI-014", 4588
            "cerkvenjak", "Cerkvenjak", "SI-153", 2170
            "cirkulane", "Cirkulane", "SI-196", 2374
            "črenšovci", "Črenšovci", "SI-015", 3995
            "črna_na_koroškem", "Črna na Koroškem", "SI-016", 3271
            "črnomelj", "Črnomelj", "SI-017", 14318
            "destrnik", "Destrnik", "SI-018", 2640
            "divača", "Divača", "SI-019", 4255
            "dobje", "Dobje", "SI-154", 942
            "dobrepolje", "Dobrepolje", "SI-020", 3894
            "dobrna", "Dobrna", "SI-155", 2241
            "dobrova-polhov_gradec", "Dobrova - Polhov Gradec", "SI-021", 7830
            "dobrovnik", "Dobrovnik/Dobronak", "SI-156", 1273
            "dol_pri_ljubljani", "Dol pri Ljubljani", "SI-022", 6315
            "dolenjske_toplice", "Dolenjske Toplice", "SI-157", 3570
            "domžale", "Domžale", "SI-023", 36648
            "dornava", "Dornava", "SI-024", 2910
            "dravograd", "Dravograd", "SI-025", 8891
            "duplek", "Duplek", "SI-026", 6978
            "gorenja_vas-poljane", "Gorenja vas - Poljane", "SI-027", 7631
            "gorišnica", "Gorišnica", "SI-028", 4152
            "gorje", "Gorje", "SI-207", 2759
            "gornja_radgona", "Gornja Radgona", "SI-029", 8439
            "gornji_grad", "Gornji Grad", "SI-030", 2490
            "gornji_petrovci", "Gornji Petrovci", "SI-031", 2000
            "grad", "Grad", "SI-158", 2055
            "grosuplje", "Grosuplje", "SI-032", 21406
            "hajdina", "Hajdina", "SI-159", 3905
            "hoče-slivnica", "Hoče - Slivnica", "SI-160", 11721
            "hodoš", "Hodoš/Hodos", "SI-161", 355
            "horjul", "Horjul", "SI-162", 3014
            "hrastnik", "Hrastnik", "SI-034", 9139
            "hrpelje-kozina", "Hrpelje - Kozina", "SI-035", 4671
            "idrija", "Idrija", "SI-036", 11793
            "ig", "Ig", "SI-037", 7595
            "ilirska_bistrica", "Ilirska Bistrica", "SI-038", 13337
            "ivančna_gorica", "Ivančna Gorica", "SI-039", 17235
            "izola", "Izola/Isola", "SI-040", 16589
            "jesenice", "Jesenice", "SI-041", 21519
            "jezersko", "Jezersko", "SI-163", 645
            "juršinci", "Juršinci", "SI-042", 2397
            "kamnik", "Kamnik", "SI-043", 29933
            "kanal", "Kanal", "SI-044", 5295
            "kidričevo", "Kidričevo", "SI-045", 6526
            "kobarid", "Kobarid", "SI-046", 4045
            "kobilje", "Kobilje", "SI-047", 540
            "kočevje", "Kočevje", "SI-048", 15702
            "komen", "Komen", "SI-049", 3549
            "komenda", "Komenda", "SI-164", 6422
            "koper", "Koper/Capodistria", "SI-050", 52773
            "kostanjevica_na_krki", "Kostanjevica na Krki", "SI-197", 2443
            "kostel", "Kostel", "SI-165", 674
            "kozje", "Kozje", "SI-051", 3010
            "kranj", "Kranj", "SI-052", 57133
            "kranjska_gora", "Kranjska Gora", "SI-053", 5590
            "križevci", "Križevci", "SI-166", 3542
            "krško", "Krško", "SI-054", 26078
            "kungota", "Kungota", "SI-055", 4796
            "kuzma", "Kuzma", "SI-056", 1581
            "laško", "Laško", "SI-057", 13085
            "lenart", "Lenart", "SI-058", 8532
            "lendava", "Lendava/Lendva", "SI-059", 10474
            "litija", "Litija", "SI-060", 15593
            "ljubljana", "Ljubljana", "SI-061", 294054
            "ljubno", "Ljubno", "SI-062", 2564
            "ljutomer", "Ljutomer", "SI-063", 11233
            "log-dragomer", "Log - Dragomer", "SI-208", 3642
            "logatec", "Logatec", "SI-064", 14514
            "loška_dolina", "Loška dolina", "SI-065", 3770
            "loški_potok", "Loški Potok", "SI-066", 1814
            "lovrenc_na_pohorju", "Lovrenc na Pohorju", "SI-167", 2982
            "luče", "Luče", "SI-067", 1444
            "lukovica", "Lukovica", "SI-068", 5907
            "majšperk", "Majšperk", "SI-069", 4042
            "makole", "Makole", "SI-198", 2042
            "maribor", "Maribor", "SI-070", 112395
            "markovci", "Markovci", "SI-168", 4019
            "medvode", "Medvode", "SI-071", 16781
            "mengeš", "Mengeš", "SI-072", 8402
            "metlika", "Metlika", "SI-073", 8458
            "mežica", "Mežica", "SI-074", 3596
            "miklavž_na_dravskem_polju", "Miklavž na Dravskem polju", "SI-169", 6980
            "miren-kostanjevica", "Miren - Kostanjevica", "SI-075", 4976
            "mirna", "Mirna", "SI-212", 2689
            "mirna_peč", "Mirna Peč", "SI-170", 3018
            "mislinja", "Mislinja", "SI-076", 4567
            "mokronog-trebelno", "Mokronog - Trebelno", "SI-199", 3138
            "moravče", "Moravče", "SI-077", 5479
            "moravske_toplice", "Moravske Toplice", "SI-078", 5879
            "mozirje", "Mozirje", "SI-079", 4216
            "murska_sobota", "Murska Sobota", "SI-080", 18684
            "muta", "Muta", "SI-081", 3415
            "naklo", "Naklo", "SI-082", 5417
            "nazarje", "Nazarje", "SI-083", 2650
            "nova_gorica", "Nova Gorica", "SI-084", 31881
            "novo_mesto", "Novo mesto", "SI-085", 37430
            "odranci", "Odranci", "SI-086", 1630
            "oplotnica", "Oplotnica", "SI-171", 4146
            "ormož", "Ormož", "SI-087", 11916
            "osilnica", "Osilnica", "SI-088", 347
            "pesnica", "Pesnica", "SI-089", 7457
            "piran", "Piran/Pirano", "SI-090", 18079
            "pivka", "Pivka", "SI-091", 6196
            "podčetrtek", "Podčetrtek", "SI-092", 3529
            "podlehnik", "Podlehnik", "SI-172", 1825
            "podvelka", "Podvelka", "SI-093", 2354
            "poljčane", "Poljčane", "SI-200", 4469
            "polzela", "Polzela", "SI-173", 6346
            "postojna", "Postojna", "SI-094", 16518
            "prebold", "Prebold", "SI-174", 5155
            "preddvor", "Preddvor", "SI-095", 3776
            "prevalje", "Prevalje", "SI-175", 6829
            "ptuj", "Ptuj", "SI-096", 23636
            "puconci", "Puconci", "SI-097", 5861
            "rače-fram", "Rače - Fram", "SI-098", 7549
            "radeče", "Radeče", "SI-099", 4142
            "radenci", "Radenci", "SI-100", 5132
            "radlje_ob_dravi", "Radlje ob Dravi", "SI-101", 6178
            "radovljica", "Radovljica", "SI-102", 19034
            "ravne_na_koroškem", "Ravne na Koroškem", "SI-103", 11343
            "razkrižje", "Razkrižje", "SI-176", 1296
            "rečica_ob_savinji", "Rečica ob Savinji", "SI-209", 2369
            "renče-vogrsko", "Renče - Vogrsko", "SI-201", 4361
            "ribnica", "Ribnica", "SI-104", 9619
            "ribnica_na_pohorju", "Ribnica na Pohorju", "SI-177", 1127
            "rogaška_slatina", "Rogaška Slatina", "SI-106", 11210
            "rogašovci", "Rogašovci", "SI-105", 3046
            "rogatec", "Rogatec", "SI-107", 3105
            "ruše", "Ruše", "SI-108", 7002
            "selnica_ob_dravi", "Selnica ob Dravi", "SI-178", 4579
            "semič", "Semič", "SI-109", 3885
            "sevnica", "Sevnica", "SI-110", 17729
            "sežana", "Sežana", "SI-111", 13702
            "slovenj_gradec", "Slovenj Gradec", "SI-112", 16609
            "slovenska_bistrica", "Slovenska Bistrica", "SI-113", 25890
            "slovenske_konjice", "Slovenske Konjice", "SI-114", 15161
            "sodražica", "Sodražica", "SI-179", 2215
            "solčava", "Solčava", "SI-180", 524
            "središče_ob_dravi", "Središče ob Dravi", "SI-202", 1921
            "starše", "Starše", "SI-115", 4034
            "straža", "Straža", "SI-203", 3910
            "sveta_ana", "Sveta Ana", "SI-181", 2298
            "sveta_trojica_v_slovenskih_goricah", "Sveta Trojica v Slov. goricah", "SI-204", 2161
            "sveti_andraž_v_slovenskih_goricah", "Sveti Andraž v Slov. goricah", "SI-182", 1184
            "sveti_jurij_ob_ščavnici", "Sveti Jurij ob Ščavnici", "SI-116", 2789
            "sveti_jurij_v_slovenskih_goricah", "Sveti Jurij v Slov. goricah", "SI-210", 2096
            "sveti_tomaž", "Sveti Tomaž", "SI-205", 2007
            "šalovci", "Šalovci", "SI-033", 1381
            "šempeter-vrtojba", "Šempeter - Vrtojba", "SI-183", 6245
            "šenčur", "Šenčur", "SI-117", 8797
            "šentilj", "Šentilj", "SI-118", 8324
            "šentjernej", "Šentjernej", "SI-119", 7216
            "šentjur", "Šentjur", "SI-120", 19333
            "šentrupert", "Šentrupert", "SI-211", 2983
            "škocjan", "Škocjan", "SI-121", 3410
            "škofja_loka", "Škofja Loka", "SI-122", 23336
            "škofljica", "Škofljica", "SI-123", 11692
            "šmarje_pri_jelšah", "Šmarje pri Jelšah", "SI-124", 10243
            "šmarješke_toplice", "Šmarješke Toplice", "SI-206", 3507
            "šmartno_ob_paki", "Šmartno ob Paki", "SI-125", 3291
            "šmartno_pri_litiji", "Šmartno pri Litiji", "SI-194", 5672
            "šoštanj", "Šoštanj", "SI-126", 8870
            "štore", "Štore", "SI-127", 4511
            "tabor", "Tabor", "SI-184", 1667
            "tišina", "Tišina", "SI-010", 3953
            "tolmin", "Tolmin", "SI-128", 10997
            "trbovlje", "Trbovlje", "SI-129", 16014
            "trebnje", "Trebnje", "SI-130", 13262
            "trnovska_vas", "Trnovska vas", "SI-185", 1368
            "trzin", "Trzin", "SI-186", 3929
            "tržič", "Tržič", "SI-131", 15003
            "turnišče", "Turnišče", "SI-132", 3193
            "velenje", "Velenje", "SI-133", 33638
            "velika_polana", "Velika Polana", "SI-187", 1391
            "velike_lašče", "Velike Lašče", "SI-134", 4452
            "veržej", "Veržej", "SI-188", 1344
            "videm", "Videm", "SI-135", 5680
            "vipava", "Vipava", "SI-136", 5763
            "vitanje", "Vitanje", "SI-137", 2277
            "vodice", "Vodice", "SI-138", 4956
            "vojnik", "Vojnik", "SI-139", 8980
            "vransko", "Vransko", "SI-189", 2642
            "vrhnika", "Vrhnika", "SI-140", 17655
            "vuzenica", "Vuzenica", "SI-141", 2655
            "zagorje_ob_savi", "Zagorje ob Savi", "SI-142", 16402
            "zavrč", "Zavrč", "SI-143", 1485
            "zreče", "Zreče", "SI-144", 6586
            "žalec", "Žalec", "SI-190", 21557
            "železniki", "Železniki", "SI-146", 6686
            "žetale", "Žetale", "SI-191", 1311
            "žiri", "Žiri", "SI-147", 4952
            "žirovnica", "Žirovnica", "SI-192", 4425
            "žužemberk", "Žužemberk", "SI-193", 4694        ]
        |> List.map (fun (key, name, code, population) -> key,  { Key = key ; Name = name ; Code = code ; Population = population})
        |> Map.ofList

    type WastewaterTreatmentPlant = {
        Key: string
        Name: string
        Color: string
        Municipalities: (string*string)[]
    }
    let wastewaterTreatmentPlants =
        [
            "ljubljana", "OČN Ljubljana", "#457844", [|"lj", "ljubljana"|]
            "domzale", "Domžale - Kamnik", "#10829a", [|"lj", "domžale"; "lj", "kamnik"; "lj", "mengeš"; "lj", "trzin"; "lj", "komenda"; "kr", "cerklje_na_gorenjskem"|]
            "celje", "Celje", "#665191", [|"ce", "celje"; "ce", "štore"; "ce", "žalec"|] // Žalec: only Levec, part of Žalec
            "velenje", "OČN Šaleške doline", "#777c29", [|"ce", "velenje"; "ce", "šoštanj"|]
            "koper", "Koper", "#70a471", [|"kp", "koper"; "kp", "izola"; "kp", "ankaran"|]
            "kranj", "Kranj", "#ffa600", [|"kr", "kranj"; "kr", "naklo"; "kr", "šenčur"|]
            "maribor", "Maribor", "#f95d6a", [|"mb", "maribor"; "mb", "miklavž_na_dravskem_polju"; "mb", "duplek"; "mb", "hoče-slivnica"|]]
        |> List.map(fun (key, name, color, municipalities_) -> key, {
            Key = key
            Name = name
            Color = color
            Municipalities = municipalities_
        }) |> Map.ofList
    type School = {
        Key : string
        KeyMain : string
        Type : string
        Region : string
        Name : string
    }

    let schoolsList =
        [|
            "121","121","OS","ng","Osnovna šola Col"
            "645","121","OS","ng","Osnovna šola Col Podružnica Podkraj"
            "118","118","OS","ng","Osnovna šola Danila Lokarja Ajdovščina"
            "703","118","OS","ng","Osnovna šola Danila Lokarja Ajdovščina Podružnica Lokavec"
            "115","115","OS","ng","Osnovna šola Dobravlje"
            "658","115","OS","ng","Osnovna šola Dobravlje Podružnica Črniče"
            "666","115","OS","ng","Osnovna šola Dobravlje Podružnica Skrilje"
            "657","115","OS","ng","Osnovna šola Dobravlje Podružnica Vipavski Križ"
            "664","115","OS","ng","Osnovna šola Dobravlje Podružnica Vrtovin"
            "117","117","OS","ng","Osnovna šola Otlica"
            "10609","10609","OS","ng","Osnovna šola Šturje, Ajdovščina"
            "10849","10609","OS","ng","Osnovna šola Šturje, Ajdovščina, Podružnica Budanje"
            "22415","22415","OS","kp","Osnovna šola in vrtec Ankaran"
            "177","177","OS","ms","Osnovna šola in vrtec Apače"
            "9856","177","OS","ms","Osnovna šola in vrtec Apače, Podružnica Stogovci"
            "726","177","OS","ms","Osnovna šola Bakovci, Podružnica Dokležovje"
            "351","351","OS","ms","Osnovna šola Beltinci"
            "249","249","OS","mb","Osnovna šola Benedikt"
            "502","502","OS","kk","Osnovna šola Bistrica ob Sotli"
            "436","436","OS","kr","Osnovna šola prof. dr. Josipa Plemlja Bled"
            "883","436","OS","kr","Osnovna šola prof. dr. Josipa Plemlja Bled, Podružnična šola Bohinjska Bela"
            "884","436","OS","kr","Osnovna šola prof. dr. Josipa Plemlja Bled, Podružnična šola Ribno"
            "146","146","OS","po","Osnovna šola Toneta Šraja Aljoše"
            "437","437","OS","kr","Osnovna šola dr. Janeza Mencingerja Bohinjska Bistrica"
            "885","437","OS","kr","Osnovna šola dr. Janeza Mencingerja Boh. Bistrica, Podružnična šola Srednja vas"
            "535","535","OS","lj","Osnovna šola dr. Ivana Korošca Borovnica"
            "510","510","OS","ng","Osnovna šola Bovec"
            "847","510","OS","ng","Osnovna šola Bovec, Podružnična šola Soča"
            "848","510","OS","ng","Osnovna šola Bovec, Podružnična šola Žaga"
            "552","552","OS","ce","Osnovna šola Braslovče"
            "876","552","OS","ce","Osnovna šola Braslovče, Podružnična osnovna šola Gomilsko"
            "877","552","OS","ce","Osnovna šola Braslovče, Podružnična osnovna šola Letuš"
            "1377","552","OS","ce","Osnovna šola Braslovče, Podružnična osnovna šola Trnava"
            "362","362","OS","ng","Osnovna šola Alojza Gradnika Dobrovo"
            "971","362","OS","ng","Osnovna šola Alojza Gradnika Dobrovo Podružnica Kojsko"
            "322","322","OS","lj","Osnovna šola Brezovica pri Ljubljani"
            "961","322","OS","lj","Osnovna šola Brezovica pri Ljubljani, Podružnica Notranje gorice"
            "323","323","OS","lj","Osnovna šola Preserje"
            "956","323","OS","lj","Osnovna šola Preserje Podružnica Jezero"
            "957","323","OS","lj","Osnovna šola Preserje Podružnica Rakitna"
            "124","124","OS","kk","Osnovna šola Artiče"
            "601","601","OS","kk","Osnovna šola Bizeljsko"
            "122","122","OS","kk","Osnovna šola Brežice"
            "129","129","OS","kk","Osnovna šola Cerklje ob Krki"
            "127","127","OS","kk","Osnovna šola dr. Jožeta Toporišiča Dobova"
            "899","127","OS","kk","Osnovna šola dr. Jožeta Toporišiča Dobova, Podružnična šola Kapele"
            "125","125","OS","kk","Osnovna šola Globoko"
            "126","126","OS","kk","Osnovna šola Maksa Pleteršnika Pišece"
            "128","128","OS","kk","Osnovna šola Velika Dolina"
            "353","353","OS","ms","Osnovna šola Cankova"
            "134","134","OS","ce","I. osnovna šola Celje"
            "135","135","OS","ce","II. osnovna šola Celje"
            "136","136","OS","ce","III. osnovna šola Celje"
            "133","133","OS","ce","IV. osnovna šola Celje"
            "132","132","OS","ce","Osnovna šola Frana Kranjca"
            "137","137","OS","ce","Osnovna šola Frana Roša Celje"
            "130","130","OS","ce","Osnovna šola Hudinja"
            "131","131","OS","ce","Osnovna šola Lava Celje"
            "1341","1341","OS","ce","Osnovna šola Ljubečna"
            "651","1341","OS","ce","Osnovna šola Vojnik Podružnica Šmartno v Rožni dolini"
            "232","232","OS","kr","Osnovna šola Davorina Jenka Cerklje na Gorenjskem"
            "3561","232","OS","kr","Osnovna šola Davorina Jenka Cerklje na Gorenjskem, Podružnica Zalog"
            "145","145","OS","po","Osnovna šola Jožeta Krajca Rakek"
            "652","145","OS","po","Osnovna šola Jožeta Krajca Rakek Podružnica Rudolfa Maistra Unec"
            "144","144","OS","po","Osnovna šola Notranjski odred Cerknica"
            "667","144","OS","po","Osnovna šola Notranjski odred Cerknica, Podružnična šola Maksim Gaspari Begunje pri Cerknici"
            "668","144","OS","po","Osnovna šola Notranjski odred Cerknica, Podružnična šola 11.maj Grahovo"
            "189","189","OS","ng","Osnovna šola Cerkno"
            "895","189","OS","ng","Osnovna šola Cerkno, Podružnična šola Šebrelje"
            "251","251","OS","mb","Osnovna šola Cerkvenjak - Vitomarci"
            "418","418","OS","mb","Osnovna šola Cirkulane - Zavrč"
            "260","260","OS","ms","Osnovna šola Franceta Prešerna Črenšovci"
            "1351","1351","OS","ms","Osnovna šola Prežihovega Voranca Bistrica"
            "444","444","OS","sg","Osnovna šola Črna na Koroškem"
            "779","444","OS","sg","Osnovna šola Črna na Koroškem Podružnica Žerjav"
            "155","155","OS","nm","Osnovna šola Komandanta Staneta Dragatuš"
            "151","151","OS","nm","Osnovna šola Loka Črnomelj"
            "654","151","OS","nm","Osnovna šola Loka Črnomelj, Podružnična šola Adlešiči"
            "653","151","OS","nm","Osnovna šola Loka Črnomelj, Podružnična šola Griblje"
            "150","150","OS","nm","Osnovna šola Mirana Jarca Črnomelj"
            "154","154","OS","nm","Osnovna šola Stari trg ob Kolpi"
            "156","156","OS","nm","Osnovna šola Vinica"
            "414","414","OS","mb","Osnovna šola Destrnik - Trnovska vas"
            "458","458","OS","kp","Osnovna šola dr. Bogomirja Magajne Divača"
            "803","458","OS","kp","Osnovna šola dr. Bogomirja Magajne Divača, Podružnična šola Senožeče"
            "804","458","OS","kp","Osnovna šola dr. Bogomirja Magajne Divača, Podružnična šola Vreme"
            "598","598","OS","ce","Osnovna šola Dobje"
            "180","180","OS","lj","Osnovna šola Dobrepolje"
            "680","180","OS","lj","Osnovna šola Dobrepolje, Podružnična šola Kompolje"
            "1237","180","OS","lj","Osnovna šola Dobrepolje, Podružnična šola Struge"
            "140","140","OS","ce","Osnovna šola Dobrna"
            "326","326","OS","lj","Osnovna šola Dobrova"
            "325","325","OS","lj","Osnovna šola Polhov Gradec"
            "954","325","OS","lj","Osnovna šola Polhov Gradec Podružnica Črni vrh"
            "955","325","OS","lj","Osnovna šola Polhov Gradec Podružnica Šentjošt"
            "257","257","OS","ms","Dvojezična osnovna šola Dobrovnik"
            "273","273","OS","lj","Osnovna šola Janka Modra Dol pri Ljubljani"
            "1272","273","OS","lj","Osnovna šola Janka Modra Dol pri Ljubljani Podružnica Dolsko"
            "3708","273","OS","lj","Osnovna šola Janka Modra Dol pri Ljubljani Podružnica Senožeti"
            "385","385","OS","nm","Osnovna šola Dolenjske Toplice"
            "163","163","OS","lj","Osnovna šola Dob"
            "644","163","OS","lj","Osnovna šola Dob Podružnica Krtina"
            "158","158","OS","lj","Osnovna šola Domžale"
            "674","158","OS","lj","Osnovna šola Domžale, Podružnična šola Ihan"
            "10549","10549","OS","lj","Osnovna šola Dragomelj"
            "166","166","OS","lj","Osnovna šola Preserje pri Radomljah"
            "159","159","OS","lj","Osnovna šola Rodica"
            "160","160","OS","lj","Osnovna šola Venclja Perka"
            "413","413","OS","mb","Osnovna šola dr. Franja Žgeča Dornava"
            "168","168","OS","sg","Osnovna šola Neznanih talcev Dravograd"
            "928","168","OS","sg","Osnovna šola Neznanih talcev Dravograd Podružnica Črneče"
            "929","168","OS","sg","Osnovna šola Neznanih talcev Dravograd Podružnica Libeliče"
            "930","168","OS","sg","Osnovna šola Neznanih talcev Dravograd Podružnica Ojstrica"
            "931","168","OS","sg","Osnovna šola Neznanih talcev Dravograd Podružnica Trbonje"
            "1236","1236","OS","sg","Osnovna šola Šentjanž pri Dravogradu"
            "573","573","OS","mb","Osnovna šola Duplek"
            "942","573","OS","mb","Osnovna šola Duplek Podružnica Dvorjane"
            "941","573","OS","mb","Osnovna šola Duplek Podružnica Zg. Duplek"
            "574","574","OS","mb","Osnovna šola Korena"
            "488","488","OS","kr","Osnovna šola Ivana Tavčarja Gorenja vas"
            "827","488","OS","kr","Osnovna šola Ivana Tavčarja Gorenja vas, Podružnična šola Lučine"
            "828","488","OS","kr","Osnovna šola Ivana Tavčarja Gorenja vas, Podružnična šola Sovodenj"
            "1315","1315","OS","kr","Osnovna šola Poljane"
            "1316","1315","OS","kr","Osnovna šola Poljane, Podružnična šola Javorje"
            "416","416","OS","mb","Osnovna šola Gorišnica"
            "434","434","OS","kr","Osnovna šola Gorje"
            "170","170","OS","ms","Osnovna šola dr. Antona Trstenjaka Negova"
            "171","171","OS","ms","Osnovna šola Gornja Radgona"
            "342","342","OS","ce","Osnovna šola Frana Kocbeka Gornji Grad"
            "9590","342","OS","ce","Osnovna šola Frana Kocbeka Gornji grad, Podružnica Bočna"
            "717","342","OS","ce","Osnovna šola Frana Kocbeka Gornji grad, Podružnica Nova Štifta"
            "346","346","OS","ms","Osnovna šola Gornji Petrovci"
            "356","356","OS","ms","Osnovna šola Grad"
            "1307","1307","OS","lj","Osnovna šola Brinje Grosuplje"
            "1308","1307","OS","lj","Osnovna šola Brinje Grosuplje, Podružnica Polica"
            "178","178","OS","lj","Osnovna šola Louisa Adamiča Grosuplje"
            "1036","178","OS","lj","Osnovna šola Louisa Adamiča Grosuplje Podružnica Kopanj"
            "1037","178","OS","lj","Osnovna šola Louisa Adamiča Grosuplje Podružnica Št.Jurij"
            "1038","178","OS","lj","Osnovna šola Louisa Adamiča Grosuplje Podružnica Žalna"
            "23155","23155","OS","lj","Osnovna šola Šmarje - Sap"
            "421","421","OS","mb","Osnovna šola Hajdina"
            "575","575","OS","mb","Osnovna šola Dušana Flisa Hoče"
            "938","575","OS","mb","Osnovna šola Dušana Flisa Hoče, Podružnična šola Reka-Pohorje"
            "577","577","OS","mb","Osnovna šola Franca Lešnika - Vuka Slivnica pri Mariboru"
            "722","577","OS","ms","Dvojezična osnovna šola Prosenjakovci Podružnica Hodoš"
            "324","324","OS","lj","Osnovna šola Horjul"
            "181","181","OS","za","Osnovna šola narodnega heroja Rajka Hrastnik"
            "3548","181","OS","za","Osnovna šola narodnega heroja Rajka Hrastnik, Podružnična šola Dol pri Hrastniku"
            "461","461","OS","kp","Osnovna šola Dragomirja Benčiča - Brkina Hrpelje"
            "184","184","OS","ng","Osnovna šola Črni Vrh"
            "185","185","OS","ng","Osnovna šola Idrija"
            "890","185","OS","ng","Osnovna šola Idrija Podružnica Godovič"
            "891","185","OS","ng","Osnovna šola Idrija, Podružnična šola Zavratec"
            "188","188","OS","ng","Osnovna šola Spodnja Idrija"
            "896","188","OS","ng","Osnovna šola Spodnja Idrija, Podružnična šola Ledine"
            "320","320","OS","lj","Osnovna šola Ig"
            "951","320","OS","lj","Osnovna šola Ig, Podružnica Golo"
            "952","320","OS","lj","Osnovna šola Ig, Podružnica Iška vas"
            "953","320","OS","lj","Osnovna šola Ig, Podružnica Tomišelj"
            "599","599","OS","po","Osnovna šola Antona Žnideršiča Ilirska Bistrica"
            "600","600","OS","po","Osnovna šola Dragotina Ketteja Ilirska Bistrica"
            "195","195","OS","po","Osnovna šola Jelšane"
            "192","192","OS","po","Osnovna šola Podgora Kuteževo"
            "190","190","OS","po","Osnovna šola Rudija Mahniča - Brkinca Pregarje"
            "191","191","OS","po","Osnovna šola Rudolfa Ukoviča Podgrad"
            "194","194","OS","po","Osnovna šola Toneta Tomšiča Knežak"
            "179","179","OS","lj","Osnovna šola Ferda Vesela Šentvid pri Stični"
            "1282","179","OS","lj","Osnovna šola Ferda Vesela Podružnica v Centru za zdravljenje bolezni otrok"
            "683","179","OS","lj","Osnovna šola Ferda Vesela Šentvid pri Stični Podružnica Temenica"
            "602","602","OS","lj","Osnovna šola Stična"
            "676","602","OS","lj","Osnovna šola Stična, Podružnična šola Ambrus"
            "677","602","OS","lj","Osnovna šola Stična Podružnična šola Krka"
            "678","602","OS","lj","Osnovna šola Stična Podružnična šola Muljava"
            "1215","602","OS","lj","Osnovna šola Stična, Podružnična šola Stična"
            "675","602","OS","lj","Osnovna šola Stična Podružnična šola Višnja Gora"
            "679","602","OS","lj","Osnovna šola Stična, Podružnična šola Zagradec"
            "196","196","OS","kp","Osnovna šola Dante Alighieri Izola"
            "1053","1053","OS","kp","Osnovna šola Livade - Izola"
            "197","197","OS","kp","Osnovna šola Vojke Šmuc Izola"
            "906","197","OS","kp","Osnovna šola Vojke Šmuc Izola, Podružnična šola Korte"
            "1344","1344","OS","kr","Osnovna šola Koroška Bela Jesenice"
            "16570","1344","OS","kr","Osnovna šola Koroška Bela Jesenice Podružnica Blejska Dobrava"
            "1206","1206","OS","kr","Osnovna šola Prežihovega Voranca Jesenice"
            "1345","1345","OS","kr","Osnovna šola Toneta Čufarja Jesenice"
            "981","1345","OS","kr","Osnovna šola Matije Valjavca Preddvor Podružnica Jezersko"
            "415","415","OS","mb","Osnovna šola Juršinci"
            "200","200","OS","lj","Osnovna šola Frana Albrehta Kamnik"
            "913","200","OS","lj","Osnovna šola Frana Albrehta Kamnik Podružnica Mekinje"
            "914","200","OS","lj","Osnovna šola Frana Albrehta Kamnik Podružnica Nevlje"
            "915","200","OS","lj","Osnovna šola Frana Albrehta Kamnik Podružnica Tunjice"
            "916","200","OS","lj","Osnovna šola Frana Albrehta Kamnik Podružnica Vranja peč"
            "199","199","OS","lj","Osnovna šola Marije Vere"
            "204","204","OS","lj","Osnovna šola Stranje"
            "907","204","OS","lj","Osnovna šola Stranje Podružnica Gozd"
            "1259","1259","OS","lj","Osnovna šola Šmartno v Tuhinju"
            "3701","1259","OS","lj","Osnovna šola Šmartno v Tuhinju Podružnica Motnik"
            "3702","1259","OS","lj","Osnovna šola Šmartno v Tuhinju Podružnica Sela"
            "3703","1259","OS","lj","Osnovna šola Šmartno v Tuhinju Podružnica Zgornji Tuhinj"
            "201","201","OS","lj","Osnovna šola Toma Brejca Kamnik"
            "361","361","OS","ng","Osnovna šola Deskle"
            "363","363","OS","ng","Osnovna šola Kanal"
            "918","363","OS","ng","Osnovna šola Kanal Podružnica Kal"
            "424","424","OS","mb","Osnovna šola Cirkovce"
            "423","423","OS","mb","Osnovna šola Kidričevo"
            "766","423","OS","mb","Osnovna šola Kidričevo, Podružnica Lovrenc na Dravskem polju"
            "509","509","OS","ng","Osnovna šola Simona Gregorčiča Kobarid"
            "839","509","OS","ng","Osnovna šola Simona Gregorčiča Kobarid, Podružnica Breginj"
            "841","509","OS","ng","Osnovna šola Simona Gregorčiča Kobarid, Podružnica Drežnica"
            "842","509","OS","ng","Osnovna šola Simona Gregorčiča Kobarid, Podružnica Smast"
            "256","256","OS","ms","Osnovna šola Kobilje"
            "612","612","OS","nm","Osnovna šola Ob Rinži Kočevje"
            "974","612","OS","nm","Osnovna šola Ob Rinži Kočevje Podružnica Kočevska Reka"
            "975","612","OS","nm","Osnovna šola Ob Rinži Kočevje Podružnica Livold"
            "613","613","OS","nm","Osnovna šola Stara Cerkev"
            "973","613","OS","nm","Osnovna šola Stara Cerkev, Podružnična šola Željne"
            "205","205","OS","nm","Osnovna šola Zbora odposlancev Kočevje"
            "460","460","OS","kp","Osnovna šola Antona Šibelja - Stjenka Komen"
            "3790","460","OS","kp","Osnovna šola Antona Šibelja - Stjenka Komen, Podružnica Štanjel"
            "198","198","OS","lj","Osnovna šola Komenda Moste"
            "3545","198","OS","lj","Osnovna šola Komenda Moste Podružnica Moste"
            "209","209","OS","kp","Osnovna šola Antona Ukmarja Koper"
            "216","216","OS","kp","Osnovna šola Dekani"
            "220","220","OS","kp","Osnovna šola dr. Aleš Bebler - Primož Hrvatini"
            "213","213","OS","kp","Osnovna šola Dušana Bordona Semedela - Koper"
            "210","210","OS","kp","Osnovna šola Elvire Vatovec Prade - Koper"
            "687","210","OS","kp","Osnovna šola Elvire Vatovec Prade - Koper, Podružnica Sv. Anton"
            "217","217","OS","kp","Osnovna šola Istrskega odreda Gračišče"
            "218","218","OS","kp","Osnovna šola Ivana Babiča - Jagra Marezige"
            "10550","10550","OS","kp","Osnovna šola Koper"
            "221","221","OS","kp","Osnovna šola Oskarja Kovačiča Škofije"
            "214","214","OS","kp","Osnovna šola Pier Paolo Vergerio il Vecchio Koper"
            "685","214","OS","kp","Osnovna šola Pier Paolo Vergerio il Vecchio Koper, Podružnična šola Bertoki"
            "686","214","OS","kp","Osnovna šola Pier Paolo Vergerio il Vecchio Koper, Podružnična šola Hrvatini"
            "3509","214","OS","kp","Osnovna šola Pier Paolo Vergerio il Vecchio Koper, Podružnična šola Semedela"
            "219","219","OS","kp","Osnovna šola Šmarje pri Kopru"
            "242","242","OS","kk","Osnovna šola Jožeta Gorjupa Kostanjevica na Krki"
            "208","208","OS","nm","Osnovna šola Fara"
            "503","503","OS","ce","Osnovna šola Kozje"
            "504","504","OS","ce","Osnovna šola Lesično"
            "222","222","OS","kr","Osnovna šola Franceta Prešerna Kranj"
            "992","222","OS","kr","Osnovna šola Franceta Prešerna Kranj, Podružnica Kokrica"
            "227","227","OS","kr","Osnovna šola Jakoba Aljaža Kranj"
            "223","223","OS","kr","Osnovna šola Matije Čopa Kranj"
            "1370","1370","OS","kr","Osnovna šola Orehek Kranj"
            "1371","1370","OS","kr","Osnovna šola Orehek Kranj, Podružnična šola Mavčiče"
            "228","228","OS","kr","Osnovna šola Predoslje Kranj"
            "224","224","OS","kr","Osnovna šola Simona Jenka Kranj"
            "977","224","OS","kr","Osnovna šola Simona Jenka Kranj Podružnica Center"
            "978","224","OS","kr","Osnovna šola Simona Jenka Kranj Podružnica Goriče"
            "976","224","OS","kr","Osnovna šola Simona Jenka Kranj Podružnica Primskovo"
            "1358","224","OS","kr","Osnovna šola Simona Jenka Kranj Podružnica Trstenik"
            "226","226","OS","kr","Osnovna šola Staneta Žagarja Kranj"
            "225","225","OS","kr","Osnovna šola Stražišče Kranj"
            "987","225","OS","kr","Osnovna šola Stražišče Kranj Podružnica Besnica"
            "990","225","OS","kr","Osnovna šola Stražišče Kranj Podružnica Podblica"
            "991","225","OS","kr","Osnovna šola Stražišče Kranj Podružnica Žabnica"
            "1207","1207","OS","kr","Osnovna šola Josipa Vandota Kranjska Gora"
            "1208","1208","OS","kr","Osnovna šola 16. decembra Mojstrana"
            "332","332","OS","ms","Osnovna šola Križevci"
            "239","239","OS","kk","Osnovna šola Adama Bohoriča Brestanica"
            "234","234","OS","kk","Osnovna šola Jurija Dalmatina Krško"
            "241","241","OS","kk","Osnovna šola Koprivnica"
            "237","237","OS","kk","Osnovna šola Leskovec pri Krškem"
            "688","237","OS","kk","Osnovna šola Leskovec pri Krškem, Podružnica Veliki Podlog"
            "243","243","OS","kk","Osnovna šola Podbočje"
            "238","238","OS","kk","Osnovna šola Raka"
            "240","240","OS","kk","Osnovna šola XIV. divizije Senovo"
            "582","582","OS","mb","Osnovna šola Kungota"
            "904","582","OS","mb","Osnovna šola Kungota, Podružnična šola Spodnja Kungota"
            "905","582","OS","mb","Osnovna šola Kungota, Podružnična šola Svečina"
            "355","355","OS","ms","Osnovna šola Kuzma"
            "1353","1353","OS","ce","Osnovna šola Antona Aškerca Rimske Toplice"
            "1354","1353","OS","ce","Osnovna šola Antona Aškerca Rimske Toplice Podružnica Jurklošter"
            "1356","1353","OS","ce","Osnovna šola Antona Aškerca Rimske Toplice Podružnica Sedraž"
            "3752","1353","OS","ce","Osnovna šola Antona Aškerca Rimske Toplice Podružnica Zidani Most"
            "491","491","OS","ce","Osnovna šola Primoža Trubarja Laško"
            "9873","491","OS","ce","Osnovna šola Primoža Trubarja Laško Podružnica Debro"
            "690","491","OS","ce","Osnovna šola Primoža Trubarja Laško Podružnica Rečica"
            "689","491","OS","ce","Osnovna šola Primoža Trubarja Laško Podružnica Šentrupert"
            "691","491","OS","ce","Osnovna šola Primoža Trubarja Laško Podružnica Vrh"
            "246","246","OS","mb","Osnovna šola Lenart"
            "247","247","OS","mb","Osnovna šola Voličina"
            "258","258","OS","ms","Dvojezična osnovna šola Genterovci"
            "253","253","OS","ms","Dvojezična osnovna šola I Lendava"
            "701","253","OS","ms","Dvojezična osnovna šola I Lendava, Podružnična šola Gaberje"
            "262","262","OS","za","Osnovna šola Gabrovka - Dole"
            "707","262","OS","za","Osnovna šola Gabrovka - Dole, Podružnična osnovna šola Dole pri Litiji"
            "1027","1027","OS","za","Osnovna šola Gradec"
            "1028","1027","OS","za","Osnovna šola Gradec, Podružnična osnovna šola Hotič"
            "1029","1027","OS","za","Osnovna šola Gradec, Podružnična osnovna šola Jevnica"
            "1030","1027","OS","za","Osnovna šola Gradec, Podružnična osnovna šola Kresnice"
            "3629","1027","OS","za","Osnovna šola Gradec, Podružnična osnovna šola Vače"
            "1346","1346","OS","za","Osnovna šola Litija"
            "1349","1346","OS","za","Osnovna šola Litija, Podružnična osnovna šola Darinke Ribič Polšnik"
            "1348","1346","OS","za","Osnovna šola Litija, Podružnična osnovna šola Sava"
            "23135","23135","OS","lj","Inštitut Lila, OE OŠ Lila Ljubljana"
            "17530","17530","OS","lj","Inštitut montessori, Zavod za pomoč staršem pri razvoju otrok"
            "270","270","OS","lj","Osnovna šola Bežigrad"
            "314","314","OS","lj","Osnovna šola Bičevje"
            "605","605","OS","lj","Osnovna šola Božidarja Jakca Ljubljana"
            "945","605","OS","lj","Osnovna šola Božidarja Jakca Ljubljana Podružnica Hrušica"
            "266","266","OS","lj","Osnovna šola Danile Kumar"
            "16431","266","OS","lj","Osnovna šola Danile Kumar Ljubljana, mednarodna šola"
            "268","268","OS","lj","Osnovna šola dr. Vita Kraigherja Ljubljana"
            "303","303","OS","lj","Osnovna šola Dravlje"
            "302","302","OS","lj","Osnovna šola Franca Rozmana - Staneta Ljubljana"
            "269","269","OS","lj","Osnovna šola Franceta Bevka"
            "295","295","OS","lj","Osnovna šola Hinka Smrekarja Ljubljana"
            "284","284","OS","lj","Osnovna šola Jožeta Moškriča"
            "288","288","OS","lj","Osnovna šola Karla Destovnika Kajuha Ljubljana"
            "17510","17510","OS","lj","Osnovna šola Kašelj"
            "289","289","OS","lj","Osnovna šola Ketteja in Murna"
            "316","316","OS","lj","Osnovna šola Kolezija"
            "298","298","OS","lj","Osnovna šola Koseze"
            "112","112","OS","lj","Osnovna šola Ledina"
            "614","614","OS","lj","Osnovna šola Livada"
            "275","275","OS","lj","Osnovna šola Majde Vrhovnik Ljubljana"
            "285","285","OS","lj","Osnovna šola Martina Krpana Ljubljana"
            "271","271","OS","lj","Osnovna šola Milana Šuštaršiča"
            "267","267","OS","lj","Osnovna šola Mirana Jarca"
            "301","301","OS","lj","Osnovna šola Miška Kranjca"
            "272","272","OS","lj","Osnovna šola narodnega heroja Maksa Pečarja"
            "287","287","OS","lj","Osnovna šola Nove Fužine"
            "286","286","OS","lj","Osnovna šola Nove Jarše"
            "315","315","OS","lj","Osnovna šola Oskarja Kovačiča"
            "962","315","OS","lj","Osnovna šola Oskarja Kovačiča, Podružnica Rudnik"
            "279","279","OS","lj","Osnovna šola Poljane Ljubljana"
            "293","293","OS","lj","Osnovna šola Polje"
            "274","274","OS","lj","Osnovna šola Prežihovega Voranca"
            "280","280","OS","lj","Osnovna šola Prule"
            "300","300","OS","lj","Osnovna šola Riharda Jakopiča Ljubljana"
            "265","265","OS","lj","Osnovna šola Savsko naselje"
            "294","294","OS","lj","Osnovna šola Sostro"
            "946","294","OS","lj","Osnovna šola Sostro Podružnica Besnica"
            "947","294","OS","lj","Osnovna šola Sostro Podružnica Janče"
            "949","294","OS","lj","Osnovna šola Sostro Podružnica Lipoglav"
            "950","294","OS","lj","Osnovna šola Sostro Podružnica Prežganje"
            "296","296","OS","lj","Osnovna šola Spodnja Šiška"
            "305","305","OS","lj","Osnovna šola Šentvid"
            "306","306","OS","lj","Osnovna šola Šmartno pod Šmarno goro"
            "277","277","OS","lj","Osnovna šola Toneta Čufarja"
            "312","312","OS","lj","Osnovna šola Trnovo"
            "299","299","OS","lj","Osnovna šola Valentina Vodnika Ljubljana"
            "317","317","OS","lj","Osnovna šola Vič"
            "283","283","OS","lj","Osnovna šola Vide Pregarc Ljubljana"
            "297","297","OS","lj","Osnovna šola Vižmarje - Brod"
            "278","278","OS","lj","Osnovna šola Vodmat"
            "313","313","OS","lj","Osnovna šola Vrhovci"
            "291","291","OS","lj","Osnovna šola Zadobrova"
            "292","292","OS","lj","Osnovna šola Zalog"
            "3767","3767","OS","lj","Waldorfska šola Ljubljana"
            "14850","3767","OS","lj","Zavod sv. Stanislava, Osnovna šola Alojzija Šuštarja Ljubljana"
            "340","340","OS","ce","Osnovna šola Ljubno ob Savinji"
            "330","330","OS","ms","Osnovna šola Ivana Cankarja Ljutomer"
            "709","330","OS","ms","Osnovna šola Ivana Cankarja Ljutomer Podružnica Cven"
            "329","329","OS","ms","Osnovna šola Janka Ribiča Cezanjevci"
            "333","333","OS","ms","Osnovna šola Mala Nedelja"
            "328","328","OS","ms","Osnovna šola Stročja vas"
            "1234","1234","OS","lj","Osnovna šola Rovte"
            "1238","1234","OS","lj","Osnovna šola Rovte Podružnica Vrh Svetih treh kraljev"
            "335","335","OS","lj","Osnovna šola Tabor Logatec"
            "731","335","OS","lj","Osnovna šola Tabor Logatec Podružnica Hotedršica"
            "732","335","OS","lj","Osnovna šola Tabor Logatec Podružnica Rovtarske Žibrše"
            "334","334","OS","lj","Osnovna šola 8 talcev Logatec"
            "730","334","OS","lj","Osnovna šola 8 talcev Logatec Podružnica Laze"
            "534","534","OS","lj","Osnovna šola Log - Dragomer"
            "148","148","OS","po","Osnovna šola heroja Janeza Hribarja Stari trg pri Ložu"
            "705","148","OS","po","Osnovna šola heroja Janeza Hribarja Stari trg pri Ložu, Podružnica Iga vas"
            "448","448","OS","nm","Osnovna šola dr. Antona Debeljaka Loški Potok"
            "1203","1203","OS","mb","Osnovna šola Lovrenc na Pohorju"
            "341","341","OS","ce","Osnovna šola Blaža Arniča Luče"
            "157","157","OS","lj","Osnovna šola Janka Kersnika Brdo"
            "671","157","OS","lj","Osnovna šola Janka Kersnika Brdo, Podružnična šola Blagovica"
            "672","157","OS","lj","Osnovna šola Janka Kersnika Brdo, Podružnična šola Krašnja"
            "422","422","OS","mb","Osnovna šola Majšperk"
            "767","422","OS","mb","Osnovna šola Majšperk Podružnica Ptujska gora"
            "768","422","OS","mb","Osnovna šola Majšperk Podružnica Stoperce"
            "476","476","OS","mb","Osnovna šola Anice Černejeve Makole"
            "560","560","OS","mb","Osnovna šola Angela Besednjaka Maribor"
            "557","557","OS","mb","Osnovna šola Bojana Ilicha Maribor"
            "558","558","OS","mb","Osnovna šola Borcev za severno mejo Maribor"
            "559","559","OS","mb","Osnovna šola bratov Polančičev Maribor"
            "564","564","OS","mb","Osnovna šola Draga Kobala Maribor"
            "934","564","OS","mb","Osnovna šola Draga Kobala Maribor, Podružnična šola Brezje"
            "565","565","OS","mb","Osnovna šola Franca Rozmana - Staneta Maribor"
            "943","565","OS","mb","Osnovna šola Franca Rozmana - Staneta Maribor Podružnična šola Ivana Cankarja Košaki"
            "556","556","OS","mb","Osnovna šola Franceta Prešerna Maribor"
            "933","556","OS","mb","Osnovna šola Franceta Prešerna Maribor, Podružnica Stane Lenardon Razvanje"
            "561","561","OS","mb","Osnovna šola Janka Padežnika Maribor"
            "581","581","OS","mb","Osnovna šola Kamnica"
            "604","604","OS","mb","Osnovna šola Leona Štuklja Maribor"
            "568","568","OS","mb","Osnovna šola Ludvika Pliberška Maribor"
            "563","563","OS","mb","Osnovna šola Maksa Durjave Maribor"
            "554","554","OS","mb","Osnovna šola Malečnik"
            "555","555","OS","mb","Osnovna šola Martina Konšaka Maribor"
            "553","553","OS","mb","Osnovna šola Prežihovega Voranca Maribor"
            "580","580","OS","mb","Osnovna šola Rada Robiča Limbuš"
            "567","567","OS","mb","Osnovna šola Slave Klavore Maribor"
            "603","603","OS","mb","Osnovna šola Tabor I Maribor"
            "562","562","OS","mb","Osnovna šola Toneta Čufarja Maribor"
            "21915","21915","OS","mb","Waldorfska šola Maribor"
            "21875","21875","OS","mb","Zavod Antona Martina Slomška Maribor, Osnovna šola Montessori"
            "417","417","OS","mb","Osnovna šola Markovci"
            "307","307","OS","lj","Osnovna šola Medvode"
            "309","309","OS","lj","Osnovna šola Pirniče"
            "308","308","OS","lj","Osnovna šola Preska"
            "965","308","OS","lj","Osnovna šola Preska, Podružnica Sora"
            "747","308","OS","lj","Osnovna šola Preska, Podružnica Topol"
            "310","310","OS","lj","Osnovna šola Simona Jenka"
            "164","164","OS","lj","Osnovna šola Mengeš"
            "336","336","OS","nm","Osnovna šola Metlika"
            "710","336","OS","nm","Osnovna šola Metlika, Podružnična šola Suhor"
            "338","338","OS","nm","Osnovna šola Podzemelj"
            "443","443","OS","sg","Osnovna šola Mežica"
            "576","576","OS","mb","Osnovna šola Miklavž na Dravskem polju"
            "935","576","OS","mb","Osnovna šola Miklavž na Dravskem polju Podružnica Dobrovce"
            "368","368","OS","ng","Osnovna šola Miren"
            "926","368","OS","ng","Osnovna šola Miren, Podružnica Bilje"
            "927","368","OS","ng","Osnovna šola Miren, Podružnica Kostanjevica"
            "517","517","OS","nm","Osnovna šola Mirna"
            "380","380","OS","nm","Osnovna šola Toneta Pavčka"
            "467","467","OS","sg","Osnovna šola Mislinja"
            "808","467","OS","sg","Osnovna šola Mislinja Podružnica Dolič"
            "515","515","OS","nm","Osnovna šola Mokronog"
            "854","515","OS","nm","Osnovna šola Mokronog Podružnica Trebelno"
            "167","167","OS","lj","Osnovna šola Jurija Vege"
            "673","167","OS","lj","Osnovna šola Jurija Vege, Podružnična šola Vrhpolje"
            "348","348","OS","ms","Dvojezična osnovna šola Prosenjakovci"
            "350","350","OS","ms","Osnovna šola Bogojina"
            "349","349","OS","ms","Osnovna šola Fokovci"
            "339","339","OS","ce","Osnovna šola Mozirje"
            "715","339","OS","ce","Osnovna šola Mozirje, Podružnica Šmihel"
            "343","343","OS","ms","Osnovna šola Bakovci"
            "634","634","OS","ms","Osnovna šola I Murska Sobota"
            "635","635","OS","ms","Osnovna šola II Murska Sobota"
            "781","635","OS","ms","Osnovna šola II Murska Sobota, Podružnična šola Krog"
            "636","636","OS","ms","Osnovna šola III Murska Sobota"
            "24415","636","OS","ms","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Pomurje"
            "429","429","OS","sg","Osnovna šola Muta"
            "1312","1312","OS","kr","Osnovna šola Naklo"
            "1313","1312","OS","kr","Osnovna šola Naklo, Podružnična šola Duplje"
            "1314","1312","OS","kr","Osnovna šola Naklo, Podružnična šola Podbrezje"
            "19790","1312","OS","kr","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Gorenjska, Enota osnovna šola"
            "1374","1374","OS","ce","Osnovna šola Nazarje"
            "1375","1374","OS","ce","Osnovna šola Nazarje Podružnica Šmartno ob Dreti"
            "371","371","OS","ng","Osnovna šola Branik"
            "364","364","OS","ng","Osnovna šola Čepovan"
            "370","370","OS","ng","Osnovna šola Dornberk"
            "925","370","OS","ng","Osnovna šola Dornberk Podružnica Prvačina"
            "357","357","OS","ng","Osnovna šola Frana Erjavca Nova Gorica"
            "358","358","OS","ng","Osnovna šola Milojke Štrukelj Nova Gorica"
            "3547","358","OS","ng","Osnovna šola Milojke Štrukelj Nova Gorica, Podružnična osnovna šola Ledine"
            "359","359","OS","ng","Osnovna šola Solkan"
            "924","359","OS","ng","Osnovna šola Solkan, Podružnica Grgar"
            "923","359","OS","ng","Osnovna šola Solkan, Podružnica Trnovo"
            "365","365","OS","ng","Osnovna šola Šempas"
            "377","377","OS","nm","Osnovna šola Bršljin"
            "383","383","OS","nm","Osnovna šola Brusnice"
            "376","376","OS","nm","Osnovna šola Center Novo mesto"
            "736","376","OS","nm","Osnovna šola Center Novo mesto, Podružnična šola Mali Slatnik"
            "9854","9854","OS","nm","Osnovna šola Drska"
            "374","374","OS","nm","Osnovna šola Grm Novo mesto"
            "372","372","OS","nm","Osnovna šola Otočec"
            "384","384","OS","nm","Osnovna šola Stopiče"
            "739","384","OS","nm","Osnovna šola Stopiče Podružnica Dolž"
            "740","384","OS","nm","Osnovna šola Stopiče Podružnica Podgrad"
            "375","375","OS","nm","Osnovna šola Šmihel"
            "738","375","OS","nm","Osnovna šola Šmihel, Podružnica Birčna vas"
            "261","261","OS","ms","Osnovna šola Odranci"
            "472","472","OS","mb","Osnovna šola Pohorskega bataljona Oplotnica"
            "816","472","OS","mb","Osnovna šola Pohorskega bataljona Oplotnica, Podružnična šola Prihova"
            "1233","1233","OS","mb","Osnovna šola Ivanjkovci"
            "394","394","OS","mb","Osnovna šola Miklavž pri Ormožu"
            "3521","394","OS","mb","Osnovna šola Miklavž pri Ormožu Podružnica Kog"
            "390","390","OS","mb","Osnovna šola Ormož"
            "393","393","OS","mb","Osnovna šola Velika Nedelja"
            "748","393","OS","mb","Osnovna šola Velika Nedelja, Podružnica Podgorci"
            "1026","393","OS","nm","Osnovna šola Fara Podružnica Osilnica"
            "587","587","OS","mb","Osnovna šola Jakobski Dol"
            "586","586","OS","mb","Osnovna šola Jarenina"
            "583","583","OS","mb","Osnovna šola Pesnica"
            "902","583","OS","mb","Osnovna šola Pesnica, Podružnična osnovna šola Pernica"
            "399","399","OS","kp","Osnovna šola Cirila Kosmača Piran"
            "753","399","OS","kp","Osnovna šola Cirila Kosmača Piran, Podružnična šola Portorož"
            "396","396","OS","kp","Osnovna šola Lucija"
            "752","396","OS","kp","Osnovna šola Lucija, Podružnična šola Strunjan"
            "400","400","OS","kp","Osnovna šola Sečovlje"
            "754","400","OS","kp","Osnovna šola Sečovlje, Podružnična šola Sveti Peter"
            "398","398","OS","kp","Osnovna šola Vincenzo e Diego de Castro Piran"
            "749","398","OS","kp","Osnovna šola Vincenzo e Diego de Castro Piran, Podružnična šola Lucija"
            "751","398","OS","kp","Osnovna šola Vincenzo e Diego de Castro Piran, Podružnična šola Sečovlje"
            "404","404","OS","po","Osnovna šola Košana"
            "405","405","OS","po","Osnovna šola Pivka"
            "755","405","OS","po","Osnovna šola Pivka Podružnična šola Šmihel"
            "501","501","OS","ce","Osnovna šola Podčetrtek"
            "785","501","OS","ce","Osnovna šola Podčetrtek, Podružnična osnovna šola Pristava pri Mestinju"
            "420","420","OS","mb","Osnovna šola Podlehnik"
            "427","427","OS","sg","Osnovna šola Brezno - Podvelka"
            "772","427","OS","sg","Osnovna šola Brezno - Podvelka Podružnica Kapla na Kozjaku"
            "770","427","OS","sg","Osnovna šola Brezno - Podvelka Podružnica Lehen na Pohorju"
            "475","475","OS","mb","Osnovna šola Kajetana Koviča Poljčane"
            "551","551","OS","ce","Osnovna šola Polzela"
            "878","551","OS","ce","Osnovna šola Polzela, Podružnična osnovna šola Andraž"
            "402","402","OS","po","Osnovna šola Antona Globočnika Postojna"
            "758","402","OS","po","Osnovna šola Antona Globočnika, Podružnična šola Bukovje"
            "759","402","OS","po","Osnovna šola Antona Globočnika, Podružnična šola Planina"
            "760","402","OS","po","Osnovna šola Antona Globočnika, Podružnična šola Studeno"
            "401","401","OS","po","Osnovna šola Miroslava Vilharja Postojna"
            "757","401","OS","po","Osnovna šola Miroslava Vilharja Postojna Podružnica Hruševje"
            "406","406","OS","po","Osnovna šola Prestranek"
            "550","550","OS","ce","Osnovna šola Prebold"
            "231","231","OS","kr","Osnovna šola Matije Valjavca Preddvor"
            "980","231","OS","kr","Osnovna šola Matije Valjavca Preddvor, Podružnica Kokra"
            "442","442","OS","sg","Osnovna šola Franja Goloba Prevalje"
            "791","442","OS","sg","Osnovna šola Franja Goloba Prevalje, Podružnična šola Holmec"
            "780","442","OS","sg","Osnovna šola Franja Goloba Prevalje, Podružnična šola Leše"
            "792","442","OS","sg","Osnovna šola Franja Goloba Prevalje, Podružnična šola Šentanel"
            "409","409","OS","mb","Osnovna šola Breg"
            "410","410","OS","mb","Osnovna šola Ljudski vrt Ptuj"
            "761","410","OS","mb","Osnovna šola Ljudski vrt Ptuj Podružnica Grajena"
            "407","407","OS","mb","Osnovna šola Mladika Ptuj"
            "408","408","OS","mb","Osnovna šola Olge Meglič"
            "345","345","OS","ms","Osnovna šola Puconci"
            "1379","345","OS","ms","Osnovna šola Puconci, Podružnična šola Bodonci"
            "723","345","OS","ms","Osnovna šola Puconci, Podružnična šola Mačkovci"
            "578","578","OS","mb","Osnovna šola Fram"
            "579","579","OS","mb","Osnovna šola Rače"
            "1342","1342","OS","kk","Javni zavod Osnovna šola Marjana Nemca Radeče"
            "1343","1342","OS","kk","Javni zavod Osnovna šola Marjana Nemca Radeče, Podružnična šola Svibno"
            "175","175","OS","ms","Osnovna šola Kajetana Koviča Radenci"
            "174","174","OS","ms","Osnovna šola Kapela"
            "425","425","OS","sg","Osnovna šola Radlje ob Dravi"
            "3711","425","OS","sg","Osnovna šola Radlje ob Dravi Podružnica Remšnik"
            "774","425","OS","sg","Osnovna šola Radlje ob Dravi Podružnica Vuhred"
            "431","431","OS","kr","Osnovna šola Antona Tomaža Linharta Radovljica"
            "889","431","OS","kr","Osnovna šola Antona Tomaža Linharta Radovljica Podružnica Ljubno"
            "888","431","OS","kr","Osnovna šola Antona Tomaža Linharta Radovljica Podružnica Mošnje"
            "435","435","OS","kr","Osnovna šola F. S. Finžgarja Lesce"
            "9877","435","OS","kr","Osnovna šola F. S. Finžgarja Lesce, Podružnica Begunje"
            "433","433","OS","kr","Osnovna šola Staneta Žagarja Lipnica"
            "886","433","OS","kr","Osnovna šola Staneta Žagarja Lipnica Podružnica Ovsiše"
            "439","439","OS","sg","Osnovna šola Koroški jeklarji Ravne na Koroškem"
            "776","439","OS","sg","Osnovna šola Koroški jeklarji Ravne na Koroškem, Podružnična šola Kotlje"
            "438","438","OS","sg","Osnovna šola Prežihovega Voranca Ravne na Koroškem"
            "327","327","OS","ms","Osnovna šola Janeza Kuharja Razkrižje"
            "6688","6688","OS","ce","Osnovna šola Rečica ob Savinji"
            "920","6688","OS","ng","Osnovna šola Ivana Roba Šempeter pri Gorici Podružnica Vogrsko"
            "369","369","OS","ng","Osnovna šola Lucijana Bratkoviča Bratuša Renče"
            "922","369","OS","ng","Osnovna šola Lucijana Bratkoviča Bratuša Renče, Podružnična osnovna šola Bukovica"
            "22615","369","OS","ng","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Primorska"
            "445","445","OS","nm","Osnovna šola dr. Franceta Prešerna"
            "794","445","OS","nm","Osnovna šola dr. Franceta Prešerna, Podružnica Dolenja vas"
            "795","445","OS","nm","Osnovna šola dr. Franceta Prešerna, Podružnica Sušje"
            "793","445","OS","nm","Osnovna šola dr. Ivana Prijatelja Sodražica, Podružnična šola Sv. Gregor"
            "428","428","OS","sg","Osnovna šola Ribnica na Pohorju"
            "497","497","OS","ce","JVIZ I. Osnovna šola Rogaška Slatina"
            "498","498","OS","ce","VIZ II. OŠ Rogaška Slatina"
            "1240","498","OS","ce","VIZ II. OŠ Rogaška Slatina, Podružnična šola Sveti Florijan"
            "782","498","OS","ce","VIZ II.OŠ Rogaška Slatina, Podružnična šola Kostrivnica"
            "354","354","OS","ms","Osnovna šola Sveti Jurij"
            "724","354","OS","ms","Osnovna šola Sveti Jurij, Podružnica Pertoča"
            "500","500","OS","ce","VIZ Osnovna šola Rogatec"
            "783","500","OS","ce","VIZ Osnovna šola Rogatec Podružnična šola Dobovec"
            "784","500","OS","ce","VIZ Osnovna šola Rogatec Podružnična šola Donačka gora"
            "588","588","OS","mb","Osnovna šola Janka Glazerja Ruše"
            "590","590","OS","mb","Osnovna šola Selnica ob Dravi"
            "901","590","OS","mb","Osnovna šola Selnica ob Dravi, Podružnična šola Gradišče na Kozjaku"
            "900","590","OS","mb","Osnovna šola Selnica ob Dravi, Podružnična šola Sveti Duh na Ostrem vrhu"
            "149","149","OS","nm","Osnovna šola Belokranjskega odreda Semič"
            "669","149","OS","nm","Osnovna šola Belokranjskega odreda Semič, Podružnična šola Štrekljevec"
            "450","450","OS","kk","Osnovna šola Blanca"
            "449","449","OS","kk","Osnovna šola Boštanj"
            "454","454","OS","kk","Osnovna šola Krmelj"
            "455","455","OS","kk","Osnovna šola Milan Majcen Šentjanž"
            "451","451","OS","kk","Osnovna šola Sava Kladnika Sevnica"
            "797","451","OS","kk","Osnovna šola Sava Kladnika Podružnica Loka"
            "798","451","OS","kk","Osnovna šola Sava Kladnika Podružnica Studenec"
            "453","453","OS","kk","Osnovna šola Tržišče"
            "459","459","OS","kp","Osnovna šola Dutovlje"
            "807","459","OS","kp","Osnovna šola Dutovlje Podružnica Tomaj"
            "456","456","OS","kp","Osnovna šola Srečka Kosovela Sežana"
            "3531","456","OS","kp","Osnovna šola Srečka Kosovela Sežana, Podružnična šola Lokev"
            "969","969","OS","sg","Druga osnovna šola Slovenj Gradec"
            "811","969","OS","sg","Druga osnovna šola Slovenj Gradec Podružnična osnovna šola Pameče-Troblje"
            "466","466","OS","sg","Osnovna šola Podgorje pri Slovenj Gradcu"
            "814","466","OS","sg","Osnovna šola Podgorje pri Slovenj Gradcu, Podružnica Razbor"
            "813","466","OS","sg","Osnovna šola Podgorje pri Slovenj Gradcu, Podružnica Šmiklavž"
            "462","462","OS","sg","Osnovna šola Šmartno pri Slovenj Gradcu"
            "463","463","OS","sg","Prva osnovna šola Slovenj Gradec"
            "812","463","OS","sg","Prva osnovna šola Slovenj Gradec, Podružnica Sele-Vrhe"
            "477","477","OS","mb","Osnovna šola Antona Ingoliča Spodnja Polskava"
            "817","477","OS","mb","Osnovna šola Antona Ingoliča Spodnja Polskava, Podružnična šola Pragersko"
            "818","477","OS","mb","Osnovna šola Antona Ingoliča Spodnja Polskava, Podružnična šola Zgornja Polskava"
            "474","474","OS","mb","Osnovna šola dr. Jožeta Pučnika Črešnjevec"
            "473","473","OS","mb","Osnovna šola Gustava Šiliha Laporje"
            "471","471","OS","mb","Osnovna šola Partizanska bolnišnica Jesen Tinje"
            "815","471","OS","mb","Osnovna šola Pohorskega bataljona Oplotnica, Podružnična šola Kebelj"
            "468","468","OS","mb","Osnovna šola Pohorskega odreda Slovenska Bistrica"
            "3533","468","OS","mb","Osnovna šola Pohorskega odreda Slov. Bistrica Podružnica Zgornja Ložnica"
            "470","470","OS","mb","Osnovna šola Šmartno na Pohorju"
            "14849","14849","OS","mb","2. osnovna šola Slovenska Bistrica"
            "609","609","OS","ce","Osnovna šola Loče"
            "819","609","OS","ce","Osnovna šola Loče, Podružnična šola Jernej"
            "820","609","OS","ce","Osnovna šola Loče, Podružnična šola Žiče"
            "607","607","OS","ce","Osnovna šola Ob Dravinji Slovenske Konjice"
            "3535","607","OS","ce","Osnovna šola Ob Dravinji, Podružnična šola Tepanje"
            "608","608","OS","ce","Osnovna šola Pod goro Slovenske Konjice"
            "3789","608","OS","ce","Osnovna šola Pod goro Slovenske Konjice Podružnica Špitalič"
            "447","447","OS","nm","Osnovna šola dr. Ivana Prijatelja Sodražica"
            "711","447","OS","ce","Osnovna šola Blaža Arniča Luče, Podružnična šola Solčava"
            "395","395","OS","mb","Osnovna šola Središče ob Dravi"
            "572","572","OS","mb","Osnovna šola Starše"
            "936","572","OS","mb","Osnovna šola Starše, Podružnica Marjeta na Dravskem polju"
            "386","386","OS","nm","Osnovna šola Vavta vas"
            "248","248","OS","mb","Osnovna šola Sveta Ana"
            "698","248","OS","mb","Osnovna šola Sveta Ana, Podružnična šola Lokavec"
            "250","250","OS","mb","Osnovna šola in vrtec Sveta Trojica"
            "9857","250","OS","mb","Osnovna šola Cerkvenjak - Vitomarci, Podružnična šola Vitomarci"
            "169","169","OS","ms","Osnovna šola Sveti Jurij ob Ščavnici"
            "245","245","OS","mb","Osnovna šola Jožeta Hudalesa Jurovski dol"
            "389","389","OS","mb","Osnovna šola Sveti Tomaž"
            "721","389","OS","ms","Dvojezična osnovna šola Prosenjakovci Podružnica Domanjševci"
            "347","347","OS","ms","Osnovna šola Šalovci"
            "366","366","OS","ng","Osnovna šola Ivana Roba Šempeter pri Gorici"
            "921","366","OS","ng","Osnovna šola Ivana Roba Šempeter pri Gorici Podružnica Vrtojba"
            "233","233","OS","kr","Osnovna šola Šenčur"
            "1337","233","OS","kr","Osnovna šola Šenčur Podružnica Olševek"
            "1378","233","OS","kr","Osnovna šola Šenčur Podružnica Trboje"
            "982","233","OS","kr","Osnovna šola Šenčur Podružnica Voklo"
            "584","584","OS","mb","Osnovna šola Rudolfa Maistra Šentilj"
            "903","584","OS","mb","Osnovna šola Rudolfa Maistra Šentilj, Podružnična šola Ceršak"
            "585","585","OS","mb","Osnovna šola Sladki Vrh"
            "718","585","OS","mb","Osnovna šola Sladki Vrh, Podružnična šola Velka"
            "382","382","OS","nm","Osnovna šola Šentjernej"
            "737","382","OS","nm","Osnovna šola Šentjernej Podružnica Orehovica"
            "1352","1352","OS","ce","Osnovna šola Blaža Kocena Ponikva"
            "478","478","OS","ce","Osnovna šola Dramlje"
            "480","480","OS","ce","Osnovna šola Franja Malgaja Šentjur"
            "823","480","OS","ce","Osnovna šola Franja Malgaja Šentjur, Podružnična osnovna šola Blagovna"
            "1241","1241","OS","ce","Osnovna šola Hruševec Šentjur"
            "3781","1241","OS","ce","Osnovna šola Hruševec Šentjur, Podružnična šola Kalobje"
            "479","479","OS","ce","Osnovna šola Planina pri Sevnici"
            "482","482","OS","ce","Osnovna šola Slivnica pri Celju"
            "824","482","OS","ce","Osnovna šola Slivnica pri Celju, Podružnična šola Loka pri Žusmu"
            "825","482","OS","ce","Osnovna šola Slivnica pri Celju, Podružnična šola Prevorje"
            "516","516","OS","nm","Osnovna šola dr. Pavla Lunačka Šentrupert"
            "373","373","OS","nm","Osnovna šola Frana Metelka Škocjan"
            "1265","373","OS","nm","Osnovna šola Frana Metelka Škocjan Podružnica Bučka"
            "485","485","OS","kr","Osnovna šola Cvetka Golarja Škofja Loka"
            "832","485","OS","kr","Osnovna šola Cvetka Golarja Škofja Loka Podružnica Reteče"
            "483","483","OS","kr","Osnovna šola Ivana Groharja"
            "829","483","OS","kr","Osnovna šola Ivana Groharja, Podružnična šola Bukovica"
            "830","483","OS","kr","Osnovna šola Ivana Groharja, Podružnična šola Bukovščica"
            "831","483","OS","kr","Osnovna šola Ivana Groharja, Podružnična šola Sv. Lenart"
            "484","484","OS","kr","Osnovna šola Škofja Loka - Mesto"
            "319","319","OS","lj","Osnovna šola Škofljica"
            "3788","319","OS","lj","Osnovna šola Škofljica Podružnica Lavrica"
            "958","319","OS","lj","Osnovna šola Škofljica Podružnica Želimlje"
            "495","495","OS","ce","Osnovna šola Šmarje pri Jelšah"
            "3530","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Kristan vrh"
            "789","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Mestinje"
            "788","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Sladka gora"
            "3529","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Sv. Štefan"
            "787","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Šentvid"
            "786","495","OS","ce","Osnovna šola Šmarje pri Jelšah Podružnica Zibika"
            "381","381","OS","nm","Osnovna šola Šmarjeta"
            "533","533","OS","ce","Osnovna šola bratov Letonja Šmartno ob Paki"
            "263","263","OS","lj","Osnovna šola Šmartno"
            "745","263","OS","lj","Osnovna šola Šmartno, Podružnica Kostrevnica"
            "741","263","OS","lj","Osnovna šola Šmartno, Podružnica Primskovo"
            "744","263","OS","lj","Osnovna šola Šmartno, Podružnica Štangarske Poljane"
            "10167","10167","OS","ce","Osnovna šola Karla Destovnika-Kajuha Šoštanj"
            "10188","10167","OS","ce","Osnovna šola Karla Destovnika-Kajuha  Šoštanj Podružnica Topolšica"
            "143","143","OS","ce","Osnovna šola Štore"
            "647","143","OS","ce","Osnovna šola Štore Podružnična šola Kompole"
            "882","143","OS","ce","Osnovna šola Vransko -Tabor,  Podružnična osnovna šola Tabor"
            "352","352","OS","ms","Osnovna šola Tišina"
            "729","352","OS","ms","Osnovna šola Tišina Podružnica Gederovci"
            "505","505","OS","ng","Osnovna šola Dušana Muniha Most na Soči"
            "843","505","OS","ng","Osnovna šola Dušana Muniha Most na Soči, Podružnična šola Dolenja Trebuša"
            "846","505","OS","ng","Osnovna šola Dušana Muniha Most na Soči, Podružnična šola Podmelec"
            "845","505","OS","ng","Osnovna šola Dušana Muniha Most na Soči, Podružnična šola Šentviška gora"
            "506","506","OS","ng","Osnovna šola Franceta Bevka Tolmin"
            "838","506","OS","ng","Osnovna šola Franceta Bevka Tolmin Podružnica Anton Majnik Volče"
            "837","506","OS","ng","Osnovna šola Franceta Bevka Tolmin Podružnica Kamno"
            "511","511","OS","ng","Osnovna šola Simona Kosa Podbrdo"
            "1217","1217","OS","za","Osnovna šola Ivana Cankarja Trbovlje"
            "1218","1218","OS","za","Osnovna šola Tončke Čeč Trbovlje"
            "1216","1216","OS","za","Osnovna šola Trbovlje"
            "1244","1216","OS","za","Osnovna šola Trbovlje Podružnična šola Alojza Hohkrauta"
            "1242","1216","OS","za","Osnovna šola Trbovlje Podružnična šola Dobovec"
            "513","513","OS","nm","Osnovna šola Trebnje"
            "852","513","OS","nm","Osnovna šola Trebnje, Podružnična šola Dobrnič"
            "850","513","OS","nm","Osnovna šola Trebnje, Podružnična šola Dolenja Nemška vas"
            "853","513","OS","nm","Osnovna šola Trebnje, Podružnična šola Šentlovrenc"
            "514","514","OS","nm","Osnovna šola Veliki Gaber"
            "3743","514","OS","mb","Osnovna šola Destrnik Podružnica Trnovska vas"
            "165","165","OS","lj","Osnovna šola Trzin"
            "520","520","OS","kr","Osnovna šola Bistrica"
            "855","520","OS","kr","Osnovna šola Bistrica, Podružnična šola Kovor"
            "519","519","OS","kr","Osnovna šola Križe"
            "521","521","OS","kr","Osnovna šola Tržič"
            "857","521","OS","kr","Osnovna šola Tržič, Podružnična šola Lom"
            "858","521","OS","kr","Osnovna šola Tržič, Podružnična šola Podljubelj"
            "259","259","OS","ms","Osnovna šola Turnišče"
            "525","525","OS","ce","Osnovna šola Antona Aškerca Velenje"
            "861","525","OS","ce","Osnovna šola Antona Aškerca Velenje, Podružnična šola Pesje"
            "526","526","OS","ce","Osnovna šola Gorica Velenje"
            "1239","526","OS","ce","Osnovna šola Gorica Velenje, Podružnična šola Vinska Gora"
            "527","527","OS","ce","Osnovna šola Gustava Šiliha Velenje"
            "859","527","OS","ce","Osnovna šola Gustava Šiliha Velenje, Podružnična šola Šentilj"
            "524","524","OS","ce","Osnovna šola Livada Velenje"
            "1336","524","OS","ce","Osnovna šola Livada Velenje, Podružnična šola Škale"
            "528","528","OS","ce","Osnovna šola Mihe Pintarja - Toleda Velenje"
            "860","528","OS","ce","Osnovna šola Mihe Pintarja - Toleda Velenje Podružnica Plešivec"
            "523","523","OS","ce","Osnovna šola Šalek Velenje"
            "252","252","OS","ms","Osnovna šola Miška Kranjca Velika Polana"
            "321","321","OS","lj","Osnovna šola Primoža Trubarja Velike Lašče"
            "3553","321","OS","lj","Osnovna šola Primoža Trubarja Velike Lašče Podružnica Rob"
            "960","321","OS","lj","Osnovna šola Primoža Trubarja Velike Lašče Podružnica Turjak"
            "3709","3709","OS","ms","Osnovna šola Veržej"
            "419","419","OS","mb","Osnovna šola Videm"
            "968","419","OS","mb","Osnovna šola Videm, Podružnična šola Leskovec"
            "762","419","OS","mb","Osnovna šola Videm, Podružnična šola Sela"
            "120","120","OS","ng","Osnovna šola Draga Bajca Vipava"
            "660","120","OS","ng","Osnovna šola Draga Bajca Vipava Podružnica Goče"
            "661","120","OS","ng","Osnovna šola Draga Bajca Vipava Podružnica Podnanos"
            "665","120","OS","ng","Osnovna šola Draga Bajca Vipava Podružnica Vrhpolje"
            "610","610","OS","ce","Osnovna šola Vitanje"
            "311","311","OS","lj","Osnovna šola Vodice"
            "964","311","OS","lj","Osnovna šola Vodice Podružnica Utik"
            "142","142","OS","ce","Osnovna šola Antona Bezenška Frankolovo"
            "141","141","OS","ce","Osnovna šola Vojnik"
            "650","141","OS","ce","Osnovna šola Vojnik Podružnica Nova Cerkev"
            "649","141","OS","ce","Osnovna šola Vojnik Podružnica Socka"
            "545","545","OS","ce","Osnovna šola Vransko - Tabor"
            "6687","6687","OS","lj","Osnovna šola Antona Martina Slomška Vrhnika"
            "536","536","OS","lj","Osnovna šola Ivana Cankarja Vrhnika"
            "9230","536","OS","lj","Osnovna šola Ivana Cankarja Vrhnika, Podružnica Drenov grič"
            "867","536","OS","lj","Osnovna šola Log - Dragomer, Podružnična šola Bevke"
            "430","430","OS","sg","Osnovna šola Vuzenica"
            "541","541","OS","za","Osnovna šola Ivana Kavčiča Izlake"
            "870","541","OS","za","Osnovna šola Ivana Kavčiča Izlake Podružnica Mlinše"
            "539","539","OS","za","Osnovna šola Ivana Skvarče"
            "871","539","OS","za","Osnovna šola Ivana Skvarče, Podružnica Čemšenik"
            "872","539","OS","za","Osnovna šola Ivana Skvarče, Podružnica Podkum"
            "540","540","OS","za","Osnovna šola Toneta Okrogarja"
            "3543","540","OS","za","Osnovna šola Toneta Okrogarja, Podružnična šola Kisovec"
            "869","540","OS","za","Osnovna šola Toneta Okrogarja, Podružnična šola Šentlambert"
            "765","540","OS","mb","Osnovna šola Cirkulane - Zavrč, Podružnica Zavrč"
            "611","611","OS","ce","Osnovna šola Zreče"
            "821","611","OS","ce","Osnovna šola Zreče, Podružnična šola Gorenje"
            "1369","611","OS","ce","Osnovna šola Zreče, Podružnična šola Stranice"
            "546","546","OS","ce","I. osnovna šola Žalec"
            "874","546","OS","ce","I. osnovna šola Žalec Podružnica Gotovlje"
            "875","546","OS","ce","I. osnovna šola Žalec Podružnica Ponikva"
            "544","544","OS","ce","Osnovna šola Griže"
            "873","544","OS","ce","Osnovna šola Griže, Podružnična osnovna šola Liboje"
            "543","543","OS","ce","Osnovna šola Petrovče"
            "9855","543","OS","ce","Osnovna šola Petrovče Podružnica Trje"
            "549","549","OS","ce","Osnovna šola Šempeter v Savinjski dolini"
            "18511","549","OS","ce","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Savinja, Enota osnovna šola"
            "490","490","OS","kr","Osnovna šola Železniki"
            "833","490","OS","kr","Osnovna šola Železniki, Podružnica Davča"
            "834","490","OS","kr","Osnovna šola Železniki, Podružnica Dražgoše"
            "835","490","OS","kr","Osnovna šola Železniki, Podružnica Selca"
            "836","490","OS","kr","Osnovna šola Železniki, Podružnica Sorica"
            "1260","1260","OS","mb","Osnovna šola Žetale"
            "489","489","OS","kr","Osnovna šola Žiri"
            "1229","1229","OS","kr","Osnovna šola Žirovnica"
            "388","388","OS","nm","Osnovna šola Prevole"
            "387","387","OS","nm","Osnovna šola Žužemberk"
            "733","387","OS","nm","Osnovna šola Žužemberk, Podružnica Ajdovec"
            "734","387","OS","nm","Osnovna šola Žužemberk Podružnica Dvor"
            "735","387","OS","nm","Osnovna šola Žužemberk, Podružnica Šmihel"
            "1089","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA"
            "14049","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA OB HUBLJU"
            "14050","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA RIBNIK"
            "14051","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VIPAVA"
            "14052","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC ČRNIČE"
            "14053","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC SELO"
            "14054","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC VIPAVSKI KRIŽ"
            "14055","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC COL"
            "14056","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC BUDANJE"
            "14057","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC PODNANOS"
            "14058","1089","PV","ng","OTROŠKI VRTEC AJDOVŠČINA, ENOTA VRTEC VRHPOLJE"
            "11089","11089","PV","ng","Osnovna šola Otlica, Enota Vrtec"
            "24295","24295","PV","ng","Vrtec Gnezdo, zasebni zavod"
            "24296","24295","PV","ng","Vrtec Gnezdo, zasebni zavod, poslovna enota"
            "22535","22535","PV","kp","Osnovna šola in vrtec Ankaran, Enota Vrtec Ankaran"
            "13969","13969","PV","ms","Osnovna šola in vrtec Apače, Enota vrtca Apače-Stogovci"
            "10709","10709","PV","ms","Vrtec Beltinci, Jugovo 33, 9231 Beltinci"
            "10710","10709","PV","ms","Vrtec Beltinci, enota Dokležovje"
            "10711","10709","PV","ms","Vrtec Beltinci, enota Gančani"
            "10712","10709","PV","ms","Vrtec Beltinci, enota Ižakovci"
            "10713","10709","PV","ms","Vrtec Beltinci, enota Lipovci"
            "10714","10709","PV","ms","Vrtec Beltinci, enota Melinci"
            "3785","3785","PV","mb","Osnovna šola Benedikt, Enota Vrtec"
            "1249","1249","PV","kk","Osnovna šola Bistrica ob Sotli, Enota Vrtec Pikapolonica"
            "1324","1324","PV","kr","Vrtec Bled"
            "13929","1324","PV","kr","Vrtec Bled, enota Bled"
            "13949","1324","PV","kr","Vrtec Bled enota Bohinjska Bela"
            "6684","6684","PV","po","Osnovna šola Toneta Šraja Aljoše, enota Vrtec Nova vas"
            "1332","1332","PV","kr","Osnovna šola dr.Janeza Mencingerja Bohinjska Bistrica, Enota Vrtec"
            "10087","10087","PV","lj","Osnovna šola dr. Ivana Korošca Borovnica, Enota Vrtec"
            "1187","1187","PV","ng","Osnovna šola Bovec, Enota Vrtec"
            "1366","1366","PV","ce","Osnovna šola Braslovče, enota Vrtec"
            "1171","1171","PV","ng","VVE pri OŠ Alojza Gradnika Dobrovo v Brdih"
            "19450","19450","PV","ng","Osnovna šola Alojza Gradnika Dobrovo, enota vrtca Kojsko"
            "19470","19470","PV","ng","Osnovna šola Alojza Gradnika Dobrovo, enota vrtca Dobrovo"
            "24115","24115","PV","ng","Osnovna šola Alojza Gradnika, DO vrtca Dobrovo v Biljani"
            "1279","1279","PV","lj","VIZ Vrtci Brezovica"
            "11633","1279","PV","lj","VIZ Vrtci Brezovica, Enota Vnanje Gorice"
            "11630","1279","PV","lj","VIZ Vrtci Brezovica, Enota OŠ Notranje Gorice"
            "11631","1279","PV","lj","VIZ Vrtci Brezovica, Enota Podpeč"
            "11632","1279","PV","lj","VIZ Vrtci Brezovica, Enota Rakitna"
            "11629","1279","PV","lj","VIZ Vrtci Brezovica, Enota Brezovica"
            "17790","1279","PV","lj","VIZ Vrtci Brezovica, Enota pri OŠ Brezovica pri Ljubljani"
            "21755","1279","PV","lj","VIZ Vrtci Brezovica, enota Preserje"
            "23675","1279","PV","lj","VIZ Vrtci Brezovica, enota NOTRANJE GORICE"
            "1063","1063","PV","kk","Vrtec Mavrica Brežice"
            "22055","1063","PV","kk","Vrtec Mavrica Brežice, Vrtec Zvezdica"
            "17217","1063","PV","kk","Vrtec Mavrica Brežice, Vrtec Oblaček"
            "1224","1224","PV","kk","Osnovna šola dr. Jožeta Toporišiča Dobova,  Vrtec Najdihojca"
            "16990","16990","PV","kk","Vrtec Najdihojca pri OŠ dr. Jožeta Toporišiča Dobova, Enota Sonček, Kapele"
            "11349","11349","PV","kk","Osnovna šola Globoko, Enota Vrtec"
            "1120","1120","PV","kk","Osnovna šola Maksa Pleteršnika Pišece, Enota Vrtec"
            "1128","1128","PV","kk","Osnovna šola Artiče, Vrtec Ringa Raja"
            "3688","3688","PV","kk","Osnovna šola Cerklje ob Krki, enota Vrtec Pikapolonica"
            "1202","1202","PV","kk","Vrtec pri Osnovni šoli Velika Dolina"
            "1170","1170","PV","kk","Osnovna šola Bizeljsko, Enota Vrtec"
            "1141","1141","PV","ms","Osnovna šola Cankova, Enota Vrtec"
            "12771","12771","PV","ce","Vrtec Danijelov levček, Opatijsko-mestni župnijski zavod Sv. Danijela Celje"
            "14149","12771","PV","ce","Vrtec Danijelov levček, enota SLOMŠEK"
            "18950","12771","PV","ce","Danijelov levček, enota VOJNIK"
            "1062","1062","PV","ce","Vrtec Zarja Celje"
            "11431","1062","PV","ce","Vrtec Zarja Celje, ENOTA ISKRICA"
            "11430","1062","PV","ce","Vrtec Zarja Celje, ČIRA-ČARA"
            "11429","1062","PV","ce","Vrtec Zarja Celje, ENOTA RINGA RAJA"
            "11449","1062","PV","ce","Vrtec Zarja Celje, enota vrtca Žiž žav"
            "19570","1062","PV","ce","Vrtec Zarja Celje, MEHURČEK, enota"
            "1090","1090","PV","ce","Vrtec Tončke Čečeve Celje"
            "11969","1090","PV","ce","Vrtec Tončke Čečeve, ENOTA HUDINJA"
            "11970","1090","PV","ce","Vrtec Tončke Čečeve, ENOTA CENTER"
            "11989","1090","PV","ce","Vrtec Tončke Čečeve, ENOTA GABERJE"
            "11990","1090","PV","ce","Vrtec Tončke Čečeve, ENOTA ALJAŽEV HRIB"
            "12009","1090","PV","ce","Vrtec Tončke Čečeve, ENOTA LJUBEČNA"
            "1103","1103","PV","ce","Vrtec Anice Černejeve Celje"
            "11290","1103","PV","ce","Vrtec Anice Černejeve Celje, Enota Sonce"
            "11289","1103","PV","ce","Vrtec Anice Černejeve Celje, Enota Luna"
            "11269","1103","PV","ce","Vrtec Anice Černejeve Celje, Enota Mavrica"
            "11291","1103","PV","ce","Vrtec Anice Černejeve Celje, Enota Bolnišnica"
            "13109","1103","PV","ce","Vrtec Anice Černejeve Celje, Enota Hribček"
            "21795","21795","PV","ce","Zasebni vrtec Vesela hiša, predšolska vzgoja in izobraževanje, d.o.o."
            "1148","1148","PV","kr","VVE pri OŠ Davorina Jenka Cerklje na Gorenjskem"
            "17230","17230","PV","kr","VVE pri OŠ Davorina Jenka Cerklje na Gorenjskem, enota Cerklje"
            "17231","17231","PV","kr","VVE pri OŠ Davorina Jenka Cerklje na Gorenjskem, enota Zalog"
            "1391","1391","PV","kr","Marijin vrtec, Cerklje"
            "21895","21895","PV","kr","Zasebni vrtec Kepica Mojca d.o.o."
            "1194","1194","PV","po","Vrtec Martin Krpan Cerknica"
            "13329","1194","PV","po","Vrtec Martin Krpan Cerknica, Enota Rakek"
            "13330","1194","PV","po","Vrtec Martin Krpan Cerknica, Enota Grahovo"
            "23915","1194","PV","po","Vrtec Martin Krpan Cerknica, enota Rakek - šola"
            "1364","1364","PV","ng","Osnovna šola Cerkno, Enota Vrtec Cerkno"
            "3786","3786","PV","mb","Osnovna šola Cerkvenjak - Vitomarci, Enota Vrtec Cerkvenjak"
            "13169","13169","PV","mb","Osnovna šola Cerkvenjak - Vitomarci, Enota Vrtec Vitomarci"
            "15149","15149","PV","mb","Osnovna šola Cirkulane - Zavrč, Vrtec Cirkulane"
            "15169","15169","PV","mb","Osnovna šola Cirkulane - Zavrč, Vrtec Zavrč"
            "3718","3718","PV","ms","Osnovna šola Franceta Prešerna Črenšovci, Enota Vrtec Črenšovci"
            "3750","3750","PV","ms","VVE pri Osnovni šoli Prežihovega Voranca Bistrica"
            "6685","6685","PV","sg","OŠ Črna na Koroškem Vrtec Črna na Koroškem"
            "17110","17110","PV","sg","OŠ Črna na Koroškem Vrtec Črna na Koroškem, enota Žerjav"
            "1114","1114","PV","nm","Vrtec Otona Župančiča Črnomelj"
            "19890","1114","PV","nm","Vrtec Otona Župančiča Črnomelj, enota LOKA"
            "19891","1114","PV","nm","Vrtec Otona Župančiča Črnomelj, enota ČARDAK"
            "19892","1114","PV","nm","Vrtec Otona Župančiča Črnomelj, enota MAJER"
            "19893","1114","PV","nm","Vrtec Otona Župančiča Črnomelj, enota DIJAŠKI DOM"
            "19910","1114","PV","nm","Vrtec Otona Župančiča Črnomelj, enota LOKA, oddelek OŠ LOKA"
            "1121","1121","PV","nm","Osnovna šola Komandanta Staneta Dragatuš, Enota Vrtec Dragatuš"
            "3651","3651","PV","nm","Osnovna šola Vinica, Enota Vrtec"
            "3687","3687","PV","nm","Osnovna šola Stari trg ob Kolpi, Enota Vrtec"
            "22515","22515","PV","nm","Osnovna šola Stari trg ob Kolpi, Enota Vrtec, enota"
            "1333","1333","PV","mb","OŠ Destrnik - Trnovska vas, Vrtec pri OŠ Destrnik"
            "12129","12129","PV","mb","VVE pri PŠ Trnovska vas"
            "14609","14609","PV","ce","Osnovna šola Dobje, enota Vrtec Dobje pri Planini"
            "1327","1327","PV","lj","VVE Ringaraja pri OŠ Dobrepolje"
            "3810","3810","PV","ce","Osnovna šola Dobrna, Enota Vrtec Dobrna"
            "13989","13989","PV","lj","Osnovna šola Dobrova, Vrtec Dobrova"
            "1133","1133","PV","lj","Osnovna šola Dobrova, Vrtec Brezje"
            "1135","1135","PV","lj","Osnovna šola Polhov Gradec, Enota Vrtec"
            "11229","11229","PV","lj","Osnovna šola Polhov Gradec, Enota Vrtec pri PŠ Šentjošt"
            "23875","23875","PV","lj","Osnovna šola Polhov Gradec, Enota Vrtec pri PŠ Črni Vrh"
            "3830","3830","PV","ms","Dvojezična osnovna šola Dobrovnik, enota Vrtec"
            "19770","19770","PV","lj","Silva Peterca - zasebna vzgojiteljica"
            "10088","10088","PV","lj","Osnovna šola Janka Modra Dol pri Ljubljani, Enota Vrtec"
            "1152","1152","PV","nm","Osnovna šola Dolenjske Toplice, Vrtec Gumbek"
            "1325","1325","PV","lj","Vrtec Urša"
            "11609","1325","PV","lj","Vrtec Urša, enota Čebelica"
            "11610","1325","PV","lj","Vrtec Urša, enota Bistra"
            "16610","1325","PV","lj","Vrtec Urša, enota Češmin"
            "21335","1325","PV","lj","Vrtec Urša, enota Mavrica"
            "22675","1325","PV","lj","Vrtec Urša, Enota Pikapolonica"
            "22676","1325","PV","lj","Vrtec Urša, Enota Urša"
            "10594","10594","PV","lj","HIŠA OTROK MALI PRINC storitve d.o.o."
            "1111","1111","PV","lj","Vrtec Domžale"
            "14697","1111","PV","lj","Vrtec Domžale, Enota Savska"
            "17810","1111","PV","lj","Vrtec Domžale, Enota Gaj"
            "14696","1111","PV","lj","Vrtec Domžale, Enota Krtek"
            "14694","1111","PV","lj","Vrtec Domžale, Enota Cicidom"
            "14692","1111","PV","lj","Vrtec Domžale, Enota Kekec"
            "14698","1111","PV","lj","Vrtec Domžale, Enota Ostržek"
            "14693","1111","PV","lj","Vrtec Domžale, Enota Palček"
            "14695","1111","PV","lj","Vrtec Domžale, Enota Mlinček"
            "14699","1111","PV","lj","Vrtec Domžale, Enota Racman"
            "3700","3700","PV","lj","Osnovna šola Roje, razvojni oddelek"
            "1388","1388","PV","lj","Zavod Dominik Savio Karitas Domžale, OE Vrtec Dominik Savio Karitas Domžale"
            "1339","1339","PV","mb","Osnovna šola dr. Franja Žgeča Dornava, Enota Vrtec"
            "1061","1061","PV","sg","VRTEC DRAVOGRAD"
            "14349","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC ROBINDVOR"
            "14350","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC ŠENTJANŽ"
            "14351","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC ČRNEČE"
            "14352","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC LIBELIČE"
            "14353","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC TRBONJE"
            "14729","1061","PV","sg","VRTEC DRAVOGRAD, VRTEC DRAVOGRAD, ENOTA DRAVOGRAD"
            "18531","1061","PV","sg","VRTEC DRAVOGRAD, oddelek OJSTRICA"
            "1276","1276","PV","mb","Osnovna šola Duplek, Enota Vrtec"
            "22455","22455","PV","mb","Osnovna šola Duplek, Enota Vrtec, enota Duplek"
            "22456","22456","PV","mb","Osnovna šola Duplek, Enota Vrtec, enota Dvorjane"
            "22458","22458","PV","mb","Osnovna šola Duplek, Enota Vrtec, enota Zg. Duplek"
            "1277","1277","PV","mb","VVE pri OŠ Korena"
            "1328","1328","PV","kr","Osnovna šola Poljane, Vrtec Agata"
            "23935","23935","PV","kr","Osnovna šola Poljane, Vrtec Agata, enota Javorje"
            "23955","23955","PV","kr","Osnovna šola Poljane, Vrtec Agata, enota Poljane 59"
            "3672","3672","PV","kr","Osnovna šola Ivana Tavčarja Gorenja vas, Vrtec Zala"
            "17870","17870","PV","kr","Osnovna šola Ivana Tavčarja Gorenja vas, enota Vrtec Dobrava"
            "12050","12050","PV","kr","Osnovna šola Ivana Tavčarja Gorenja vas, enota Vrtec Sovodenj"
            "17430","17430","PV","kr","Osnovna šola Ivana Tavčarja Gorenja vas, Vrtec Zala, enota Lučine"
            "1283","1283","PV","mb","Osnovna šola Gorišnica, Vrtec Gorišnica"
            "13909","13909","PV","kr","Osnovna šola Gorje, Enota Vrtec Zgornje Gorje"
            "17590","17590","PV","kr","Osnovna šola Gorje, Enota Vrtec Gorje"
            "1091","1091","PV","ms","Vrtec Manka Golarja Gornja Radgona"
            "14329","1091","PV","ms","Vrtec Manka Golarja Gornja Radgona, enota Črešnjevci"
            "23475","1091","PV","ms","Vrtec Manka Golarja Gornja Radgona, enota Kocljeva 4"
            "24135","24135","PV","ms","Vrtec pri Osnovna šola dr. Antona Trstenjaka Negova"
            "1253","1253","PV","ce","Osnovna šola Frana Kocbeka Gornji Grad, Enota Vrtec Gornji Grad"
            "13809","13809","PV","ce","Osnovna šola Frana Kocbeka Gornji Grad, Enota Vrtec Bočna"
            "1140","1140","PV","ms","VVE pri OŠ Gornji Petrovci"
            "3660","3660","PV","ms","Osnovna šola Grad, Enota Vrtec"
            "18250","18250","PV","lj","Zasebni vrtec Kobacaj"
            "19030","18250","PV","lj","Zasebni vrtec Kobacaj, enota Adamičeva 20"
            "22275","18250","PV","lj","Zasebni vrtec Kobacaj, enota Brezje 79"
            "19031","18250","PV","lj","Zasebni vrtec Kobacaj, enota Brezje"
            "19810","18250","PV","lj","Zasebni vrtec Kobacaj, enota Mlačevo"
            "21115","18250","PV","lj","Zasebni vrtec Kobacaj, enota Sončni dvori"
            "21356","18250","PV","lj","Zasebni vrtec Kobacaj, enota Sončni dvori, oddelek"
            "20030","20030","PV","lj","Zasebni vrtec Biba d.o.o."
            "18870","18870","PV","lj","Vrtec Jurček, Vzgoja in varstvo otrok, d.o.o."
            "1112","1112","PV","lj","VVZ Kekec Grosuplje"
            "11489","1112","PV","lj","VVZ Kekec Grosuplje enota Tinkara"
            "11509","1112","PV","lj","VVZ Kekec Grosuplje, enota Rožle"
            "11510","1112","PV","lj","VVZ Kekec Grosuplje, enota Pika Šmarje Sap"
            "11511","1112","PV","lj","VVZ Kekec Grosuplje, enota Pastirček"
            "11512","1112","PV","lj","VVZ Kekec Grosuplje, enota Kosobrin"
            "11513","1112","PV","lj","VVZ Kekec Grosuplje, enota Mojca"
            "11514","1112","PV","lj","VVZ Kekec Grosuplje, enota Kekec"
            "15269","1112","PV","lj","VVZ Kekec Grosuplje, oddelek Zvonček pri POŠ Žalna"
            "19630","1112","PV","lj","VVZ Kekec Grosuplje, enota Trobentica"
            "21695","21695","PV","lj","Zasebni vrtec Malček"
            "17930","17930","PV","mb","Osnovna šola Hajdina, enota vrtca"
            "6680","6680","PV","mb","Osnovna šola Franca Lešnika - Vuka Slivnica pri Mariboru, Enota Vrtec"
            "3831","3831","PV","mb","Osnovna šola Dušana Flisa Hoče, VVE Rogoza"
            "12269","12269","PV","mb","Osnovna šola Dušana Flisa Hoče, VVE Hoče"
            "1134","1134","PV","lj","Osnovna šola Horjul, Enota Vrtec"
            "14193","14193","PV","za","Vrtec Hrastnik"
            "13269","14193","PV","za","Vrtec Hrastnik, enota Sonček na Rudniku"
            "13270","14193","PV","za","Vrtec Hrastnik, enota Lučka na Dolu"
            "13289","14193","PV","za","Vrtec Hrastnik, enota Dolinca na Logu"
            "17650","14193","PV","za","Vrtec Hrastnik, enota na OŠ n. h. Rajka Hrastnik"
            "1231","1231","PV","ng","JVIZ Vrtec Idrija"
            "11190","1231","PV","ng","JVIZ Vrtec Idrija, Enota Spodnja Idrija"
            "11191","1231","PV","ng","JVIZ Vrtec Idrija, Enota Godovič"
            "11193","1231","PV","ng","JVIZ Vrtec Idrija, Enota Prelovčeva"
            "11192","1231","PV","ng","JVIZ Vrtec Idrija, Enota Črni Vrh"
            "1318","1318","PV","lj","VRTEC IG"
            "14569","1318","PV","lj","VRTEC IG enota KRIMČEK"
            "14570","1318","PV","lj","VRTEC IG enota STUDENČEK"
            "14571","1318","PV","lj","VRTEC IG enota HRIBČEK"
            "16550","1318","PV","lj","VRTEC IG, enota SONČEK"
            "16551","1318","PV","lj","VRTEC IG, enota MAVRICA"
            "16529","16529","PV","po","ZAVOD ŠOLSKIH SESTER ND ILIRSKA BISTRICA, OE VRTEC ANTONINA"
            "17670","17670","PV","po","Vrtec Jožefe Maslo Ilirska Bistrica"
            "1123","1123","PV","po","Osnovna šola Jelšane, Enota Vrtec"
            "10989","10989","PV","po","Osnovna šola Toneta Tomšiča Knežak, Enota Vrtec"
            "1228","1228","PV","po","Osnovna šola Rudolfa Ukoviča Podgrad, Enota Vrtec"
            "1147","1147","PV","po","Osnovna šola Podgora  Kuteževo, Enota vrtec"
            "3770","3770","PV","po","Osnovna šola Rudija Mahniča- Brkinca Pregarje, enota vrtec"
            "1319","1319","PV","lj","VVZ VRTEC IVANČNA GORICA"
            "11029","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC MARJETICA, disloc. Muljava"
            "14249","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC MARJETICA"
            "14250","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC ČEBELICA"
            "14251","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC PIKAPOLONICA"
            "14252","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC MIŠKA"
            "14253","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC POLŽEK"
            "14254","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, VRTEC SONČEK"
            "18570","1319","PV","lj","VVZ VRTEC IVANČNA GORICA, enota KRKA"
            "21155","21155","PV","lj","Zasebni vrtec Ribniček Ivančna Gorica"
            "22735","21155","PV","lj","Zasebni vrtec Ribniček Ivančna Gorica, enota Komenskega"
            "1064","1064","PV","kp","VIZ VRTEC MAVRICA IZOLA"
            "11549","1064","PV","kp","VIZ VRTEC MAVRICA IZOLA ENOTA ŠKOLJKA"
            "11550","1064","PV","kp","VIZ VRTEC MAVRICA IZOLA ENOTA LIVADE"
            "17730","1064","PV","kp","VIZ VRTEC MAVRICA IZOLA, ENOTA LIVADE, DISLOCIRANI ODDELEK LIVADE II."
            "15471","1064","PV","kp","VIZ VRTEC MAVRICA IZOLA, ENOTA LIVADE, ODDELEK KORTE"
            "1183","1183","PV","kp","Osnovna šola Dante Alighieri Izola, Enota Vrtec L Aquilone"
            "1200","1200","PV","kr","Vrtec Jesenice"
            "11809","1200","PV","kr","Vrtec Jesenice, VVE Angelce Ocepek"
            "11810","1200","PV","kr","Vrtec Jesenice, VVE Angelce Ocepek"
            "11811","1200","PV","kr","Vrtec Jesenice, VVE Julke Pibernik"
            "11812","1200","PV","kr","Vrtec Jesenice, VVE Cilke Zupančič"
            "11813","1200","PV","kr","Vrtec Jesenice, DE Frančiške Ambrožič"
            "11814","1200","PV","kr","Vrtec Jesenice, DE Ivanke Krničar"
            "3746","3746","PV","mb","Osnovna šola Juršinci, Enota Vrtec"
            "1310","1310","PV","lj","Peter pan d.o.o., Zasebni vrtec"
            "18050","18050","PV","lj","Zasebni vrtec Zarja d.o.o."
            "18130","18130","PV","lj","Zasebni vrtec Sonček, varstvo, vzgoja in izobraževanje d.o.o."
            "20815","18130","PV","lj","Zasebni vrtec Sonček, enota Trobentica"
            "20816","18130","PV","lj","Zasebni vrtec Sonček, enota Marjetica"
            "20817","18130","PV","lj","Zasebni vrtec Sonček, enota Zvonček"
            "22155","18130","PV","lj","Zasebni vrtec Sonček, enota Sončni dvori"
            "1095","1095","PV","lj","Vrtec Antona Medveda Kamnik"
            "13350","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Tinkara"
            "13429","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Pedenjped"
            "13391","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Marjetica"
            "13470","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Kekec"
            "13409","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Mojca"
            "13369","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Pestrna"
            "13450","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Cepetavček"
            "13449","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Sonček"
            "13390","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Palček"
            "13469","1095","PV","lj","Vrtec Antona Medveda Kamnik, enota Polžki"
            "13389","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Sneguljčica"
            "17910","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Oblaček"
            "18210","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Kamenček"
            "13349","1095","PV","lj","Vrtec  Antona Medveda Kamnik, enota Rožle"
            "1144","1144","PV","ng","Osnovna šola Deskle, enota vrtec Deskle"
            "18830","18830","PV","ng","OŠ Kanal, podružnica Kal"
            "1172","1172","PV","ng","Osnovna šola Kanal, Enota Vrtec Kanal"
            "13729","13729","PV","ng","OŠ Kanal, Vrtec KAL"
            "19930","19930","PV","mb","Osnovna šola Cirkovce, Enota Vrtec Cirkovce"
            "19850","19850","PV","mb","Osnovna šola Borisa Kidriča, Enota Vrtec Kidričevo"
            "1186","1186","PV","ng","VVE pri OŠ Simona Gregorčiča Kobarid"
            "20715","20715","PV","ng","VVE pri OŠ Simona Gregorčiča Kobarid, enota"
            "15231","15231","PV","ng","OŠ Simona Gregorčiča Kobarid, Vrtec Breginj"
            "15229","15229","PV","ng","OŠ Simona Gregorčiča Kobarid, Vrtec Smast"
            "15230","15230","PV","ng","OŠ Simona Gregorčiča Kobarid, Vrtec Drežnica"
            "3713","3713","PV","ms","Osnovna šola Kobilje, enota Vrtec"
            "1105","1105","PV","nm","Vrtec Kočevje"
            "12529","1105","PV","nm","Vrtec Kočevje, Enota Čebelica"
            "12530","1105","PV","nm","Vrtec Kočevje, Enota Kekec"
            "12531","1105","PV","nm","Vrtec Kočevje, Enota Mojca"
            "12532","1105","PV","nm","Vrtec Kočevje, Enota Narcisa"
            "12669","1105","PV","nm","Vrtec Kočevje, enota Ostržek"
            "16209","1105","PV","nm","Vrtec Kočevje, enota BIBA"
            "19510","1105","PV","nm","Vrtec Kočevje, enota Brlogec"
            "21855","21855","PV","lj","Zasebni vrtec Zvezdnato nebo, d.o.o."
            "21516","21516","PV","lj","Vrtec Mehurčki"
            "21655","21516","PV","lj","Vrtec Mehurčki, dislocirana enota Čebelica"
            "21656","21516","PV","lj","Vrtec Mehurčki, dislocirana enota Moste"
            "21015","21015","PV","kp","Vzgojno - varstvena družina Pikapolonica"
            "18911","18911","PV","kp","Zasebni vrtec Račka, d.o.o."
            "1065","1065","PV","kp","Vrtec Koper"
            "11309","1065","PV","kp","Vrtec Koper, Enota Ribica"
            "11310","1065","PV","kp","Vrtec Koper, Enota Šalara-Polžek"
            "11329","1065","PV","kp","Vrtec Koper, Enota Vanganel-Pikapolonica"
            "11330","1065","PV","kp","Vrtec Koper, Enota Kekec"
            "11331","1065","PV","kp","Vrtec Koper, Enota Bertoki"
            "11332","1065","PV","kp","Vrtec Koper, Enota Pobegi-Zajček"
            "1096","1096","PV","kp","Vrtec Semedela"
            "11729","1096","PV","kp","Vrtec Semedela, enota Hrvatini"
            "11730","1096","PV","kp","Vrtec Semedela, enota Markovec"
            "11731","1096","PV","kp","Vrtec Semedela, enota Prisoje"
            "11749","1096","PV","kp","Vrtec Semedela, enota Rozmanova"
            "11769","1096","PV","kp","Vrtec Semedela, enota Slavnik"
            "1124","1124","PV","kp","Osnovna šola Istrskega odreda Gračišče, Enota Vrtec"
            "3652","3652","PV","kp","Osnovna šola Ivana Babiča - Jagra Marezige, Enota Vrtec"
            "13709","13709","PV","kp","Osnovna šola Dekani, Enota vrtec"
            "15330","15330","PV","kp","Osnovna šola Dekani, Enota vrtec Dekani"
            "15331","15331","PV","kp","Osnovna šola Dekani, Enota vrtca Rižana"
            "1130","1130","PV","kp","Osnovna šola Šmarje pri Kopru - vrtec"
            "1138","1138","PV","kp","Osnovna šola Oskarja Kovačiča Škofije, Vrtec Škofije"
            "1201","1201","PV","kp","Vrtec Delfino Blu Koper"
            "17190","1201","PV","kp","Vrtec Delfino Blu Koper, enota Semedela"
            "17191","1201","PV","kp","Vrtec Delfino Blu Koper, enota Bertoki"
            "17192","1201","PV","kp","Vrtec Delfino Blu Koper, enota Hrvatini"
            "17193","1201","PV","kp","Vrtec Delfino Blu Koper, enota Koper"
            "23275","1201","PV","kp","Vrtec Delfino Blu Koper, enota Ankaran"
            "22236","22236","PV","kp","Montessori razvojni center Obala, Zasebni vrtec Hiša otrok na Obali, Koper"
            "21735","21735","PV","kp","Zasebni zavod Mali mehurčki Koper"
            "1232","1232","PV","kk","Osnovna šola Jožeta Gorjupa Kostanjevica na Krki, enota Vrtec"
            "11531","11531","PV","nm","Osnovna šola Fara, enota Vrtec Fara"
            "1246","1246","PV","ce","VVE pri OŠ Kozje VRTEC ZMAJČEK"
            "3772","3772","PV","kr","Osnovna šola Orehek Kranj, Enota vrtca"
            "17250","17250","PV","kr","Osnovna šola Orehek Kranj, enota vrtec Mavčiče"
            "12774","12774","PV","kr","Župnijski zavod sv. Martina, Baragov vrtec"
            "1066","1066","PV","kr","Kranjski vrtci"
            "14469","1066","PV","kr","Kranjski vrtci, Enota Biba"
            "14470","1066","PV","kr","KRANJSKI VRTCI, ENOTA ČEBELICA"
            "14489","1066","PV","kr","KRANJSKI VRTCI, ENOTA JANINA"
            "14490","1066","PV","kr","KRANJSKI VRTCI, ENOTA KEKEC"
            "14491","1066","PV","kr","KRANJSKI VRTCI, ENOTA ČENČA"
            "14492","1066","PV","kr","KRANJSKI VRTCI, ENOTA ŽIV-ŽAV"
            "14493","1066","PV","kr","KRANJSKI VRTCI, ENOTA SONČEK"
            "14494","1066","PV","kr","KRANJSKI VRTCI, ENOTA ČIRA ČARA"
            "14495","1066","PV","kr","KRANJSKI VRTCI, ENOTA NAJDIHOJCA"
            "14509","1066","PV","kr","Kranjski vrtci, Enota Mojca"
            "14529","1066","PV","kr","Kranjski vrtci, Enota Mojca razvojni oddelek"
            "14530","1066","PV","kr","KRANJSKI VRTCI, ENOTA OSTRŽEK"
            "14549","1066","PV","kr","KRANJSKI VRTCI, ENOTA ČIRČE"
            "14550","1066","PV","kr","KRANJSKI VRTCI, ENOTA JEŽEK"
            "16369","1066","PV","kr","Kranjski vrtci, enota"
            "16450","1066","PV","kr","VRTEC V OŠ MATIJA ČOP"
            "16449","1066","PV","kr","VRTEC V OŠ JAKOBA ALJAŽA"
            "16451","1066","PV","kr","Kranjski vrtci, Enota Ciciban"
            "3653","3653","PV","kr","Osnovna šola Simona Jenka Kranj, Enota Vrtec"
            "14009","14009","PV","kr","Osnovna šola Simona Jenka Kranj, Vrtec v Podružnični šoli Primskovo"
            "1205","1205","PV","kr","Osnovna šola Predoslje, enota Vrtec"
            "589","589","PV","kr","Osnovna šola Stražišče Kranj, Enota Vrtec v Podružnici Besnica"
            "14969","14969","PV","kr","Osnovna šola Stražišče Kranj, Enota Vrtec v Podružnici Žabnica"
            "1149","1149","PV","kr","Osnovna šola Franceta Prešerna Kranj, Enota Vrtec Kokrica"
            "20575","20575","PV","kr","Zavod za kreativno varstvo Pri Dobri teti"
            "19690","19690","PV","kr","Zavod Za Jutri"
            "20070","19690","PV","kr","Zavod Za Jutri, enota Zasebni vrtec Vila Mezinček"
            "20090","20090","PV","kr","Zasebni vrtec Dobra teta d.o.o., enota Pri dobri Evi"
            "20515","20090","PV","kr","Zasebni vrtec Dobra teta d.o.o., enota Pri dobri Tinci"
            "21016","20090","PV","kr","Zasebni vrtec Dobra teta d.o.o., enota Pri dobri Lučki"
            "22075","20090","PV","lj","Zasebni vrtec Dobra teta d.o.o., enota Pri dobri Ajdi"
            "22115","22115","PV","kr","BUAN d.o.o., PE Zasebni vrtec Pri Dobri Vesni"
            "23055","23055","PV","kr","Čarobni svet, zasebni vrtec d.o.o., PE Mikujčki"
            "1199","1199","PV","kr","VVE pri OŠ Kranjska Gora"
            "3677","3677","PV","kr","Osnovna šola 16. decembra Mojstrana, Enota Vrtec"
            "3690","3690","PV","ms","Osnovna šola Križevci, Enota Vrtec"
            "1127","1127","PV","kk","Osnovna šola Raka, Enota Vrtec"
            "1131","1131","PV","kk","Osnovna šola Adama Bohoriča Brestanica, enota Vrtec"
            "15209","15209","PV","kk","Osnovna šola XIV. divizije Senovo, Enota Vrtec"
            "3659","3659","PV","kk","Osnovna šola Leskovec pri Krškem, Enota vrtec"
            "15831","15831","PV","kk","OŠ Leskovec pri Krškem, Enota vrtec - ENOTA PIKA NOGAVIČKA"
            "18233","18233","PV","kk","OŠ Leskovec pri Krškem, Enota vrtec - ENOTA VILA"
            "15830","15830","PV","kk","OŠ Leskovec pri Krškem, Enota vrtec - ENOTA PETER PAN"
            "15829","15829","PV","kk","OŠ Leskovec pri Krškem, Enota vrtec - ENOTA MALI PRINC"
            "24335","24335","PV","kk","Osnovna šola Leskovec pri Krškem, Enota vrtec, enota DROBIŽEK"
            "3665","3665","PV","kk","Osnovna šola Koprivnica, Enota Vrtec"
            "1150","1150","PV","kk","Osnovna šola Podbočje, Enota Vrtec"
            "3648","3648","PV","kk","Vrtec Krško"
            "18810","3648","PV","kk","Vrtec Krško, enota Kekec"
            "18811","3648","PV","kk","Vrtec Krško, enota Pravljica"
            "18812","3648","PV","kk","Vrtec Krško, enota Grič"
            "18813","3648","PV","kk","Vrtec Krško, enota Dolenja vas"
            "18814","3648","PV","kk","Vrtec Krško, enota Zdole"
            "18815","3648","PV","kk","Vrtec Krško, enota Ciciban"
            "21955","3648","PV","kk","Vrtec Krško, enota Sonček"
            "3710","3710","PV","mb","Osnovna šola Kungota, Enota Vrtec Zgornja Kungota"
            "3661","3661","PV","ms","Osnovna šola Kuzma, Enota vrtec"
            "1057","1057","PV","ce","JVVZ Vrtec Laško"
            "14059","1057","PV","ce","JVVZ Vrtec Laško, ENOTA ZIDANI MOST"
            "14269","1057","PV","ce","JVVZ Vrtec Laško, ENOTA JURKLOŠTER"
            "14270","1057","PV","ce","JVVZ Vrtec Laško, ENOTA SEDRAŽ"
            "14271","1057","PV","ce","JVVZ Vrtec Laško, ENOTA RIMSKE TOPLICE"
            "14289","1057","PV","ce","JVVZ Vrtec Laško, ENOTA DEBRO"
            "15150","1057","PV","ce","JVVZ Vrtec Laško, ENOTA LAŠKO"
            "17390","1057","PV","ce","JVVZ Vrtec Laško, enota DRUŽINSKO VARSTVO GRADIŠNIK"
            "18530","1057","PV","ce","JVVZ Vrtec Laško, ENOTA ŠENTRUPERT"
            "18550","1057","PV","ce","JVVZ Vrtec Laško, ENOTA REČICA"
            "24355","1057","PV","ce","JVVZ Vrtec Laško, ENOTA VRH NAD LAŠKIM"
            "12609","12609","PV","mb","Osnovna šola Voličina, enota vrtec Voličina"
            "12610","12610","PV","mb","Osnovna šola Voličina, enota vrtec Selce"
            "10058","10058","PV","mb","Osnovna šola Lenart, Enota vrtec Lenart"
            "1184","1184","PV","ms","Vrtec Lendava Lendvai Ovoda"
            "14079","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE II LENDAVA"
            "14080","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE DOLGA VAS"
            "14081","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE GENTEROVCI"
            "14082","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE GABERJE"
            "14083","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE PETIŠOVCI"
            "14084","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, E HOTIZA"
            "14229","1184","PV","ms","Vrtec Lendava Lendvai Ovoda, DE I Lendava"
            "1068","1068","PV","za","Vrtec Litija"
            "12109","1068","PV","za","Vrtec Litija, Enota Medvedek"
            "12110","1068","PV","za","Vrtec Litija, Enota Najdihojca"
            "12111","1068","PV","za","Vrtec Litija, Enota Jurček"
            "12112","1068","PV","za","Vrtec Litija, Enota Kresnička"
            "12113","1068","PV","za","Vrtec Litija, Enota Sonček"
            "12114","1068","PV","za","Vrtec Litija, Enota Kekec"
            "12116","1068","PV","za","Vrtec Litija, Enota Taček"
            "21255","1068","PV","za","Vrtec Litija, Enota Ribica"
            "21256","1068","PV","za","Vrtec Litija, Enota Griček"
            "18450","18450","PV","za","Osnovna šola Gabrovka - Dole, Enota vrtec Čebelica"
            "1225","1225","PV","za","Osnovna šola Gabrovka - Dole, Enota Vrtec Čebelica"
            "17350","17350","PV","za","Osnovna šola Gabrovka - Dole, oddelek Vrtca Čebelica, Dole pri Litiji"
            "15769","15769","PV","za","Osnovna šola Litija, Enota Vrtec Polhek Polšnik"
            "1389","1389","PV","lj","Pingvin - Zavod za kulturo, izobraževanje in predšolsko vzgojo"
            "22355","1389","PV","lj","ZASEBNI VRTEC PINGVIN"
            "22356","1389","PV","lj","ZASEBNI VRTEC PINGVIN, oddelek"
            "12770","12770","PV","lj","Uršulinski zavod za vzgojo, izobraževanje in kulturo, Angelin vrtec"
            "19610","19610","PV","lj","G-Rega, zavod za rekreacijo, šport in prosti čas"
            "10051","10051","PV","lj","Vrtec Miškolin"
            "13609","10051","PV","lj","Vrtec Miškolin, Novo Polje"
            "13629","10051","PV","lj","Vrtec Miškolin, Rjava cesta"
            "13649","10051","PV","lj","Vrtec Miškolin, Sneberje"
            "13669","10051","PV","lj","Vrtec Miškolin, Zajčja Dobrava"
            "15449","10051","PV","lj","Vrtec Miškolin, Rjava cesta, oddelek blok"
            "19170","10051","PV","lj","Vrtec Miškolin, enota Zajčja Dobrava 2"
            "10053","10053","PV","lj","Vrtec Pod Gradom"
            "17290","10053","PV","lj","Vrtec Pod Gradom, enota Prule"
            "17291","10053","PV","lj","Vrtec Pod Gradom, enota Poljane"
            "17293","10053","PV","lj","Vrtec Pod Gradom, oddelek Zemljemerska"
            "17294","10053","PV","lj","Vrtec Pod Gradom, oddelek Poljanska"
            "17292","10053","PV","lj","Vrtec Pod Gradom, oddelek Stara Ljubljana"
            "10055","10055","PV","lj","Vrtec Ledina"
            "10056","10056","PV","lj","Vrtec Dr. France Prešeren"
            "21575","10056","PV","lj","Vrtec Dr. France Prešeren, enota Vrtača"
            "21576","10056","PV","lj","Vrtec Dr. France Prešeren, enota Prešernova"
            "21577","10056","PV","lj","Vrtec Dr. France Prešeren, enota Puharjeva"
            "10054","10054","PV","lj","Vrtec Kolezija"
            "14429","10054","PV","lj","Vrtec Kolezija, enota Kolezija"
            "14430","10054","PV","lj","Vrtec Kolezija, enota Murgle"
            "14431","10054","PV","lj","Vrtec Kolezija, enota Mencingerjeva"
            "18710","10054","PV","lj","Vrtec Kolezija, enota Kolezija, dislokacija Koseski"
            "10048","10048","PV","lj","Vrtec Galjevica"
            "14649","10048","PV","lj","Vrtec Galjevica, Enota Galjevica"
            "14650","10048","PV","lj","Vrtec Galjevica, Enota Orlova"
            "14651","10048","PV","lj","Vrtec Galjevica, Lokacija pot k ribniku"
            "14652","10048","PV","lj","Vrtec Galjevica, Lokacija Dolenjska cesta"
            "23296","10048","PV","lj","Vrtec Galjevica, Enota Jurček"
            "10057","10057","PV","lj","Vrtec Vrhovci"
            "12629","10057","PV","lj","Vrtec Vrhovci,enota Brdo"
            "12630","10057","PV","lj","Vrtec Vrhovci, enota Rožnik"
            "21535","10057","PV","lj","Vrtec Vrhovci, enota Vrhovci"
            "21815","10057","PV","lj","Vrtec Vrhovci, enota Tehnološki park"
            "21835","10057","PV","lj","Vrtec Vrhovci, enota Iga Grudna"
            "10049","10049","PV","lj","Vrtec Trnovo"
            "18770","10049","PV","lj","Vrtec Trnovo, enota Karunova"
            "18771","10049","PV","lj","Vrtec Trnovo, enota Trnovski pristan"
            "10052","10052","PV","lj","VRTEC VIŠKI GAJ"
            "12889","10052","PV","lj","VRTEC VIŠKI GAJ, ENOTA ZARJA"
            "12909","10052","PV","lj","VRTEC VIŠKI GAJ, ENOTA KOZARJE"
            "12910","10052","PV","lj","VRTEC VIŠKI GAJ, ENOTA BONIFACIJA"
            "12775","12775","PV","lj","Zavod Mali svet Ljubljana"
            "18890","12775","PV","lj","Zavod Mali svet, PE Mali svet"
            "19490","12775","PV","lj","Zavod Mali svet Ljubljana, enota Vič"
            "15849","15849","PV","lj","Inštitut montessori, Zavod za pomoč staršem pri razvoju otrok, PE Zasebni vrtec rumena hiša Dravlje"
            "19670","19670","PV","lj","Inštitut montessori, Zavod za pomoč staršem pri razvoju otrok, PE Zasebni vrtec oranžna hiša Dravlje"
            "23595","23595","PV","lj","Inštitut montessori, Zavod za pomoč staršem pri razvoju otrok, Zasebni vrtec zelena hiša otrok Podutik"
            "19550","19550","PV","lj","SONCE, Zavod za vzgojo in izobraževanje"
            "23855","19550","PV","po","SONCE, Zavod za vzgojo in izobraževanje, enota Waldorfski vrtec Slavček"
            "24175","19550","PV","lj","SONCE, Zavod za vzgojo in izobraževanje, OE Waldorfski vrtec Hišica sonca, PE Ljubljana"
            "19110","19110","PV","lj","Zakladnica Montessori, Zavod za vzgojo in izobraževanje"
            "19130","19110","PV","lj","Zakladnica Montessori, OE zasebni vrtec Hiša otrok srečnih rok"
            "18912","18912","PV","lj","Akademija montessori, OE zasebni vrtec Hiša otrok ABC"
            "18850","18850","PV","lj","Zasebni vrtec Metulj d.o.o."
            "19590","19590","PV","lj","Svetovalna Ustvarjalnica SU, PE Zasebni vrtec Volkec"
            "1069","1069","PV","lj","Vrtec Črnuče"
            "14189","1069","PV","lj","Vrtec Črnuče, ENOTA SAPRAMIŠKA"
            "14190","1069","PV","lj","Vrtec Črnuče, ENOTA SONČEK"
            "14191","1069","PV","lj","Vrtec Črnuče, ENOTA GMAJNA"
            "14192","1069","PV","lj","Vrtec Črnuče, ENOTA OSTRŽEK"
            "1070","1070","PV","lj","VRTEC JELKA"
            "12709","1070","PV","lj","VRTEC JELKA, ENOTA VILA"
            "12710","1070","PV","lj","VRTEC JELKA, ENOTA PALČKI"
            "12711","1070","PV","lj","VRTEC JELKA, ENOTA SNEGULJČICA"
            "13769","1070","PV","lj","VRTEC JELKA, ENOTA JELKA"
            "1071","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana"
            "10729","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana, enota Vetrnica"
            "10730","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana, enota Čira čara"
            "10731","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana, enota Kostanjčkov vrtec"
            "10732","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana, enota Stonoga"
            "10733","1071","PV","lj","VIZ Vrtec Mladi rod Ljubljana, enota Mavrica"
            "1072","1072","PV","lj","Vrtec Ciciban"
            "11149","1072","PV","lj","Vrtec Ciciban, Enota Ajda"
            "11151","1072","PV","lj","Vrtec Ciciban, Enota Lenka"
            "11152","1072","PV","lj","Vrtec Ciciban, Enota Čebelica"
            "11154","1072","PV","lj","Vrtec Ciciban, Enota Pastirčki"
            "11155","1072","PV","lj","Vrtec Ciciban, Enota Mehurčki"
            "11156","1072","PV","lj","Vrtec Ciciban, DE Lenka"
            "18750","1072","PV","lj","Vrtec Ciciban, enota Žabice"
            "1073","1073","PV","lj","Vrtec Jarše"
            "13129","1073","PV","lj","Vrtec Jarše, enota Kekec"
            "13130","1073","PV","lj","Vrtec Jarše, enota Mojca"
            "13131","1073","PV","lj","Vrtec Jarše, enota Rožle"
            "13132","1073","PV","lj","Vrtec Jarše, enota Rožle - hiša Zelena jama"
            "1074","1074","PV","lj","Vrtec Najdihojca"
            "12929","1074","PV","lj","Vrtec Najdihojca, Enota Palček"
            "12949","1074","PV","lj","Vrtec Najdihojca, Enota Čenča"
            "12969","1074","PV","lj","Vrtec Najdihojca, Enota Palček, dislocirani oddelki Kebetova"
            "12970","1074","PV","lj","Vrtec Najdihojca, Enota Biba"
            "12971","1074","PV","lj","Vrtec Najdihojca, Enota Biba, dislocirani oddelki LEK"
            "18650","1074","PV","lj","Vrtec Najdihojca, enota AETERNIA"
            "1104","1104","PV","lj","Vrtec Otona Župančiča Ljubljana"
            "10649","1104","PV","lj","Vrtec Otona Župančiča Ljubljana, enota Mehurčki"
            "10650","1104","PV","lj","Vrtec Otona Župančiča Ljubljana, enota Čebelica"
            "10651","1104","PV","lj","Vrtec Otona Župančiča Ljubljana, enota Ringaraja"
            "13309","1104","PV","lj","Vrtec Otona Župančiča Ljubljana, Enota Čurimuri"
            "13310","1104","PV","lj","Vrtec Otona Župančiča Ljubljana, Enota Živ-Žav"
            "1115","1115","PV","lj","Vrtec Hansa Christiana Andersena"
            "11829","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Marjetica"
            "11830","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Andersen"
            "11831","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Lastovica"
            "11832","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Palčica"
            "14209","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Polžek"
            "16049","1115","PV","lj","Vrtec Hansa Christiana Andersena, enota Krtek"
            "1116","1116","PV","lj","Vrtec Šentvid"
            "12849","1116","PV","lj","Vrtec Šentvid, Enota Sapramiška"
            "12850","1116","PV","lj","Vrtec Šentvid, Enota Mravljinček"
            "12851","1116","PV","lj","Vrtec Šentvid, Enota Vid"
            "18630","1116","PV","lj","Vrtec Šentvid, Enota Mišmaš"
            "1119","1119","PV","lj","Vrtec Zelena jama"
            "13069","1119","PV","lj","Vrtec Zelena jama, Enota Vrba"
            "13070","1119","PV","lj","Vrtec Zelena jama, Enota Zelena jama"
            "13071","1119","PV","lj","Vrtec Zelena jama, Enota Zmajček"
            "24156","1119","PV","lj","Vrtec Zelena jama, Enota Zmajčica"
            "24157","1119","PV","lj","Vrtec Zelena jama, Enota Vejica"
            "12789","12789","PV","lj","Osnovna šola Danile Kumar, International Kindergarten"
            "1211","1211","PV","lj","Vrtec Mojca Ljubljana"
            "14709","1211","PV","lj","VRTEC MOJCA ENOTA MUCA"
            "14710","1211","PV","lj","VRTEC MOJCA ENOTA KEKEC"
            "15029","1211","PV","lj","VRTEC MOJCA ENOTA MOJCA"
            "14711","1211","PV","lj","VRTEC MOJCA ENOTA TINKARA"
            "14712","1211","PV","lj","VRTEC MOJCA ENOTA ROŽLE"
            "3674","3674","PV","lj","Viški vrtci"
            "16870","3674","PV","lj","Viški vrtci, enota Jamova"
            "16890","3674","PV","lj","Viški vrtci, enota Bičevje"
            "16910","3674","PV","lj","Viški vrtci, enota Rožna dolina"
            "16911","3674","PV","lj","Viški vrtci, enota Hiša pri ladji"
            "24055","3674","PV","lj","Viški vrtci, enota Hiša pri ladji, lokacija Študentski dom"
            "10050","10050","PV","lj","Vrtec Vodmat"
            "11909","10050","PV","lj","Vrtec Vodmat, enota KLINIČNI CENTER"
            "1197","1197","PV","lj","Vrtec Pedenjped"
            "11869","1197","PV","lj","Vrtec Pedenjped, enota Učenjak"
            "13549","1197","PV","lj","Vrtec Pedenjped, Enota Korenjak"
            "13249","1197","PV","lj","Vrtec Pedenjped, Enota Radovednež"
            "13569","1197","PV","lj","Vrtec Pedenjped, Enota Vrtnar"
            "13589","1197","PV","lj","Vrtec Pedenjped, Enota Potepuh"
            "17830","1197","PV","lj","Vrtec Pedenjped, enota Pedenjškrat"
            "17831","1197","PV","lj","Vrtec Pedenjped, enota Zalog, oddelek v OŠ Nove Fužine"
            "23555","1197","PV","lj","Vrtec Pedenjped, enota PEDENJCARSTVO"
            "9231","9231","PV","lj","Waldorfska šola Ljubljana, enota Vrtec"
            "16750","16750","PV","kr","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Gorenjska, Enota Vrtec"
            "16751","16751","PV","kr","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Gorenjska, Vrtec Radovljica"
            "19250","19250","PV","ms","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Pomurje, Enota Vrtec"
            "18232","18232","PV","ce","Waldorfska šola Ljubljana, OE Waldorfski vrtec in šola Celje, Enota Vrtec"
            "21435","21435","PV","lj","Zavod Sv. Stanislava, Vrtec Dobrega pastirja"
            "1384","1384","PV","lj","Waldorfski vrtec Mavrica"
            "1383","1383","PV","lj","Vrtec Nazaret"
            "18910","1383","PV","lj","Vrtec Nazaret"
            "21405","21405","PV","lj","NBK, svetovanje in raziskave, d.o.o., PE Zasebni vrtec, Mini vrtec"
            "22175","22175","PV","lj","Moja zgodba, vzgoja in varstvo predšolskih otrok, d.o.o., PE Zasebni vrtec Fridolin"
            "21616","21616","PV","lj","Otroška igralnica Malina d.o.o., PE Zasebni vrtec Malina"
            "20756","20756","PV","lj","Zavod zasebni vrtec Bosopet"
            "21517","21517","PV","lj","Montessori - zavod zasebni vrtec Cinca Binca za vzgojo in izobraževanje"
            "23795","21517","PV","lj","Enota Hiša otrok Cinca Binca - Galjevica"
            "21515","21515","PV","lj","Waldorfski zasebni vrtec Lira"
            "21455","21455","PV","lj","Vrtec Zvezdica, zasebno varstvo otrok, d.o.o."
            "22215","22215","PV","lj","Vesolje malčkov, zasebni vrtec, d.o.o."
            "22235","22215","PV","lj","Vesolje malčkov, zasebni vrtec, d.o.o., enota"
            "22755","22755","PV","lj","Zavod Eneja so.p.,"
            "22777","22755","PV","lj","Zavod Eneja so.p., PE Zasebni vrtec Lunba"
            "22475","22475","PV","lj","Modra pikica, Zavod za izobraževanje"
            "23895","22475","PV","lj","Modra pikica, Zavod za izobraževanje, PE Hiša otrok Bežigrad, zasebni montessori vrtec, enota"
            "23896","22475","PV","lj","Modra pikica, Zavod za izobraževanje, PE Hiša otrok Bežigrad 2, zasebni montessori vrtec, enota"
            "22775","22775","PV","lj","Rastem z montessori, zavod za vzgojo in izobraževanje"
            "22776","22775","PV","lj","Rastem z Montessori, zavod, PE Zasebni vrtec Hiša otrok Ivančna Gorica"
            "23495","23495","PV","lj","Montessori sanje d.o.o."
            "24195","24195","PV","lj","Montessori za življenje d.o.o."
            "24196","24195","PV","lj","Montessori za življenje d.o.o., PE Sončna hiša otrok, zasebni vrtec"
            "1278","1278","PV","ce","Osnovna šola Ljubno ob Savinji, Enota Vrtec pri šoli, Vrtec Ljubno ob Savinji"
            "1097","1097","PV","ms","VRTEC LJUTOMER"
            "12869","1097","PV","ms","VRTEC LJUTOMER, VRTEC GRESOVŠČAK"
            "1136","1136","PV","ms","Osnovna šola Mala Nedelja, Enota vrtec"
            "1137","1137","PV","ms","Osnovna šola Janka Ribiča Cezanjevci, Enota Vrtec"
            "3658","3658","PV","ms","Osnovna šola Stročja vas, Enota Vrtec"
            "1139","1139","PV","ms","Osnovna šola Ivana Cankarja Ljutomer, enota Vrtec"
            "1075","1075","PV","lj","VRTEC KURIRČEK LOGATEC"
            "11471","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA HOTEDRŠICA"
            "11470","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA ROVTE"
            "12149","1075","PV","lj","VRTEC KURIRČEK LOGATEC - ENOTA TABOR"
            "16029","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA TIČNICA"
            "18512","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA POD KOŠEM"
            "18410","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA LAZE"
            "15069","1075","PV","lj","VRTEC KURIRČEK LOGATEC, ENOTA CENTRALNI VRTEC"
            "1213","1213","PV","lj","Miklavžev vrtec - župnijski vrtec"
            "16289","16289","PV","lj","Zavod za predšolsko vzgojo ZVEZDICA, Dragomer"
            "20955","16289","PV","lj","Zavod za predšolsko vzgojo ZVEZDICA, Dragomer, enota"
            "11530","11530","PV","lj","Osnovna šola Log - Dragomer, Enota vrtec Log - Dragomer"
            "20635","20635","PV","lj","LEILA d.o.o., OE Mala akademija"
            "9901","9901","PV","po","Osnovna šola heroja Janeza Hribarja Stari trg pri Ložu, Vrtec Polhek"
            "23416","23416","PV","po","Osnovna šola heroja Janeza Hribarja Stari trg pri Ložu, Vrtec Polhek, enota Iga vas"
            "1161","1161","PV","nm","Osnovna šola dr. Antona Debeljaka Loški potok, Enota Vrtec"
            "1058","1058","PV","mb","Osnovna šola Lovrenc na Pohorju, Vrtec Lovrenc na Pohorju"
            "3742","3742","PV","ce","Osnovna šola Blaža Arniča Luče, Enota Vrtec Luče"
            "11929","11929","PV","ce","VVE Solčava"
            "1254","1254","PV","lj","Osnovna šola Janka Kersnika Brdo, VVE Medo"
            "15329","15329","PV","lj","VRTEC MEDO Prevoje"
            "15332","15332","PV","lj","VRTEC MEDO Krašnja"
            "3748","3748","PV","mb","Osnovna šola Majšperk, Vrtec Majšperk"
            "12769","12769","PV","mb","Zavod za razvoj waldorfske pedagogike Maribor"
            "12749","12749","PV","mb","Zavod Antona Martina Slomška, Hiša otrok - vrtec montessori"
            "18350","12749","PV","mb","Zavod Antona Martina Slomška, Hiša otrok-vrtec montessori, podenota Tezno"
            "20895","20895","PV","mb","Zavod Za življenje Maribor, PE Zasebni vrtec Hiša otrok montessori"
            "19050","19050","PV","mb","Inštitut Sofijin izvir Maribor, Zasebni waldorfski vrtec Studenček"
            "24095","19050","PV","mb","Institut Sofijin izvir, zdravilna pedagogika, Zasebni vrtec Studenček, PE Čarobni vrt"
            "1056","1056","PV","mb","JVIZ Vrtec Otona Župančiča"
            "14949","1056","PV","mb","JVIZ OTONA ŽUPANČIČA MARIBOR, OE MEHURČKI"
            "14950","1056","PV","mb","JVIZ Vrtec Otona Župančiča Maribor, OE Lenka"
            "1076","1076","PV","mb","Vrtec Borisa Pečeta"
            "14669","1076","PV","mb","Vrtec Borisa Pečeta, Enota Košaki"
            "14670","1076","PV","mb","Vrtec Borisa Pečeta, Enota Kamnica"
            "14671","1076","PV","mb","Vrtec Borisa Pečeta, Enota Bresternica"
            "1078","1078","PV","mb","JVIZ Vrtec Ivana Glinška Maribor"
            "14449","1078","PV","mb","Vrtec Ivana Glinška, enota Pristan"
            "14450","1078","PV","mb","Vrtec Ivana Glinška, enota Gregorčičeva - Krekova"
            "14452","1078","PV","mb","Vrtec Ivana Glinška Maribor, enota Gledališka"
            "14453","1078","PV","mb","Vrtec Ivana Glinška, enota Ribiška"
            "14454","1078","PV","mb","Vrtec Ivana Glinška, enota Smetanova"
            "14455","1078","PV","mb","Vrtec Ivana Glinška, enota Kosarjeva"
            "1080","1080","PV","mb","Vrtec Jadvige Golež"
            "18491","1080","PV","mb","Vrtec Jadvige Golež, Cesta zmage"
            "18492","1080","PV","mb","Vrtec Jadvige Golež, Ob gozdu, Ertlova"
            "1060","1060","PV","mb","Vrtec Studenci Maribor"
            "10689","1060","PV","mb","Vrtec Studenci Maribor, OE Iztokova"
            "10690","1060","PV","mb","Vrtec Studenci Maribor, OE Pekre"
            "14169","1060","PV","mb","Vrtec Studenci Maribor, OE Radvanje"
            "14170","1060","PV","mb","Vrtec Studenci Maribor, OE Limbuš"
            "14171","1060","PV","mb","Vrtec Studenci Maribor, OE Pekrska"
            "1098","1098","PV","mb","Vrtec Jožice Flander"
            "12169","1098","PV","mb","Vrtec Jožice Flander, OE Fochova"
            "12189","1098","PV","mb","Vrtec Jožice Flander, OE VANČKA ŠARHA"
            "12209","1098","PV","mb","Vrtec Jožice Flander, OE RAZVANJE"
            "1099","1099","PV","mb","VRTEC TEZNO- UPRAVA , ENOTA MIŠMAŠ"
            "14029","1099","PV","mb","VRTEC TEZNO, ENOTA PEDENJPED VRTEC IN JASLI"
            "14030","1099","PV","mb","VRTEC TEZNO ENOTA LUPINICA VRTEC IN JASLI"
            "14031","1099","PV","mb","VRTEC TEZNO, ENOTA MEHURČKI"
            "19190","19190","PV","mb","Center za sluh in govor Maribor, OE Osnovna šola in vrtec"
            "1209","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor"
            "14853","1209","PV","mb","JVIZ VRTEC POBREŽJE"
            "15349","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Kekec"
            "15649","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Mojca"
            "15650","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Čebelica"
            "15651","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Ob gozdu"
            "15652","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Najdihojca"
            "15653","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Brezje"
            "15654","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota GRINČ"
            "15655","1209","PV","mb","JVIZ Vrtec Pobrežje Maribor, enota Jasli Grinič"
            "6681","6681","PV","mb","Osnovna šola Gustava Šiliha Maribor, enota vrtca Lenka"
            "19210","19210","PV","mb","Osnovna šola Gustava Šiliha Maribor, enota vrtca Mehurčki"
            "19211","19211","PV","mb","Osnovna šola Gustava Šiliha Maribor, enota vrtca Veveriček"
            "23735","23735","PV","mb","Osnovna šola Gustava Šiliha Maribor, enota vrtca Sovice"
            "23975","23975","PV","mb","Zasebni zavod vrtec za kreativno varstvo otrok Pika Poka"
            "23015","23015","PV","mb","Zasebni zavod vrtec Želvica"
            "23575","23575","PV","mb","Zasebni zavod otroško veselje, Vrtec Copatki Maribor"
            "24455","24455","PV","mb","Zasebni zavod Nanin vrtec, vzgoja in varstvo otrok Maribor"
            "10089","10089","PV","mb","Osnovna šola Markovci, Enota Vrtec"
            "15089","15089","PV","lj","Župnijski zavod Sv. Janeza Krstnika, OE Rahelin vrtec - hiša otrok montessori"
            "1273","1273","PV","lj","Vrtec Medvode"
            "14889","1273","PV","lj","Vrtec Medvode, enota Ostržek"
            "14890","1273","PV","lj","Vrtec Medvode, enota Preska"
            "14891","1273","PV","lj","Vrtec Medvode, enota Ostržek, DO Senica"
            "14892","1273","PV","lj","Vrtec Medvode, enota Pirniče"
            "14893","1273","PV","lj","Vrtec Medvode, enota Smlednik"
            "14894","1273","PV","lj","Vrtec Medvode, enota Medvoška"
            "21355","1273","PV","lj","Vrtec Medvode, enota Sora"
            "1331","1331","PV","lj","Vrtec Mengeš"
            "15569","1331","PV","lj","Vrtec Mengeš, Enota Sonček"
            "21275","1331","PV","lj","Vrtec Mengeš, enota Oblaček"
            "22695","1331","PV","lj","Vrtec Mengeš, enota Gobica"
            "1189","1189","PV","nm","OTROŠKI VRTEC METLIKA"
            "9598","9598","PV","sg","Osnovna šola Mežica, Enota vrtec Mežica"
            "23835","23835","PV","sg","Hiša otrok Mali koraki - zasebni Montessori vrtec, Mežica"
            "11030","11030","PV","mb","Osnovna šoli Miklavž na Dravskem polju, Vrtec Ciciban"
            "3828","3828","PV","mb","Osnovna šola Miklavž na Dravskem polju, Vrtec Vrtiljak"
            "21775","21775","PV","ng","Antonina, zavod za vzgojo, izobraževanje in družino, Hiša otrok Antonina, zasebni vrtec montessori"
            "10629","10629","PV","ng","Osnovna šola Miren, Enota vrtec Bilje"
            "11370","11370","PV","ng","Osnovna šola Miren, Enota vrtec Miren"
            "11371","11371","PV","ng","Osnovna šola Miren, Enota Vrtec Opatje selo"
            "11372","11372","PV","ng","Osnovna šola Miren, Enota vrtec Kostanjevica"
            "3694","3694","PV","nm","Osnovna šola Mirna, Enota Vrtec Mirna"
            "3666","3666","PV","nm","Osnovna šola Toneta Pavčka, Vrtec Cepetavček"
            "3692","3692","PV","nm","OŠ Mokronog, Vrtec Mokronožci"
            "12549","12549","PV","lj","Osnovna šola Jurija Vege, VVE Vojke Napokoj"
            "14809","14809","PV","ms","Dvojezična osnovna šola Prosenjakovci, vrtec Hodoš"
            "14810","14810","PV","ms","Dvojezična osnovna šola Prosenjakovci, Vrtec Domanjševci"
            "1320","1320","PV","ms","Vrtci občine Moravske Toplice"
            "11889","1320","PV","ms","Vrtci Občine Moravske Toplice, enota Bogojina"
            "11890","1320","PV","ms","Vrtci Občine Moravske Toplice, enota Filovci"
            "11891","1320","PV","ms","Vrtci Občine Moravske Toplice, enota Fokovci"
            "11892","1320","PV","ms","Vrtci Občine Moravske Toplice, enota Martjanci"
            "11893","1320","PV","ms","Vrtci Občine Moravske Toplice, enota Moravske Toplice"
            "11894","1320","PV","ms","Vrtci Občine Moravske Toplice, dvojezična enota Prosenjakovci"
            "1191","1191","PV","ce","JVIZ Mozirje, OE Vrtec Mozirje"
            "12649","1191","PV","ce","JVIZ Mozirje, OE Vrtec Mozirje, DE Mozirje"
            "12650","1191","PV","ce","JVIZ Mozirje, OE Vrtec Mozirje, DE Rečica ob Savinji"
            "1100","1100","PV","ms","Vrtec Murska Sobota"
            "18970","1100","PV","ms","Vrtec Murska Sobota, enota MIŠKE"
            "18971","1100","PV","ms","Vrtec Murska Sobota, enota KRTEK"
            "18990","1100","PV","ms","Vrtec Murska Sobota, enota Srnica"
            "18991","1100","PV","ms","Vrtec Murska Sobota, enota ROMANO"
            "18992","1100","PV","ms","Vrtec Murska Sobota, enota VEVERIČKA"
            "18996","1100","PV","ms","Vrtec Murska Sobota, enota GOZDIČEK"
            "18993","1100","PV","ms","Vrtec Murska Sobota, enota URŠKA"
            "18994","1100","PV","ms","Vrtec Murska Sobota, enota RINGARAJA"
            "18995","1100","PV","ms","Vrtec Murska Sobota, enota ČEBELICA"
            "1387","1387","PV","ms","Miklavžev zavod Murska Sobota - Vrtec Lavra"
            "16349","1387","PV","ms","Miklavžev zavod Murska Sobota, Vrtec Lavra - enota Tišina"
            "1160","1160","PV","sg","Osnovna šola Muta, VVE Muta"
            "1311","1311","PV","kr","Osnovna šola Naklo, OE Vrtec, Vrtec Mlinček"
            "19390","19390","PV","kr","Osnovna šola Naklo, OE Vrtec, Vrtec Jelka"
            "9599","9599","PV","ce","VVE pri OŠ Nazarje"
            "1110","1110","PV","ng","Vrtec Nova Gorica"
            "12289","1110","PV","ng","Vrtec Nova Gorica, Enota Centralni vrtec"
            "12309","1110","PV","ng","Vrtec Nova Gorica, Enota Najdihojca"
            "12329","1110","PV","ng","Vrtec Nova Gorica, Enota Čriček"
            "12349","1110","PV","ng","Vrtec Nova Gorica, Enota Julka Pavletič Solkan"
            "12369","1110","PV","ng","Vrtec Nova Gorica, Enota Kekec"
            "12370","1110","PV","ng","Vrtec Nova Gorica, Enota Kurirček"
            "12371","1110","PV","ng","Vrtec Nova Gorica, Enota Mojca"
            "19290","1110","PV","ng","Vrtec Nova Gorica, enota Ciciban, Cankarjeva 1"
            "1145","1145","PV","ng","Osnovna šola Branik, Vrtec Rastja"
            "1146","1146","PV","ng","Osnovna šola Dornberk, enota vrtca Dornberk"
            "15790","15790","PV","ng","Osnovna šola Dornberk, enota vrtca Prvačina"
            "3669","3669","PV","ng","VVE pri OŠ Šempas"
            "15589","15589","PV","ng","Osnovna šola Čepovan, Vrtec Čepovan"
            "1175","1175","PV","ng","Osnovna šola Solkan, enota Vrtec Solkan"
            "13749","13749","PV","ng","Osnovna šola Solkan, Enota Vrtec Grgar"
            "13750","13750","PV","ng","Osnovna šola Solkan, enota Vrtec Trnovo"
            "20876","20876","PV","ng","Hiša otrok Frančiška Sedeja, vzgoja in varstvo predšoslkih otrok d.o.o."
            "9871","9871","PV","nm","Vrtec Pedenjped Novo mesto"
            "11409","9871","PV","nm","Vrtec Pedenjped Novo mesto, METKA"
            "11410","9871","PV","nm","Vrtec Pedenjped Novo mesto, OSTRŽEK"
            "11411","9871","PV","nm","Vrtec Pedenjped Novo mesto, PEDENJPED"
            "11412","9871","PV","nm","Vrtec Pedenjped Novo mesto, PIKAPOLONICA"
            "11413","9871","PV","nm","Vrtec Pedenjped Novo mesto, RDEČA KAPICA"
            "11414","9871","PV","nm","Vrtec Pedenjped Novo mesto, VIDEK"
            "17671","9871","PV","nm","Vrtec Pedenjped Novo mesto, SAPRAMIŠKA"
            "17672","9871","PV","nm","Vrtec Pedenjped Novo mesto, ŠKRATEK"
            "18590","9871","PV","nm","Vrtec Pedenjped Novo mesto, CEPETAVČEK"
            "17950","17950","PV","nm","JANA, predšolska vzgoja, svetovanje, storitve, d.o.o., Zasebni vrtec"
            "1109","1109","PV","nm","Vrtec Ciciban Novo mesto"
            "12449","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota CICIBAN"
            "12469","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota KEKEC"
            "12470","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota NAJDIHOJCA"
            "12471","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota MARJETICA"
            "12489","1109","PV","nm","Vrtec Ciciban Novo mesto, enota PALČEK - bolnišnični oddelek"
            "18150","1109","PV","nm","Vrtec Ciciban Novo mesto, enota ŽABICA"
            "12450","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota LABOD"
            "12451","1109","PV","nm","Vrtec Ciciban Novo mesto, Enota BIBE"
            "18611","1109","PV","nm","Vrtec Ciciban Novo mesto, enota MEHURČKI"
            "3670","3670","PV","nm","Osnovna šola Brusnice, vrtec Brusnice"
            "1154","1154","PV","nm","Osnovna šola Stopiče, Vrtec Stopiče"
            "1390","1390","PV","nm","Petrov vrtec Šentpeter"
            "20875","1390","PV","nm","Petrov vrtec Šentpeter, enota Petrov dom"
            "20755","20755","PV","nm","Zasebni družinski vrtec Ringa raja"
            "22835","22835","PV","nm","Modrinček, zasebni vrtec, d.o.o."
            "3714","3714","PV","ms","Osnovna šola Odranci, Vrtec Mavrica"
            "15253","15253","PV","mb","Osnovna šola Miklavž pri Ormožu, enota Vrtec Kog"
            "15252","15252","PV","mb","Osnovna šola Miklavž pri Ormožu, enota Vrtec Miklavž pri Ormožu"
            "23195","23195","PV","mb","Osnovna šola Velika Nedelja, Enota vrtca v Podgorcih"
            "1190","1190","PV","mb","Vzgojno izobraževalni zavod Vrtec Ormož"
            "15249","1190","PV","mb","Vzgojnoizobraževalni zavod vrtec Ormož, enota Velika Nedelja"
            "15250","1190","PV","mb","Vzgojnoizobraževali zavod vrtec Ormož, enota Podgorci"
            "15251","15251","PV","mb","Osnovna šola Ivanjkovci, vrtec Ivanjkovci"
            "1263","1263","PV","mb","VVE pri OŠ Pesnica"
            "3707","3707","PV","mb","Osnovna šola Jakobski Dol, enota vrtec"
            "3706","3706","PV","mb","Osnovna šola Jarenina, Vrtec Jarenina"
            "1101","1101","PV","kp","Vrtec Mornarček Piran"
            "14689","1101","PV","kp","Vrtec Mornarček Piran, Enota Barčica Portorož"
            "1155","1155","PV","kp","Osnovna šola Sečovlje, enota Vrtec"
            "24215","24215","PV","kp","Osnovna šola Sečovlje, Podružnični vrtec Sveti Peter"
            "1192","1192","PV","kp","Vrtec Morje Lucija"
            "17090","1192","PV","kp","Vrtec Morje Lucija, Enota Ježek"
            "17091","1192","PV","kp","Vrtec Morje Lucija, Enota Vrtec Strunjan"
            "1196","1196","PV","kp","Vrtec La Coccinella Piran"
            "17130","1196","PV","kp","Vrtec La Coccinella Piran, enota Piran"
            "17131","1196","PV","kp","Vrtec La Coccinella Piran, enota Strunjan"
            "17132","1196","PV","kp","Vrtec La Coccinella Piran, enota Sečovlje"
            "17133","1196","PV","kp","Vrtec La Coccinella Piran, matična enota Lucija"
            "17150","1196","PV","kp","Vrtec La Coccinella Piran, oddelek Ježek Lucija"
            "1156","1156","PV","po","Osnovna šola Košana, enota Vrtec Košana"
            "3721","3721","PV","po","VRTEC PRI OŠ PIVKA"
            "19150","19150","PV","po","Osnovna šola Pivka, enota Vrtec Mavrica"
            "19151","19151","PV","po","Osnovna šola Pivka, enota Vrtec Vetrnica"
            "1247","1247","PV","ce","Osnovna šola Podčetrtek, Vrtec Podčetrtek z oddelki v Podčetrtku in Pristavi pri Mestinju"
            "19710","19710","PV","mb","Osnovna šola Podlehnik, Enota Vrtec"
            "14311","14311","PV","sg","Osnovna šola Brezno - Podvelka, Oddelek vrtca Lehen"
            "14310","14310","PV","sg","Osnovna šola Brezno - Podvelka, Oddelek vrtca Brezno"
            "14312","14312","PV","sg","Osnovna šola Brezno - Podvelka, Oddelek vrtca Kapla"
            "14313","14313","PV","sg","Osnovna šola Brezno - Podvelka, Oddelek vrtca Podvelka"
            "16389","16389","PV","sg","Osnovna šola Brezno - Podvelka, Oddelek vrtca Ožbalt"
            "1365","1365","PV","ce","Osnovna šola Polzela, Enota Vrtec Polzela"
            "21495","21495","PV","ce","VVE pri OŠ Polzela, oddelek"
            "1059","1059","PV","po","Vrtec Postojna"
            "18292","1059","PV","po","Vrtec Postojna, Enota Ravbarček"
            "18290","1059","PV","po","Vrtec Postojna, Enota Zmajček"
            "18291","1059","PV","po","Vrtec Postojna, Enota Škratek"
            "19990","1059","PV","po","Vrtec Postojna, Enota Pudgurček"
            "20315","1059","PV","po","Vrtec Postojna, Enota Pastirček"
            "15189","15189","PV","po","Osnovna šola Prestranek, Vrtec Prestranek"
            "3768","3768","PV","ce","Osnovna šola Prebold, enota Vrtec"
            "20835","20835","PV","kr","Vrtec pri OŠ Matija Valjavec Preddvor, enota Palček Zg. Jezersko"
            "1125","1125","PV","kr","Vrtec pri OŠ Matije Valjavca Preddvor, Enota Storžek Preddvor"
            "17490","17490","PV","kr","Vrtec pri OŠ Matije Valjavca Preddvor, Enota Čriček Zg. Bela"
            "21215","21215","PV","kr","Osnovna šola Matije Valjavca Preddvor, Podružnica Kokra"
            "13149","13149","PV","sg","POŠ, Osnovna šola Franja Goloba Prevalje, Vrtec Leše"
            "20915","20915","PV","sg","OSNOVNA ŠOLA FRANJA GOLOBA PREVALJE VRTEC KROJAČEK HLAČEK PREVALJE, enota"
            "23315","23315","PV","sg","OSNOVNA ŠOLA FRANJA GOLOBA PREVALJE VRTEC KROJAČEK HLAČEK PREVALJE, enota, enota"
            "1085","1085","PV","mb","Vrtec Ptuj"
            "14109","1085","PV","mb","Vrtec Ptuj, Enota Zvonček"
            "14110","1085","PV","mb","Vrtec Ptuj, Enota Deteljica"
            "18997","1085","PV","mb","Vrtec Ptuj, Enota Podlesek"
            "14111","1085","PV","mb","Vrtec Ptuj, Enota Marjetica"
            "14112","1085","PV","mb","Vrtec Ptuj, Enota Tulipan"
            "14113","1085","PV","mb","Vrtec Ptuj, Enota Spominčica"
            "14114","1085","PV","mb","Vrtec Ptuj, Enota Mačice"
            "14115","1085","PV","mb","Vrtec Ptuj, Enota Vijolica"
            "14116","1085","PV","mb","Vrtec Ptuj, Enota Narcisa"
            "14117","1085","PV","mb","Vrtec Ptuj, Enota Trobentica"
            "20995","20995","PV","mb","Zasebni vrtec Vilinski gaj, zasebni vzgojno-izobraževalni zavod"
            "22655","20995","PV","mb","Zasebni vrtec Vilinski gaj, zasebni vzgojno-izobraževalni zavod, enota MEZINČICA"
            "24255","20995","PV","mb","Zasebni vrtec Vilinski gaj, zasebni vzgojno-izobraževalni zavod, enota VIKTORINČEK"
            "3662","3662","PV","ms","Vrtec pri OŠ Puconci"
            "15689","15689","PV","ms","Vrtec pri OŠ Puconci, enota Puconci"
            "15690","15690","PV","ms","Vrtec pri OŠ Puconci, enota Brezovci"
            "15691","15691","PV","ms","Vrtec pri OŠ Puconci, enota Bodonci"
            "15692","15692","PV","ms","Vrtec pri OŠ Puconci, enota Mačkovci"
            "17570","17570","PV","ms","Vrtec pri OŠ Puconci, enota šola"
            "1329","1329","PV","mb","Osnovna šola Fram, Enota Vrtec Fram"
            "1285","1285","PV","mb","Osnovna šola Rače, enota vrtec Rače"
            "3823","3823","PV","kk","Javni zavod Osnovna šola Marjana Nemca Radeče, Enota Vrtec"
            "1092","1092","PV","ms","Vrtec Radenci-Radenski mehurčki"
            "14369","1092","PV","ms","Vrtec Radenci-Radenski mehurčki, enota Grozdek"
            "12069","12069","PV","sg","Osnovna šola Radlje ob Dravi, Enota vrtca Vuhred"
            "12049","12049","PV","sg","Osnovna šola Radlje ob Dravi, Enota vrtca Radlje"
            "22795","22795","PV","sg","Osnovna šola Radlje ob Dravi, Enota Vrtca Remšnik"
            "21035","21035","PV","kr","Vzgojno-izobraževalni zavod Montessori, Zasebni vrtec Montessori, Gorenjska hiša otrok"
            "21635","21035","PV","lj","Vzgojno-izobraževalni zavod Montessori, Zasebni vrtec Montessori, Vrhniška hiša otrok"
            "1212","1212","PV","kr","Vrtec Radovljica"
            "12409","1212","PV","kr","Vrtec Radovljica, Enota Radovljica"
            "12389","1212","PV","kr","Vrtec Radovljica, Enota Begunje"
            "12414","1212","PV","kr","Vrtec Radovljica, Enota Kamna Gorica"
            "12410","1212","PV","kr","Vrtec Radovljica, Enota Lesce"
            "12411","1212","PV","kr","Vrtec Radovljica, Enota Kropa"
            "12412","1212","PV","kr","Vrtec Radovljica, Enota Posavec"
            "12413","1212","PV","kr","Vrtec Radovljica, Enota Brezje"
            "21235","1212","PV","kr","Vrtec Radovljica, enota"
            "24037","1212","PV","kr","Vrtec Radovljica, enota Čebelica"
            "22995","1212","PV","sg","Vrtec Ravne na Koroškem, oddelek na OŠ Prežihov Voranc"
            "1081","1081","PV","sg","Vrtec Ravne na Koroškem"
            "11849","1081","PV","sg","Vrtec Ravne na Koroškem, ENOTA SOLZICE"
            "11850","1081","PV","sg","Vrtec Ravne na Koroškem, ENOTA AJDA"
            "11851","1081","PV","sg","Vrtec Ravne na Koroškem, ENOTA LEVI DEVŽEJ"
            "1180","1180","PV","ms","Osnovna šola Razkrižje, Enota Vrtec"
            "1173","1173","PV","ng","Osnovna šola Lucijana Bratkoviča Bratuša Renče, Vrtec pri OŠ Renče"
            "12809","12809","PV","ng","Osnovna šola Lucijana Bratkoviča Bratuša Renče, Vrtec pri OŠ Renče, enota Bukovica"
            "1188","1188","PV","nm","Vrtec Ribnica"
            "1158","1158","PV","sg","Osnovna šola Ribnica na Pohorju, Enota Vrtec Ribnica na Pohorju"
            "3650","3650","PV","ce","JVIZ Vrtec Rogaška Slatina"
            "17310","3650","PV","ce","JVIZ Vrtec Rogaška Slatina, enota POTOCEK"
            "17313","3650","PV","ce","JVIZ Vrtec Rogaška Slatina, enota IZVIR"
            "17311","3650","PV","ce","JVIZ Vrtec Rogaška Slatina, enota STUDENČEK"
            "17312","3650","PV","ce","JVIZ Vrtec Rogaška Slatina, enota KAPLJICA"
            "1142","1142","PV","ms","Osnovna šola Sveti Jurij, Enota Vrtec"
            "15111","15111","PV","ms","Osnovna šola Sveti Jurij, Enota Vrtec Rogašovci"
            "15110","15110","PV","ms","Osnovna Sveti Jurij, Enota Vrtec Pertoča"
            "1248","1248","PV","ce","VIZ Osnovna šola Rogatec, Enota Vrtec Rogatec"
            "11949","11949","PV","mb","Osnovna šola Janka Glazerja Ruše - enota Vrtec Ruše"
            "17330","17330","PV","mb","Osnovna šola Janka Glazerja Ruše - enota Vrtec Ruše, Bistrica ob Dravi"
            "13489","13489","PV","mb","Osnovna šola Selnica ob Dravi, enota Vrtec"
            "18690","18690","PV","mb","Osnovna šola Selnica ob Dravi, podružnica Sveti Duh na Ostren vrhu - enota vrtec, enota"
            "19530","19530","PV","mb","Osnovna šola Selnica ob Dravi, enota Vrtec Gradišče na Kozjaku"
            "1274","1274","PV","nm","Osnovna šola Belokranjskega odreda Semič, enota vrtec Sonček"
            "1106","1106","PV","kk","Vrtec Ciciban Sevnica"
            "17210","1106","PV","kk","Vrtec Ciciban Sevnica, Centralni vrtec"
            "17211","1106","PV","kk","Vrtec Ciciban Sevnica, enota Kekec"
            "17213","1106","PV","kk","Vrtec Ciciban Sevnica, enota Studenec"
            "17214","1106","PV","kk","Vrtec Ciciban Sevnica, enota Boštanj"
            "17212","1106","PV","kk","Vrtec Ciciban Sevnica, enota Bibarija"
            "3671","3671","PV","kk","Osnovna šola Blanca, Enota Vrtec"
            "22955","22955","PV","kk","Osnovna šola Blanca, Enota vrtca Blanca"
            "1166","1166","PV","kk","Osnovna šola Krmelj, Enota Vrtec"
            "1179","1179","PV","kk","Osnovna šola Milan Majcen Šentjanž, Vrtec Šentjanž"
            "1193","1193","PV","kp","Vrtec Sežana"
            "11649","1193","PV","kp","VRTEC SEŽANA, enota Povir"
            "11650","1193","PV","kp","VRTEC SEŽANA, enota Lokev"
            "11669","1193","PV","kp","Vrtec Sežana, enota Senožeče"
            "11689","1193","PV","kp","Vrtec Sežana, enota Divača"
            "11690","1193","PV","kp","Vrtec Sežana, enota Komen"
            "11691","1193","PV","kp","Vrtec Sežana, enota Hrpelje"
            "11692","1193","PV","kp","Vrtec Sežana, enota Tomaj"
            "11693","1193","PV","kp","Vrtec Sežana, enota Dutovlje"
            "11694","1193","PV","kp","Vrtec Sežana, enota Jasli"
            "11695","1193","PV","kp","Vrtec Sežana, enota Materija"
            "11696","1193","PV","kp","Vrtec Sežana, enota Štanjel"
            "21595","21595","PV","sg","EDUKA zavod za izobraževanje in svetovanje, Slovenj Gradec, PE Zasebni montessori vrtec, Hiša otrok"
            "24475","21595","PV","sg","EDUKA zavod za izobraževanje in svetovanje, Slovenj Gradec, PE Zasebni montessori vrtec, Hiša otrok, enota"
            "14637","14637","PV","sg","VVZ SLOVENJ GRADEC"
            "11209","14637","PV","sg","VVZ Slovenj Gradec, ENOTA LEGEN"
            "11210","14637","PV","sg","VVZ Slovenj Gradec, ENOTA PAMEČE"
            "11211","14637","PV","sg","VVZ Slovenj Gradec, ENOTA ŠMARTNO"
            "11212","14637","PV","sg","VVZ Slovenj Gradec, ENOTA PODGORJE"
            "11213","14637","PV","sg","VVZ Slovenj Gradec, ENOTA MAISTROVA"
            "11214","14637","PV","sg","VVZ Slovenj Gradec, ENOTA MISLINJA"
            "11215","14637","PV","sg","VVZ Slovenj Gradec, ENOTA MISLINJA oddelek DOLIČ"
            "17370","14637","PV","sg","VVZ SLOVENJ GRADEC, enota"
            "1107","14637","PV","sg","VVZ Slovenj Gradec, ENOTA MAISTROVA, oddelek Sele"
            "18110","14637","PV","sg","VVZ SLOVENJ GRADEC, enota CELJSKA"
            "12773","12773","PV","mb","Zavod Sv. Jerneja, Vrtec Blaže in Nežica"
            "1082","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica"
            "11109","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Sonček"
            "11110","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Mlinček Oplotnica"
            "11111","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Oplotnica I."
            "11112","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Prihova"
            "11113","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Ciciban"
            "11114","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Ozka ulica"
            "11129","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Poljčane"
            "11131","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Studenice"
            "11132","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Makole"
            "11150","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Pragersko"
            "11153","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Sp. Polskava"
            "11157","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Zg. Polskava"
            "11160","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Kebelj"
            "11161","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Tinje"
            "11163","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Zgornja Ložnica"
            "22715","1082","PV","mb","Vrtec Otona Župančiča Slovenska Bistrica, Enota Šmartno na Pohorju"
            "16931","16931","PV","mb","OŠ dr. Jožeta Pučnika Črešnjevec, Vrtec Črešnjevec - Leskovec, Enota Leskovec"
            "16930","16930","PV","mb","OŠ dr. Jožeta Pučnika Črešnjevec, Vrtec Črešnjevec - Leskovec, enota Črešnjevec"
            "13229","13229","PV","mb","OŠ dr. Jožeta Pučnika Črešnjevec,Vrtec Črešnjevec - Leskovec, Enota Črešnjevec"
            "15989","15989","PV","mb","Osnovna šola Gustava Šiliha Laporje, Vrtec Laporje"
            "21195","21195","PV","mb","Osnovna šola Gustava Šiliha Laporje, Vrtec Laporje, enota"
            "16009","16009","PV","mb","Osnovni šoli Šmartno na Pohorju, Vrtec Šmartno na Pohorju"
            "16549","16549","PV","ce","Zasebni vrtec Mali grof d.o.o."
            "1083","1083","PV","ce","Vrtec Slovenske Konjice"
            "14070","1083","PV","ce","Vrtec Slovenske Konjice, Enota Tatenbachova"
            "14071","1083","PV","ce","Vrtec Slovenske Konjice, Enota Prevrat"
            "14073","1083","PV","ce","Vrtec Slovenske Konjice, Enota Loče"
            "14074","1083","PV","ce","Vrtec Slovenske Konjice, Enota Tepanje"
            "14075","1083","PV","ce","Vrtec Slovenske Konjice, dislociran oddelek KS Jernej"
            "14078","1083","PV","ce","Vrtec Slovenske Konjice, dislociran oddelek KS Špitalič"
            "14076","1083","PV","ce","Vrtec Slovenske Konjice, dislociran oddelek KS Zbelovo"
            "14077","1083","PV","ce","Vrtec Slovenske Konjice, dislociran oddelek KS Žiče"
            "1162","1162","PV","nm","Osnovna šola dr. Ivana Prijatelja Sodražica, enota Vrtec"
            "14089","14089","PV","mb","Osnovna šola Središče ob Dravi, enota Vrtec Navihanček"
            "1275","1275","PV","mb","Osnovna šola Starše, Vrtec Najdihojca Starše"
            "11790","11790","PV","mb","Osnovna šola Starše, Vrtec Pikapolonica Marjeta"
            "1151","1151","PV","nm","Vrtec pri OŠ Vavta vas"
            "3787","3787","PV","mb","Osnovna šola Sveta Ana, Enota vrtec"
            "13689","13689","PV","mb","Osnovna šola in vrtec Sveta Trojica, Enota Vrtec"
            "1330","1330","PV","ms","Osnovna šola Sv. Jurij ob Ščavnici, Enota Vrtec Sonček"
            "11169","11169","PV","mb","Osnovna šola Jožeta Hudalesa Jurovski Dol, Enota Vrtec"
            "1177","1177","PV","mb","Osnovna šola Sveti Tomaž, Vzgojno-varstvena enota"
            "3712","3712","PV","ms","Osnovna šola Šalovci, Enota Vrtca Šalovci"
            "1176","1176","PV","ng","Osnovna šola Ivana Roba Šempeter pri Gorici, Enota vrtec"
            "1126","1126","PV","kr","Osnovna šola Šenčur, Enota Vrtec"
            "16310","16310","PV","kr","Osnovna šola Šenčur, Enota Vrtec, oddelek Voklo"
            "16309","16309","PV","kr","Osnovna šola Šenčur, Enota Vrtec, oddelek Visoko"
            "22195","22195","PV","kr","Zavod za kreativno varstvo Ela - Ela, zasebni vrtec"
            "3705","3705","PV","mb","Osnovna šola Sladki vrh, enota Vrtec Sladki Vrh"
            "3704","3704","PV","mb","Osnovna šola Rudolfa Maistra Šentilj, Enota Vrtec Šentilj"
            "17295","17295","PV","mb","Osnovna šola Rudolfa Maistra Šentilj, Enota Vrtca Ceršak"
            "19870","19870","PV","nm","Vrtec Čebelica Šentjernej"
            "20195","19870","PV","nm","Vrtec Čebelica Šentjernej, enota Čebelica"
            "20215","19870","PV","nm","Vrtec Čebelica Šentjernej, enota Petelinček"
            "20256","19870","PV","nm","Vrtec Čebelica Šentjernej, enota Živžav"
            "23455","23455","PV","nm","Zavod Na kmetiji je lepo, zasebni vrtec"
            "24015","24015","PV","ce","Zasebni vrtec Viljem Julijan"
            "1084","1084","PV","ce","Vrtec Šentjur"
            "14629","1084","PV","ce","Vrtec Šentjur, enota Pešnica"
            "14630","1084","PV","ce","Vrtec Šentjur, enota Hruševec"
            "14631","1084","PV","ce","Vrtec Šentjur, enota Slivnica"
            "14632","1084","PV","ce","Vrtec Šentjur, enota Ponikva"
            "14633","1084","PV","ce","Vrtec Šentjur, enota Planina"
            "14634","1084","PV","ce","Vrtec Šentjur, enota Dramlje"
            "14635","1084","PV","ce","Vrtec Šentjur, enota Loka"
            "14636","1084","PV","ce","Vrtec Šentjur, enota Blagovna"
            "15889","1084","PV","ce","Vrtec Šentjur, enota Prevorje"
            "17630","1084","PV","ce","Vrtec Šentjur, enota Kalobje"
            "21375","1084","PV","ce","Vrtec Šentjur, enota Šentjur"
            "1227","1227","PV","nm","Osnovna šola dr. Pavla Lunačka Šentrupert, Vrtec Čebelica"
            "1153","1153","PV","nm","Vrtec pri OŠ Frana Metelka Škocjan"
            "17010","17010","PV","nm","Vrtec pri OŠ Frana Metelka Škocjan, Bučka"
            "12772","12772","PV","kr","Župnijski zavod Sv. Jurija Stara Loka, Vrtec Sončni žarek"
            "1086","1086","PV","kr","Vrtec Škofja Loka"
            "16069","1086","PV","kr","Vrtec Škofja Loka, enota Ciciban"
            "16089","1086","PV","kr","Vrtec Škofja Loka, enota Čebelica"
            "16090","1086","PV","kr","Vrtec Škofja Loka, enota Najdihojca"
            "16092","1086","PV","kr","Vrtec Škofja Loka, enota Pedenjped, oddelek"
            "16093","1086","PV","kr","Vrtec Škofja Loka, enota Rožle"
            "17030","1086","PV","kr","Vrtec Škofja Loka, enota Biba"
            "16091","1086","PV","kr","Vrtec Škofja Loka, enota Pedenjped"
            "19090","1086","PV","kr","Vrtec Škofja Loka, enota Bukovica"
            "24275","1086","PV","kr","Vrtec Škofja Loka, enota"
            "24276","1086","PV","kr","Vrtec Škofja Loka, enota Kamnitnik"
            "23196","23196","PV","kr","Zasebni vrtec Duhec, vzgoja in varstvo predšolskih otrok d.o.o."
            "23197","23196","PV","kr","Zasebni vrtec Duhec, Enota Kranj"
            "18090","18090","PV","lj","Smrkolin d.o.o."
            "20295","20295","PV","lj","Vrtec Škofljica"
            "20395","20295","PV","lj","Vrtec Škofljica, enota Bisernik"
            "20375","20295","PV","lj","Vrtec Škofljica, Enota Cekinček"
            "20355","20295","PV","lj","Vrtec Škofljica, Enota Citronček"
            "20415","20295","PV","lj","Vrtec Škofljica, Enota Škratec"
            "20455","20295","PV","lj","Vrtec Škofljica, dislocirani oddelek Modrin"
            "23755","20295","PV","lj","Vrtec Škofljica, enota Pisanček"
            "1321","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah"
            "13049","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, Enota Mavrica Mestinje"
            "13189","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, enota Sonček Šmarje"
            "13209","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, dislocirani oddelek enote Sonček na Svetem Štefanu"
            "13210","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, dislocirani oddelek enote Sonček v Zibiki"
            "13211","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, dislocirani oddelek enote Sonček na Kristan Vrhu"
            "13212","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, dislocirani oddelek enote Sonček na Sladki Gori"
            "13213","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, enota Ciciban Šentvid"
            "17990","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, disloc. odd. na OŠ Šmarje pri Jelšah"
            "19650","1321","PV","ce","JZ Otroški vrtec Šmarje pri Jelšah, enota Livada"
            "3668","3668","PV","nm","Osnovna šola Šmarjeta, Enota Vrtec Sonček"
            "16269","16269","PV","ce","Osnovna šola bratov Letonja Šmartno ob Paki, enota vrtec Sonček"
            "20010","20010","PV","ce","Zasebni vrtec Bambi d.o.o."
            "22135","20010","PV","ce","Zasebni vrtec Bambi d.o.o., Enota Parižlje"
            "12569","12569","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban"
            "17410","17410","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Polhek"
            "17411","17411","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Ostržek"
            "18670","18670","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Bučke"
            "18671","18671","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Sovice"
            "18672","18672","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Škratki"
            "18673","18673","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Mravljivce"
            "18674","18674","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Petelinčki"
            "23655","23655","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Palčki"
            "23995","23995","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Zvezdica"
            "23996","23996","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Lunica"
            "24315","24315","PV","lj","Osnovna šola Šmartno, enota vrtca Ciciban, enota Čukci"
            "1118","1118","PV","ce","Vrtec Šoštanj"
            "11249","1118","PV","ce","Vrtec Šoštanj, enota Mojca"
            "11251","1118","PV","ce","Vrtec Šoštanj, enota Urška"
            "21518","1118","PV","ce","Vrtec Šoštanj, Enota Biba"
            "1258","1258","PV","ce","Osnovna šola Štore, Enota Vrtec Lipa"
            "1143","1143","PV","ms","Osnovna šola Tišina, Enota Vrtec Tišina"
            "22855","22855","PV","ms","Osnovna šola Tišina, Enota Vrtec Tišina, enota"
            "1168","1168","PV","ng","Osnovna šola Simona Kosa Podbrdo, Enota Vrtec"
            "1182","1182","PV","ng","Osnovna šola Dušana Muniha Most na Soči, Enota Vrtec"
            "14314","14314","PV","ng","VVE pri OŠ Most na Soči - oddelek vrtca Podmelec"
            "14317","14317","PV","ng","VVE pri OŠ Most na Soči - oddelek vrtca Dolenja Trebuša"
            "14316","14316","PV","ng","VVE pri OŠ Most na Soči - oddelek vrtca Šentviška Gora"
            "1198","1198","PV","ng","VVZ Ilke Devetak Bignami Tolmin"
            "10669","1198","PV","ng","VVZ Ilke Devetak Bignami Tolmin, enota Volče"
            "13029","1198","PV","ng","VVZ Ilke Devetak Bignami Tolmin, enota Volarje"
            "18430","1198","PV","ng","VVZ Ilke Devetak Bignami Tolmin, enota v OŠ Franceta Bevka Tolmin"
            "1113","1113","PV","za","Vrtec Trbovlje"
            "12089","1113","PV","za","Vrtec Trbovlje, Enota Pikapolonica"
            "12090","1113","PV","za","Vrtec Trbovlje, Enota Barbara"
            "12091","1113","PV","za","Vrtec Trbovlje, Enota Ciciban"
            "12092","1113","PV","za","Vrtec Trbovlje, Enota Mojca"
            "22335","1113","PV","za","Vrtec Trbovlje, oddelek na OŠ Alojza Hohkrauta"
            "23235","23235","PV","za","Zasebni vrtec Čarobni gozd"
            "21995","21995","PV","nm","Osnovna šola Veliki Gaber, enota Vrtec, oddelek Sela pri Šumberku"
            "3715","3715","PV","nm","Osnovna šola Veliki Gaber, enota Vrtec"
            "1195","1195","PV","nm","Vrtec Mavrica Trebnje"
            "22035","1195","PV","nm","Vrtec Mavrica Trebnje, enota Videk"
            "22036","1195","PV","nm","Vrtec Mavrica Trebnje, enota Ostržek"
            "22037","1195","PV","nm","Vrtec Mavrica Trebnje, enota Kekec"
            "22038","1195","PV","nm","Vrtec Mavrica Trebnje, enota Mojca"
            "22039","1195","PV","nm","Vrtec Mavrica Trebnje, enota Gubčeva"
            "22040","1195","PV","nm","Vrtec Mavrica Trebnje, enota Šentlovrenc"
            "22041","1195","PV","nm","Vrtec Mavrica Trebnje, enota Dobrnič"
            "22042","1195","PV","nm","Vrtec Mavrica Trebnje, enota Mavrica"
            "22043","1195","PV","nm","Vrtec Mavrica Trebnje, enota Romano"
            "23536","23536","PV","nm","Zasebni vrtec Maja, hiša otrok Trebnje"
            "15509","15509","PV","lj","GEKO, zasebni vrtec d.o.o."
            "21404","15509","PV","lj","GEKO, zasebni vrtec d.o.o., enota"
            "22815","22815","PV","lj","Kulturno umetniški zavod za kreativno spodbujanje otrok CutePlay"
            "22816","22815","PV","lj","P.E. Zasebni vrtec CutePlay"
            "11589","11589","PV","lj","Osnovna šola Trzin, Enota vrtec Žabica"
            "1108","1108","PV","kr","Vrtec Tržič"
            "15709","1108","PV","kr","Vrtec Tržič, enota Palček"
            "15710","1108","PV","kr","Vrtec Tržič, enota Deteljica"
            "15729","1108","PV","kr","Vrtec Tržič, enota Križe"
            "15749","1108","PV","kr","Vrtec Tržič, Lom"
            "3716","3716","PV","ms","Osnovna šola Turnišče, VVE Turnišče"
            "1087","1087","PV","ce","Vrtec Velenje"
            "14129","1087","PV","ce","Vrtec Velenje enota Tinkara"
            "14130","1087","PV","ce","Vrtec Velenje enota Najdihojca"
            "14131","1087","PV","ce","Vrtec Velenje enota Lučka"
            "14132","1087","PV","ce","Vrtec Velenje enota Vrtiljak"
            "14133","1087","PV","ce","Vrtec Velenje enota Ciciban"
            "14134","1087","PV","ce","Vrtec Velenje enota Jakec"
            "14135","1087","PV","ce","Vrtec Velenje enota Vinska gora"
            "14136","1087","PV","ce","Vrtec Velenje enota Jurček"
            "14137","1087","PV","ce","Vrtec Velenje enota Čebelica"
            "14138","1087","PV","ce","Vrtec Velenje enota Sonček"
            "14139","1087","PV","ce","Vrtec Velenje enota Cirkovce"
            "17791","1087","PV","ce","Vrtec Velenje enota Enci Benci"
            "3720","3720","PV","ms","Osnovna šola Miška Kranjca Velika Polana, Enota Vrtec"
            "1372","1372","PV","lj","Osnovna šola Primoža Trubarja Velike Lašče, Enota Vrtec Sončni žarek"
            "24375","24375","PV","lj","Osnovna šola Primoža Trubarja Velike Lašče, Enota Vrtec Sončni žarek, enota Karlovica"
            "24376","24376","PV","lj","Osnovna šola Primoža Trubarja Velike Lašče, Enota Vrtec Sončni žarek"
            "24377","24377","PV","lj","Osnovna šola Primoža Trubarja Velike Lašče, Enota Vrtec Sončni žarek, enota v šoli"
            "12689","12689","PV","ms","VVE pri Osnovni šoli Veržej"
            "3749","3749","PV","mb","Osnovna šola Videm, enota Vrtec"
            "1326","1326","PV","ce","Osnovna šola Vitanje, Enota Vrtec"
            "15009","15009","PV","lj","Vrtec Škratek Svit Vodice"
            "16850","15009","PV","lj","Vrtec Škratek Svit Vodice, enota Utik"
            "16249","15009","PV","lj","Vrtec Škratek Svit Vodice, enota Skaručna"
            "1322","1322","PV","ce","Vrtec Mavrica Vojnik"
            "14829","1322","PV","ce","VRTEC MAVRICA VOJNIK - ENOTA FRANKOLOVO"
            "14830","1322","PV","ce","VRTEC MAVRICA VOJNIK - ENOTA SOCKA"
            "14831","1322","PV","ce","VRTEC MAVRICA VOJNIK - ENOTA NOVA CERKEV"
            "14832","1322","PV","ce","VRTEC MAVRICA VOJNIK - ENOTA ŠMARTNO V ROŽNI DOLINI"
            "23115","23115","PV","ce","ZASEBNI VRTEC - HIŠA OTROK MONTESSORI SVET, zasebni zavod"
            "23116","23115","PV","ce","ZASEBNI VRTEC - HIŠA OTROK MONTESSORI SVET, zasebni zavod, Poslovna enota vrtca"
            "23215","23215","PV","ce","Zavod Montessori vsakdan"
            "23216","23215","PV","ce","Zavod Montessori vsakdan, PE Hiša otrok Nova Cerkev"
            "1367","1367","PV","ce","Osnovna šola Vransko - Tabor, Enota Vrtec"
            "14331","14331","PV","ce","Osnovna šola Vransko - Tabor, Enota Vrtec Tabor"
            "1088","1088","PV","lj","Vrtec Vrhnika"
            "11069","1088","PV","lj","Vrtec Vrhnika, enota Barjanček"
            "11070","1088","PV","lj","Vrtec Vrhnika, enota Želvica"
            "11071","1088","PV","lj","Vrtec Vrhnika, enota Rosika"
            "16790","1088","PV","lj","Vrtec Vrhnika, enota Žabica"
            "19350","1088","PV","lj","Vrtec Vrhnika, enota Komarček"
            "1386","1386","PV","lj","ŽUPNIJSKI VRTEC VRHNIKA"
            "1159","1159","PV","sg","Osnovna šola Vuzenica, Enota Vrtec Vuzenica"
            "1169","1169","PV","za","Vrtec Zagorje ob Savi"
            "14770","1169","PV","za","Vrtec Zagorje ob Savi, Enota Maja"
            "14771","1169","PV","za","Vrtec Zagorje ob Savi, Enota Center"
            "14772","1169","PV","za","Vrtec Zagorje ob Savi, Enota Smrkci"
            "14773","1169","PV","za","Vrtec Zagorje ob Savi, Enota Ciciban"
            "14774","1169","PV","za","Vrtec Zagorje ob Savi, Enota Kekec"
            "1323","1323","PV","ce","Javni zavod Vrtec Zreče"
            "13529","1323","PV","ce","Javni zavod Vrtec Zreče, Enota Stranice"
            "13509","1323","PV","ce","Javni zavod Vrtec Zreče, Enota Gorenje"
            "1102","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC"
            "11569","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA ŽALEC I."
            "11570","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA ŽALEC II."
            "11571","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA ŽALEC III."
            "11572","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA PONIKVA"
            "11573","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA GRIŽE"
            "11574","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA ZABUKOVICA"
            "11575","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA PETROVČE"
            "11576","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA LIBOJE"
            "11577","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA LEVEC"
            "11578","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA TRJE"
            "11579","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA ŠEMPETER"
            "18470","1102","PV","ce","JVIZ VRTCI OBČINE ŽALEC, ENOTA NOVO CELJE"
            "1167","1167","PV","kr","Osnovna šola Železniki, Vrtec Železniki"
            "1264","1264","PV","kr","Antonov vrtec Železniki"
            "19071","19071","PV","mb","Osnovna šola Žetale, Enota vrtec"
            "1181","1181","PV","kr","Osnovna šola Žiri, Enota Vrtec"
            "1392","1392","PV","kr","Vrtec pri Sv.Ani Žiri"
            "3695","3695","PV","kr","Osnovna šola Žirovnica, VVE pri OŠ Žirovnica"
            "3667","3667","PV","nm","Osnovna šola Prevole, Vrtec Prevole"
            "1226","1226","PV","nm","Osnovna šola Žužemberk - Vrtec Sonček"
            "138","138","OSPP","ce","Osnovna šola Glazija"
            "152","152","OSPP","nm","Osnovna šola Milke Šobar - Nataše Črnomelj"
            "161","161","OSPP","lj","Osnovna šola Roje"
            "3492","3492","OSPP","kr","Osnovna šola Poldeta Stražišarja Jesenice"
            "202","202","OSPP","lj","Osnovna šola 27. julij"
            "206","206","OSPP","nm","Osnovna šola Ljubo Šercer"
            "229","229","OSPP","kr","Osnovna šola Helene Puhar Kranj"
            "235","235","OSPP","kk","Osnovna šola dr. Mihajla Rostoharja Krško"
            "254","254","OSPP","ms","Dvojezična osnovna šola II Lendava"
            "281","281","OSPP","lj","Center za usposabljanje, vzgojo in izobraževanje Janeza Levca Ljubljana"
            "1040","1040","OSPP","mb","Dom Antona Skale Maribor"
            "494","494","OSPP","mb","Osnovna šola Gustava Šiliha Maribor"
            "637","637","OSPP","ms","Osnovna šola IV. Murska Sobota"
            "360","360","OSPP","ng","Osnovna šola Kozara Nova Gorica"
            "378","378","OSPP","nm","Osnovna šola Dragotina Ketteja"
            "391","391","OSPP","mb","Osnovna šola Stanka Vraza Ormož"
            "397","397","OSPP","kp","Center za usposabljanje Elvira Vatovec Strunjan"
            "411","411","OSPP","mb","Osnovna šola dr. Ljudevita Pivka Ptuj"
            "3518","3518","OSPP","kr","Osnovna šola Antona Janše Radovljica"
            "440","440","OSPP","sg","Osnovna šola Juričevega Drejčka Ravne na Koroškem"
            "499","499","OSPP","ce","JVIZ III. Osnovna šola Rogaška Slatina"
            "452","452","OSPP","kk","Osnovna šola Ane Gale Sevnica"
            "464","464","OSPP","sg","Tretja osnovna šola Slovenj Gradec"
            "469","469","OSPP","mb","Osnovna šola Minke Namestnik- Sonje Slovenska Bistrica"
            "486","486","OSPP","kr","Osnovna šola Jela Janežiča"
            "529","529","OSPP","ce","Center za vzgojo, izobraževanje in usposabljanje Velenje"
            "542","542","OSPP","za","Osnovna šola dr. Slavka Gruma"
            "547","547","OSPP","ce","II. Osnovna šola Žalec"
            "119","119","GS","ng","Glasbena šola Vinka Vodopivca Ajdovščina"
            "15309","15309","GS","ms","Zavod svetega Cirila in Metoda Beltinci, OE Glasbena šola Beltinci"
            "21295","21295","GS","kr","Glasbeni center DO RE MI, Zasebni zavod za vzgojo in izobraževanje"
            "123","123","GS","kk","Glasbena šola Brežice"
            "139","139","GS","ce","Glasbena šola Celje"
            "17651","17651","GS","kr","Glasbena šola Lartko, zavod za glasbeno vzgojo"
            "147","147","GS","po","Glasbena šola Frana Gerbiča Cerknica"
            "153","153","GS","nm","Glasbena šola Črnomelj"
            "22459","22459","GS","lj","Glasbena šola Emil Adamič, Zasebni zavod za glasbeno vzgojo in izobraževanje, Dobrova"
            "162","162","GS","lj","Glasbena šola Domžale"
            "173","173","GS","ms","Glasbena šola Gornja Radgona"
            "15809","15809","GS","ms","Zasebna glasbena šola Maestro, zavod za glasbeno kulturo, Gornja Radgona"
            "3475","3475","GS","lj","Glasbena šola Grosuplje"
            "183","183","GS","za","Glasbena šola Hrastnik"
            "186","186","GS","ng","Glasbena šola Idrija"
            "193","193","GS","po","Glasbena šola Ilirska Bistrica"
            "3491","3491","GS","kr","Glasbena šola Jesenice"
            "203","203","GS","lj","Glasbena šola Kamnik"
            "207","207","GS","nm","Glasbena šola Kočevje"
            "215","215","GS","kp","Glasbena šola Koper"
            "230","230","GS","kr","Glasbena šola Kranj"
            "236","236","GS","kk","Glasbena šola Krško"
            "255","255","GS","ms","Glasbena šola Lendava"
            "10128","10128","GS","za","Glasbena šola Litija - Šmartno"
            "23175","23175","GS","lj","AMARILIS, glasbeno in drugo izobraževanje"
            "304","304","GS","lj","Glasbena šola Franca Šturma"
            "290","290","GS","lj","Glasbena šola Ljubljana Moste - Polje"
            "318","318","GS","lj","Glasbena šola Ljubljana Vič - Rudnik"
            "1385","1385","GS","lj","Glasbeni atelje Tartini zasebna glasbena šola d.o.o."
            "10569","10569","GS","lj","Glasbeni center Edgar Willems, zasebni zavod za glasbeno vzgojo in izobraževanje"
            "24395","24395","GS","lj","Glasbeni center Zvočna zgodba"
            "22255","22255","GS","lj","Kreativnost, zasebna glasbena šola Arsem, d.o.o."
            "9906","9906","GS","lj","Zavod Salesianum, OE Glasbena šola Rakovnik"
            "331","331","GS","ms","Glasbena šola Slavka Osterca Ljutomer"
            "337","337","GS","lj","Glasbena šola Logatec"
            "15529","15529","GS","mb","Zavod Antona Martina Slomška, Glasbena in baletna šola Antona Martina Slomška Maribor"
            "344","344","GS","ms","Glasbena šola Murska Sobota"
            "9232","9232","GS","sg","Melodija zasebna glasbena šola Muta d.o.o."
            "1025","1025","GS","ce","Glasbena šola Nazarje"
            "367","367","GS","ng","Glasbena šola Nova Gorica"
            "379","379","GS","nm","Glasbena šola Marjana Kozine Novo mesto"
            "392","392","GS","mb","Glasbena šola Ormož"
            "403","403","GS","po","Glasbena šola Postojna"
            "412","412","GS","mb","Glasbena šola Karol Pahor Ptuj"
            "10969","10969","GS","mb","Zasebna glasbena šola v samostanu sv. Petra in Pavla"
            "244","244","GS","kk","Glasbena šola Laško - Radeče"
            "983","983","GS","sg","Glasbena šola Radlje ob Dravi"
            "432","432","GS","kr","Glasbena šola Radovljica"
            "441","441","GS","sg","Glasbena šola Ravne na Koroškem"
            "446","446","GS","nm","Glasbena šola Ribnica"
            "984","984","GS","ce","Glasbena šola Rogaška Slatina"
            "3783","3783","GS","kk","Glasbena šola Sevnica"
            "457","457","GS","kp","Glasbena šola Sežana"
            "465","465","GS","sg","Glasbena šola Slovenj Gradec"
            "17550","17550","GS","mb","Glasbena šola Slovenska Bistrica"
            "591","591","GS","ce","Glasbena šola Slovenske Konjice"
            "481","481","GS","ce","Glasbena šola skladateljev Ipavcev Šentjur"
            "487","487","GS","kr","Glasbena šola Škofja Loka"
            "985","985","GS","ng","Glasbena šola Tolmin"
            "512","512","GS","za","Glasbena šola Trbovlje"
            "1261","1261","GS","nm","Glasbena šola Trebnje"
            "642","642","GS","kr","Glasbena šola Tržič"
            "530","530","GS","ce","Glasbena šola Fran Korun Koželjski Velenje"
            "537","537","GS","lj","Glasbena šola Vrhnika"
            "538","538","GS","za","Glasbena šola Zagorje"
            "548","548","GS","ce","Glasbena šola Risto Savin Žalec"
            "72","","SS","kr","Biotehniški center Naklo"
            "10749","","SS","kr","Biotehniški center Naklo, Srednja šola"
            "1256","","SS","kr","Ekonomska gimnazija in srednja šola Radovljica"
            "20157","","SS","kr","Gimnazija Franceta Prešerna"
            "1055","","SS","kr","Gimnazija Jesenice"
            "17","","SS","kr","Gimnazija Kranj"
            "65","","SS","kr","Gimnazija Škofja Loka"
            "1255","","SS","kr","Srednja gostinska in turistična šola Radovljica"
            "1054","","SS","kr","Srednja šola Jesenice"
            "20110","","SS","kr","Šolski center Kranj"
            "20134","","SS","kr","Šolski center Kranj, Srednja ekonomska, storitvena in gradbena šola"
            "20163","","SS","kr","Šolski center Kranj, Srednja tehniška šola"
            "20160","","SS","kr","Šolski center Kranj, Strokovna gimnazija"
            "66","","SS","kr","Šolski center Škofja Loka"
            "64","","SS","kr","Šolski center Škofja Loka, Srednja šola za lesarstvo"
            "9886","","SS","kr","Šolski center Škofja Loka, Srednja šola za strojništvo"
            "49","","SS","ng","Gimnazija Jurija Vege Idrija"
            "98","","SS","ng","Gimnazija Nova Gorica"
            "3320","","SS","ng","Gimnazija Tolmin"
            "37","","SS","ng","Srednja šola Veno Pilon Ajdovščina"
            "639","","SS","ng","Škofijska gimnazija Vipava"
            "77","","SS","ng","Šolski center Nova Gorica"
            "3734","","SS","ng","Šolski center Nova Gorica, Biotehniška šola"
            "3844","","SS","ng","Šolski center Nova Gorica, Elektrotehniška in računalniška šola"
            "1301","","SS","ng","Šolski center Nova Gorica, Gimnazija in zdravstvena šola"
            "53","","SS","ng","Šolski center Nova Gorica, Srednja ekonomska in trgovska šola"
            "10789","","SS","ng","Šolski center Nova Gorica, Strojna, prometna in lesarska šola"
            "80","","SS","nm","Ekonomska šola Novo mesto"
            "14775","","SS","nm","Ekonomska šola Novo mesto, Srednja šola in gimnazija"
            "19672","","SS","nm","Gimnazija in srednja šola Kočevje"
            "81","","SS","nm","Gimnazija Novo mesto"
            "79","","SS","nm","Grm Novo mesto - center biotehnike in turizma"
            "10769","","SS","nm","Grm Novo mesto - center biotehnike in turizma, Kmetijska šola Grm in biotehniška gimnazija"
            "82","","SS","nm","Grm Novo mesto - center biotehnike in turizma, Srednja šola za gostinstvo in turizem"
            "48","","SS","nm","Srednja šola Črnomelj"
            "1262","","SS","nm","Šolski center Novo mesto"
            "1287","","SS","nm","Šolski center Novo mesto, Srednja elektro šola in tehniška gimnazija"
            "3722","","SS","nm","Šolski center Novo mesto, Srednja gradbena, lesarska in vzgojiteljska šola"
            "1288","","SS","nm","Šolski center Novo mesto, Srednja strojna šola"
            "3723","","SS","nm","Šolski center Novo mesto, Srednja zdravstvena in kemijska šola"
            "109","","SS","sg","Šolski center Ravne na Koroškem"
            "108","","SS","sg","Šolski center Ravne na Koroškem, Gimnazija"
            "10809","","SS","sg","Šolski center Ravne na Koroškem, Srednja šola"
            "84","","SS","sg","Šolski center Slovenj Gradec"
            "1298","","SS","sg","Šolski center Slovenj Gradec, Gimnazija"
            "19671","","SS","sg","Šolski center Slovenj Gradec, Srednja šola Slovenj Gradec in Muta"
            "1299","","SS","sg","Šolski center Slovenj Gradec, Srednja zdravstvena šola"
            "61","","SS","kp","Gimnazija Antonio Sema Piran"
            "20111","","SS","kp","Gimnazija, elektro in pomorska šola Piran"
            "14","","SS","kp","Gimnazija Gian Rinaldo Carli Koper"
            "13","","SS","kp","Gimnazija Koper"
            "71","","SS","kp","Srednja ekonomsko - poslovna šola Koper"
            "20112","","SS","kp","Srednja šola Izola"
            "15","","SS","kp","Srednja šola Pietro Coppo Izola"
            "7","","SS","kp","Srednja tehniška šola Koper"
            "1","","SS","kp","Šolski center Srečka Kosovela Sežana"
            "14751","","SS","kp","Šolski center Srečka Kosovela Sežana, Gimnazija in ekonomska šola"
            "26","","SS","lj","Biotehniški izobraževalni center Ljubljana"
            "1376","","SS","lj","Biotehniški izobraževalni center Ljubljana, Gimnazija in veterinarska šola"
            "3778","","SS","lj","Biotehniški izobraževalni center Ljubljana, Živilska šola"
            "9594","","SS","lj","Center za izobraževanje, rehabilitacijo in usposabljanje Kamnik, Srednja šola"
            "1359","","SS","lj","Ekonomska šola Ljubljana"
            "21","","SS","lj","Elektrotehniško-računalniška strokovna šola in gimnazija Ljubljana"
            "20175","","SS","lj","ERUDIO zasebna gimnazija"
            "11","","SS","lj","Gimnazija Bežigrad"
            "10829","","SS","lj","Gimnazija Bežigrad, Gimnazija"
            "10830","","SS","lj","Gimnazija Bežigrad, Mednarodna šola"
            "94","","SS","lj","Gimnazija in srednja šola Rudolfa Maistra Kamnik"
            "1304","","SS","lj","Gimnazija Jožeta Plečnika Ljubljana"
            "23","","SS","lj","Gimnazija Ledina"
            "24","","SS","lj","Gimnazija Moste"
            "102","","SS","lj","Gimnazija Poljane"
            "36","","SS","lj","Gimnazija Šentvid"
            "970","","SS","lj","Gimnazija Šiška"
            "25","","SS","lj","Gimnazija Vič"
            "16630","","SS","lj","Konservatorij za glasbo in balet Ljubljana"
            "73","","SS","lj","Konservatorij za glasbo in balet Ljubljana, Srednja glasbena in baletna šola"
            "22","","SS","lj","Srednja ekonomska šola Ljubljana"
            "641","","SS","lj","Srednja frizerska šola Ljubljana"
            "20","","SS","lj","Srednja gradbena, geodetska in okoljevarstvena šola Ljubljana"
            "55","","SS","lj","Srednja medijska in grafična šola Ljubljana"
            "30","","SS","lj","Srednja šola Domžale"
            "9849","","SS","lj","Srednja šola Domžale, Gimnazija"
            "9848","","SS","lj","Srednja šola Domžale, Poklicna in strokovna šola"
            "1289","","SS","lj","Srednja šola Josipa Jurčiča Ivančna Gorica"
            "1031","","SS","lj","Srednja šola tehniških strok Šiška"
            "35","","SS","lj","Srednja šola za farmacijo, kozmetiko in zdravstvo"
            "33","","SS","lj","Srednja šola za gastronomijo in turizem Ljubljana"
            "51","","SS","lj","Srednja šola za oblikovanje in fotografijo Ljubljana"
            "54","","SS","lj","Srednja trgovska šola Ljubljana"
            "50","","SS","lj","Srednja upravno administrativna šola Ljubljana"
            "34","","SS","lj","Srednja vzgojiteljska šola, gimnazija in umetniška gimnazija Ljubljana"
            "99","","SS","lj","Srednja zdravstvena šola Ljubljana"
            "88","","SS","lj","Strokovni izobraževalni center Ljubljana"
            "14749","","SS","lj","Strokovni izobraževalni center Ljubljana, Srednja poklicna in strokovna šola Bežigrad"
            "3827","","SS","lj","Šolski center Ljubljana"
            "3835","","SS","lj","Šolski center Ljubljana, Gimnazija Antona Aškerca"
            "3837","","SS","lj","Šolski center Ljubljana, Srednja lesarska šola"
            "3838","","SS","lj","Šolski center Ljubljana, Srednja strojna in kemijska šola"
            "9","","SS","lj","Šolski center za pošto, ekonomijo in telekomunikacije Ljubljana"
            "3816","","SS","lj","Šolski center za pošto, ekonomijo in telekomunikacije Ljubljana, Srednja tehniška in strokovna šola"
            "633","","SS","lj","Zavod sv. Frančiška Saleškega Gimnazija Želimlje"
            "11009","","SS","lj","Zavod sv. Stanislava"
            "1016","","SS","lj","Zavod sv. Stanislava, Škofijska klasična gimnazija"
            "42","","SS","mb","Biotehniška šola Maribor"
            "21675","","SS","mb","DOBA EPIS, d.o.o., Srednja šola"
            "3395","","SS","mb","Gimnazija in srednja kemijska šola Ruše"
            "1360","","SS","mb","Gimnazija Ormož"
            "1266","","SS","mb","Gimnazija Ptuj"
            "76","","SS","mb","II. gimnazija Maribor"
            "3","","SS","mb","III. gimnazija Maribor"
            "41","","SS","mb","Izobraževalni center Piramida Maribor"
            "3777","","SS","mb","Izobraževalni center Piramida Maribor, Srednja šola za prehrano in živilstvo"
            "92","","SS","mb","Konservatorij za glasbo in balet Maribor"
            "21395","","SS","mb","Lesarska šola Maribor"
            "21397","","SS","mb","Lesarska šola Maribor, Srednja šola"
            "3389","","SS","mb","Prometna šola Maribor"
            "9977","","SS","mb","Prometna šola Maribor, Srednja prometna šola in dijaški dom"
            "44","","SS","mb","Prva gimnazija Maribor"
            "43","","SS","mb","Srednja ekonomska šola in gimnazija Maribor"
            "20775","","SS","mb","Srednja elektro-računalniška šola Maribor"
            "89","","SS","mb","Srednja gradbena šola in gimnazija Maribor"
            "3773","","SS","mb","Srednja šola Slovenska Bistrica"
            "58","","SS","mb","Srednja šola za gostinstvo in turizem Maribor"
            "56","","SS","mb","Srednja šola za oblikovanje Maribor"
            "57","","SS","mb","Srednja šola za trženje in dizajn Maribor"
            "74","","SS","mb","Srednja zdravstvena in kozmetična šola Maribor"
            "104","","SS","mb","Šolski center Ptuj"
            "1268","","SS","mb","Šolski center Ptuj, Biotehniška šola"
            "1267","","SS","mb","Šolski center Ptuj, Ekonomska šola"
            "1269","","SS","mb","Šolski center Ptuj, Elektro in računalniška šola"
            "1270","","SS","mb","Šolski center Ptuj, Strojna šola"
            "21396","","SS","mb","Tehniški šolski center Maribor"
            "21398","","SS","mb","Tehniški šolski center Maribor, Srednja strojna šola"
            "12989","","SS","mb","Zavod Antona Martina Slomška"
            "1303","","SS","mb","Zavod Antona Martina Slomška, Škofijska gimnazija Antona Martina Slomška"
            "85","","SS","ms","Biotehniška šola Rakičan"
            "19","","SS","ms","Dvojezična srednja šola Lendava"
            "97","","SS","ms","Ekonomska šola Murska Sobota"
            "14769","","SS","ms","Ekonomska šola Murska Sobota, Srednja šola in gimnazija"
            "27","","SS","ms","Gimnazija Franca Miklošiča Ljutomer"
            "106","","SS","ms","Gimnazija Murska Sobota"
            "107","","SS","ms","Srednja poklicna in tehniška šola Murska Sobota"
            "31","","SS","ms","Srednja šola za gostinstvo in turizem Radenci"
            "45","","SS","ms","Srednja zdravstvena šola Murska Sobota"
            "3780","","SS","kk","Ekonomska in trgovska šola Brežice"
            "3812","","SS","kk","Ekonomska in trgovska šola Brežice, Poklicna in strokovna šola"
            "3779","","SS","kk","Gimnazija Brežice"
            "18","","SS","kk","Šolski center Krško - Sevnica"
            "10107","","SS","kk","Šolski center Krško - Sevnica, Srednja šola Krško"
            "63","","SS","kk","Šolski center Krško - Sevnica, Srednja šola Sevnica"
            "86","","SS","po","Srednja gozdarska in lesarska šola Postojna"
            "62","","SS","po","Šolski center Postojna"
            "10309","","SS","po","Šolski center Postojna, Gimnazija Ilirska Bistrica"
            "9858","","SS","po","Šolski center Postojna, Srednja šola"
            "20113","","SS","ce","Ekonomska šola Celje"
            "20132","","SS","ce","Ekonomska šola Celje, Gimnazija in srednja šola"
            "4","","SS","ce","Gimnazija Celje - Center"
            "29","","SS","ce","I. gimnazija v Celju"
            "23075","","SS","ce","PRAH, Izobraževalni center, izobraževanje in usposabljanje d.o.o., Srednja strokovna in poklicna šola Rogaška Slatina-zasebna šola"
            "46","","SS","ce","Srednja šola za gostinstvo in turizem Celje"
            "12","","SS","ce","Srednja zdravstvena in kozmetična šola Celje"
            "1305","","SS","ce","Šola za hortikulturo in vizualne umetnosti Celje"
            "9890","","SS","ce","Šola za hortikulturo in vizualne umetnosti Celje, Srednja poklicna in strokovna šola"
            "5","","SS","ce","Šolski center Celje"
            "3731","","SS","ce","Šolski center Celje, Gimnazija Lava"
            "1297","","SS","ce","Šolski center Celje, Srednja šola za gradbeništvo in varovanje okolja"
            "1296","","SS","ce","Šolski center Celje, Srednja šola za kemijo, elektrotehniko in računalništvo"
            "68","","SS","ce","Šolski center Celje, Srednja šola za storitvene dejavnosti in logistiko"
            "1295","","SS","ce","Šolski center Celje, Srednja šola za strojništvo, mehatroniko in medije"
            "67","","SS","ce","Šolski center Rogaška Slatina"
            "114","","SS","ce","Šolski center Slovenske Konjice - Zreče"
            "9972","","SS","ce","Šolski center Slovenske Konjice - Zreče, Gimnazija Slovenske Konjice"
            "9973","","SS","ce","Šolski center Slovenske Konjice - Zreče, Srednja poklicna in strokovna šola Zreče"
            "1306","","SS","ce","Šolski center Šentjur"
            "9850","","SS","ce","Šolski center Šentjur, Srednja poklicna in strokovna šola"
            "103","","SS","ce","Šolski center Velenje"
            "3730","","SS","ce","Šolski center Velenje, Elektro in računalniška šola"
            "1291","","SS","ce","Šolski center Velenje, Gimnazija"
            "1294","","SS","ce","Šolski center Velenje, Šola za storitvene dejavnosti"
            "23695","","SS","ce","Šolski center Velenje, Šola za strojništvo, geotehniko in okolje"
            "91","","SS","za","Gimnazija in ekonomska srednja šola Trbovlje"
            "1286","","SS","za","Gimnazija Litija"
            "3299","","SS","za","Srednja šola Zagorje"
            "100","","SS","za","Srednja tehniška in poklicna šola Trbovlje"
            "9571","9571","VSS","kr","Višja strokovna šola za gostinstvo, velnes in turizem Bled"
            "9189","9189","VSS","kk","Ekonomska in trgovska šola Brežice, Višja strokovna šola"
            "10348","10348","VSS","ce","ABITURA, Podjetje za izobraževanje, d.o.o., Celje, Višja strokovna šola"
            "20158","20158","VSS","ce","Ekonomska šola Celje, Višja strokovna šola"
            "10290","10290","VSS","ce","Šola za hortikulturo in vizualne umetnosti Celje, Višja strokovna šola"
            "8907","8907","VSS","ce","Šolski center Celje, Višja strokovna šola"
            "10360","10360","VSS","lj","MINERVA, inštitut za spodbujanje razvoja osebnosti,  OE Višja šola"
            "13869","13869","VSS","nm","Center za poslovno usposabljanje, Višja strokovna šola Ljubljana, Podužnica Kočevje"
            "13889","13889","VSS","nm","Izobraževalni zavod HERA, Višja strokovna šola Enota Kočevje"
            "14410","14410","VSS","kp","Izobraževalni center Memory, d.o.o. Višja strokovna šola Koper"
            "10356","10356","VSS","kr","B&B Izobraževanje in usposabljanje d.o.o., Višja strokovna šola v Kranju"
            "20535","20535","VSS","kr","Center za poslovno usposabljanje, Višja strokovna šola Ljubljana, Enota Kranj"
            "10354","10354","VSS","kr","EDC - zavod za strokovno izobraževanje, Kranj, Višja strokovna šola"
            "20133","20133","VSS","kr","Šolski center Kranj, Višja strokovna šola"
            "10358","10358","VSS","za","GRI d.o.o., Višja strokovna šola"
            "10357","10357","VSS","lj","B&B Izobraževanje in usposabljanje d.o.o., Višja strokovna šola v Ljubljani"
            "10389","10389","VSS","lj","Biotehniški izobraževalni center Ljubljana, Višja strokovna šola"
            "10352","10352","VSS","lj","B2 Izobraževanje in informacijske storitve d. o. o., Višja strokovna šola"
            "10","10","VSS","lj","Center za poslovno usposabljanje, Višja strokovna šola Ljubljana"
            "10361","10361","VSS","lj","EMONA EFEKTA, izobraževanje in svetovanje, d.o.o., Višja strokovna šola"
            "14390","14390","VSS","lj","ERUDIO Višja strokovna šola"
            "10345","10345","VSS","lj","GEA College CVŠ, Družba za višješolsko izobraževanje - Center višjih šol, d.o.o."
            "10338","10338","VSS","lj","Inštitut in akademija za multimedije, Višja šola za multimedije"
            "10430","10430","VSS","lj","Izobraževalni center energetskega sistema Višja strokovna šola"
            "10337","10337","VSS","lj","Izobraževalni zavod HERA, Višja strokovna šola"
            "16329","16329","VSS","lj","Konservatorij za glasbo in balet Ljubljana, Višja baletna šola"
            "10344","10344","VSS","lj","LEILA, Višja strokovna šola d.o.o."
            "10359","10359","VSS","lj","MUCH, izobraževanje, d.o.o. Višja strokovna šola"
            "22915","22915","VSS","lj","PARATUS izobraževanje in svetovanje d.o.o., OE Višja strokovna šola"
            "19430","19430","VSS","lj","PRAH, izobraževalni center, avtošola in drugo izobraževanje, d.o.o., Višja strokovna šola Rogaška Slatina, Podružnica Ljubljana"
            "10355","10355","VSS","lj","SKALDENS, zasebni zdravstveni zavod, Višja strokovna šola za ustne higienike"
            "21615","21615","VSS","lj","Šolski center Ljubljana, Višja strokovna šola"
            "9574","9574","VSS","lj","Šolski center za pošto, ekonomijo in telekomunikacije Ljubljana, Višja strokovna šola"
            "10332","10332","VSS","lj","Višja policijska šola"
            "21315","21315","VSS","lj","Višja strokovna šola za kozmetiko in velnes Ljubljana"
            "22916","22916","VSS","lj","Višja šola za računovodstvo in finance, Ljubljana"
            "10347","10347","VSS","mb","ACADEMIA Višja strokovna šola"
            "8581","8581","VSS","mb","B2 d.o.o., Višja strokovna šola, enota Maribor"
            "9575","9575","VSS","mb","DOBA EPIS, d.o.o."
            "21715","21715","VSS","mb","DOBA EPIS, d.o.o., Višja strokovna šola"
            "9114","9114","VSS","mb","Izobraževalni center Piramida Maribor, Višja strokovna šola"
            "21402","21402","VSS","mb","Lesarska šola Maribor, Višja strokovna šola"
            "10229","10229","VSS","mb","Prometna šola Maribor, Višja prometna šola"
            "21403","21403","VSS","mb","Tehniški šolski center Maribor, Višja strokovna šola"
            "10342","10342","VSS","mb","Višja strokovna šola za gostinstvo in turizem Maribor"
            "9579","9579","VSS","ms","Ekonomska šola Murska Sobota, Višja strokovna šola"
            "10750","10750","VSS","kr","Biotehniški center Naklo, Višja strokovna šola"
            "10339","10339","VSS","ng","LAMPRET CONSULTING d.o.o. Višja  strokovna šola"
            "10369","10369","VSS","ng","Šolski center Nova Gorica, Višja strokovna šola"
            "9576","9576","VSS","nm","Ekonomska šola Novo mesto, Višja strokovna šola"
            "9572","9572","VSS","nm","Grm Novo mesto - center biotehnike in turizma, Višja strokovna šola"
            "9001","9001","VSS","nm","Šolski center Novo Mesto, Višja strokovna šola"
            "10346","10346","VSS","kp","GEA College CVŠ, Družba za višješolsko izobraževanje - Center višjih šol, d.o.o., Podružnica v Piranu"
            "9859","9859","VSS","po","Šolski center Postojna, Višja strokovna šola"
            "13849","13849","VSS","mb","Center za poslovno usposabljanje, Višja strokovna šola Ljubljana, Podružnica Ptuj"
            "13789","13789","VSS","mb","GEA College CVŠ, Družba za višješolsko izobraževanje - Center višjih šol, d.o.o., Podružnica na Ptuju"
            "10249","10249","VSS","mb","Šolski center Ptuj, Višja strokovna šola"
            "10810","10810","VSS","sg","Šolski center Ravne na Koroškem, Višja strokovna šola"
            "19410","19410","VSS","ce","PRAH, izobraževalni center, avtošola in drugo izobraževanje, d.o.o., Višja strokovna šola Rogaška Slatina"
            "14409","14409","VSS","kp","Izobraževalni center Memory, d.o.o. Višja strokovna šola Dutovlje"
            "13829","13829","VSS","kp","Šolski center Srečka Kosovela Sežana, Višja strokovna šola"
            "9577","9577","VSS","sg","Šolski center Slovenj Gradec, Višja strokovna šola"
            "9851","9851","VSS","ce","Šolski center Šentjur, Višja strokovna šola"
            "10271","10271","VSS","kr","Šolski center Škofja Loka, Višja strokovna šola"
            "8001","8001","VSS","ce","Šolski center Velenje, Višja strokovna šola"
            "15430","15430","ZAV","ce","Center za usposabljanje, delo in varstvo Dobrna"
            "15429","15429","ZAV","mb","Zavod za usposabljanje, delo in varstvo Dr.Marijana Borštnarja Dornava"
            "15409","15409","ZAV","lj","Center za usposabljanje, delo in varstvo Dolfke Boštjančič"
            "3568","3568","ZAV","lj","Vzgojno-izobraževalni zavod Višnja Gora"
            "1002","1002","ZAV","lj","Center za izobraževanje, rehabilitacijo in usposabljanje Kamnik"
            "3569","3569","ZAV","kr","Vzgojni zavod Kranj"
            "1001","1001","ZAV","lj","Center IRIS - Center za izobraževanje, rehabilitacijo, inkluzijo in svetovanje za slepe in slabovidne"
            "3572","3572","ZAV","lj","Mladinski dom Jarše Ljubljana"
            "3577","3577","ZAV","lj","Mladinski dom Malči Beličeve Ljubljana"
            "3631","3631","ZAV","lj","Zavod za gluhe in naglušne Ljubljana"
            "3571","3571","ZAV","lj","Zavod za vzgojo in izobraževanje Logatec"
            "999","999","ZAV","mb","Center za sluh in govor Maribor"
            "1005","1005","ZAV","mb","Mladinski dom Maribor"
            "3567","3567","ZAV","lj","Vzgojnoizobraževalni zavod Frana Milčinskega Smlednik"
            "1000","1000","ZAV","kp","Center za komunikacijo, sluh in govor Portorož"
            "998","998","ZAV","po","Vzgojni zavod Planina"
            "15469","15469","ZAV","kr","Center za usposabljanje, delo in varstvo Matevža Langusa"
            "10529","10529","ZAV","ms","Osnovna šola Veržej - Zav"
            "1004","1004","ZAV","ng","Center za izobraževanje, rehabilitacijo in usposabljanje Vipava"
        |]
        |> Array.map (fun (key, keyM, sType, region, name) ->
            { Key = key; KeyMain = keyM; Type = sType; Region = region; Name = name})

    let schools =
        schoolsList
        |> Array.map (fun school -> school.Key, school)
        |> Map.ofArray

module AgePopulationStats =
    type AgeGroupId = string

    type AgeGroupPopulationStats = {
        Key: AgeGroupId
        Male: int
        Female: int
    }

    let agePopulationStats =
        [
            "0-4", 53183, 50328
            "5-14", 106600, 100566
            "15-24", 100391, 93739
            "25-34", 133471, 122333
            "35-44", 162436, 146922
            "45-54", 153735, 146868
            "55-64", 147957, 147089
            "65-74", 101173, 113253
            "75-84", 54460, 81981
            "85+", 13635, 36760
        ]
        |> List.map (fun (ageGroupId,  male,  female) ->
            ageGroupId, { Key = ageGroupId;  Male = male;  Female = female })
        |> Map.ofList

    let parseAgeGroupId (ageGroupId: AgeGroupId): AgeGroupKey =
        if ageGroupId.Contains('-') then
            let i = ageGroupId.IndexOf('-')
            let fromAge = Int32.Parse(ageGroupId.Substring(0, i))
            let toAge = Int32.Parse(ageGroupId.Substring(i+1))
            { AgeFrom = Some fromAge; AgeTo =  Some toAge }
        else if ageGroupId.Contains('+') then
            let i = ageGroupId.IndexOf('+')
            let fromAge = Int32.Parse(ageGroupId.Substring(0, i-1))
            { AgeFrom = Some fromAge; AgeTo = None }
        else
            sprintf "Invalid age group ID: %s" ageGroupId
            |> ArgumentException |> raise

    let toAgeGroupId (groupKey: AgeGroupKey): AgeGroupId =
        match groupKey.AgeFrom, groupKey.AgeTo with
        | Some fromValue, Some toValue -> sprintf "%d-%d" fromValue toValue
        | Some fromValue, None -> sprintf "%d+" fromValue
        | _ -> sprintf "Invalid age group key (%A)" groupKey
                |> ArgumentException |> raise

    let populationStatsForAgeGroup (groupKey: AgeGroupKey)
        : AgeGroupPopulationStats =
        let ageGroupId = toAgeGroupId groupKey

        if agePopulationStats.ContainsKey ageGroupId then
            agePopulationStats.[ageGroupId]
        else
            sprintf "Age group '%s' does not exist." ageGroupId
            |> ArgumentException |> raise
