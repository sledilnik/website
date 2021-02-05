# Models and Analyses

The [COVID-19 Sledilnik](https://covid-19.sledilnik.org/en/) community includes experts in statistical modeling and computer simulations. This page includes a selection of some Slovenian models, analyses and contributions prepared by experts using publicly available data published on the [GitHub](https://github.com/sledilnik/data) repository.

## <a id="limitations"></a>Limitations of epidemiological modeling
Uncertainties enter the models and analyses of the COVID-19 epidemic in different ways:
- the spread of SARS-CoV-2 virus is unpredictable, as the infectious period occurs before the onset of symptoms,
- there is uncertainty about the impact of measures taken by the public and the government,
- we lack more accurate, deeper and machine readable data, and
- any change in testing methodology can completely change the assumptions on which the models and analyses were based on.

All of these are reasons why the uncertainty associated with the models is relatively large, and, the further we look to the future, the larger it becomes.

<!-- The models take into account the information known so far about COVID-19 and its spread in Slovenia. No model can offer accurate predictions for the future course of the epidemic, but we seek to carefully disclose all the assumptions of the model. Data on the number of COVID-19 tests performed and the number of COVID-19 cases confirmed in Slovenia does not necessarily reveal the number of asymptomatic infections, so most of our models are calibrated to data on hospitalizations. The uncertainty about the exact number confirmed cases propagates to the uncertainty in the predictions.-->

<!--Scientists worldwide are making great efforts to combat COVID-19, but many aspects of the spread and development of the disease have not yet been fully explored. In particular, there is uncertainty about the impact of measures taken by the public and the governments around the world to control the disease. In addition, due to the time lag between the infection and the onset of symptoms, it is practically impossible to accurately estimate the actual state and the rate of spread of the infection in the population. All of these are reasons why the uncertainty associated with the models is relatively large, and, the further we look to the future, the larger it becomes.-->

<!-- *Mathematical modeling with an accompanying display of the possible outcomes of an epidemic helps to shape public health measures. In order to make the modeling results more reliable, it is very important to critically evaluate the data used and to check whether all the possible ways of the disease spreading in a population have been taken into account.* -->

## <a id="tableOfContent"></a>Content
- [Upgraded SEIR models](#seir)
- [Social Network Model](#social_network)
- [Evaluating the Reproduction Rate](#reproduction-rate)
- [Determining alarms](#alarms)
- [Interesting Articles](#articles)

<!--# Models-->

## <a id="seir"></a>Upgraded SEIR models
[Prof. Janez Žibert](https://pacs.zf.uni-lj.si/janez-zibert/) from the Faculty of Medicine, University of Ljubljana, developed an [upgraded SEIR model](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221) with compartments for modeling hospitalizations, intensive care units, and deaths.

<a href="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png" class="img-link">
<img alt="SEIR model" src="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png"></a>

More detailed projections of the model are to be found [at this link](https://apps.lusy.fri.uni-lj.si/~janezz/reports/report_latest.html).

[Dr. Matjaž Leskovar](https://r4.ijs.si/leskovar#elf_l1_Lw) from the Jožef Stefan Institute prepares [daily analysis and projections](https://r4.ijs.si/COVID19#elf_l1_Lw) of the epidemic in Slovenia. The calculations are based on the SEIR type model with extensions.

<a href="https://r4.ijs.si/files/figures/COVID19/Prognoza-IJS-R4.png" class="img-link">
<img alt="Model IJS-R4" src="https://r4.ijs.si/files/figures/COVID19/Prognoza-IJS-R4.png"></a>

## <a id="social_network"></a>The Social Network Model
[Dr. Žiga Zaplotnik](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani je razvil model prenosa virusa po socialnem omrežju prebivalcev v Sloveniji, ki je bil objavljen v znanstveni reviji [PLOS ONE](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0238090).

[Dr. Žiga Zaplotnik](https://twitter.com/ZaplotnikZiga) from the Faculty of Mathematics and Physics of the University of Ljubljana developed a probabilistic Model of virus transmission via the Social Network of Slovenia, which was published in the scientific journal [PLOS ONE](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0238090).

<a href="/images/zaplotnik-plos-social-network-model.png" class="img-link">
<img class="rightThumbnail" alt="Social network model" src="/images/zaplotnik-plos-social-network-model.png"></a>

In the simulation, the virus is transmitted according to a realistic model of the social network of Slovenians, which contains more than 2 million nodes (1 for each inhabitant of Slovenia), divided into households and retirement homes. The nodes are also randomly connected outside of these units, according to the known distribution of contacts - some people have more daily contacts, others less. This allows the model to effectively simulate different virus containment strategies. A probabilistic prediction is obtained by preparing a set of simulations with slightly altered initial conditions and parameters that determine coronavirus spread and the course of the COVID-19 disease.

The history of the calculations can be found [here](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).

<!--
<a href="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" class="img-link">
<img alt="Omrežje model" src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png"></a>
-->

<!--# Analysis-->

## <a id="reproduction-rate"></a>Evaluating the Reproduction Rate
The University of Ljubljana Medical Faculty’s [Institute for Biostatistics and Medical Informatics](https://ibmi.mf.uni-lj.si/en) team, led by Prof. Maja Pohar Perme, is [estimating the reproduction rate](https://oblak8.mf.uni-lj.si/covid19/),  i.e. the rate of spread of the infection. To create their model, they used Bayesian statistics, allowing the estimation of complex parameters with only a limited amount of data.

<a href="https://oblak8.mf.uni-lj.si/covid19/" class="img-link">
<img alt="R_t model" src="https://oblak8.mf.uni-lj.si/covid19/rt_graph.svg" width=600>
<!--<img alt="R_t model" src="/docs/ibmi-model-20200627.png">-->
<!--<img alt="R_t model" src="https://stat.columbia.edu/~jakulin/Covid/ocene_rt.png">-->
</a>

More about the methodology and results can be found in the articles in the [Zdravniški vestnik](https://vestnik.szd.si/index.php/ZdravVest/article/view/3068) ([summary](https://ibmi.mf.uni-lj.si/files/Pregledni%20povzetek_74e.pdf)) and the scientific journal [Mathematical Biosciences](https://www.sciencedirect.com/science/article/abs/pii/S0025556420301279).

## <a id="alarms"></a>Determining alarms
Dr. Janez Stare and dr. Nina Ružić Gorenjec ([IBMI](http://ibmi.mf.uni-lj.si/)) in the analysis [Determining alarms in the COVID-19 epidemic in Slovenia](https://ibmi.mf.uni-lj.si/sl/centri/biostatisticni-center/interaktivno/dolocanje-alarmov-pri-epidemiji-covid-19-v-sloveniji) assess the extent to which the epidemic has been restarted. The results are particularly relevant for periods of low virus incidence in the population. In the calculations the ratios of confirmed cases among all tested cases are used.


## <a id="articles"></a>Interesting Articles

- IBMI researchers, led by Dr. Nina Ružić Gorenjec, in their analysis, [From the story of success to the disaster](https://medium.com/sledilnik/od-zgodbe-o-uspehu-do-katastrofe-63b77b1a23e1), emphasized the importance of the reduced public mobility and epidemiological contact tracing for a successful response to the epidemic. (2021/1/13)

- [What has math to do with epidemic?](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221) The article introduces you to basic ideas and mathematics behind the modeling. Written based on the lecture of prof. Janez Žibert. (2020/12/17)

- [Statistics - the basis for understanding the epidemic](https://www.youtube.com/watch?v=Bwn6cfgPZ1Q&t=3s). The following people took part in the conversation hosted by STAznanost: prof. dr. Leon Cizelj, Head of the Reactor Engineering Department at the Jožef Stefan Institute; Dr. Zarja Muršič, representative of Sledilnik; Dr. Mario Fafangel, head of the Center for Infectious Diseases of the National Institute of Public Health. 

- In the article ["Two Slovenias - are we really standing still?"](https://medium.com/sledilnik/dve-sloveniji-ali-res-stopicamo-na-mestu-27fac63d9e6f) we drew attention to local differences in the spread of the virus and that a simultaneous response across the country is important to reduce the epidemic. (2020/12/2)

- Members of the COVID-19 Sledilnik presented [the importance of data and modeling for decision-makers](https://medium.com/sledilnik/povzetek-nastopov-strokovnjakov-s-seje-parlamentarnega-odbora-12-11-2020-5a3ead7b4898) in the Slovenian Parliament. (2020/11/12)

- ["Epidemic and Models - Fundamentals of Epidemiological Modeling"](https://medium.com/sledilnik/epidemija-in-modeli-786e02f1bd8a) introduces you to the basic concepts that are most necessary for understanding the epidemic in numbers. Watch short clips. (2020/11/3)

- In the editorial of the Zdravstveno varstvo journal, which is published by the Slovenian National Institute of Public Health (NIJZ), Eržen et al. summarized the [current findings and key challenges in modeling the COVID-19 epidemic](https://content.sciendo.com/view/journals/sjph/59/3/article-p117.xml?tab_body=abstract). (2020/6/25)

- Meeting of Slovenian Scientists on the Topic of COVID-19 Measures: the Young Statistician Section of the [Statistical Society of Slovenia](https://stat-d.si/) organized an online interview hosted by the Institute of Biostatistics and Medical Informatics (IBMI) of the Ljubljana University’s Medical Faculty. The conversation was anchored by Dr. Andrej Srakar and Dr. Ana Slavec. The Slovenian Press Agency, STA, broadcasted the event live to more than 850 participants. You can view the recording and lectures [here](https://medium.com/sledilnik/64233b35580c). (2020/4/21)

-  [Dr. Andrej Srakar](https://sites.google.com/site/andrejsrakar1975/) wrote an excellent review article,
 [An Introduction to Modeling and Statistical Aspects of the COVID-19 Epidemic](https://udomacenastatistika.wordpress.com/2020/04/20/uvod-v-modeliranje-in-statisticne-vidike-covid-19/), about the first models related to the COVID-19 epidemic that were developed in Slovenia. (2020/4/20)
