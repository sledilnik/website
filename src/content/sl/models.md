# Modeli in analize

V skupnosti [COVID-19 Sledilnik](https://covid-19.sledilnik.org) sodelujejo strokovnjaki za statistično analizo, modeliranje in numerične simulacije.
Na tej strani je objavljen izbor nekaterih slovenskih modelov, analiz in prispevkov,
ki so jih strokovnjaki pripravili z uporabo javno dostopnih podatkov, objavljenih na portalu [GitHub](https://github.com/sledilnik/data).

## <a id="limitations"></a>Omejitve epidemiološkega modeliranja
Negotovosti v modele in analize epidemije COVID-19 vstopajo na različne načine:
- nepredvidljiva je širitev virusa SARS-CoV-2, saj kužnost nastopi pred simptomi bolezni,
- nepredvidljiva je družbena povezanost in učinki ukrepov nanjo,
- imamo pomanjkanje natančnih, globokih in strojno berljivih podatkov ter
- vsakršna sprememba metodologije testiranja lahko popolnoma spremeni pretekle predpostavke, na katerih so modeli in analize sloneli - več lahko preberete v članku [Kako vemo, da ne vemo?](https://medium.com/sledilnik/kako-vemo-da-ne-vemo-6570b92a8b3c)

Vse to so razlogi, da so modelom in analizam pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot projiciramo v prihodnost, hitreje rastejo.

## <a id="tableOfContent"></a>Vsebina
- [Nadgrajeni modeli SEIR](#seir)
- [Model socialnega omrežja](#social_network)
- [Ocenjevanje stopnje reprodukcije](#reproduction-rate)
- [Določanje alarmov](#alarms)
- [Prispevki](#articles)


<!--# Modeli-->

## <a id="seir"></a>Nadgrajeni modeli SEIR
[Prof. Janez Žibert](https://pacs.zf.uni-lj.si/janez-zibert/) z Zdravstvene fakultete, Univerze v Ljubljani je pripravil [nadgrajen model SEIR](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221) za spremljanje bolnišničnih obravnav, obravnav v enotah intenzivne terapije in dnevnega oz. kumulativnega števila smrti v Sloveniji.

<a href="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png" class="img-link">
<img alt="SEIR model" src="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png"></a>

Oglejte si tudi [podrobnejše prikaze projekcij in simulacij](https://apps.lusy.fri.uni-lj.si/~janezz/reports/report_latest.html).

[Dr. Matjaž Leskovar](https://r4.ijs.si/leskovar#elf_l1_Lw) iz Instituta “Jožef Stefan” (IJS, Odsek R4) pripravlja [dnevno analizo stanja in projekcije](https://r4.ijs.si/COVID19#elf_l1_Lw) razvoja epidemije v Sloveniji. Izračuni temeljijo na modelu tipa SEIR z razširitvami.

<a href="https://r4.ijs.si/files/figures/COVID19/Prognoza-IJS-R4.png" class="img-link">
<img alt="Model IJS-R4" src="https://r4.ijs.si/files/figures/COVID19/Prognoza-IJS-R4.png"></a>

## <a id="social_network"></a>Model socialnega omrežja
[Dr. Žiga Zaplotnik](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani je razvil model prenosa virusa po socialnem omrežju prebivalcev v Sloveniji, ki je bil objavljen v znanstveni reviji [PLOS ONE](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0238090).

<a href="/images/zaplotnik-plos-social-network-model.png" class="img-link">
<img class="rightThumbnail" alt="Social network model" src="/images/zaplotnik-plos-social-network-model.png"></a>

Virus se v simulaciji prenaša po karseda realističnem modelu socialnega omrežja Slovencev, ki vsebuje več kot 2 milijona vozlišč (1 za vsakega prebivalca Slovenije). Vozlišča so najprej zbrana v skupke, ki ponazarjajo gospodinjstva in domove oskrbovancev, nato pa jih ob vsakem koraku simulacije naključno prevezujemo med izbranimi skupki, glede na znane porazdelitve kontaktov – nekatere osebe imajo dnevno več kontaktov, druge manj. To omogoča, da lahko z modelom simuliramo različne pristope spopada z virusom. Verjetnostno napoved dobimo tako, da pripravimo množico simulacij z rahlo spremenjenim začetnim pogojem in parametri, ki določajo širjenje koronavirusa in potek bolezni COVID-19.

<!--
<a href="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" class="img-link">
<img alt="Omrežje model" src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png"></a>
-->
Zgodovina izračunov je dostopna v [arhivu](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).


<!--# Analize-->

## <a id="reproduction-rate"></a>Ocenjevanje stopnje reprodukcije
Skupina [Inštituta za biostatistiko in medicinsko informatiko](http://ibmi.mf.uni-lj.si/) (IBMI) Medicinske fakultete Univerze v Ljubljani pod vodstvom prof. Maje Pohar Perme na podlagi podatkov [ocenjuje stopnjo reprodukcije](https://oblak8.mf.uni-lj.si/covid19/), to je hitrost širjenja okužbe, med posamičnimi intervencijami.

<a href="https://oblak8.mf.uni-lj.si/covid19/" class="img-link">
<img alt="R_t model" src="https://oblak8.mf.uni-lj.si/covid19/rt_graph.svg" width=600>
<!--<img alt="R_t model" src="/docs/ibmi-model-20200627.png">-->
<!--<img alt="R_t model" src="https://stat.columbia.edu/~jakulin/Covid/ocene_rt.png">-->
</a>

Več o metodologiji in izsledkih najdete v člankih v [Zdravniškem vestniku](https://vestnik.szd.si/index.php/ZdravVest/article/view/3068) ([povzetek](https://ibmi.mf.uni-lj.si/files/Pregledni%20povzetek_74e.pdf)) in znanstveni reviji [Mathematical Biosciences](https://www.sciencedirect.com/science/article/abs/pii/S0025556420301279).

<!--# Simulacije-->

## <a id="alarms"></a>Določanje alarmov
Dr. Janez Stare in dr. Nina Ružić Gorenjec ([IBMI](http://ibmi.mf.uni-lj.si/) MF UL) v analizi [Določanje alarmov pri epidemiji COVID-19 v Sloveniji](https://ibmi.mf.uni-lj.si/sl/centri/biostatisticni-center/interaktivno/dolocanje-alarmov-pri-epidemiji-covid-19-v-sloveniji) ocenjujeta zgornje meje pragov, ko je že prišlo do ponovnega zagona epidemije. Rezultati so relevantni predvsem za obdobja nizke pojavnosti virusa v populaciji. Za oceno uporabljata delež potrjeno okuženih izmed testiranih.

## <a id="articles"></a>Prispevki

- Raziskovalci IBMI so pod vodstvom dr. Nine Ružić Gorenjec v svoji analizi ["Od zgodbe o uspehu do katastrofe"](https://medium.com/sledilnik/od-zgodbe-o-uspehu-do-katastrofe-63b77b1a23e1) poudaril, kako zelo važna sta naša mobilnost in epidemiološko sledenje stikom za uspešen odziv proti epidemiji. (13. 1. 2021)

- ["Kaj ima matematika z epidemijo?"](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221) Članek razloži osnovne pojme in predstavi matematični vidik modeliranja. Povzeto po predavanju prof. Janeza Žiberta. (17. 12. 2020)

- Spletni pogovor STAznanost z naslovom ["Statistični podatki - osnova za razumevanje epidemije"](https://www.youtube.com/watch?v=Bwn6cfgPZ1Q&t=3s). V pogovoru so sodelovali: prof. dr. Leon Cizelj, vodja odseka za reaktorsko tehniko na Inštitutu Jožef Stefan; dr. Zarja Muršič, predstavnica Sledilnika; dr. Mario Fafangel, predstojnik Centra za nalezljive bolezni Nacionalnega inštituta za javno zdravje (NIJZ). (11. 12. 2020)

- Člani Sledilnika smo v prispevku ["Dve Sloveniji - ali res stopicamo na mestu?"](https://medium.com/sledilnik/dve-sloveniji-ali-res-stopicamo-na-mestu-27fac63d9e6f) opozorili na krajevne razlike v širitvi virusa in da je za zmanjšanje epidemije pomemben sočasen odziv v vsej državi. (2. 12. 2020)

- Sodelavci Sledilnika so v Državnem zboru RS predstavili [pomen podatkov in modeliranja za odločevalce](https://medium.com/sledilnik/povzetek-nastopov-strokovnjakov-s-seje-parlamentarnega-odbora-12-11-2020-5a3ead7b4898). (12. 11. 2020)<br><iframe class="youtube" src="https://www.youtube.com/embed/rwcqGV0fyC0?rel=0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

- ["Epidemija in modeli - Osnove epidemiološkega modeliranja"](https://medium.com/sledilnik/epidemija-in-modeli-786e02f1bd8a) vam v krajših posnetkih predstavi osnovne pojme, ki so najnujnejši za razumevanje epidemije v številkah. (3. 11. 2020)

- V uvodniku revije Zdravstveno varstvo, ki jo izdaja NIJZ, so I. Eržen, T. Kamenšek, M. Fošnarič in J. Žibert povzeli [trenutna spoznanja in ključne izzive pri modeliranju epidemije COVID-19](https://content.sciendo.com/view/journals/sjph/59/3/article-p117.xml?tab_body=abstract). (25. 6. 2020)

- Mlada sekcija Statističnega društva Slovenije je 21. aprila 2020 organizirala [največji spletni pogovor slovenskih znanstvenikov na temo COVID-19](https://medium.com/sledilnik/najve%C4%8Dji-posvet-znanstvenikov-zdru%C5%BEiti-je-treba-mo%C4%8D-institucij-in-znanstvene-skupnosti-v-boju-64233b35580c), ki ga je gostil Inštitut za biostatistiko in medicinsko informatiko (IBMI) Medicinske fakultete Univerze v Ljubljani. Pogovor sta vodila dr. Andrej Srakar in dr. Ana Slavec. STA je dogodek prenašala v živo več kot 850 udeležencem. (22. 4. 2020)

- Doc. dr. Andrej Srakar je za blog Udomačena statistika napisal izčrpen pregledni članek ["Uvod v modeliranje in statistične vidike COVID-19"](https://udomacenastatistika.wordpress.com/2020/04/20/uvod-v-modeliranje-in-statisticne-vidike-covid-19/) o prvih modelih širjenja epidemije bolezni COVID-19, ki so nastali v Sloveniji. (20. 4. 2020)
