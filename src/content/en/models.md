# Models and Forecasts

Our community includes experts in statistical modeling and computer simulations who actively participate in it. This page is dedicated to some of their models. All models are calibrated using [COVID-19 Sledilnik data](https://covid-19.sledilnik.org). The following models are introduced:
- [Interactive SEIR Model](#seir)
- [Evaluating the Reproduction Rate](#reproduction-rate)
- [Model of Virus Transmission via the Social Network of Slovenia](#social_network)
- [Other Models for Slovenia](#other)
- [Interesting Articles about Epidemic Modeling](#articles)

> **_NOTE:_**  The models take into account the information known so far about COVID-19 and its spread in Slovenia. No model can offer accurate predictions for the future course of the epidemic, but we seek to carefully disclose all the assumptions of the model. Data on the number of COVID-19 tests performed and the number of COVID-19 cases confirmed in Slovenia does not necessarily reveal the number of asymptomatic infections, so most of our models are calibrated to data on hospitalizations. The uncertainty about the exact number confirmed cases propagates to the uncertainty in the predictions.

Scientists worldwide are making great efforts to combat COVID-19, but many aspects of the spread and development of the disease have not yet been fully explored. In particular, there is uncertainty about the impact of measures taken by the public and the governments around the world to control the disease. In addition, due to the time lag between the infection and the onset of symptoms, it is practically impossible to accurately estimate the actual state and the rate of spread of the infection in the population. All of these are reasons why the uncertainty associated with the models is relatively large, and, the further we look to the future, the larger it becomes.


## <a id="seir"></a>Interactive SEIR Model
In collaboration with [*Prof. Janez Žibert*](https://pacs.zf.uni-lj.si/janez-zibert/) from the Faculty of Medicine, University of Ljubljana, the SEIR model (Susceptible, Exposed, Infected, and Recovered) with submodels for modeling hospitalizations, intensive care, and deaths was developed. The parameters of the SEIR model are consistent with data on hospitalizations and the clinical picture of COVID-19 in Slovenia.

***More detailed projections of the model are to be found [at this link](https://apps.lusy.fri.uni-lj.si/appsR/CoronaV2/).***

<a href="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png" class="img-link">
<img alt="SEIR model" src="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png"></a>


## <a id="reproduction-rate"></a>Evaluating the Reproduction Rate
The University of Ljubljana Medical Faculty’s [*Institute for Biostatistics and Medical Informatics*](https://ibmi.mf.uni-lj.si/en) team, led by *Prof. Maja Pohar Perme*, [estimated the reproduction rate](http://ibmi.mf.uni-lj.si/files/Pregledni%20povzetek_74e.pdf) ,  i.e. the rate of spread of the infection, on the basis of the mortality data. To create their model, they used Bayesian statistics, allowing the estimation of complex parameters with only a limited amount of data. This offers the possibility of a faster response.

A more detailed description of the original methodology and results is published in an article in the [Zdravniški vestnik](https://vestnik.szd.si/index.php/ZdravVest/article/view/3068) medical journal.

***The original model was later upgraded, and daily results are now posted [at this link](https://oblak8.mf.uni-lj.si/covid19/).***

<a href="https://oblak8.mf.uni-lj.si/covid19/" class="img-link">
<img alt="R_t model" src="https://oblak8.mf.uni-lj.si/covid19/rt_graph.svg" width=600>
<!--<img alt="R_t model" src="/docs/ibmi-model-20200627.png">-->
<!--<img alt="R_t model" src="https://stat.columbia.edu/~jakulin/Covid/ocene_rt.png">-->
</a>


## <a id="social_network"></a>Model of Virus Transmission via the Social Network of Slovenia
In cooperation with [*Dr. Žiga Zaplotnik*](https://twitter.com/ZaplotnikZiga) from the Faculty of Mathematics and Physics of the University of Ljubljana, we prepared a probabilistic forecast of the pandemic in Slovenia. In the simulation, the virus is transmitted according to a realistic model of the social network of Slovenians, which contains more than 2 million nodes (1 for each inhabitant of Slovenia), divided into households and retirement homes. The nodes are also randomly connected outside of these units, according to the known distribution of contacts - some people have more daily contacts, others less. This allows the model to effectively simulate different virus containment strategies. A probabilistic prediction is obtained by preparing a set of simulations with slightly altered initial conditions and parameters that determine coronavirus spread and the course of the COVID-19 disease. The probabilistic prediction also varies between individuals. 

A more detailed description of the model can be found [in this article](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0238090), while the history of the calculations can be found [here](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).s

<!--
<a href="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" class="img-link">
<img alt="Omrežje model" src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png"></a>
-->

## <a id="other"></a>Other Models for Slovenia

-  The model in the [*Determining alarms in the COVID-19 epidemic in Slovenia (IBMI MF UL)*](https://ibmi.mf.uni-lj.si/sl/centri/biostatisticni-center/interaktivno/dolocanje-alarmov-pri-epidemiji-covid-19-v-sloveniji) article assesses the extent to which the epidemic has been restarted. It uses the proportion of confirmed infected among those tested for evaluation and is updated daily with Slednik data.

-  [*Dr. Matjaž Leskovar*](http://r4.ijs.si/leskovar) from the Reactor Engineering Department of the Jožef Stefan Institute is preparing a forecast for the development of the epidemic in Slovenia based on publicly available data on confirmed infections and current hospitalizations. The linksto the [description of the model](http://r4.ijs.si/COVID19model#elf_l1_Lw), [an archive of previous forecasts](http://r4.ijs.si/COVID19arhiv), and a [display of the latest results](http://r4.ijs.si/COVID19) are available.

-  [*Dr. Andrej Srakar*](https://sites.google.com/site/andrejsrakar1975/) wrote an excellent review article,
 [*An Introduction to Modeling and Statistical Aspects of the COVID-19 Epidemic*](https://udomacenastatistika.wordpress.com/2020/04/20/uvod-v-modeliranje-in-statisticne-vidike-covid-19/), for the *Tamed Statistics* blog post about key models of the spread of the COVID-19 epidemic that have developed in Slovenia.


## <a id="articles"></a>Interesting Articles about Epidemic Modeling

- [What has math to do with epidemic?](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221) - basic ideas and mathematics behind the modeling. Written based on the lecture of prof.Janeza Žiberta.

- *Presentation of modeling in Slovenian Parliament*: on November 12, 2020 members of Sledilni presented the nead for data and modeling in Slovenian Parliament. You can see [abstract and recordings](https://medium.com/sledilnik/povzetek-nastopov-strokovnjakov-s-seje-parlamentarnega-odbora-12-11-2020-5a3ead7b4898).

- *Mathematical modeling with an accompanying display of the possible outcomes of an epidemic helps to shape public health measures. In order to make the modeling results more reliable, it is very important to critically evaluate the data used and to check whether all the possible ways of the disease spreading in a population have been taken into account.* – In the **editorial of the Zdravstveno varstvo journal**, which is published by the Slovenian National Institute of Public Health (NIJZ), Eržen et al. summarized the current findings and key challenges in modeling the COVID-19 epidemic.

- *Meeting of Slovenian Scientists on the Topic of COVID-19 Measures*: On 21 April 2020, the Young Statistician Section of the  [*Statistical Society of Slovenia*](https://stat-d.si/) organized an online interview hosted by the Institute of Biostatistics and Medical Informatics (IBMI) of the Ljubljana University’s Medical Faculty. The conversation was anchored by *Dr. Andrej Srakar* and *Dr. Ana Slavec*. The Slovenian Press Agency, STA, broadcasted the event live to more than 850 participants. You can view the recording and lectures [here](https://medium.com/sledilnik/64233b35580c).  
