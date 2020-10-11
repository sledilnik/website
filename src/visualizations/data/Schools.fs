module Data.Schools

open Fable.SimpleJson

let url =
    "https://raw.githubusercontent.com/sledilnik/" +
    "data/master/json/mizs-stats.json"


type ByInstitutionType = {
    Vrtec: int
    Osnovna_sola: int
    Osnovna_sola_s_prilagojenim_programom: int
    Glasbena_sola: int
    Srednja_sola: int
    Dijaski_dom: int
    Zavodi: int
}

type SchoolsStatsDay = {
    year: int
    month: int
    day: int

    ucenci_okuzbe_nove: ByInstitutionType
    zaposleni_okuzbe_nove: ByInstitutionType

    oddelki_v_karanteni_novi: ByInstitutionType
    ucenci_v_karanteni_novi: ByInstitutionType

    ucenci_okuzbe_aktivne: ByInstitutionType
    zaposleni_okuzbe_aktivne: ByInstitutionType

    oddelki_v_karanteni_aktivne: ByInstitutionType
    ucenci_v_karanteni_aktivne: ByInstitutionType

    zavodi_pouk_na_daljavo: ByInstitutionType
}

//type ByInstitutions = {
//    Kindergartens: int
//    ElementarySchools: int
//    ElementarySchoolsAdapted: int
//    MusicSchools: int
//    HighSchools: int
//    Dormitories: int
//    Institutes: int
//}
//
//type SchoolsStatsDay = {
//    Date: DateTime
//
//    InfectionsPupilsNew: ByInstitutions
//    InfectionsEmployeesNew: ByInstitutions
//
//    InfectionsPupilsActive: ByInstitutions
//    InfectionsEmployeesActive: ByInstitutions
//
//    QuarantinedDepartmentsNew: ByInstitutions
//    QuarantinedPupilsNew: ByInstitutions
//
//    QuarantinedDepartmentsActive: ByInstitutions
//    QuarantinedPupilsActive: ByInstitutions
//
//    DistanceLearning: ByInstitutions
//}
//
//type SchoolsStats = {
//    ByDay: SchoolsStatsDay []
//}

//
//type SchoolsStatsHistory = SchoolsStatsDay[]
//
//type SchoolsStatsHeader = {
//    Posodobljeno: DateTime
//}
//
//type SchoolsStats = {
//    Header: SchoolsStatsHeader
//    History: SchoolsStatsHistory
//}

type SchoolsStats = SchoolsStatsDay []

let parseMizsJson (json: string): SchoolsStats =
    let secondObjectStartIndex = json.IndexOf("},") + 2
    let adjustedJson = "[" + json.Substring(secondObjectStartIndex)

    Json.parseNativeAs<SchoolsStats> adjustedJson

let getOrFetch =
    DataLoader.makeDataLoaderWithCustomParser<SchoolsStats> parseMizsJson url
