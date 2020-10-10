module Data.Schools

open System

let url =
    "https://raw.githubusercontent.com/sledilnik/" +
    "data/master/json/mizs-stats.json"

type ByInstitutions = {
    Kindergartens: int
    ElementarySchools: int
    ElementarySchoolsAdapted: int
    MusicSchools: int
    HighSchools: int
    Dormitories: int
    Institutes: int
}

type SchoolsStatsDay = {
    Date: DateTime
    InfectionsPupilsNew: ByInstitutions
    InfectionsEmployeesNew: ByInstitutions

    InfectionsPupilsActive: ByInstitutions
    InfectionsEmployeesActive: ByInstitutions

    QuarantinedDepartmentsNew: ByInstitutions
    QuarantinedPupilsNew: ByInstitutions

    QuarantinedDepartmentsActive: ByInstitutions
    QuarantinedPupilsActive: ByInstitutions

    DistanceLearning: ByInstitutions
}

type SchoolsStats = SchoolsStatsDay[]

let getOrFetch = DataLoader.makeDataLoader<SchoolsStats> url
