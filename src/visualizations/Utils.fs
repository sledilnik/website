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

    let excludedRegions = Set.ofList ["si" ; "unknown" ; "foreign"]

    let regions =
        [ "si",  "SLOVENIJA",  Some 2095861
          "ms",  "Pomurska",  Some 114238
          "mb",  "Podravska",  Some 325994
          "sg",  "Koroška",  Some 70755
          "ce",  "Savinjska",  Some 257847
          "za",  "Zasavska",  Some 57156
          "kk",  "Posavska",  Some 75824
          "nm",  "Jugovzhodna Slovenija",  Some 145357
          "lj",  "Osrednjeslovenska",  Some 555274
          "kr",  "Gorenjska",  Some 206621
          "po",  "Primorsko-notranjska",  Some 52841
          "ng",  "Goriška",  Some 118041
          "kp",  "Obalno-kraška",  Some 115913
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
            "ajdovščina", "Ajdovščina", "SI-001", 19418
            "ankaran", "Ankaran/Ancarano", "SI-213", 3224
            "apače", "Apače", "SI-195", 3533
            "beltinci", "Beltinci", "SI-002", 8059
            "benedikt", "Benedikt", "SI-148", 2615
            "bistrica_ob_sotli", "Bistrica ob Sotli", "SI-149", 1336
            "bled", "Bled", "SI-003", 7868
            "bloke", "Bloke", "SI-150", 1564
            "bohinj", "Bohinj", "SI-004", 5145
            "borovnica", "Borovnica", "SI-005", 4579
            "bovec", "Bovec", "SI-006", 3073
            "braslovče", "Braslovče", "SI-151", 5639
            "brda", "Brda", "SI-007", 5619
            "brezovica", "Brezovica", "SI-008", 12671
            "brežice", "Brežice", "SI-009", 24051
            "cankova", "Cankova", "SI-152", 1734
            "celje", "Celje", "SI-011", 49540
            "cerklje_na_gorenjskem", "Cerklje na Gorenjskem", "SI-012", 7769
            "cerknica", "Cerknica", "SI-013", 11580
            "cerkno", "Cerkno", "SI-014", 4547
            "cerkvenjak", "Cerkvenjak", "SI-153", 2083
            "cirkulane", "Cirkulane", "SI-196", 2345
            "črenšovci", "Črenšovci", "SI-015", 3968
            "črna_na_koroškem", "Črna na Koroškem", "SI-016", 3285
            "črnomelj", "Črnomelj", "SI-017", 14241
            "destrnik", "Destrnik", "SI-018", 2643
            "divača", "Divača", "SI-019", 4213
            "dobje", "Dobje", "SI-154", 949
            "dobrepolje", "Dobrepolje", "SI-020", 3878
            "dobrna", "Dobrna", "SI-155", 2234
            "dobrova-polhov_gradec", "Dobrova - Polhov Gradec", "SI-021", 7800
            "dobrovnik", "Dobrovnik/Dobronak", "SI-156", 1284
            "dol_pri_ljubljani", "Dol pri Ljubljani", "SI-022", 6310
            "dolenjske_toplice", "Dolenjske Toplice", "SI-157", 3553
            "domžale", "Domžale", "SI-023", 36675
            "dornava", "Dornava", "SI-024", 2902
            "dravograd", "Dravograd", "SI-025", 8884
            "duplek", "Duplek", "SI-026", 6996
            "gorenja_vas-poljane", "Gorenja vas - Poljane", "SI-027", 7557
            "gorišnica", "Gorišnica", "SI-028", 4112
            "gorje", "Gorje", "SI-207", 2738
            "gornja_radgona", "Gornja Radgona", "SI-029", 8417
            "gornji_grad", "Gornji Grad", "SI-030", 2476
            "gornji_petrovci", "Gornji Petrovci", "SI-031", 1995
            "grad", "Grad", "SI-158", 2042
            "grosuplje", "Grosuplje", "SI-032", 21314
            "hajdina", "Hajdina", "SI-159", 3879
            "hoče-slivnica", "Hoče - Slivnica", "SI-160", 11753
            "hodoš", "Hodoš/Hodos", "SI-161", 353
            "horjul", "Horjul", "SI-162", 3024
            "hrastnik", "Hrastnik", "SI-034", 9113
            "hrpelje-kozina", "Hrpelje - Kozina", "SI-035", 4604
            "idrija", "Idrija", "SI-036", 11811
            "ig", "Ig", "SI-037", 7617
            "ilirska_bistrica", "Ilirska Bistrica", "SI-038", 13297
            "ivančna_gorica", "Ivančna Gorica", "SI-039", 17008
            "izola", "Izola/Isola", "SI-040", 16486
            "jesenice", "Jesenice", "SI-041", 21340
            "jezersko", "Jezersko", "SI-163", 625
            "juršinci", "Juršinci", "SI-042", 2380
            "kamnik", "Kamnik", "SI-043", 29925
            "kanal", "Kanal", "SI-044", 5291
            "kidričevo", "Kidričevo", "SI-045", 6541
            "kobarid", "Kobarid", "SI-046", 4052
            "kobilje", "Kobilje", "SI-047", 542
            "kočevje", "Kočevje", "SI-048", 15674
            "komen", "Komen", "SI-049", 3529
            "komenda", "Komenda", "SI-164", 6387
            "koper", "Koper/Capodistria", "SI-050", 52630
            "kostanjevica_na_krki", "Kostanjevica na Krki", "SI-197", 2432
            "kostel", "Kostel", "SI-165", 635
            "kozje", "Kozje", "SI-051", 2989
            "kranj", "Kranj", "SI-052", 57065
            "kranjska_gora", "Kranjska Gora", "SI-053", 5225
            "križevci", "Križevci", "SI-166", 3537
            "krško", "Krško", "SI-054", 26224
            "kungota", "Kungota", "SI-055", 4724
            "kuzma", "Kuzma", "SI-056", 1573
            "laško", "Laško", "SI-057", 13067
            "lenart", "Lenart", "SI-058", 8502
            "lendava", "Lendava/Lendva", "SI-059", 10426
            "litija", "Litija", "SI-060", 15499
            "ljubljana", "Ljubljana", "SI-061", 295504
            "ljubno", "Ljubno", "SI-062", 2550
            "ljutomer", "Ljutomer", "SI-063", 11268
            "log-dragomer", "Log - Dragomer", "SI-208", 3656
            "logatec", "Logatec", "SI-064", 14396
            "loška_dolina", "Loška dolina", "SI-065", 3757
            "loški_potok", "Loški Potok", "SI-066", 1789
            "lovrenc_na_pohorju", "Lovrenc na Pohorju", "SI-167", 2963
            "luče", "Luče", "SI-067", 1448
            "lukovica", "Lukovica", "SI-068", 5890
            "majšperk", "Majšperk", "SI-069", 4056
            "makole", "Makole", "SI-198", 2010
            "maribor", "Maribor", "SI-070", 112682
            "markovci", "Markovci", "SI-168", 4039
            "medvode", "Medvode", "SI-071", 16829
            "mengeš", "Mengeš", "SI-072", 8279
            "metlika", "Metlika", "SI-073", 8470
            "mežica", "Mežica", "SI-074", 3595
            "miklavž_na_dravskem_polju", "Miklavž na Dravskem polju", "SI-169", 6929
            "miren-kostanjevica", "Miren - Kostanjevica", "SI-075", 4977
            "mirna", "Mirna", "SI-212", 2649
            "mirna_peč", "Mirna Peč", "SI-170", 2988
            "mislinja", "Mislinja", "SI-076", 4566
            "mokronog-trebelno", "Mokronog - Trebelno", "SI-199", 3089
            "moravče", "Moravče", "SI-077", 5449
            "moravske_toplice", "Moravske Toplice", "SI-078", 5837
            "mozirje", "Mozirje", "SI-079", 4150
            "murska_sobota", "Murska Sobota", "SI-080", 18758
            "muta", "Muta", "SI-081", 3414
            "naklo", "Naklo", "SI-082", 5384
            "nazarje", "Nazarje", "SI-083", 2639
            "nova_gorica", "Nova Gorica", "SI-084", 31884
            "novo_mesto", "Novo mesto", "SI-085", 37587
            "odranci", "Odranci", "SI-086", 1632
            "oplotnica", "Oplotnica", "SI-171", 4137
            "ormož", "Ormož", "SI-087", 11906
            "osilnica", "Osilnica", "SI-088", 353
            "pesnica", "Pesnica", "SI-089", 7410
            "piran", "Piran/Pirano", "SI-090", 17676
            "pivka", "Pivka", "SI-091", 6201
            "podčetrtek", "Podčetrtek", "SI-092", 3446
            "podlehnik", "Podlehnik", "SI-172", 1801
            "podvelka", "Podvelka", "SI-093", 2339
            "poljčane", "Poljčane", "SI-200", 4449
            "polzela", "Polzela", "SI-173", 6258
            "postojna", "Postojna", "SI-094", 16442
            "prebold", "Prebold", "SI-174", 5103
            "preddvor", "Preddvor", "SI-095", 3714
            "prevalje", "Prevalje", "SI-175", 6799
            "ptuj", "Ptuj", "SI-096", 23530
            "puconci", "Puconci", "SI-097", 5873
            "rače-fram", "Rače - Fram", "SI-098", 7531
            "radeče", "Radeče", "SI-099", 4139
            "radenci", "Radenci", "SI-100", 5112
            "radlje_ob_dravi", "Radlje ob Dravi", "SI-101", 6169
            "radovljica", "Radovljica", "SI-102", 19072
            "ravne_na_koroškem", "Ravne na Koroškem", "SI-103", 11348
            "razkrižje", "Razkrižje", "SI-176", 1277
            "rečica_ob_savinji", "Rečica ob Savinji", "SI-209", 2322
            "renče-vogrsko", "Renče - Vogrsko", "SI-201", 4371
            "ribnica", "Ribnica", "SI-104", 9527
            "ribnica_na_pohorju", "Ribnica na Pohorju", "SI-177", 1116
            "rogaška_slatina", "Rogaška Slatina", "SI-106", 11132
            "rogašovci", "Rogašovci", "SI-105", 3024
            "rogatec", "Rogatec", "SI-107", 3106
            "ruše", "Ruše", "SI-108", 7017
            "selnica_ob_dravi", "Selnica ob Dravi", "SI-178", 4493
            "semič", "Semič", "SI-109", 3864
            "sevnica", "Sevnica", "SI-110", 17642
            "sežana", "Sežana", "SI-111", 13551
            "slovenj_gradec", "Slovenj Gradec", "SI-112", 16591
            "slovenska_bistrica", "Slovenska Bistrica", "SI-113", 25827
            "slovenske_konjice", "Slovenske Konjice", "SI-114", 15042
            "sodražica", "Sodražica", "SI-179", 2185
            "solčava", "Solčava", "SI-180", 513
            "središče_ob_dravi", "Središče ob Dravi", "SI-202", 1902
            "starše", "Starše", "SI-115", 4046
            "straža", "Straža", "SI-203", 3936
            "sveta_ana", "Sveta Ana", "SI-181", 2294
            "sveta_trojica_v_slovenskih_goricah", "Sveta Trojica v Slovenskih goricah", "SI-204", 2101
            "sveti_andraž_v_slovenskih_goricah", "Sveti Andraž v Slovenskih goricah", "SI-182", 1180
            "sveti_jurij_ob_ščavnici", "Sveti Jurij ob Ščavnici", "SI-116", 2782
            "sveti_jurij_v_slovenskih_goricah", "Sveti Jurij v Slovenskih goricah", "SI-210", 2098
            "sveti_tomaž", "Sveti Tomaž", "SI-205", 1988
            "šalovci", "Šalovci", "SI-033", 1380
            "šempeter-vrtojba", "Šempeter - Vrtojba", "SI-183", 6292
            "šenčur", "Šenčur", "SI-117", 8814
            "šentilj", "Šentilj", "SI-118", 8351
            "šentjernej", "Šentjernej", "SI-119", 7199
            "šentjur", "Šentjur", "SI-120", 19225
            "šentrupert", "Šentrupert", "SI-211", 2969
            "škocjan", "Škocjan", "SI-121", 3352
            "škofja_loka", "Škofja Loka", "SI-122", 23297
            "škofljica", "Škofljica", "SI-123", 11577
            "šmarje_pri_jelšah", "Šmarje pri Jelšah", "SI-124", 10271
            "šmarješke_toplice", "Šmarješke Toplice", "SI-206", 3462
            "šmartno_ob_paki", "Šmartno ob Paki", "SI-125", 3265
            "šmartno_pri_litiji", "Šmartno pri Litiji", "SI-194", 5642
            "šoštanj", "Šoštanj", "SI-126", 8784
            "štore", "Štore", "SI-127", 4529
            "tabor", "Tabor", "SI-184", 1669
            "tišina", "Tišina", "SI-010", 3970
            "tolmin", "Tolmin", "SI-128", 11003
            "trbovlje", "Trbovlje", "SI-129", 16105
            "trebnje", "Trebnje", "SI-130", 13133
            "trnovska_vas", "Trnovska vas", "SI-185", 1366
            "trzin", "Trzin", "SI-186", 3940
            "tržič", "Tržič", "SI-131", 14961
            "turnišče", "Turnišče", "SI-132", 3145
            "velenje", "Velenje", "SI-133", 33656
            "velika_polana", "Velika Polana", "SI-187", 1392
            "velike_lašče", "Velike Lašče", "SI-134", 4410
            "veržej", "Veržej", "SI-188", 1325
            "videm", "Videm", "SI-135", 5625
            "vipava", "Vipava", "SI-136", 5703
            "vitanje", "Vitanje", "SI-137", 2272
            "vodice", "Vodice", "SI-138", 4985
            "vojnik", "Vojnik", "SI-139", 8907
            "vransko", "Vransko", "SI-189", 2624
            "vrhnika", "Vrhnika", "SI-140", 17529
            "vuzenica", "Vuzenica", "SI-141", 2649
            "zagorje_ob_savi", "Zagorje ob Savi", "SI-142", 16439
            "zavrč", "Zavrč", "SI-143", 1479
            "zreče", "Zreče", "SI-144", 6550
            "žalec", "Žalec", "SI-190", 21492
            "železniki", "Železniki", "SI-146", 6720
            "žetale", "Žetale", "SI-191", 1309
            "žiri", "Žiri", "SI-147", 4933
            "žirovnica", "Žirovnica", "SI-192", 4394
            "žužemberk", "Žužemberk", "SI-193", 4702
        ]
        |> List.map (fun (key, name, code, population) -> key,  { Key = key ; Name = name ; Code = code ; Population = population})
        |> Map.ofList

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
