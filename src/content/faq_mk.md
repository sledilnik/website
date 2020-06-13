<h1>Frequently Asked Questions *MK</h1>

_The purpose of the Sledilnik project and all its statistical graphs and tables/presentations is to display all existing data about the COVID19 epidemic in Slovenia in an understandable fashion. We've compiled some of the most frequently asked questions. The FAQ list is not exhaustive at this moment, but it is updated frequently so keep checking back. In case of any questions, please contact us: info@slednik.org. (We also look forward to your compliments, feedback, or suggestions!)_

## General Information

<details>
  <summary id=why-sledilnik>Why Sledilnik?</summary>

Our goal is to help understand the spread of the virus and to help raise awareness, responsiveness, and the effectiveness of the measures implemented to curb the virus. You can find more about the project in the [About tab](/en/about). 

</details>

<details>
  <summary id=virus-vs-disease>What is the difference between SARS-CoV-2 and COVID-19?</summary>

**SARS-CoV-2** is the abbreviation for ‘Severe Acute Respiratory Syndrome Coronavirus 2’ – it is the internationally accepted name of the virus that causes the disease named **COVID-19**. The latter name is also an acronym, coined from the words COrona VIrus Disease, and 2019, the year when the disease first erupted.


</details>

<details>
  <summary id=confirmed-cases>What is the difference between ‘new cases’ and ‘confirmed cases’?
</summary>

Terminology in use on Sledilnik is explained under [What does it mean](#chart-terminology). According to the WHO definition, a *confirmed case* is a person with laboratory confirmation of COVID-19 infection, irrespective of clinical signs and symptoms. Other terms, such as newly infected, may appear in the media but are not used in our graphs. All terms used by Sledilnik are explained in these FAQ. 

</details>

<details>
  <summary id=all-infected>Is it possible to get statistical information on all infected persons, even the asymptomatic cases?
</summary>

Unfortunately, this data is unavailable for now. There are several reasons: Previously, tests have only covered a certain proportion of the population (patients with signs and symptoms of acute respiratory infection who may need hospital treatment, healthcare professionals, retirement home residents with respiratory symptoms, and people above 60 if their doctor so requires). Even though now the testing guidance for COVID-19 is expanded to include anyone displaying symptoms of the disease, many might be carriers with no or only mild symptoms. For this reason, our statistics can cover only part of the population that clearly shows signs of infection. Thus, the younger and the untested populations are disproportionately represented. Data for asymptomatic patients who do not show symptoms and are not recorded anywhere can therefore not be obtained.

</details>

<details>
  <summary id=other-countries>Where can I find a comparison between Slovenia and other countries?</summary>

You can find [a comparison graph](/en/stats#countries-chart) at the very bottom of the dashboard. The graph displays a comparison between Slovenia and different clusters of countries in relation to the *number of deaths* caused by COVID-19 *per million inhabitants*. 
The clusters of countries that are compared to Slovenia are as follows:
-   Neighboring countries (excluding Italy)
-   Critical countries (EU)
-   Critical countries (global)
-   Nordic countries
-   ex-Yugoslavian countries
-   East Asian countries and Oceania

The graph is arranged chronologically, from January 1, from the first death, and from the first death per million, respectively. You can change the view of different chronological displays of comparisons of different clusters of countries by clicking on the appropriate tabs.

</details>

<details>
  <summary id=english-translation>Is your webpage available in English?</summary>

Currently, only the [About part](/en/about) and these FAQ are available, while the rest of the website is yet to be fully translated. However, both the text part and the source code are available as open source if you're interested in helping us translate. All the [data in the database](https://github.com/sledilnik) is already marked with English tags, so its international use (export) is also possible. 

</details>

<details>
  <summary id=are-you-paid>Are you paid for creating the dashboard (charts, tables, models)?</summary>

Not at all. Sledilnik is a non-profit initiative created to support the ongoing compiling and editing of key data on the spread of the coronavirus in Slovenia. Our database is public and freely available, free of charge, and non-commercial, and will remain so. Please check [How can I obtain and use your database?](#data-usage)

</details>

<details>
  <summary id=tech-used>Which tools did you use to build the website/web app?</summary>

The site is in JavaScript using Vue.js, the visualizations and graphs are made in F# using Highcharts libraries, and the project is open and available on [GitHub – Sledilnik](https://github.com/sledilnik).

</details>

## Data

<details>
  <summary id=data-reliability>Are your data and visualizations reliable?</summary>

Data is collected from verified public sources, which are listed in the [Resources tab](/en/sources). 

Sledilnik receives official data on COVID-19 directly from the Ministry of Health, the NIJZ (National Institute for Public Health), and other national health institutions. The Sledilnik team does not guarantee the accuracy of the original data and publishes solely data obtained from official sources or the media, but we do cross-check if all data is correct and consistent with the given source.

</details>

<details>
  <summary id=data-collection>How do you collect and edit the data?</summary>

[The database](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) is built from the NIJZ source data (by category). Data by region and age is processed with delay and is finally updated once the ongoing epidemiological demographic research results are known. The municipalities are tracked in the [Kraji (Municipalities) table](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Editing Hospital Care Data – [Table Pacienti (Patients)](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

- We receive daily reports and monitor the announcements of all COVID-19 hospitals  (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje) – at around 8 am.

- We monitor the number of hospitalizations: all wards, in ICUs, and on ventilators.

- We also record transitions (acceptance/dismissal) between individual stages of the disease (when detectable) from the obtained data.

- Where the transition (admission/dismissal) information is incomplete, the values are determined by inference (using a formula).

- All sources and conclusions are recorded as a commentary in individual cells (checkable).

- The data is compared with the summary data on hospitalized patients in ICU published by the Government of the Republic of Slovenia.
  
  </details>

<details>
  <summary id=data-publish-time>When do you publish the data? Why are the dates on the visualizations different?
</summary>

Most data for the previous day is collected at 11:59 pm (tests, confirmed cases ...), and hospitalization data is mostly obtained by 9 am every day for all hospitals. **Our data is usually updated between 10.00 and 12.00.**
When we publish updated daily data, it is available on all our distribution channels (CSV, REST, website), and we also report it on social networks ([Facebook](https://www.facebook.com/COVID19Sledilnik) and [Twitter](https://twitter.com/sledilnik)).

</details>

<details>
  <summary id=data-differences>Several portals display numbers of infected people that are different from yours. Why?</summary>

Sledilnik uses only validated and official data reported daily by the National Institute of Public Health (NIJZ) and all Slovenian hospitals treating COVID-19. Our data thus comes directly from verified sources, and we have also cross-compared information from the very beginning (4 March 2020). Differences usually occur because different media and portals obtain the data at different times of the day or use dubious methodology. See also [Are your data and visualizations reliable?](#data-reliability) 

</details>

<details>
  <summary id=data-hospital-in>How do you obtain data on hospital admissions?</summary>

Hospitals do not always report individual admissions or discharges from which we can obtain accurate data. The number of admissions is usually calculated from data on the currently hospitalized and from the difference compared to the previous day, to which we add the number of discharged and dead on a given day. We keep records of admissions and discharges in intensive care units and for connection and disconnection to/from ventilators in a similar way.

</details>

<details>
  <summary id=data-hospital-out>How do you obtain data on hospital discharges?</summary>

The information on the discharged from hospitals is calculated from data daily obtained directly from hospitals, i.e. from a verified source. We mostly get the daily number of discharges for all hospitals, from which we can deduce the number of newly admitted. See also [How do you obtain data on hospital admissions?](#data-hospital-in)

</details>

<details>
  <summary id=data-recovered>Why did you replace the number of *cured* with the number of *recovered* patients?</summary>

Sledilnik used to rely on official sources (Government of the Republic of Slovenia, media) for the number of cured people. Unfortunately, reports on when a person is totally cured from Covid-19 are scarce – for now, we only have few confirmed reports of "cured patients", and there is, as of yet, still no official definition of when a particular person is cured. A [COVID-19 national survey](https://covid19.biolab.si/) is currently underway at the *Institute of Microbiology and Immunology*, which will also, through voluntary sample testing, show how many people have already fallen ill and recovered from COVID-19. As it is not yet known what the potential consequences of COVID-19 could be (various studies are addressing this question and the results will not be known for a long time), and because health institutions are also talking about recovered rather than cured patients, we are thus changing both the terminology and the method of calculating the number of recoveries. See also [Do you keep an Active Case counter and do you know how many people are currently infected?](#data-active-cases)) 

On April 14, the Department of Health released [Priporočila za zaključek izolacije in vrnitev na delovno mesto (Recommendations for Discontinuing Isolation and Returning to Work)](https://www.zbornica-zveza.si/wp-content/uploads/2020/04/PRIPORO%C4%8CILO-Zaklju%C4%8Dek-izolacije-in-vrnitev-na-delovna-mesta-po-preboleli-bolezni-COVID-19.pdf) from which we can deduce when a person has recovered from the disease and can return to work. For people with symptoms, this is 14 days after the symptoms have subsided, and for healthcare professionals, a control swab is required after 14 days, which must be negative 2x in a row. The Government of the Republic of Slovenia regularly reports on discharges from the hospital, but we do not know if they have already recovered from the disease. The published recommendations indicate that these patients require two consecutive negative control swabs after discharge to home care in order to be considered able to return to work. [In its report](https://www.ecdc.europa.eu/sites/default/files/documents/covid-19-rapid-risk-assessment-coronavirus-disease-2019-ninth-update-23-april-2020.pdf) the ECDC stated that *The 14-day incidence of reported COVID-19 cases in the EU/EEA and UK, providing an estimate of the prevalence of active cases in the population...* – so we chose the 14-day period as the time when the average confirmed case can still be counted as an *active case*. 

We noticed that the [Worldometer](https://www.worldometers.info/coronavirus/#countries) was reporting recovery numbers, but unfortunately we were unable to obtain data on where this information was coming from. Also, some other sources simply combine survivors with the number of discharged patients from hospitals. Since we believe that these two categories are interchangeable, we have decided to present them separately; these are the *Discharged from Hospital* and *Recovered* indicators.
 
*Note: The calculation of recoveries was changed on 9 May 2020. We now consider a patient has recovered in 14 days after their infection was confirmed (previously 21 days), so there was be a noticeable jump in the number of survivors. Please take this difference into account when estimating the number of survivors. A more detailed explanation of the changed calculation is available in the Medium article [Od potrjeno okuženih do prebolelih (From Confirmed Case to Recovery )](https://medium.com/@sledilnik/94c81674718e).*

</details>

<details>
  <summary id=data-active-cases>Do you keep an Active Case counter and do you know how many people are currently infected?</summary>

Yes, these indicators have been graphically displayed as **Confirmed Cases (active)** and **Recovered (total)** from the end of April.
 

These visualizations are not data from public sources; both indicators show the calculated value on the basis of official data, so they are indicated by a dashed line for easier distinguishing. The value of the confirmed cases (active) is calculated by simply subtracting the official data for the relevant category, the value of the Recovered (total) reflects the status of all confirmed cases three weeks ago (minus the dead). The number of recoveries is a simple estimate based on the value of all those confirmed infected in the past – based on the assumption that patients recover from the disease on average within 14 days (source: [the ECDC Report](https://www.ecdc.europa.eu/sites/default/files/documents/covid-19-rapid-risk-assessment-coronavirus-disease-2019-ninth-update-23-april-2020.pdf)); thus, the number of recoveries on a given day equals the number of all confirmed cases three weeks prior to a given date, from which the number of deaths by that day is deducted. This simplified estimation does not take into account the more serious cases of COVID-19 with longer recovery times.     

*Note: The calculation of recoveries was changed on 9 May 2020. We now consider a patient has recovered in 14 days after their infection was confirmed (previously 21 days), so there was be a noticeable jump in the number of survivors. Please take this difference into account when estimating the number of survivors. A more detailed explanation of the changed calculation is available in the Medium article [Od potrjeno okuženih do prebolelih (From Confirmed Case to Recovery )](https://medium.com/@sledilnik/94c81674718e).*

Value formula:
- Recovered (total) = Confirmed cases (total) 21 days ago – Died (total) by the day of calculation

- Confirmed cases (active) = Confirmed cases (total) - Recovered (total) - Died (total)

</details>

<details>
  <summary id=data-contribute>How can users get actively involved in data gathering? How can I participate?</summary>

You can voluntarily help by collecting and verifying data from the media (as well as from the field), with statistical and other analyzes, etc. Contact us at info@sledilnik.org if you’d like to participate.

Sledilnik does not collect users’ personal information nor information that individuals would like to share about their condition or hospital status.


</details>

<details>
  <summary id=data-usage>How can I obtain and use your database?</summary>

Our database is public and freely available in the form of  [**CSV**, **REST**, and **Google Sheet**](/en/datasources). Kindly let us know the purpose for which you will use the information and make sure you include Sledilnik as the source of your data.

Since all the data in the database is already marked with English tags (see also [Is your webpage available in English?](#english-translation)), their international use (export, display) is also possible.

</details>

## About the calculations and graphs


<details>
  <summary id=chart-usage>Can I use your graphs on my website? How?</summary>

Sure! You can embed any graph or display on your site – citing the source, of course. [Click here](/en/embed) and select the graph you want to embed from the list. Please let us know about your use (info@slednik.org) and we will be happy to add your site to our collection of [recommended links](/en/links). 

</details>

<details>
  <summary id=chart-infocard-percent>What do the percentages in the infocards at the top of the webpage mean?</summary>

This is a percentage growth rate on a particular date in the number of newly confirmed cases compared to the previous day. If, for example, there were 16 people in the intensive care unit yesterday and today they accepted four more, that is 25% more than yesterday's situation.

</details>

<details>
  <summary id=metrics-comparison-chart>What does the “COVID-19 Situation in Slovenia” graph mean?
</summary>

The [graph](/en/stats#metrics-comparison-chart) shows the daily and overall dynamics of the spread of the infection from the beginning to the present. The indicators used (see [Which indicators does the “COVID-19 Situation in Slovenia” graph include?](#chart-metrics-included)) help us understand whether and how successfully we are controlling the spread of the virus. We can monitor the daily growth rate of newly confirmed cases and indirectly see if the measures work; information on the number of hospitalizations and the proportion of those in ICU shows how many people are seriously at risk from the disease, but at the same time, this data also shows us the real burden on the health system.

The breakpoints are indicated below, on the timeline: from the first confirmed case (March 4, 2020) to the measures (by keyword and date) taken to curb the spread and their relaxation. This helps us monitor the dynamics of the variables relative to the measures.

</details>

<details>
  <summary id=chart-metrics-included>Which indicators does the “COVID-19 Situation in Slovenia” graph include?</summary>

[Graf](/en/stats#metrics-comparison-chart) vključuje:
  
* **Tests (per day)** = Number of tests for the presence of SARS-CoV-2 virus causing COVID-19 performed. In the first stages of the epidemic, this was an important indicator of the prevalence of the virus, but with the change in testing methodology, ie. of the tested sample, it turned into an indicator of the national health and diagnostics system’s capacity.

* **Tests (total)** = Sum of tests up to; data is useful in terms of comparison or in terms of the proportion of the entire population tested, but it can be misleading as certain individuals can be tested several times (eg. health professionals, retirement home employees, etc.).

* **Confirmed Cases (per day)** = Number of confirmed infected per day based on tests. This indicator does not reflect the actual dynamics of newly infected people in the population, as the tests do not sample the entire population but target the at-risk people and certain occupational groups.

* **Confirmed cases (total)** = Total number of all confirmed cases by a given day.

* **Confirmed cases (active)** = Confirmed cases (total) – Recovered (total) – Died (total)

* **Recovered (total)** = Number of recoveries on a given day is a simple estimate equal to the number of all confirmed cases two weeks prior to a given date (assuming an average of 14 days needed to recover), from which the number of fatalities till that very date is subtracted. See also [Why did you replace the number of cured with the number of recovered patients?](#data-recovered)

* **Hospitalized (active)** = Current number of people in hospital care (either in the ordinary ward or in the ICU).

* **Hospitalized (total) ** = Sum of hospital admissions by date.

* **ICU (active)** = Current number of people in ICUs (intensive care units).

* **On ventilator (active) ** = Current number of persons in need of a ventilator.

* **Discharged from a hospital (daily)** = Number of discharged from hospital on that day.

* **Discharged from hospital (total)** = Sum of all discharged from a hospital up to this day.

* **Deaths (per day) ** = Number of deaths due to COVID-19 on that day.

* **Deaths (total) ** = Sum of all deaths to date.
  
</details>

<details>
  <summary id=chart-terminology>What does it mean?
</summary>
  
Sledilnik uses terminology which is consistent with the official directives of the WHO and ECDC (European Center for Disease Prevention and Control). We use the following tags in the displays:  
* **Confirmed cases** = This is the number of people who tested positive for the SARS-CoV-2 virus. Since the number of confirmed cases depends solely on testing, the number of confirmed cases is significantly lower than the actual number of infected people.

* **Hospitalized** = This is the number of confirmed cases such severe symptoms of COVID-19 that they have been admitted to hospital.

* **In ICU** = Indicates the number of hospitalized persons who are at risk of death because of the severe symptoms of COVID-19 and require placement in the intensive care unit. This is a subset of the *Hospitalized* category. 

* **On ventilator** = Indicates the number of hospitalized persons in the intensive care unit who require a ventilator to breathe. It is a subset of the *Intensive Care* and *Hospitalized* categories.

* **Recovered** = This is an estimate of the number confirmed cases that are expected to have recovered after 14 days. The number of recoveries is thus equal to the number of all confirmed cases two weeks prior – assuming that the disease should be overcome within 14 days – from which the number of deaths by that given day is subtracted. (See also the question [Why did you replace the number of cured with the number of recovered patients?](#data-recovered)
  
</details>

<details>
  <summary id=cases-chart>What are the Closed cases and what are the Active cases?</summary>

All confirmed cases are shown in the [Confirmed Cases graph](/en/stats#cases-chart). In order to be able to monitor the epidemic, it is important to know how many are still infected. For this reason, we use the following terminology:

**Closed cases**  are the sum of all confirmed cases who are no longer infected with the virus, that is, the recovered and the deceased.

**Active cases** are all confirmed virus infections that still haven’t recovered (are still infected with the virus). See also
 [Which indicators does the “COVID-19 Situation in Slovenia” graph include?](#chart-metrics-included)

</details>


<details>
  <summary id=chart-phases>What do the different phases (phases 1-7) in the graph mean?</summary>

The vertical lines divide the stages, delimited by the dates, when the authorities changed the way information about the spread of the infection was collected (the test method was changed, self-isolation interventions were introduced, bans on gathering and movement of persons, and mandatory basic protection were required).

The phases are shown because the change in testing methodology has also changed the importance of certain indicators by which the prevalence of infections can be judged.

* **Phase 1 (March 4-12, 2020)**: The first cases of infection in Slovenia are recorded. All cases are followed, all contacts are tested.

* **Phase 2 (March 13-19, 2020)**: The testing methodology is changed, and self-isolation and social distancing measures are introduced.

* **Phase 3 (March 20-April 7)**: The testing methodology is revised and a ban on gathering more than five people in public areas is put in place.

* **Phase 4 (April 8-15)**: New change in testing methodology – people with mild symptoms from households with more than one respiratory infections are also tested.

* **Phase 5 (April 15-21)**: New change in testing methodology – re-testing, if possible, of **all suspected SARS-CoV-2 virus infections**.

* ** Phase 6 (April 21–May 15**: New change in testing methodology – All persons **suspected of being infected with SARS-CoV-2** are tested again. A national survey of 3000 randomly sampled persons (additional testing, blood testing for the presence of antibodies) begins.

* **Phase 7 (May 15–present)**: The Slovenian government called an official end to its coronavirus epidemic – a quarantine of at least 14 days will remain in place for people from non-EU states, except for some exemptions (diplomats, cargo). Citizens still have to follow basic rules to prevent a possible spread of infection.

</details>

<details>
  <summary id=patients-chart>What does the “Hospitalizations” graph tell us?</summary>

The [graph](/en/stats#patients-chart) in the default view *All Hospitals* shows us the whole picture of hospitalizations by date arranged by the condition of patients: columns with a positive value (those above the horizontal axis) show the number admitted to hospital, the number hospitalized, shades of red are used to demark individuals in ICUs, specifically depicting how many of these are in critical condition on the ventilators. Columns with a negative value (those below the horizontal axis) show the number of discharges and deaths that day. You can also select specific hospital and see only hospitalizations there. If you select the *By Hospitals* view below, you can see the number of people in hospital care by day for each of the COVID-19 hospitals.  
The graph can offer a good insight into the workload of hospitals and can be the basis for assessing hospital capacity and planning their possible increase.

</details>

<!-- <details>
  <summary id=ratios-chart>Kaj nam pove graf "Delež resnih primerov"?</summary>

[Graf](/en/stats#ratios-chart) prikazuje deleže resnih primerov bolezni in smrtnosti v treh različnih prikazih. Vsi podatki so prikazani kot procent (%). 

(*Resni primeri*) nam kaže hospitalizirane, v intenzivni enoti, na respiratorju in umrle kot delež vseh potrjeno okuženih. Iz tega je razvidno kako velik delež vseh potrjeno okuženih oseb ima težjo obliko bolezni, ki zahteva hospitalizacijo, sprejem v intenzivno enoto in uporabo respiratorja.

(*Hospitalizirani*) nam kaže osebe v intenzivni enoti, na respiratorju in umrle v bolnišnici kot delež vseh hospitaliziranih. Ta prikaz ponazori na kakšnem oddelku in kakšno obravnavo potrebujejo hospitalizirani bolniki.

(*Smrtnost*) nam pokaže delež smrti v bolnišnici glede na vse umrle in delež smrti v intenzivni enoti glede na vse umrle v bolnišnici - prikazano s polno črto. Prikaz nam tudi prikaže Smrtnost v bolnišnici (koliko oseb umre glede na vse hospitalizirane) in Smrtnost v intenzivni enoti (koliko oseb umre glede na vse sprejete v intenzivno enoto) - prikazano s črtkano črto. 

Graf je uporaben za razumevanje obravnave bolnikov v Sloveniji in primerjave z ostalimi državami (glede na njihova poročila).

</details> -->

<details>
  <summary id=hcenters-chart>What does the “Healthcare Center Treatment” graph mean?</summary>

The [graph](/en/stats#hcenters-chart) shows the treatment of suspicions of COVID-19 in healthcare centers (primary health care level). You can show data for whole country or select specific region. Healthcare centers are the first entry point for taking swabs to be tested for the presence of the virus, so an increase in the number of suspicions and referrals to self-isolation may be an early indicator that new outbreaks have occurred.

The graph thus shows the number of all emergency medical visits (also for other diseases) in healthcare centers (see notes below), the number of suspected cases of COVID-19 based on the number of examinations at the COVID-19 entry point, and all suspicions of infections based on telephone conversation with suspected infected patients. Some people may be recorded several times, first by telephone and then during the examination. We also show the total number of referrals to self-isolation.

*Note 1: in some municipalities, the control point for COVID-19 is within the hospital premises (for example the Celje and Novo mesto General Hospitals). Data before 14.4. is not available for these general hospitals. 
Note 2: the methodology for recording suspicions of inspections via telephone conversation has changed, so all suspicions were initially recorded. Since April 23, however, only those suspicions via telephone conversation have been recorded, where no examination and swabbing (testing) was ordered. Therefore it is possible that there are differences in how individual healthcare centers report this data and that this number is too high.*

When reporting the number of tests performed, all tests (including repeated tests) are recorded. The number of positive tests therefore includes all positive tests – the same person can be tested several times and counted as positive several times. The number of tests performed may therefore be greater than the number of positive tests reported by laboratories (there, each person is recorded only once). See also [What does the “Testing” graph tell us?](#test-charts) 

</details>


<details>
  <summary id=tests-chart>What does the “Testing” graph tell us?</summary>

The [graph](/en/stats#tests-chart) shows the total number of regular tests (the *Regular* display), and the national IMI survey tests (by selecting the *Survey* display). The columns show the number of negative and positive tests on a specific day, and the curve shows the daily percentage of positive tests.

All important health organizations and institutions are aware of the fact that testing for coronavirus infection is one of the most important factors, as only through testing can we understand the course and extent of the pandemic and thus respond appropriately to the threat it poses. However, each country deals with the lack of testing capacities in its own way. On *March 14*, 2020, Slovenia changed the method of monitoring the spread of the infection. From that point on, people with a respiratory infection who did not require hospital care (the situation is therefore only assessed on the basis of the number of diseased patients) were no longer tested. However, all persons with a mild respiratory infection older than 60 years, persons with other confirmed diseases (hypertension, diabetes, and cardiovascular, lung, kidney, and severe liver diseases) and people with immune deficiencies (regardless of age) were tested. On *April 21*, 2020, the method of testing changed again as healthcare professionals were instructed to test all patients with suspected respiratory infection (including those with milder symptoms and regardless of their age).
 
</details>

<details>
  <summary id=infections-chart>What does the “Structure of Confirmed Cases” graph tell us?</summary>

The [graph](/en/stats#infections-chart)provides an insight into the daily share of confirmed cases from high-risk groups or employees in high-risk areas. Due to insufficiently accurate input data on confirmed infections, daily values (By days (average)) are shown as a moving average of 5 days. The sum of the values on a particular day, from 2 days prior, and 2 days after, is divided by 5. Therefore, the graph shows the situation three days before a specific day, and in this way we get a better idea of trends by individual groups. If we select the *Total* or *Relative* display, we will jump from the confirmed cases curve to the histogram, which shows the number of confirmed infected persons within each category on a given day.

The increase in infected healthcare workers does not mean that they were discovered exactly on that day; they may have been positive before but information on their status was obtained subsequently. The *Retirement Home Employees* category includes healthcare workers, associates, and external assistance (health students), so the daily data on healthcare workers (blue curve or columns) are reduced accordingly. This means that the number of health professionals is a very conservative estimate.

</details>

<details>
  <summary id=spread-chart>What does the “Growth of Confirmed Cases” graph mean?</summary>

The [graph](/en/stats#spread-chart) tells us how many new confirmed cases of infections there were on a given day, where the WHO and the [ECDC definition](https://www.ecdc.europa.eu/en/case-definition-and-european-surveillance-human-infection-novel-coronavirus-2019-ncov) that confirmed cases are “persons with a lab confirmation of infection with COVID19” is followed. As the number of confirmed cases still depends on testing, the data in confirmed cases is estimated to be much smaller that the actual number of infected people.
  
</details>

<details>
  <summary id=regions-chart>What does the “Confirmed Cases by Region” tell us?</summary>

The [graph](/en/stats#regions-chart) shows the dynamics of growth of confirmed cases by selected regions. Individual regions can be easily compared by selecting the ones you want shown on the graph by clicking on specific regions below the graph. From the curve, we can quickly see which regions have the most and which the least confirmed cases and how this number has changed over time.

</details>

<details>
  <summary id=map-chart>What does the “Municipality Map” tell us?</summary>

The [map](/en/stats#map-chart) shows us the epidemiological picture of individual municipalities, as it allows the display of *Confirmed Cases* (red shades) or the *Dead* (gray shades). When showing confirmed cases, we can see which municipalities are the most "healthy" (white) and which are currently the more "infected" (red shades) – if new cases are still appearing or not - and relative to the share of the population (Proportion of population is the default display). On the left, we can use the filter (7, 14 or 21 days) to determine for what period of time we view data on new confirmed cases or deaths. For those municipalities where new cases are still being confirmed, we can conclude that the epidemic is still active. (Of course, this does not necessarily mean that the virus is not present in municipalities without new confirmed cases, but it is an indicator of the "health" of a certain area.) More details are available in the Medium article [Kje so “zdrave” občine? (Where Are the ‘Healthy’ Municipalities?)](https://medium.com/sledilnik/kje-so-zdrave-ob%C4%8Dine-613afc42b023) 

By clicking on *Absolute* in the upper right corner, we can change the display and see the total number of newly confirmed cases or deaths in a selected time frame (7, 14 or 21 days) in municipalities according to how they are painted.

</details>

<details>
  <summary id=municipalities-chart>What does the chart “Cases by Municipalities” show us?</summary>

The [chart](/en/stats#municipalities-chart) shows individual municipalities in columns in more detail with the number of confirmed cases by days, with active cases, recoveries (assessment) and deaths in each municipality. Below the municipality you can find the information about the time since the last confirmed case. Municipalities are classified according to when the last confirmed case was recorded there, from which we can conclude which municipalities are currently more “infected” and which are “healthier” than others.

The display can be changed by selecting different views above the graph: if you select the *Active* display, the municipalities will be sorted according to the current assessment of active cases; or if you select *All*, then the municipalities will be arranged by the largest total number of confirmed cases. If you choose *All Regions* from the dropdown menu, then confirmed cases will be shown in the municipalities belonging to that region. You can also easily search for a municipality by entering its name in the *Find Municipality* browser.

*Note: the assessment of recoveries and active cases is done 14 days after the infection was confirmed, if and when the disease is in its mild form. However, if an individual is hospitalized, this recovery will last longer, but in this case the individual is not dangerous to the environment because he is in hospital. Since we do not take into account the hospitalized in the municipality presentation, it is possible that the sum of active cases by municipality does not match the estimate of the active cases for the whole country. See also [Do you keep an Active Case counter and do you know how many people are currently infected?](#data-active-cases)*

</details>

<details>
  <summary id=age-groups-chart>What does the “Age Groups” graph show?</summary>

The [graph](/en/stats#age-groups-chart) shows the age structure of all confirmed coronavirus cases and deaths. The graph also displays demarcations by gender. The display shows absolute values and can be changed at the top right to the *Relative* display for a better insight into what the mortality rate from COVID-19 is relative to the general population throughout the epidemic period. In the Relative view, there are the options for different views below: by selecting *Proportion of confirmed cases*, the share of confirmed cases within a certain age group will be displayed. By selecting the *Death rate*, we will see the number of deaths per population size. By selecting *Deaths by no. of confirmed cases*, we can understand what the proportion of deaths in a particular age group was in relation to the number of confirmed cases.

Demographics can help us understand how the pandemic has spread and why it has disproportionately affected certain age groups. According to currently known data, COVID-19 is more dangerous to the elderly and those with comorbidities, and according to some data, men are more exposed. However, in order to understand all the factors, we would need to obtain more data: what the comorbidities were, the socio-economic situation of the patients, the geographical area, etc.   
*Note: Unlike other data that is published regularly for different categories, official sources obtain demographic data with a time lapse (age, municipality ...), so these are usually known with a one-day delay. This is also the reason that in the By Age Groups display, there may be some deviations from data in other displays, such as lower values of the number of confirmed cases and deaths.*
 
</details>

<details>
  <summary id=countries-chart>What does the “Comparison by Country” graph show?</summary>

The [chart](/en/stats#countries-chart) shows a comparison between Slovenia and different groups of countries in terms of the number of deaths due to COVID-19 per million inhabitants. The graph is arranged chronologically. You can change the view of different chronological displays of comparisons of different clusters of countries by clicking on the appropriate tabs below.   

</details>

<details>
  <summary id=chart-reality>Do your graphs represent the real picture?</summary>

Yes, as far as they can, given the limitations of the current displays and of the data itself: the graphs on this page only show what can be deduced from the information given. For example, the total number of tests represents the number of tests performed to date, but does not reflect the total number of people tested, as some people, such as healthcare professionals and people suspected of being infected, have been repeatedly tested.

However, the number of confirmed cases depends solely on testing. Since the majority of infected people, who have mild or no symptoms, have not been tested for COVID-19 at all, the number of confirmed cases is significantly lower than the actual number of infected people.

</details>

## About the project

<details>
  <summary id=what-is-sledilnik>What is Sledilnik?</summary>

[Sledilnik je projekt](/en/about) is an open-data and open-sourced project that collects, analyzes and displays some of the most useful data to better understand the spread of the coronavirus pandemic and COVID-19 disease, along with its dynamics and scope. We want to make clear graphical and statistical visualizations of what current data and reviews tell us about the spread of the virus in Slovenia, and ensure that information on the magnitude and severity of the COVID-19 problem in Slovenia becomes accessible and comprehensible to all.

</details>

<details>
  <summary id=add-link>I would like to recommend a valuable source, which is not yet among your ‘Recommended Links’, but should be. Will you add it?</summary>

Contact us at info@sledilnik.org – we will review the suggested link and, if the site is credible and useful, will be happy to include it among our recommended [Links](/en/links).

If you would like to go a step further and contribute to our common goal, submit a Pull-Request (PR) on [GitHub](https://github.com/sledilnik/website/blob/master/src/content/links.md).

</details>

<details>
  <summary id=how-to-help>I would like to help, where can I begin?</summary>

Contact us at info@sledilnik.org and briefly describe who you are and how you can contribute to the project. Warmly welcome to help.

</details>
