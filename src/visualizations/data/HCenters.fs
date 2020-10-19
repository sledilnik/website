module Data.HCenters

open System
open FsToolkit.ErrorHandling
open Fable.SimpleHttp

let sumOption a b =
    match a, b with
    | None, None -> None
    | Some x, None -> Some x
    | None, Some y -> Some y
    | Some x, Some y -> Some (x + y)

type Examinations = {
    MedicalEmergency: int option
    SuspectedCovid: int option
} with
    static member (+) (a, b) = {
        MedicalEmergency = sumOption a.MedicalEmergency b.MedicalEmergency
        SuspectedCovid = sumOption a.SuspectedCovid b.SuspectedCovid
    }
    static member None = {
        MedicalEmergency = None
        SuspectedCovid = None
    }

type PhoneTriage = {
    SuspectedCovid: int option
} with
    static member (+) (a, b) = {
        SuspectedCovid = sumOption a.SuspectedCovid b.SuspectedCovid
    }
    static member None = {
        SuspectedCovid = None
    }

type Tests = {
    Performed: int option
    Positive: int option
} with
    static member (+) (a, b) = {
        Performed = sumOption a.Performed b.Performed
        Positive = sumOption a.Positive b.Positive
    }
    static member None = {
        Performed = None
        Positive = None
    }

type SentTo = {
    Hospital: int option
    SelfIsolation: int option
} with
    static member (+) (a, b) = {
        Hospital = sumOption a.Hospital b.Hospital
        SelfIsolation = sumOption a.SelfIsolation b.SelfIsolation
    }
    static member None = {
        Hospital = None
        SelfIsolation = None
    }

type TotalHcStats = {
    Examinations: Examinations
    PhoneTriage: PhoneTriage
    Tests: Tests
    SentTo: SentTo
} with
    static member (+) (a, b) = {
        Examinations =  a.Examinations + b.Examinations
        PhoneTriage =  a.PhoneTriage + b.PhoneTriage
        Tests =  a.Tests + b.Tests
        SentTo =  a.SentTo + b.SentTo
    }
    static member None = {
        Examinations = Examinations.None
        PhoneTriage = PhoneTriage.None
        Tests = Tests.None
        SentTo = SentTo.None
    }

type HcStats = {
    Date : DateTime
    Total: TotalHcStats
    Municipalities : Map<string, Map<string, TotalHcStats>>
}

type Metric =
    | ExaminationsMedicalEmergency
    | ExaminationsSuspectedCovid
    | PhoneTriageSuspectedCovid
    | TestPerformed
    | TestsPositive
    | SentToHospital
    | SentToSelIsolation

type DataPoint = {
    Region : string
    Municipality : string
    Metric : Metric
    Value : int option
}

let parseData (csv : string) =
    let rows = csv.Split("\n")
    let header = rows.[0].Split(",")

    let headerMunicipalities =
        header.[8..]
        |> Array.map (fun col ->
            match col.Split(".") with
            | [| "hc" ; region ; municipality ; "examinations" ; "medical_emergency" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = ExaminationsMedicalEmergency ; Value = None }
            | [| "hc" ; region ; municipality ; "examinations" ; "suspected_covid" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = ExaminationsSuspectedCovid ; Value = None }
            | [| "hc" ; region ; municipality ; "phone_triage" ; "suspected_covid" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = PhoneTriageSuspectedCovid ; Value = None }
            | [| "hc" ; region ; municipality ; "tests" ; "performed" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = TestPerformed ; Value = None }
            | [| "hc" ; region ; municipality ; "tests" ; "positive" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = TestsPositive ; Value = None }
            | [| "hc" ; region ; municipality ; "sent_to" ; "hospital" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = SentToHospital ; Value = None }
            | [| "hc" ; region ; municipality ; "sent_to" ; "self_isolation" |] ->
                Some { Region = region ; Municipality = municipality ; Metric = SentToSelIsolation ; Value = None }
            | unknown ->
                printfn "Napaka pri branju glave podatkov zdravstvenih ustanov: %s" col
                None
        )

    rows.[1..]
        |> Array.map (fun row ->
            let columns = row.Split(",")
            result {
                if headerMunicipalities.Length <> columns.[8..].Length then
                    return! Error ""
                else
                    // Date is in the first column
                    let! date = Utils.parseDate(columns.[0])
                    // Total values
                    let total = {
                        Examinations = { MedicalEmergency = Utils.nativeParseInt columns.[1] ; SuspectedCovid = Utils.nativeParseInt columns.[2] }
                        PhoneTriage = { SuspectedCovid = Utils.nativeParseInt columns.[3] }
                        Tests = { Performed = Utils.nativeParseInt columns.[4] ; Positive = Utils.nativeParseInt columns.[5] }
                        SentTo = { Hospital = Utils.nativeParseInt columns.[6] ; SelfIsolation = Utils.nativeParseInt columns.[7] }
                    }
                    // Merge municipality header information with data columns
                    let regions =
                        Array.map2 (fun header value ->
                            match header with
                            | None _ -> None
                            | Some header -> Some { header with Value = Utils.nativeParseInt value }
                        ) headerMunicipalities columns.[8..]
                        |> Array.choose id
                        // Group by region
                        |> Array.groupBy (fun dp -> dp.Region)
                        |> Array.map (fun (region, dps) ->
                            let municipalities =
                                dps
                                // Group by municipality and combine values
                                |> Array.groupBy (fun dp -> dp.Municipality)
                                |> Array.map (fun (municipality, dps) ->
                                    let municipalityStats =
                                        dps
                                        |> Array.fold (fun state dp ->
                                            match dp.Metric with
                                            | ExaminationsMedicalEmergency -> { state with Examinations = { state.Examinations with MedicalEmergency = dp.Value }}
                                            | ExaminationsSuspectedCovid -> { state with Examinations = { state.Examinations with SuspectedCovid = dp.Value }}
                                            | PhoneTriageSuspectedCovid -> { state with PhoneTriage = { state.PhoneTriage with SuspectedCovid = dp.Value }}
                                            | TestPerformed -> { state with Tests = { state.Tests with Performed = dp.Value }}
                                            | TestsPositive -> { state with Tests = { state.Tests with Positive = dp.Value }}
                                            | SentToHospital -> { state with SentTo = { state.SentTo with Hospital = dp.Value }}
                                            | SentToSelIsolation -> { state with SentTo = { state.SentTo with SelfIsolation = dp.Value }}
                                        ) TotalHcStats.None
                                    (municipality, municipalityStats))
                                |> Map.ofArray
                            (region, municipalities) )
                        |> Map.ofArray
                    return { Date = date ; Total = total ; Municipalities = regions }
            })
    |> Array.choose (fun row ->
        match row with
        | Ok row -> Some row
        | Error _ -> None)

let loadData (apiEndpoint: string) =
    async {
        let! (statusCode, response) = Http.get (sprintf "%s/api/health-centers?format=csv" apiEndpoint)

        if statusCode <> 200 then
            return Error (sprintf "Napaka pri nalaganju podatkov zdravstvenih ustanov: %d" statusCode)
        else
            return Ok (parseData response)
    }
